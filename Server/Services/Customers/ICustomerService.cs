using LogiCore.Server.Models.Customers;
using LogiCore.Server.Models.Shipments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogiCore.Server.Services.Customers
{
    public interface ICustomerService
    {
        Task<List<CustomerListDto>> GetCustomersAsync(CustomerFilterDto filter);
        Task<int> GetCustomerCountAsync(CustomerFilterDto filter);
        Task<CustomerDetailDto?> GetCustomerByIdAsync(int customerId);
        Task<CustomerDetailDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDetailDto> UpdateCustomerAsync(int customerId, CreateCustomerDto dto);
        Task<bool> UpdateStatusAsync(int customerId, CustomerStatus status);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<CustomerStatsDto> GetStatsAsync();
        Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null);

        // ICustomerService.cs — add:
        Task<List<ShipmentListDto>> GetCustomerShipmentsAsync(int customerId);
    }
}
