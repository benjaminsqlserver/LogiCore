using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Server.Models.Customers;

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
    }
}
