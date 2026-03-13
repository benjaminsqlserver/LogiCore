#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace LogiCore.Server.Models.Customers
{
    // -----------------------------------------------------------------------
    //  List / search result
    // -----------------------------------------------------------------------
    public class CustomerListDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public AccountType AccountType { get; set; }
        public CustomerStatus Status { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int ShipmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // -----------------------------------------------------------------------
    //  Full detail (used on detail / edit page)
    // -----------------------------------------------------------------------
    public class CustomerDetailDto
    {
        public int CustomerId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }

        public AccountType AccountType { get; set; }
        public CustomerStatus Status { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "US";

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // -----------------------------------------------------------------------
    //  Create / Update form model
    // -----------------------------------------------------------------------
    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(50)]
        public string? TaxId { get; set; }

        public AccountType AccountType { get; set; } = AccountType.Individual;
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        [Range(0, 1_000_000, ErrorMessage = "Credit limit must be 0 – 1,000,000")]
        public decimal CreditLimit { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(250)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(50)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(3)]
        public string Country { get; set; } = "US";

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    // -----------------------------------------------------------------------
    //  Filter / search parameters
    // -----------------------------------------------------------------------
    public class CustomerFilterDto
    {
        public string? SearchTerm { get; set; }
        public AccountType? AccountType { get; set; }
        public CustomerStatus? Status { get; set; }
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 25;
    }

    // -----------------------------------------------------------------------
    //  Dashboard stats
    // -----------------------------------------------------------------------
    public class CustomerStatsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int BusinessAccounts { get; set; }
        public int EnterpriseAccounts { get; set; }
        public int NewThisMonth { get; set; }
        public int SuspendedAccounts { get; set; }
    }
}
