#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using LogiCore.Server.Models.Customers;
using LogiCore.Client.Services.Customers;
using LogiCore.Client.Shared;

namespace LogiCore.Client.Pages.Customers
{
    public partial class CreateCustomer
    {
        [Parameter] public int? CustomerId { get; set; }

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected ICustomerService CustomerService { get; set; } = default!;

        private CreateCustomerDto model = new();
        private bool isLoading;
        private bool isSaving;
        private bool IsEditMode => CustomerId.HasValue;
        private string editingName = string.Empty;

        // Dropdown sources — DropDownItem<T> exposes real named properties so
        // Radzen can resolve TextProperty="Label" / ValueProperty="Value" via reflection.
        // C# tuple fields are NOT accessible via reflection, which is why tuples break here.
        private readonly List<DropDownItem<AccountType>> accountTypeOptions =
            Enum.GetValues<AccountType>()
                .Select(a => new DropDownItem<AccountType> { Label = a.ToString(), Value = a })
                .ToList();

        private readonly List<DropDownItem<CustomerStatus>> statusOptions =
            Enum.GetValues<CustomerStatus>()
                .Select(s => new DropDownItem<CustomerStatus> { Label = s.ToString(), Value = s })
                .ToList();

        protected override async Task OnInitializedAsync()
        {
            if (!IsEditMode) return;

            isLoading = true;
            try
            {
                var detail = await CustomerService.GetCustomerByIdAsync(CustomerId!.Value);
                if (detail is null)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Not found",
                        "Customer record could not be loaded.");
                    NavigationManager.NavigateTo("/customers");
                    return;
                }

                editingName = detail.FullName;
                model = new CreateCustomerDto
                {
                    FullName = detail.FullName,
                    Email = detail.Email,
                    Phone = detail.Phone,
                    CompanyName = detail.CompanyName,
                    TaxId = detail.TaxId,
                    AccountType = detail.AccountType,
                    Status = detail.Status,
                    CreditLimit = detail.CreditLimit,
                    AddressLine1 = detail.AddressLine1,
                    AddressLine2 = detail.AddressLine2,
                    City = detail.City,
                    State = detail.State,
                    PostalCode = detail.PostalCode,
                    Country = detail.Country,
                    Notes = detail.Notes
                };
            }
            finally { isLoading = false; }
        }

        private async Task OnSubmit()
        {
            isSaving = true;
            try
            {
                CustomerDetailDto result;
                if (IsEditMode)
                    result = await CustomerService.UpdateCustomerAsync(CustomerId!.Value, model);
                else
                    result = await CustomerService.CreateCustomerAsync(model);

                NotificationService.Notify(NotificationSeverity.Success,
                    IsEditMode ? "Saved" : "Created",
                    $"Customer '{result.FullName}' {(IsEditMode ? "updated" : "created")} successfully.");

                NavigationManager.NavigateTo($"/customers/{result.CustomerId}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                NotificationService.Notify(NotificationSeverity.Error,
                    "Duplicate Email",
                    "A customer with this email address already exists.");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally { isSaving = false; }
        }

        private void OnInvalidSubmit() =>
            NotificationService.Notify(NotificationSeverity.Warning,
                "Validation", "Please fix the highlighted fields and try again.");

        private void GoBack()
        {
            if (IsEditMode)
                NavigationManager.NavigateTo($"/customers/{CustomerId}");
            else
                NavigationManager.NavigateTo("/customers");
        }
    }
}
