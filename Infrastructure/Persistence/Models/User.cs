using System;
using System.Collections.Generic;

namespace ServiceCenterAPI.Infrastructure.Persistence.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? CreatedByUserId { get; set; }

    public int? UpdatedByUserId { get; set; }

    public virtual ICollection<Customer> CustomerCreatedByUsers { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerUpdatedByUsers { get; set; } = new List<Customer>();

    public virtual ICollection<Employee> EmployeeCreatedByUsers { get; set; } = new List<Employee>();

    public virtual ICollection<Employee> EmployeeUpdatedByUsers { get; set; } = new List<Employee>();

    public virtual ICollection<Employee> EmployeeUsers { get; set; } = new List<Employee>();

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual ICollection<Part> PartCreatedByUsers { get; set; } = new List<Part>();

    public virtual ICollection<Part> PartUpdatedByUsers { get; set; } = new List<Part>();

    public virtual ICollection<SecurityLog> SecurityLogs { get; set; } = new List<SecurityLog>();

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual ICollection<Supplier> SupplierCreatedByUsers { get; set; } = new List<Supplier>();

    public virtual ICollection<Supplier> SupplierUpdatedByUsers { get; set; } = new List<Supplier>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
