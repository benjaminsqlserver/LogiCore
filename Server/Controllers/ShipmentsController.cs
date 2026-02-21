using Microsoft.AspNetCore.Mvc;
using LogiCore.Server.Models.Shipments;
using LogiCore.Server.Services.Shipments;

namespace LogiCore.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _svc;

        public ShipmentsController(IShipmentService svc) => _svc = svc;

        // GET api/shipments?SearchTerm=...&Status=...
        [HttpGet]
        public async Task<ActionResult<List<ShipmentListDto>>> GetAll([FromQuery] ShipmentFilterDto filter)
            => Ok(await _svc.GetShipmentsAsync(filter));

        // GET api/shipments/count?...
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount([FromQuery] ShipmentFilterDto filter)
            => Ok(await _svc.GetShipmentCountAsync(filter));

        // GET api/shipments/stats
        [HttpGet("stats")]
        public async Task<ActionResult<ShipmentStatsDto>> GetStats()
            => Ok(await _svc.GetStatsAsync());

        // GET api/shipments/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShipmentDetailDto>> GetById(int id)
        {
            var result = await _svc.GetShipmentByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        // GET api/shipments/track/{trackingNumber}
        [HttpGet("track/{trackingNumber}")]
        public async Task<ActionResult<ShipmentDetailDto>> GetByTracking(string trackingNumber)
        {
            var result = await _svc.GetShipmentByTrackingAsync(trackingNumber);
            return result is null ? NotFound() : Ok(result);
        }

        // POST api/shipments
        [HttpPost]
        public async Task<ActionResult<ShipmentDetailDto>> Create([FromBody] CreateShipmentDto dto)
        {
            var result = await _svc.CreateShipmentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ShipmentId }, result);
        }

        // PUT api/shipments/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ShipmentDetailDto>> Update(int id, [FromBody] CreateShipmentDto dto)
        {
            try { return Ok(await _svc.UpdateShipmentAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // PATCH api/shipments/{id}/status
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] ShipmentStatus status)
        {
            var ok = await _svc.UpdateStatusAsync(id, status);
            return ok ? NoContent() : NotFound();
        }

        // POST api/shipments/{id}/events
        [HttpPost("{id:int}/events")]
        public async Task<ActionResult<ShipmentEventDto>> AddEvent(int id, [FromBody] AddShipmentEventDto dto)
        {
            try { return Ok(await _svc.AddEventAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // DELETE api/shipments/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteShipmentAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // GET api/shipments/calculate-cost?serviceType=...&weight=...
        [HttpGet("calculate-cost")]
        public ActionResult<decimal> CalculateCost(
            [FromQuery] ServiceType serviceType,
            [FromQuery] decimal weight,
            [FromQuery] decimal length,
            [FromQuery] decimal width,
            [FromQuery] decimal height)
            => Ok(_svc.CalculateShippingCost(serviceType, weight, length, width, height));
    }
}
