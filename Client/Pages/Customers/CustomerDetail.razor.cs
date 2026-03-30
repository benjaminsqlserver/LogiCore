#nullable enable
using LogiCore.Client.Services.Customers;
using LogiCore.Server.Models.Customers;
using LogiCore.Server.Models.Shipments;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Threading.Tasks;

namespace LogiCore.Client.Pages.Customers
{
    public partial class CustomerDetail
    {
        [Parameter] public int CustomerId { get; set; }

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected ICustomerService CustomerService { get; set; } = default!;

        private CustomerDetailDto? customer;
        private bool isLoading = true;

        // State
        private List<ShipmentListDto>? shipments;

        // Load alongside customer in OnInitializedAsync:
        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            try
            {
                var tasks = await Task.WhenAll(
                    CustomerService.GetCustomerByIdAsync(CustomerId).ContinueWith(t => { customer = t.Result; return 0; }),
                    CustomerService.GetCustomerShipmentsAsync(CustomerId).ContinueWith(t => { shipments = t.Result; return 0; })
                );
            }
            finally { isLoading = false; }
        }

        private async Task ConfirmDelete()
        {
            var confirmed = await DialogService.Confirm(
                $"Are you sure you want to delete customer '{customer?.FullName}'? This cannot be undone.",
                "Delete Customer",
                new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

            if (confirmed != true) return;

            var ok = await CustomerService.DeleteCustomerAsync(CustomerId);
            if (ok)
            {
                NotificationService.Notify(NotificationSeverity.Info, "Deleted",
                    $"Customer '{customer?.FullName}' has been removed.");
                NavigationManager.NavigateTo("/customers");
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error",
                    "Could not delete this customer. Please try again.");
            }
        }

        // ---- Badge helpers ----
        private static BadgeStyle GetStatusBadge(CustomerStatus s) => s switch
        {
            CustomerStatus.Active => BadgeStyle.Success,
            CustomerStatus.Inactive => BadgeStyle.Secondary,
            CustomerStatus.Suspended => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };

        private static BadgeStyle GetAccountTypeBadge(AccountType a) => a switch
        {
            AccountType.Individual => BadgeStyle.Info,
            AccountType.Business => BadgeStyle.Primary,
            AccountType.Enterprise => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };

        private static BadgeStyle GetStatusBadge(ShipmentStatus s) => s switch
        {
            ShipmentStatus.Pending => BadgeStyle.Warning,
            ShipmentStatus.PickedUp => BadgeStyle.Info,
            ShipmentStatus.InTransit => BadgeStyle.Primary,
            ShipmentStatus.AtHub => BadgeStyle.Primary,
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
    }
}
