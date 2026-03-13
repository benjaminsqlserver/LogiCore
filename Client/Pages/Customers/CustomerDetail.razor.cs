#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using LogiCore.Server.Models.Customers;
using LogiCore.Client.Services.Customers;

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

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            try { customer = await CustomerService.GetCustomerByIdAsync(CustomerId); }
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
    }
}
