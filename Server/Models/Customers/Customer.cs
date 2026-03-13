#nullable enable
using System;
using System.Collections.Generic;

namespace LogiCore.Server.Models.Customers
{
    public enum AccountType
    {
        Individual,
        Business,
        Enterprise
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public class Customer
    {
        public int CustomerId { get; set; }

        // Identity
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? TaxId { get; set; }

        // Account
        public AccountType AccountType { get; set; } = AccountType.Individual;
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }

        // Primary Address (flat — mirrors Shipment pattern; normalised FK address added later when Auth lands)
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "US";

        // Notes
        public string? Notes { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
    }
}
