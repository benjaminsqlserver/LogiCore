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
    public partial class ShipmentList
    {
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected IShipmentService ShipmentService { get; set; } = default!;

        private List<ShipmentListDto> shipments = new();
        private ShipmentStatsDto? stats;
        private bool isLoading = true;
        private ShipmentFilterDto filter = new();

        // Dropdown options
        private List<(string Label, ShipmentStatus? Value)> statusOptions = Enum.GetValues<ShipmentStatus>()
            .Select(s => (Label: GetStatusLabel(s), Value: (ShipmentStatus?)s)).ToList();

        private List<(string Label, ServiceType? Value)> serviceOptions = Enum.GetValues<ServiceType>()
            .Select(s => (Label: s.ToString(), Value: (ServiceType?)s)).ToList();

        protected override async Task OnInitializedAsync()
        {
            await LoadShipments();
            stats = await ShipmentService.GetStatsAsync();
        }

        private async Task LoadShipments()
        {
            isLoading = true;
            try
            {
                shipments = await ShipmentService.GetShipmentsAsync(filter);
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task ClearFilters()
        {
            filter = new ShipmentFilterDto();
            await LoadShipments();
        }

        private void OnRowClick(DataGridRowMouseEventArgs<ShipmentListDto> args)
            => NavigationManager.NavigateTo($"/shipments/{args.Data.ShipmentId}");

        private void ViewShipment(int id) => NavigationManager.NavigateTo($"/shipments/{id}");
        private void EditShipment(int id) => NavigationManager.NavigateTo($"/shipments/{id}/edit");
        private void CreateNew() => NavigationManager.NavigateTo("/shipments/create");

        // ---- Badge helpers ----
        private static BadgeStyle GetStatusBadge(ShipmentStatus s) => s switch
        {
            ShipmentStatus.Pending => BadgeStyle.Warning,
            ShipmentStatus.PickupScheduled => BadgeStyle.Info,
            ShipmentStatus.PickedUp => BadgeStyle.Info,
            ShipmentStatus.InTransit => BadgeStyle.Primary,
            ShipmentStatus.AtHub => BadgeStyle.Primary,
            ShipmentStatus.OutForDelivery => BadgeStyle.Info,
            ShipmentStatus.Delivered => BadgeStyle.Success,
            ShipmentStatus.Failed => BadgeStyle.Danger,
            ShipmentStatus.ReturnInitiated => BadgeStyle.Warning,
            ShipmentStatus.Returned => BadgeStyle.Secondary,
            ShipmentStatus.Cancelled => BadgeStyle.Light,
            _ => BadgeStyle.Light
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
    }
}
