#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using LogiCore.Server.Models.Customers;

namespace LogiCore.Client.Services.Customers
{
    /// <summary>
    /// Blazor client HTTP proxy for /api/customers.
    /// Registered as Scoped in Client/Program.cs.
    /// </summary>
    public class CustomerHttpService : ICustomerService
    {
        private readonly HttpClient _http;

        // Shared options so enums are sent/received as strings ("Individual", "Active")
        // instead of integers, matching the server's JsonStringEnumConverter config.
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        public CustomerHttpService(HttpClient http) => _http = http;

        // ----------------------------------------------------------------
        //  Query
        // ----------------------------------------------------------------

        public async Task<List<CustomerListDto>> GetCustomersAsync(CustomerFilterDto filter)
        {
            var url = BuildFilterUrl("api/customers", filter);
            return await _http.GetFromJsonAsync<List<CustomerListDto>>(url, _jsonOptions)
                   ?? new List<CustomerListDto>();
        }

        public async Task<int> GetCustomerCountAsync(CustomerFilterDto filter)
        {
            var url = BuildFilterUrl("api/customers/count", filter);
            return await _http.GetFromJsonAsync<int>(url, _jsonOptions);
        }

        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int customerId)
        {
            var response = await _http.GetAsync($"api/customers/{customerId}");
            if (response.StatusCode == HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerDetailDto>(_jsonOptions);
        }

        public async Task<CustomerStatsDto> GetStatsAsync()
            => await _http.GetFromJsonAsync<CustomerStatsDto>("api/customers/stats", _jsonOptions)
               ?? new CustomerStatsDto();

        // ----------------------------------------------------------------
        //  Mutations
        // ----------------------------------------------------------------

        public async Task<CustomerDetailDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/customers", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerDetailDto>(_jsonOptions))!;
        }

        public async Task<CustomerDetailDto> UpdateCustomerAsync(int customerId, CreateCustomerDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/customers/{customerId}", dto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<CustomerDetailDto>(_jsonOptions))!;
        }

        public async Task<bool> UpdateStatusAsync(int customerId, CustomerStatus status)
        {
            var response = await _http.PatchAsJsonAsync($"api/customers/{customerId}/status", status, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var response = await _http.DeleteAsync($"api/customers/{customerId}");
            return response.IsSuccessStatusCode;
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------

        private static string BuildFilterUrl(string baseUrl, CustomerFilterDto f)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(f.SearchTerm)) query["SearchTerm"] = f.SearchTerm;
            // Send enum names as strings so the server [FromQuery] binder can parse them
            if (f.AccountType.HasValue) query["AccountType"] = f.AccountType.Value.ToString();
            if (f.Status.HasValue) query["Status"] = f.Status.Value.ToString();
            var qs = query.ToString();
            return string.IsNullOrEmpty(qs) ? baseUrl : $"{baseUrl}?{qs}";
        }
    }
}
