#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using LogiCore.Server.Models.Shipments;
using LogiCore.Client.Services.Shipments;

namespace LogiCore.Client.Pages.Shipments
{
    public partial class TrackShipment
    {
        [Parameter] public string? TrackingNumber { get; set; }

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected IShipmentService ShipmentService { get; set; } = default!;

        private string inputTracking = string.Empty;
        private ShipmentDetailDto? shipment;
        private bool isSearching;
        private bool notFound;

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrWhiteSpace(TrackingNumber))
            {
                inputTracking = TrackingNumber;
                await DoTrackShipment();
            }
        }

        private async Task DoTrackShipment()
        {
            if (string.IsNullOrWhiteSpace(inputTracking)) return;

            isSearching = true;
            notFound = false;
            shipment = null;

            shipment = await ShipmentService.GetShipmentByTrackingAsync(inputTracking.Trim().ToUpper());
            notFound = shipment == null;
            isSearching = false;
        }

        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await DoTrackShipment();
        }

        private static BadgeStyle GetStatusBadge(ShipmentStatus s) => s switch
        {
            ShipmentStatus.Pending => BadgeStyle.Warning,
            ShipmentStatus.PickupScheduled or ShipmentStatus.PickedUp => BadgeStyle.Info,
            ShipmentStatus.InTransit or ShipmentStatus.AtHub => BadgeStyle.Primary,
            ShipmentStatus.OutForDelivery => BadgeStyle.Info,
            ShipmentStatus.Delivered => BadgeStyle.Success,
            ShipmentStatus.Failed => BadgeStyle.Danger,
            ShipmentStatus.Cancelled => BadgeStyle.Light,
            _ => BadgeStyle.Secondary
        };

        private static string GetStatusLabel(ShipmentStatus s) => s switch
        {
            ShipmentStatus.PickupScheduled => "Pickup Scheduled",
            ShipmentStatus.PickedUp => "Picked Up",
            ShipmentStatus.InTransit => "In Transit",
            ShipmentStatus.AtHub => "At Hub",
            ShipmentStatus.OutForDelivery => "Out for Delivery",
            ShipmentStatus.ReturnInitiated => "Return Initiated",
            _ => s.ToString()
        };

        private static string GetEventDotClass(ShipmentEventType t) => t switch
        {
            ShipmentEventType.Delivered => "success",
            ShipmentEventType.OutForDelivery => "info",
            ShipmentEventType.Created => "secondary",
            ShipmentEventType.DeliveryFailed or ShipmentEventType.Exception => "danger",
            ShipmentEventType.CustomsClearance or ShipmentEventType.HoldAtLocation => "warning",
            _ => "primary"
        };

        private static string FormatEventType(ShipmentEventType t) => t switch
        {
            ShipmentEventType.PickupScheduled => "Pickup Scheduled",
            ShipmentEventType.PickedUp => "Picked Up",
            ShipmentEventType.DepartedFacility => "Departed Facility",
            ShipmentEventType.ArrivedAtHub => "Arrived at Hub",
            ShipmentEventType.InTransit => "In Transit",
            ShipmentEventType.OutForDelivery => "Out for Delivery",
            ShipmentEventType.DeliveryAttempted => "Delivery Attempted",
            ShipmentEventType.DeliveryFailed => "Delivery Failed",
            ShipmentEventType.ReturnInitiated => "Return Initiated",
            ShipmentEventType.ReturnedToSender => "Returned to Sender",
            ShipmentEventType.CustomsClearance => "Customs Clearance",
            ShipmentEventType.HoldAtLocation => "Hold at Location",
            _ => t.ToString()
        };
    }
}
