#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Client.Services.Shipments
{
    /// <summary>
    /// Blazor WebAssembly HTTP client that proxies all calls to
    /// /api/shipments on the server. Registered as Scoped in Program.cs.
    /// </summary>
    public class ShipmentHttpService : IShipmentService
    {
        private readonly HttpClient _http;

        public ShipmentHttpService(HttpClient http) => _http = http;

        // ----------------------------------------------------------------
        //  Query
        // ----------------------------------------------------------------

        public async Task<List<ShipmentListDto>> GetShipmentsAsync(ShipmentFilterDto filter)
        {
            var url = BuildFilterUrl("api/shipments", filter);
            return await _http.GetFromJsonAsync<List<ShipmentListDto>>(url)
                   ?? new List<ShipmentListDto>();
        }

        public async Task<int> GetShipmentCountAsync(ShipmentFilterDto filter)
        {
            var url = BuildFilterUrl("api/shipments/count", filter);
            return await _http.GetFromJsonAsync<int>(url);
        }

        public async Task<ShipmentDetailDto?> GetShipmentByIdAsync(int shipmentId)
        {
            var response = await _http.GetAsync($"api/shipments/{shipmentId}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ShipmentDetailDto>();
        }

        public async Task<ShipmentDetailDto?> GetShipmentByTrackingAsync(string trackingNumber)
        {
            var response = await _http.GetAsync($"api/shipments/track/{Uri.EscapeDataString(trackingNumber)}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ShipmentDetailDto>();
        }

        public async Task<ShipmentStatsDto> GetStatsAsync()
            => await _http.GetFromJsonAsync<ShipmentStatsDto>("api/shipments/stats")
               ?? new ShipmentStatsDto();

        // ----------------------------------------------------------------
        //  Mutations
        // ----------------------------------------------------------------

        public async Task<ShipmentDetailDto> CreateShipmentAsync(CreateShipmentDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/shipments", dto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ShipmentDetailDto>())!;
        }

        public async Task<ShipmentDetailDto> UpdateShipmentAsync(int shipmentId, CreateShipmentDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/shipments/{shipmentId}", dto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ShipmentDetailDto>())!;
        }

        public async Task<bool> UpdateStatusAsync(int shipmentId, ShipmentStatus status)
        {
            var response = await _http.PatchAsJsonAsync($"api/shipments/{shipmentId}/status", status);
            return response.IsSuccessStatusCode;
        }

        public async Task<ShipmentEventDto> AddEventAsync(int shipmentId, AddShipmentEventDto dto)
        {
            var response = await _http.PostAsJsonAsync($"api/shipments/{shipmentId}/events", dto);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ShipmentEventDto>())!;
        }

        public async Task<bool> DeleteShipmentAsync(int shipmentId)
        {
            var response = await _http.DeleteAsync($"api/shipments/{shipmentId}");
            return response.IsSuccessStatusCode;
        }

        // ----------------------------------------------------------------
        //  Rate calculation (client-side mirror of server logic)
        // ----------------------------------------------------------------

        public decimal CalculateShippingCost(ServiceType serviceType, decimal weight,
            decimal length, decimal width, decimal height)
        {
            decimal dimWeight = length > 0 && width > 0 && height > 0
                ? length * width * height / 139m
                : 0;

            decimal billableWeight = Math.Max(weight, dimWeight);

            decimal baseRate = serviceType switch
            {
                ServiceType.SameDay       => 35.00m,
                ServiceType.Overnight     => 24.99m,
                ServiceType.TwoDay        => 16.99m,
                ServiceType.Ground        => 9.99m,
                ServiceType.Freight       => 5.99m,
                ServiceType.International => 49.99m,
                _                         => 9.99m
            };

            decimal perLbRate = serviceType switch
            {
                ServiceType.SameDay       => 1.50m,
                ServiceType.Overnight     => 1.25m,
                ServiceType.TwoDay        => 0.90m,
                ServiceType.Ground        => 0.50m,
                ServiceType.Freight       => 0.30m,
                ServiceType.International => 2.00m,
                _                         => 0.50m
            };

            const decimal fuelSurcharge = 1.175m;
            return Math.Round((baseRate + perLbRate * billableWeight) * fuelSurcharge, 2);
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------

        private static string BuildFilterUrl(string baseUrl, ShipmentFilterDto f)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(f.SearchTerm)) query["SearchTerm"] = f.SearchTerm;
            if (f.Status.HasValue)      query["Status"]      = ((int)f.Status.Value).ToString();
            if (f.ServiceType.HasValue) query["ServiceType"] = ((int)f.ServiceType.Value).ToString();
            if (f.DateFrom.HasValue)    query["DateFrom"]    = f.DateFrom.Value.ToString("o");
            if (f.DateTo.HasValue)      query["DateTo"]      = f.DateTo.Value.ToString("o");
            var qs = query.ToString();
            return string.IsNullOrEmpty(qs) ? baseUrl : $"{baseUrl}?{qs}";
        }
    }
}
