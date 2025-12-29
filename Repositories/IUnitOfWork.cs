namespace ServiceCenterAPI.Repositories
{
    using global::ServiceCenterAPI.Infrastructure.Persistence.Models;
    using ServiceCenterAPI.Infrastructure.Persistence.Models;
    using System;
    using System.Threading.Tasks;

 

    /// <summary>
    /// Unit of Work Interface
    /// Coordinates all repositories and manages database transactions
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Main Entities
        IRepository<Customer> Customers { get; }
        IRepository<Employee> Employees { get; }
        IRepository<ServiceOrder> ServiceOrders { get; }
        IRepository<Job> Jobs { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<Part> Parts { get; }
        IRepository<Supplier> Suppliers { get; }
        IRepository<Payment> Payments { get; }
        IRepository<Complaint> Complaints { get; }
        IRepository<Warranty> Warranties { get; }
        IRepository<Inspection> Inspections { get; }
        IRepository<StockTransaction> StockTransactions { get; }
        IRepository<ReturnAuthorization> ReturnAuthorizations { get; }

        // User & Authentication
        IRepository<User> Users { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Role> Roles { get; }

        // Reference Data
        IRepository<Status> Statuses { get; }
        IRepository<ServiceType> ServiceTypes { get; }
        IRepository<JobStatus> JobStatuses { get; }

        /// <summary>
        /// Save all changes to database in a transaction
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin a new database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
