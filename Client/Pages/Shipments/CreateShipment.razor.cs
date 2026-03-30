#nullable enable
using LogiCore.Client.Services.Customers;
using LogiCore.Client.Services.Shipments;
using LogiCore.Server.Models.Customers;
using LogiCore.Server.Models.Shipments;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogiCore.Client.Pages.Shipments
{
    public partial class CreateShipment
    {
        [Parameter] public int? ShipmentId { get; set; }

        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected IShipmentService ShipmentService { get; set; } = default!;

        // Inject customer service
        [Inject] protected ICustomerService CustomerService { get; set; } = default!;

        // Customer picker state
        private string customerSearch = string.Empty;
        private List<CustomerListDto> customerSuggestions = new();
        private CustomerDetailDto? selectedCustomer;

        private bool IsEditMode => ShipmentId.HasValue;
        private CreateShipmentDto model = new();
        private bool isLoading;
        private bool isSaving;
        private decimal estimatedCost;
        private DateTime estimatedDelivery = DateTime.Today;
        private string editingTrackingNumber = string.Empty;

        private List<(string Label, ServiceType Value)> serviceTypes = new()
        {
            ("Same Day", ServiceType.SameDay),
            ("Overnight", ServiceType.Overnight),
            ("2-Day", ServiceType.TwoDay),
            ("Ground", ServiceType.Ground),
            ("Freight", ServiceType.Freight),
            ("International", ServiceType.International),
        };

        // Add query param
        [SupplyParameterFromQuery(Name = "customerId")]
        public int? PreselectedCustomerId { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            // Preselect customer if navigated from CustomerDetail
            if (!IsEditMode && PreselectedCustomerId.HasValue && selectedCustomer == null)
            {
                selectedCustomer = await CustomerService.GetCustomerByIdAsync(PreselectedCustomerId.Value);
                if (selectedCustomer != null)
                {
                    model.CustomerId = selectedCustomer.CustomerId;
                    customerSearch = selectedCustomer.FullName;
                }
            }

            if (IsEditMode)
            {
                isLoading = true;
                var existing = await ShipmentService.GetShipmentByIdAsync(ShipmentId!.Value);
                if (existing != null)
                {
                    editingTrackingNumber = existing.TrackingNumber;
                    model = new CreateShipmentDto
                    {
                        SenderName = existing.SenderName,
                        SenderEmail = existing.SenderEmail,
                        SenderPhone = existing.SenderPhone,
                        RecipientName = existing.RecipientName,
                        RecipientEmail = existing.RecipientEmail,
                        RecipientPhone = existing.RecipientPhone,
                        OriginAddress = existing.OriginAddress,
                        OriginCity = existing.OriginCity,
                        OriginState = existing.OriginState,
                        OriginPostalCode = existing.OriginPostalCode,
                        OriginCountry = existing.OriginCountry,
                        DestinationAddress = existing.DestinationAddress,
                        DestinationCity = existing.DestinationCity,
                        DestinationState = existing.DestinationState,
                        DestinationPostalCode = existing.DestinationPostalCode,
                        DestinationCountry = existing.DestinationCountry,
                        ServiceType = existing.ServiceType,
                        TotalWeight = existing.TotalWeight,
                        DeclaredValue = existing.DeclaredValue,
                        PackageCount = existing.PackageCount,
                        Length = existing.Length,
                        Width = existing.Width,
                        Height = existing.Height,
                        PickupDate = existing.PickupDate,
                        Notes = existing.Notes
                    };
                    RecalculateCostInternal();
                }
                isLoading = false;
            }
            else
            {
                RecalculateCostInternal();
            }
        }

        private Task RecalculateCost(object value)
        {
            RecalculateCostInternal();
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task RecalculateCostNumeric(decimal value)
        {
            RecalculateCostInternal();
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void RecalculateCostInternal()
        {
            if (model.TotalWeight > 0)
            {
                estimatedCost = ShipmentService.CalculateShippingCost(
                    model.ServiceType, model.TotalWeight, model.Length, model.Width, model.Height);
                estimatedDelivery = CalculateEstimatedDelivery(model.ServiceType, model.PickupDate);
            }
        }

        private static DateTime CalculateEstimatedDelivery(ServiceType svc, DateTime pickup) => svc switch
        {
            ServiceType.SameDay => pickup,
            ServiceType.Overnight => pickup.AddDays(1),
            ServiceType.TwoDay => pickup.AddDays(2),
            ServiceType.Ground => pickup.AddDays(5),
            ServiceType.Freight => pickup.AddDays(7),
            ServiceType.International => pickup.AddDays(14),
            _ => pickup.AddDays(5)
        };

        private async Task OnSubmit(CreateShipmentDto dto)
        {
            isSaving = true;
            try
            {
                if (IsEditMode)
                {
                    var updated = await ShipmentService.UpdateShipmentAsync(ShipmentId!.Value, dto);
                    NotificationService.Notify(NotificationSeverity.Success, "Saved", "Shipment updated successfully.");
                    NavigationManager.NavigateTo($"/shipments/{updated.ShipmentId}");
                }
                else
                {
                    var created = await ShipmentService.CreateShipmentAsync(dto);
                    NotificationService.Notify(NotificationSeverity.Success, "Created",
                        $"Shipment {created.TrackingNumber} created successfully.");
                    NavigationManager.NavigateTo($"/shipments/{created.ShipmentId}");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally
            {
                isSaving = false;
            }
        }

        private void OnInvalidSubmit()
        {
            NotificationService.Notify(NotificationSeverity.Warning, "Validation",
                "Please fill in all required fields.");
        }

        private void GoBack()
        {
            if (IsEditMode)
                NavigationManager.NavigateTo($"/shipments/{ShipmentId}");
            else
                NavigationManager.NavigateTo("/shipments");
        }


        private async Task LoadCustomerSuggestions(LoadDataArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.Filter) || args.Filter.Length < 2) return;
            customerSuggestions = await CustomerService.GetCustomersAsync(
                new CustomerFilterDto { SearchTerm = args.Filter });
            StateHasChanged();
        }

        private async Task OnCustomerSearchChange(object value)
        {
            if (value is CustomerListDto dto)
            {
                selectedCustomer = await CustomerService.GetCustomerByIdAsync(dto.CustomerId);
                model.CustomerId = selectedCustomer?.CustomerId;
            }
        }

        private void ClearCustomer()
        {
            selectedCustomer = null;
            model.CustomerId = null;
            customerSearch = string.Empty;
        }

        private void FillFromCustomer()
        {
            if (selectedCustomer == null) return;
            model.SenderName = selectedCustomer.FullName;
            model.SenderEmail = selectedCustomer.Email;
            model.SenderPhone = selectedCustomer.Phone;
            model.OriginAddress = selectedCustomer.AddressLine1;
            model.OriginCity = selectedCustomer.City;
            model.OriginState = selectedCustomer.State;
            model.OriginPostalCode = selectedCustomer.PostalCode;
            model.OriginCountry = selectedCustomer.Country;
        }
    }
}
