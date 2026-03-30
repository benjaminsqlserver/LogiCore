using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogiCore.Server.Data;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Server.Services.Shipments
{
    /// <summary>
    /// EF Core / SQL Server implementation of IShipmentService.
    /// Registered as Scoped — receives AppDbContext via DI.
    /// </summary>
    public class ShipmentService : IShipmentService
    {
        private readonly AppDbContext _db;

        public ShipmentService(AppDbContext db) => _db = db;

        // ----------------------------------------------------------------
        //  Query
        // ----------------------------------------------------------------

        public async Task<List<ShipmentListDto>> GetShipmentsAsync(ShipmentFilterDto filter)
        {
            var q = _db.Shipments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                q = q.Where(s =>
                    s.TrackingNumber.ToLower().Contains(term) ||
                    s.SenderName.ToLower().Contains(term) ||
                    s.RecipientName.ToLower().Contains(term) ||
                    s.OriginCity.ToLower().Contains(term) ||
                    s.DestinationCity.ToLower().Contains(term));
            }

            if (filter.Status.HasValue)
                q = q.Where(s => s.Status == filter.Status.Value);

            if (filter.ServiceType.HasValue)
                q = q.Where(s => s.ServiceType == filter.ServiceType.Value);

            if (filter.DateFrom.HasValue)
                q = q.Where(s => s.PickupDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                q = q.Where(s => s.PickupDate <= filter.DateTo.Value.AddDays(1));

            return await q
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new ShipmentListDto
                {
                    ShipmentId        = s.ShipmentId,
                    TrackingNumber    = s.TrackingNumber,
                    SenderName        = s.SenderName,
                    RecipientName     = s.RecipientName,
                    Origin            = s.OriginCity + ", " + s.OriginState,
                    Destination       = s.DestinationCity + ", " + s.DestinationState,
                    ServiceType       = s.ServiceType,
                    Status            = s.Status,
                    TotalWeight       = s.TotalWeight,
                    PackageCount      = s.PackageCount,
                    ShippingCost      = s.ShippingCost,
                    PickupDate        = s.PickupDate,
                    EstimatedDelivery = s.EstimatedDelivery,
                    CreatedAt         = s.CreatedAt,
                    //Added for grid display:
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer != null ? s.Customer.FullName : null,
                })
                .ToListAsync();
        }

        public async Task<int> GetShipmentCountAsync(ShipmentFilterDto filter)
        {
            var q = _db.Shipments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                q = q.Where(s =>
                    s.TrackingNumber.ToLower().Contains(term) ||
                    s.SenderName.ToLower().Contains(term) ||
                    s.RecipientName.ToLower().Contains(term));
            }

            if (filter.Status.HasValue) q = q.Where(s => s.Status == filter.Status.Value);
            if (filter.ServiceType.HasValue) q = q.Where(s => s.ServiceType == filter.ServiceType.Value);

            return await q.CountAsync();
        }

        public async Task<ShipmentDetailDto?> GetShipmentByIdAsync(int shipmentId)
        {
            var s = await _db.Shipments
                .AsNoTracking()
                .Include(x => x.Events.OrderByDescending(e => e.Timestamp))
                .Include(x => x.Packages)
                 //Added include to load customer data for detail view:
                 .Include(x => x.Customer)          // ← new
                .FirstOrDefaultAsync(x => x.ShipmentId == shipmentId);

            return s == null ? null : ToDetailDto(s);
        }

        public async Task<ShipmentDetailDto?> GetShipmentByTrackingAsync(string trackingNumber)
        {
            var s = await _db.Shipments
                .AsNoTracking()
                .Include(x => x.Events.OrderByDescending(e => e.Timestamp))
                .Include(x => x.Packages)
                .FirstOrDefaultAsync(x => x.TrackingNumber == trackingNumber.ToUpper());

            return s == null ? null : ToDetailDto(s);
        }

        public async Task<ShipmentStatsDto> GetStatsAsync()
        {
            var today = DateTime.Today;

            var counts = await _db.Shipments
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total          = g.Count(),
                    Active         = g.Count(s => s.Status != ShipmentStatus.Delivered
                                               && s.Status != ShipmentStatus.Cancelled
                                               && s.Status != ShipmentStatus.Returned),
                    OutForDelivery = g.Count(s => s.Status == ShipmentStatus.OutForDelivery),
                    DeliveredToday = g.Count(s => s.Status == ShipmentStatus.Delivered
                                               && s.ActualDelivery != null
                                               && s.ActualDelivery!.Value.Date == today),
                    Pending        = g.Count(s => s.Status == ShipmentStatus.Pending),
                    Failed         = g.Count(s => s.Status == ShipmentStatus.Failed),
                    RevenueToday   = g.Where(s => s.CreatedAt.Date == today).Sum(s => s.ShippingCost)
                })
                .FirstOrDefaultAsync();

            var deliveredWithEdd = await _db.Shipments
                .Where(s => s.Status == ShipmentStatus.Delivered
                         && s.ActualDelivery.HasValue
                         && s.EstimatedDelivery.HasValue)
                .Select(s => new { s.ActualDelivery, s.EstimatedDelivery })
                .ToListAsync();

            double onTimeRate = deliveredWithEdd.Count > 0
                ? Math.Round(
                    (double)deliveredWithEdd.Count(s => s.ActualDelivery!.Value.Date <= s.EstimatedDelivery!.Value.Date)
                    / deliveredWithEdd.Count * 100, 1)
                : 95.0;

            return new ShipmentStatsDto
            {
                TotalShipments  = counts?.Total ?? 0,
                ActiveShipments = counts?.Active ?? 0,
                OutForDelivery  = counts?.OutForDelivery ?? 0,
                DeliveredToday  = counts?.DeliveredToday ?? 0,
                PendingPickup   = counts?.Pending ?? 0,
                Failed          = counts?.Failed ?? 0,
                RevenueToday    = counts?.RevenueToday ?? 0,
                OnTimeRate      = onTimeRate
            };
        }

        // ----------------------------------------------------------------
        //  Mutations
        // ----------------------------------------------------------------

        public async Task<ShipmentDetailDto> CreateShipmentAsync(CreateShipmentDto dto)
        {
            var shipment = new Shipment
            {
                TrackingNumber        = Shipment.GenerateTracking(),
                SenderName            = dto.SenderName,
                SenderEmail           = dto.SenderEmail,
                SenderPhone           = dto.SenderPhone,
                RecipientName         = dto.RecipientName,
                RecipientEmail        = dto.RecipientEmail,
                RecipientPhone        = dto.RecipientPhone,
                OriginAddress         = dto.OriginAddress,
                OriginCity            = dto.OriginCity,
                OriginState           = dto.OriginState,
                OriginPostalCode      = dto.OriginPostalCode,
                OriginCountry         = dto.OriginCountry,
                DestinationAddress    = dto.DestinationAddress,
                DestinationCity       = dto.DestinationCity,
                DestinationState      = dto.DestinationState,
                DestinationPostalCode = dto.DestinationPostalCode,
                DestinationCountry    = dto.DestinationCountry,
                ServiceType           = dto.ServiceType,
                TotalWeight           = dto.TotalWeight,
                DeclaredValue         = dto.DeclaredValue,
                PackageCount          = dto.PackageCount,
                Length                = dto.Length,
                Width                 = dto.Width,
                Height                = dto.Height,
                PickupDate            = dto.PickupDate,
                EstimatedDelivery     = CalculateEstimatedDelivery(dto.ServiceType, dto.PickupDate),
                Notes                 = dto.Notes,
                ShippingCost          = CalculateShippingCost(dto.ServiceType, dto.TotalWeight, dto.Length, dto.Width, dto.Height),
                Status                = ShipmentStatus.Pending,
                CreatedAt             = DateTime.UtcNow,
                UpdatedAt             = DateTime.UtcNow,
                //link to customer if provided (nullable for walk-ins/legacy data):
                CustomerId = dto.CustomerId,       // ← new
            };

            _db.Shipments.Add(shipment);
            await _db.SaveChangesAsync();

            _db.ShipmentEvents.Add(new ShipmentEvent
            {
                ShipmentId     = shipment.ShipmentId,
                TrackingNumber = shipment.TrackingNumber,
                EventType      = ShipmentEventType.Created,
                Location       = $"{dto.OriginCity}, {dto.OriginState}",
                Notes          = "Shipment created",
                OperatorName   = "System",
                Timestamp      = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return (await GetShipmentByIdAsync(shipment.ShipmentId))!;
        }

        public async Task<ShipmentDetailDto> UpdateShipmentAsync(int shipmentId, CreateShipmentDto dto)
        {
            var s = await _db.Shipments.FindAsync(shipmentId)
                ?? throw new KeyNotFoundException($"Shipment {shipmentId} not found.");

            s.SenderName            = dto.SenderName;
            s.SenderEmail           = dto.SenderEmail;
            s.SenderPhone           = dto.SenderPhone;
            s.RecipientName         = dto.RecipientName;
            s.RecipientEmail        = dto.RecipientEmail;
            s.RecipientPhone        = dto.RecipientPhone;
            s.OriginAddress         = dto.OriginAddress;
            s.OriginCity            = dto.OriginCity;
            s.OriginState           = dto.OriginState;
            s.OriginPostalCode      = dto.OriginPostalCode;
            s.OriginCountry         = dto.OriginCountry;
            s.DestinationAddress    = dto.DestinationAddress;
            s.DestinationCity       = dto.DestinationCity;
            s.DestinationState      = dto.DestinationState;
            s.DestinationPostalCode = dto.DestinationPostalCode;
            s.DestinationCountry    = dto.DestinationCountry;
            s.ServiceType           = dto.ServiceType;
            s.TotalWeight           = dto.TotalWeight;
            s.DeclaredValue         = dto.DeclaredValue;
            s.PackageCount          = dto.PackageCount;
            s.Length                = dto.Length;
            s.Width                 = dto.Width;
            s.Height                = dto.Height;
            s.PickupDate            = dto.PickupDate;
            s.EstimatedDelivery     = CalculateEstimatedDelivery(dto.ServiceType, dto.PickupDate);
            s.Notes                 = dto.Notes;
            s.ShippingCost          = CalculateShippingCost(dto.ServiceType, dto.TotalWeight, dto.Length, dto.Width, dto.Height);
            s.UpdatedAt             = DateTime.UtcNow;
            // UpdateShipmentAsync — allow re-linking:
            s.CustomerId = dto.CustomerId;         // ← new line after the other property assignments

            await _db.SaveChangesAsync();
            return (await GetShipmentByIdAsync(shipmentId))!;
        }

        public async Task<bool> UpdateStatusAsync(int shipmentId, ShipmentStatus status)
        {
            var s = await _db.Shipments.FindAsync(shipmentId);
            if (s == null) return false;

            s.Status    = status;
            s.UpdatedAt = DateTime.UtcNow;

            if (status == ShipmentStatus.Delivered)
                s.ActualDelivery = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ShipmentEventDto> AddEventAsync(int shipmentId, AddShipmentEventDto dto)
        {
            var s = await _db.Shipments.FindAsync(shipmentId)
                ?? throw new KeyNotFoundException($"Shipment {shipmentId} not found.");

            var ev = new ShipmentEvent
            {
                ShipmentId     = shipmentId,
                TrackingNumber = s.TrackingNumber,
                EventType      = dto.EventType,
                Location       = dto.Location,
                Notes          = dto.Notes,
                OperatorName   = dto.OperatorName,
                Timestamp      = DateTime.UtcNow
            };

            _db.ShipmentEvents.Add(ev);

            var newStatus = EventTypeToStatus(dto.EventType);
            if (newStatus.HasValue)
            {
                s.Status    = newStatus.Value;
                s.UpdatedAt = DateTime.UtcNow;
                if (newStatus == ShipmentStatus.Delivered)
                    s.ActualDelivery = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return new ShipmentEventDto
            {
                EventId        = ev.EventId,
                ShipmentId     = ev.ShipmentId,
                TrackingNumber = ev.TrackingNumber,
                EventType      = ev.EventType,
                Location       = ev.Location,
                Notes          = ev.Notes,
                OperatorName   = ev.OperatorName,
                Timestamp      = ev.Timestamp
            };
        }

        public async Task<bool> DeleteShipmentAsync(int shipmentId)
        {
            var s = await _db.Shipments.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.ShipmentId == shipmentId);
            if (s == null) return false;

            s.IsDeleted = true;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------
        //  Rate Calculation (pure logic — no DB)
        // ----------------------------------------------------------------

        public decimal CalculateShippingCost(ServiceType serviceType, decimal weight, decimal length, decimal width, decimal height)
        {
            decimal dimWeight = length > 0 && width > 0 && height > 0
                ? (length * width * height) / 139m
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
        //  Private helpers
        // ----------------------------------------------------------------

        private static DateTime CalculateEstimatedDelivery(ServiceType svc, DateTime pickup) => svc switch
        {
            ServiceType.SameDay       => pickup,
            ServiceType.Overnight     => pickup.AddDays(1),
            ServiceType.TwoDay        => pickup.AddDays(2),
            ServiceType.Ground        => pickup.AddDays(5),
            ServiceType.Freight       => pickup.AddDays(7),
            ServiceType.International => pickup.AddDays(14),
            _                         => pickup.AddDays(5)
        };

        private static ShipmentStatus? EventTypeToStatus(ShipmentEventType t) => t switch
        {
            ShipmentEventType.PickupScheduled  => ShipmentStatus.PickupScheduled,
            ShipmentEventType.PickedUp         => ShipmentStatus.PickedUp,
            ShipmentEventType.ArrivedAtHub     => ShipmentStatus.AtHub,
            ShipmentEventType.InTransit        => ShipmentStatus.InTransit,
            ShipmentEventType.DepartedFacility => ShipmentStatus.InTransit,
            ShipmentEventType.OutForDelivery   => ShipmentStatus.OutForDelivery,
            ShipmentEventType.Delivered        => ShipmentStatus.Delivered,
            ShipmentEventType.DeliveryFailed   => ShipmentStatus.Failed,
            ShipmentEventType.ReturnInitiated  => ShipmentStatus.ReturnInitiated,
            ShipmentEventType.ReturnedToSender => ShipmentStatus.Returned,
            ShipmentEventType.Cancelled        => ShipmentStatus.Cancelled,
            _                                  => null
        };

        private static ShipmentDetailDto ToDetailDto(Shipment s) => new()
        {
            ShipmentId            = s.ShipmentId,
            TrackingNumber        = s.TrackingNumber,
            SenderName            = s.SenderName,
            SenderEmail           = s.SenderEmail,
            SenderPhone           = s.SenderPhone,
            RecipientName         = s.RecipientName,
            RecipientEmail        = s.RecipientEmail,
            RecipientPhone        = s.RecipientPhone,
            OriginAddress         = s.OriginAddress,
            OriginCity            = s.OriginCity,
            OriginState           = s.OriginState,
            OriginPostalCode      = s.OriginPostalCode,
            OriginCountry         = s.OriginCountry,
            DestinationAddress    = s.DestinationAddress,
            DestinationCity       = s.DestinationCity,
            DestinationState      = s.DestinationState,
            DestinationPostalCode = s.DestinationPostalCode,
            DestinationCountry    = s.DestinationCountry,
            Status                = s.Status,
            ServiceType           = s.ServiceType,
            TotalWeight           = s.TotalWeight,
            DeclaredValue         = s.DeclaredValue,
            PackageCount          = s.PackageCount,
            Length                = s.Length,
            Width                 = s.Width,
            Height                = s.Height,
            PickupDate            = s.PickupDate,
            EstimatedDelivery     = s.EstimatedDelivery,
            ActualDelivery        = s.ActualDelivery,
            SignedBy              = s.SignedBy,
            Notes                 = s.Notes,
            ShippingCost          = s.ShippingCost,
            IsBilled              = s.IsBilled,
            CreatedAt             = s.CreatedAt,
            UpdatedAt             = s.UpdatedAt,

            // ToDetailDto — add to the mapping:
            CustomerId = s.CustomerId,
            CustomerName = s.Customer?.FullName,
            CustomerEmail = s.Customer?.Email,
            Events = s.Events.Select(e => new ShipmentEventDto
            {
                EventId        = e.EventId,
                ShipmentId     = e.ShipmentId,
                TrackingNumber = e.TrackingNumber,
                EventType      = e.EventType,
                Location       = e.Location,
                Notes          = e.Notes,
                OperatorName   = e.OperatorName,
                Timestamp      = e.Timestamp
            }).ToList()
        };
    }
}
