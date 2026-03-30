#nullable enable
using LogiCore.Server.Models.Customers;
using System;
using System.Collections.Generic;

namespace LogiCore.Server.Models.Shipments
{
    public class Shipment
    {
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        // Sender
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPhone { get; set; } = string.Empty;

        // Recipient
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;

        // Customer link (nullable — legacy/walk-in shipments have no account)
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }   // nav property

        // Addresses
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

        // Shipment Details
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Pending;
        public ServiceType ServiceType { get; set; } = ServiceType.Ground;
        public decimal TotalWeight { get; set; }
        public decimal DeclaredValue { get; set; }
        public int PackageCount { get; set; } = 1;

        // Dimensions (inches)
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }

        // Dates
        public DateTime PickupDate { get; set; } = DateTime.Today;
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }

        // Delivery
        public string? SignedBy { get; set; }
        public string? ProofOfDeliveryUrl { get; set; }
        public string? Notes { get; set; }

        // Billing
        public decimal ShippingCost { get; set; }
        public bool IsBilled { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public List<ShipmentEvent> Events { get; set; } = new();
        public List<Package> Packages { get; set; } = new();

        public static string GenerateTracking()
            => $"LC{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }

    public class Package
    {
        public int PackageId { get; set; }
        public int ShipmentId { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal DeclaredValue { get; set; }
    }

    public class ShipmentEvent
    {
        public int EventId { get; set; }
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentEventType EventType { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum ShipmentStatus
    {
        Pending,
        PickupScheduled,
        PickedUp,
        InTransit,
        AtHub,
        OutForDelivery,
        Delivered,
        Failed,
        ReturnInitiated,
        Returned,
        Cancelled
    }

    public enum ServiceType
    {
        SameDay,
        Overnight,
        TwoDay,
        Ground,
        Freight,
        International
    }

    public enum ShipmentEventType
    {
        Created,
        PickupScheduled,
        PickedUp,
        DepartedFacility,
        ArrivedAtHub,
        InTransit,
        OutForDelivery,
        DeliveryAttempted,
        Delivered,
        DeliveryFailed,
        ReturnInitiated,
        ReturnedToSender,
        Cancelled,
        Exception,
        HoldAtLocation,
        CustomsClearance
    }
}
