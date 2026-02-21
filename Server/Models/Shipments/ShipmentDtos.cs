#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogiCore.Server.Models.Shipments
{
    public class ShipmentListDto
    {
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public ServiceType ServiceType { get; set; }
        public ShipmentStatus Status { get; set; }
        public decimal TotalWeight { get; set; }
        public int PackageCount { get; set; }
        public decimal ShippingCost { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ShipmentDetailDto
    {
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPhone { get; set; } = string.Empty;

        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;

        public string OriginAddress { get; set; } = string.Empty;
        public string OriginCity { get; set; } = string.Empty;
        public string OriginState { get; set; } = string.Empty;
        public string OriginPostalCode { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = "US";

        public string DestinationAddress { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationState { get; set; } = string.Empty;
        public string DestinationPostalCode { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = "US";

        public ShipmentStatus Status { get; set; }
        public ServiceType ServiceType { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal DeclaredValue { get; set; }
        public int PackageCount { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }

        public DateTime PickupDate { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }

        public string? SignedBy { get; set; }
        public string? ProofOfDeliveryUrl { get; set; }
        public string? Notes { get; set; }
        public decimal ShippingCost { get; set; }
        public bool IsBilled { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<ShipmentEventDto> Events { get; set; } = new();
        public List<PackageDto> Packages { get; set; } = new();
    }

    public class CreateShipmentDto
    {
        [Required]
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPhone { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;

        [Required]
        public string OriginAddress { get; set; } = string.Empty;
        [Required]
        public string OriginCity { get; set; } = string.Empty;
        public string OriginState { get; set; } = string.Empty;
        [Required]
        public string OriginPostalCode { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = "US";

        [Required]
        public string DestinationAddress { get; set; } = string.Empty;
        [Required]
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationState { get; set; } = string.Empty;
        [Required]
        public string DestinationPostalCode { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = "US";

        public ServiceType ServiceType { get; set; } = ServiceType.Ground;

        [Range(0.01, 10000)]
        public decimal TotalWeight { get; set; } = 1;
        public decimal DeclaredValue { get; set; }
        public int PackageCount { get; set; } = 1;
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }

        public DateTime PickupDate { get; set; } = DateTime.Today;
        public string? Notes { get; set; }
    }

    public class ShipmentEventDto
    {
        public int EventId { get; set; }
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentEventType EventType { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AddShipmentEventDto
    {
        [Required]
        public ShipmentEventType EventType { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string OperatorName { get; set; } = "System";
    }

    public class PackageDto
    {
        public int PackageId { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal DeclaredValue { get; set; }
    }

    public class ShipmentFilterDto
    {
        public string? SearchTerm { get; set; }
        public ShipmentStatus? Status { get; set; }
        public ServiceType? ServiceType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 25;
    }

    public class ShipmentStatsDto
    {
        public int TotalShipments { get; set; }
        public int ActiveShipments { get; set; }
        public int OutForDelivery { get; set; }
        public int DeliveredToday { get; set; }
        public int PendingPickup { get; set; }
        public int Failed { get; set; }
        public decimal RevenueToday { get; set; }
        public double OnTimeRate { get; set; }
    }
}
