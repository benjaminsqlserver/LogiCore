#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Client.Services.Shipments
{
    /// <summary>
    /// Client-side contract for shipment operations.
    /// Implemented by ShipmentHttpService which calls the REST API.
    /// </summary>
    public interface IShipmentService
    {
        Task<List<ShipmentListDto>> GetShipmentsAsync(ShipmentFilterDto filter);
        Task<int> GetShipmentCountAsync(ShipmentFilterDto filter);
        Task<ShipmentDetailDto?> GetShipmentByIdAsync(int shipmentId);
        Task<ShipmentDetailDto?> GetShipmentByTrackingAsync(string trackingNumber);
        Task<ShipmentDetailDto> CreateShipmentAsync(CreateShipmentDto dto);
        Task<ShipmentDetailDto> UpdateShipmentAsync(int shipmentId, CreateShipmentDto dto);
        Task<bool> UpdateStatusAsync(int shipmentId, ShipmentStatus status);
        Task<ShipmentEventDto> AddEventAsync(int shipmentId, AddShipmentEventDto dto);
        Task<bool> DeleteShipmentAsync(int shipmentId);
        Task<ShipmentStatsDto> GetStatsAsync();
        decimal CalculateShippingCost(ServiceType serviceType, decimal weight, decimal length, decimal width, decimal height);
    }
}
