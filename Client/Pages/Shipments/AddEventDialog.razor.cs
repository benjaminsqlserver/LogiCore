using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using LogiCore.Server.Models.Shipments;
using LogiCore.Client.Services.Shipments;

namespace LogiCore.Client.Pages.Shipments
{
    public partial class AddEventDialog
    {
        [Parameter] public int ShipmentId { get; set; }
        [Parameter] public AddShipmentEventDto EventDto { get; set; } = new();

        [Inject] protected IShipmentService ShipmentService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        private bool isSaving;

        private List<(string Label, ShipmentEventType Value)> eventTypes = Enum.GetValues<ShipmentEventType>()
            .Select(e => (Label: FormatEventType(e), Value: e))
            .ToList();

        private async Task OnSubmit(AddShipmentEventDto dto)
        {
            isSaving = true;
            try
            {
                await ShipmentService.AddEventAsync(ShipmentId, dto);
                DialogService.Close(true);
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

        private void Cancel() => DialogService.Close(false);

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
