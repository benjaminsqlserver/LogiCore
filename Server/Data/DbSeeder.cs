using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogiCore.Server.Models.Shipments;

namespace LogiCore.Server.Data
{
    /// <summary>
    /// Seeds representative data on first run (no-op if Shipments table already has rows).
    /// Call from Program.cs after EnsureCreated / Migrate.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // Only seed when the table is empty
            if (await db.Shipments.IgnoreQueryFilters().AnyAsync())
                return;

            var today = DateTime.Today;

            var shipments = new List<Shipment>
            {
                new()
                {
                    TrackingNumber   = "LC20260201ABCD1234",
                    SenderName       = "Acme Corp",
                    SenderEmail      = "shipping@acme.com",
                    SenderPhone      = "555-0100",
                    RecipientName    = "Bob Smith",
                    RecipientEmail   = "bob@example.com",
                    RecipientPhone   = "555-0200",
                    OriginAddress    = "100 Warehouse Blvd",
                    OriginCity       = "Atlanta",
                    OriginState      = "GA",
                    OriginPostalCode = "30301",
                    OriginCountry    = "US",
                    DestinationAddress    = "42 Oak Street",
                    DestinationCity       = "Miami",
                    DestinationState      = "FL",
                    DestinationPostalCode = "33101",
                    DestinationCountry    = "US",
                    Status            = ShipmentStatus.InTransit,
                    ServiceType       = ServiceType.TwoDay,
                    TotalWeight       = 5.2m,
                    DeclaredValue     = 150m,
                    PackageCount      = 1,
                    Length = 12, Width = 8, Height = 6,
                    PickupDate        = today.AddDays(-2),
                    EstimatedDelivery = today,
                    ShippingCost      = 32.75m,
                    CreatedAt         = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt         = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    TrackingNumber   = "LC20260202EFGH5678",
                    SenderName       = "Global Retail Inc",
                    SenderEmail      = "ops@globalretail.com",
                    SenderPhone      = "555-0300",
                    RecipientName    = "Jane Doe",
                    RecipientEmail   = "jane@example.com",
                    RecipientPhone   = "555-0400",
                    OriginAddress    = "500 Commerce Park",
                    OriginCity       = "Chicago",
                    OriginState      = "IL",
                    OriginPostalCode = "60601",
                    OriginCountry    = "US",
                    DestinationAddress    = "7 Elm Court",
                    DestinationCity       = "New York",
                    DestinationState      = "NY",
                    DestinationPostalCode = "10001",
                    DestinationCountry    = "US",
                    Status            = ShipmentStatus.OutForDelivery,
                    ServiceType       = ServiceType.Overnight,
                    TotalWeight       = 2.0m,
                    DeclaredValue     = 300m,
                    PackageCount      = 2,
                    Length = 10, Width = 7, Height = 5,
                    PickupDate        = today.AddDays(-1),
                    EstimatedDelivery = today,
                    ShippingCost      = 29.50m,
                    CreatedAt         = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt         = DateTime.UtcNow.AddHours(-3)
                },
                new()
                {
                    TrackingNumber   = "LC20260203IJKL9012",
                    SenderName       = "Tech Distributors LLC",
                    SenderEmail      = "dist@techd.com",
                    SenderPhone      = "555-0500",
                    RecipientName    = "Carol White",
                    RecipientEmail   = "carol@example.com",
                    RecipientPhone   = "555-0600",
                    OriginAddress    = "25 Logistics Lane",
                    OriginCity       = "Dallas",
                    OriginState      = "TX",
                    OriginPostalCode = "75201",
                    OriginCountry    = "US",
                    DestinationAddress    = "88 Pine Ave",
                    DestinationCity       = "Seattle",
                    DestinationState      = "WA",
                    DestinationPostalCode = "98101",
                    DestinationCountry    = "US",
                    Status            = ShipmentStatus.Pending,
                    ServiceType       = ServiceType.Ground,
                    TotalWeight       = 18.5m,
                    DeclaredValue     = 50m,
                    PackageCount      = 3,
                    Length = 24, Width = 18, Height = 12,
                    PickupDate        = today,
                    EstimatedDelivery = today.AddDays(5),
                    ShippingCost      = 19.20m,
                    CreatedAt         = DateTime.UtcNow.AddHours(-2),
                    UpdatedAt         = DateTime.UtcNow.AddHours(-2)
                },
                new()
                {
                    TrackingNumber   = "LC20260204MNOP3456",
                    SenderName       = "FastShip Co",
                    SenderEmail      = "send@fastship.com",
                    SenderPhone      = "555-0700",
                    RecipientName    = "David Brown",
                    RecipientEmail   = "david@example.com",
                    RecipientPhone   = "555-0800",
                    OriginAddress    = "9 Harbor View",
                    OriginCity       = "Los Angeles",
                    OriginState      = "CA",
                    OriginPostalCode = "90001",
                    OriginCountry    = "US",
                    DestinationAddress    = "33 Maple Drive",
                    DestinationCity       = "Phoenix",
                    DestinationState      = "AZ",
                    DestinationPostalCode = "85001",
                    DestinationCountry    = "US",
                    Status            = ShipmentStatus.Delivered,
                    ServiceType       = ServiceType.SameDay,
                    TotalWeight       = 1.0m,
                    DeclaredValue     = 75m,
                    PackageCount      = 1,
                    Length = 6, Width = 6, Height = 4,
                    PickupDate        = today.AddDays(-3),
                    EstimatedDelivery = today.AddDays(-3),
                    ActualDelivery    = today.AddDays(-3).AddHours(14),
                    SignedBy          = "D. Brown",
                    ShippingCost      = 44.10m,
                    CreatedAt         = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt         = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    TrackingNumber   = "LC20260205QRST7890",
                    SenderName       = "International Exports",
                    SenderEmail      = "export@intl.com",
                    SenderPhone      = "555-0900",
                    RecipientName    = "Euro Partners GmbH",
                    RecipientEmail   = "contact@europartners.de",
                    RecipientPhone   = "+49-555-1000",
                    OriginAddress    = "1 Port Terminal",
                    OriginCity       = "Houston",
                    OriginState      = "TX",
                    OriginPostalCode = "77001",
                    OriginCountry    = "US",
                    DestinationAddress    = "Hauptstraße 42",
                    DestinationCity       = "Berlin",
                    DestinationState      = "BE",
                    DestinationPostalCode = "10115",
                    DestinationCountry    = "DE",
                    Status            = ShipmentStatus.AtHub,
                    ServiceType       = ServiceType.International,
                    TotalWeight       = 35.0m,
                    DeclaredValue     = 2500m,
                    PackageCount      = 5,
                    Length = 36, Width = 24, Height = 20,
                    PickupDate        = today.AddDays(-5),
                    EstimatedDelivery = today.AddDays(9),
                    ShippingCost      = 218.40m,
                    CreatedAt         = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt         = DateTime.UtcNow.AddDays(-1)
                }
            };

            await db.Shipments.AddRangeAsync(shipments);
            await db.SaveChangesAsync();

            // Now seed events using the persisted IDs
            var s1 = shipments[0];
            var s2 = shipments[1];
            var s3 = shipments[2];
            var s4 = shipments[3];
            var s5 = shipments[4];

            var events = new List<ShipmentEvent>
            {
                // Shipment 1 — InTransit
                Ev(s1, ShipmentEventType.Created,          "Atlanta, GA",       "Shipment created",                          daysAgo: 2),
                Ev(s1, ShipmentEventType.PickedUp,         "Atlanta, GA",       "Package picked up by driver",               daysAgo: 2, hoursAgo: -4),
                Ev(s1, ShipmentEventType.DepartedFacility, "Atlanta Hub",       "Departed Atlanta sort facility",            daysAgo: 1, hoursAgo: 12),
                Ev(s1, ShipmentEventType.InTransit,        "Jacksonville, FL",  "In transit to destination",                 daysAgo: 1),

                // Shipment 2 — OutForDelivery
                Ev(s2, ShipmentEventType.Created,          "Chicago, IL",       "Shipment created",                          daysAgo: 1),
                Ev(s2, ShipmentEventType.PickedUp,         "Chicago, IL",       "Package picked up",                         daysAgo: 1, hoursAgo: -3),
                Ev(s2, ShipmentEventType.ArrivedAtHub,     "Newark Hub",        "Arrived at Newark sort facility",           hoursAgo: 6),
                Ev(s2, ShipmentEventType.OutForDelivery,   "New York, NY",      "Out for delivery — expected by 8 PM",       hoursAgo: 2),

                // Shipment 3 — Pending (just created)
                Ev(s3, ShipmentEventType.Created,          "Dallas, TX",        "Shipment created, awaiting pickup",         hoursAgo: 2),

                // Shipment 4 — Delivered
                Ev(s4, ShipmentEventType.Created,          "Los Angeles, CA",   "Shipment created",                          daysAgo: 3),
                Ev(s4, ShipmentEventType.PickedUp,         "Los Angeles, CA",   "Package picked up",                         daysAgo: 3, hoursAgo: -2),
                Ev(s4, ShipmentEventType.OutForDelivery,   "Phoenix, AZ",       "Out for delivery",                          daysAgo: 3, hoursAgo: -5),
                Ev(s4, ShipmentEventType.Delivered,        "Phoenix, AZ",       "Delivered. Signed by D. Brown",             daysAgo: 3, hoursAgo: -6),

                // Shipment 5 — AtHub (international)
                Ev(s5, ShipmentEventType.Created,          "Houston, TX",       "Shipment created for international export", daysAgo: 5),
                Ev(s5, ShipmentEventType.PickedUp,         "Houston, TX",       "Picked up for international export",        daysAgo: 5, hoursAgo: -3),
                Ev(s5, ShipmentEventType.DepartedFacility, "Houston Export Hub","Departed Houston international facility",   daysAgo: 3),
                Ev(s5, ShipmentEventType.ArrivedAtHub,     "JFK International", "Arrived at JFK international sort facility",daysAgo: 2),
                Ev(s5, ShipmentEventType.CustomsClearance, "JFK International", "Customs documentation submitted",           daysAgo: 1),
            };

            await db.ShipmentEvents.AddRangeAsync(events);
            await db.SaveChangesAsync();
        }

        private static ShipmentEvent Ev(
            Shipment s,
            ShipmentEventType type,
            string location,
            string notes,
            int daysAgo = 0,
            int hoursAgo = 0)
            => new()
            {
                ShipmentId    = s.ShipmentId,
                TrackingNumber = s.TrackingNumber,
                EventType     = type,
                Location      = location,
                Notes         = notes,
                OperatorName  = "System",
                Timestamp     = DateTime.UtcNow.AddDays(-daysAgo).AddHours(-hoursAgo)
            };
    }
}
