#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using LogiCore.Server.Models.Shipments;
using LogiCore.Client.Services.Shipments;

namespace LogiCore.Client.Pages.Shipments
{
    public partial class ShipmentDetail
    {
        [Parameter] public int ShipmentId { get; set; }

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected IShipmentService ShipmentService { get; set; } = default!;

        private ShipmentDetailDto? shipment;
        private bool isLoading = true;

        // Progress steps (simplified path for display)
        private static readonly List<(ShipmentStatus Status, string Label)> statusSteps = new()
        {
            (ShipmentStatus.Pending, "Pending"),
            (ShipmentStatus.PickedUp, "Picked Up"),
            (ShipmentStatus.InTransit, "In Transit"),
            (ShipmentStatus.AtHub, "At Hub"),
            (ShipmentStatus.OutForDelivery, "Out for Delivery"),
            (ShipmentStatus.Delivered, "Delivered"),
        };

        protected override async Task OnParametersSetAsync()
        {
            isLoading = true;
            shipment = await ShipmentService.GetShipmentByIdAsync(ShipmentId);
            isLoading = false;
        }

        private void GoBack() => NavigationManager.NavigateTo("/shipments");

        private async Task OpenAddEventDialog()
        {
            var dto = new AddShipmentEventDto();
            var result = await DialogService.OpenAsync<AddEventDialog>("Add Tracking Event",
                new Dictionary<string, object> { ["ShipmentId"] = ShipmentId, ["EventDto"] = dto },
                new DialogOptions { Width = "480px", CloseDialogOnOverlayClick = false });

            if (result == true)
            {
                shipment = await ShipmentService.GetShipmentByIdAsync(ShipmentId);
                NotificationService.Notify(NotificationSeverity.Success, "Event Added", "Tracking event recorded successfully.");
            }
        }

        // ---- Badge/style helpers ----

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

        private static BadgeStyle GetServiceBadge(ServiceType s) => s switch
        {
            ServiceType.SameDay => BadgeStyle.Danger,
            ServiceType.Overnight => BadgeStyle.Warning,
            ServiceType.TwoDay => BadgeStyle.Info,
            ServiceType.Ground => BadgeStyle.Secondary,
            ServiceType.Freight => BadgeStyle.Light,
            ServiceType.International => BadgeStyle.Primary,
            _ => BadgeStyle.Secondary
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
    }
}
