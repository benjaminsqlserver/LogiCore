using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogiCore.Server.Data;
using LogiCore.Server.Models.Customers;

namespace LogiCore.Server.Services.Customers
{
    /// <summary>
    /// EF Core / SQL Server implementation of ICustomerService.
    /// Registered as Scoped — receives AppDbContext via DI.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;

        public CustomerService(AppDbContext db) => _db = db;

        // ----------------------------------------------------------------
        //  Query
        // ----------------------------------------------------------------

        public async Task<List<CustomerListDto>> GetCustomersAsync(CustomerFilterDto filter)
        {
            var q = _db.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                q = q.Where(c =>
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                    c.City.ToLower().Contains(term) ||
                    c.Phone.ToLower().Contains(term));
            }

            if (filter.AccountType.HasValue)
                q = q.Where(c => c.AccountType == filter.AccountType.Value);

            if (filter.Status.HasValue)
                q = q.Where(c => c.Status == filter.Status.Value);

            return await q
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CustomerListDto
                {
                    CustomerId = c.CustomerId,
                    FullName = c.FullName,
                    Email = c.Email,
                    Phone = c.Phone,
                    CompanyName = c.CompanyName,
                    AccountType = c.AccountType,
                    Status = c.Status,
                    CreditLimit = c.CreditLimit,
                    CurrentBalance = c.CurrentBalance,
                    City = c.City,
                    State = c.State,
                    Country = c.Country,
                    // ShipmentCount will be wired up once Customers FK is added to Shipments
                    ShipmentCount = 0,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetCustomerCountAsync(CustomerFilterDto filter)
        {
            var q = _db.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                q = q.Where(c =>
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)));
            }

            if (filter.AccountType.HasValue) q = q.Where(c => c.AccountType == filter.AccountType.Value);
            if (filter.Status.HasValue) q = q.Where(c => c.Status == filter.Status.Value);

            return await q.CountAsync();
        }

        public async Task<CustomerDetailDto?> GetCustomerByIdAsync(int customerId)
        {
            var c = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CustomerId == customerId);

            return c is null ? null : MapToDetail(c);
        }

        // ----------------------------------------------------------------
        //  Mutations
        // ----------------------------------------------------------------

        public async Task<CustomerDetailDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Phone = dto.Phone?.Trim() ?? string.Empty,
                CompanyName = dto.CompanyName?.Trim(),
                TaxId = dto.TaxId?.Trim(),
                AccountType = dto.AccountType,
                Status = dto.Status,
                CreditLimit = dto.CreditLimit,
                CurrentBalance = 0,
                AddressLine1 = dto.AddressLine1.Trim(),
                AddressLine2 = dto.AddressLine2?.Trim(),
                City = dto.City.Trim(),
                State = dto.State?.Trim() ?? string.Empty,
                PostalCode = dto.PostalCode.Trim(),
                Country = dto.Country?.Trim() ?? "US",
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return MapToDetail(customer);
        }

        public async Task<CustomerDetailDto> UpdateCustomerAsync(int customerId, CreateCustomerDto dto)
        {
            var customer = await _db.Customers.FindAsync(customerId)
                ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

            customer.FullName = dto.FullName.Trim();
            customer.Email = dto.Email.Trim().ToLower();
            customer.Phone = dto.Phone?.Trim() ?? string.Empty;
            customer.CompanyName = dto.CompanyName?.Trim();
            customer.TaxId = dto.TaxId?.Trim();
            customer.AccountType = dto.AccountType;
            customer.Status = dto.Status;
            customer.CreditLimit = dto.CreditLimit;
            customer.AddressLine1 = dto.AddressLine1.Trim();
            customer.AddressLine2 = dto.AddressLine2?.Trim();
            customer.City = dto.City.Trim();
            customer.State = dto.State?.Trim() ?? string.Empty;
            customer.PostalCode = dto.PostalCode.Trim();
            customer.Country = dto.Country?.Trim() ?? "US";
            customer.Notes = dto.Notes?.Trim();

            await _db.SaveChangesAsync();
            return MapToDetail(customer);
        }

        public async Task<bool> UpdateStatusAsync(int customerId, CustomerStatus status)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer is null) return false;

            customer.Status = status;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer is null) return false;

            customer.IsDeleted = true;           // soft delete
            await _db.SaveChangesAsync();
            return true;
        }

        // ----------------------------------------------------------------
        //  Stats
        // ----------------------------------------------------------------

        public async Task<CustomerStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var month = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var customers = await _db.Customers
                .AsNoTracking()
                .Select(c => new { c.AccountType, c.Status, c.CreatedAt })
                .ToListAsync();

            return new CustomerStatsDto
            {
                TotalCustomers = customers.Count,
                ActiveCustomers = customers.Count(c => c.Status == CustomerStatus.Active),
                BusinessAccounts = customers.Count(c => c.AccountType == AccountType.Business),
                EnterpriseAccounts = customers.Count(c => c.AccountType == AccountType.Enterprise),
                NewThisMonth = customers.Count(c => c.CreatedAt >= month),
                SuspendedAccounts = customers.Count(c => c.Status == CustomerStatus.Suspended)
            };
        }

        // ----------------------------------------------------------------
        //  Helpers
        // ----------------------------------------------------------------

        public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null)
        {
            var normalized = email.Trim().ToLower();
            var q = _db.Customers.AsNoTracking().Where(c => c.Email == normalized);
            if (excludeCustomerId.HasValue)
                q = q.Where(c => c.CustomerId != excludeCustomerId.Value);
            return await q.AnyAsync();
        }

        private static CustomerDetailDto MapToDetail(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            FullName = c.FullName,
            Email = c.Email,
            Phone = c.Phone,
            CompanyName = c.CompanyName,
            TaxId = c.TaxId,
            AccountType = c.AccountType,
            Status = c.Status,
            CreditLimit = c.CreditLimit,
            CurrentBalance = c.CurrentBalance,
            AddressLine1 = c.AddressLine1,
            AddressLine2 = c.AddressLine2,
            City = c.City,
            State = c.State,
            PostalCode = c.PostalCode,
            Country = c.Country,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
