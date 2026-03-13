using Microsoft.AspNetCore.Mvc;
using LogiCore.Server.Models.Customers;
using LogiCore.Server.Services.Customers;

namespace LogiCore.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _svc;

        public CustomersController(ICustomerService svc) => _svc = svc;

        // GET api/customers?SearchTerm=...&Status=...&AccountType=...
        [HttpGet]
        public async Task<ActionResult<List<CustomerListDto>>> GetAll([FromQuery] CustomerFilterDto filter)
            => Ok(await _svc.GetCustomersAsync(filter));

        // GET api/customers/count?...
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount([FromQuery] CustomerFilterDto filter)
            => Ok(await _svc.GetCustomerCountAsync(filter));

        // GET api/customers/stats
        [HttpGet("stats")]
        public async Task<ActionResult<CustomerStatsDto>> GetStats()
            => Ok(await _svc.GetStatsAsync());

        // GET api/customers/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerDetailDto>> GetById(int id)
        {
            var result = await _svc.GetCustomerByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        // POST api/customers
        [HttpPost]
        public async Task<ActionResult<CustomerDetailDto>> Create([FromBody] CreateCustomerDto dto)
        {
            // Check for duplicate email
            if (await _svc.EmailExistsAsync(dto.Email))
                return Conflict(new { message = $"A customer with email '{dto.Email}' already exists." });

            var result = await _svc.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.CustomerId }, result);
        }

        // PUT api/customers/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CustomerDetailDto>> Update(int id, [FromBody] CreateCustomerDto dto)
        {
            // Check for email collision with OTHER customers
            if (await _svc.EmailExistsAsync(dto.Email, excludeCustomerId: id))
                return Conflict(new { message = $"Email '{dto.Email}' is already used by another customer." });

            try { return Ok(await _svc.UpdateCustomerAsync(id, dto)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        // PATCH api/customers/{id}/status
        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] CustomerStatus status)
        {
            var ok = await _svc.UpdateStatusAsync(id, status);
            return ok ? NoContent() : NotFound();
        }

        // DELETE api/customers/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteCustomerAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
