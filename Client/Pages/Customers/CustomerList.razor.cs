#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using LogiCore.Server.Models.Customers;
using LogiCore.Client.Services.Customers;
using LogiCore.Client.Shared;

namespace LogiCore.Client.Pages.Customers
{
    public partial class CustomerList
    {
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected ICustomerService CustomerService { get; set; } = default!;

        private List<CustomerListDto> customers = new();
        private CustomerStatsDto? stats;
        private bool isLoading = true;
        private CustomerFilterDto filter = new();

        // Dropdown data sources — using DropDownItem<T> so Radzen can resolve
        // TextProperty/ValueProperty via reflection (tuples are not reflection-accessible)
        private readonly List<DropDownItem<AccountType?>> accountTypeOptions =
            Enum.GetValues<AccountType>()
                .Select(a => new DropDownItem<AccountType?> { Label = a.ToString(), Value = a })
                .ToList();

        private readonly List<DropDownItem<CustomerStatus?>> statusOptions =
            Enum.GetValues<CustomerStatus>()
                .Select(s => new DropDownItem<CustomerStatus?> { Label = s.ToString(), Value = s })
                .ToList();

        protected override async Task OnInitializedAsync()
        {
            await Task.WhenAll(LoadCustomers(), LoadStats());
        }

        private async Task LoadCustomers()
        {
            isLoading = true;
            try { customers = await CustomerService.GetCustomersAsync(filter); }
            finally { isLoading = false; }
        }

        private async Task LoadStats()
            => stats = await CustomerService.GetStatsAsync();

        private async Task ClearFilters()
        {
            filter = new CustomerFilterDto();
            await LoadCustomers();
        }

        private void OnRowClick(DataGridRowMouseEventArgs<CustomerListDto> args)
            => NavigationManager.NavigateTo($"/customers/{args.Data.CustomerId}");

        private void ViewCustomer(int id) => NavigationManager.NavigateTo($"/customers/{id}");
        private void EditCustomer(int id) => NavigationManager.NavigateTo($"/customers/{id}/edit");
        private void CreateNew() => NavigationManager.NavigateTo("/customers/create");

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
