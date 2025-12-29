using Microsoft.EntityFrameworkCore.Storage;
using ServiceCenterAPI.Infrastructure.Persistence.DbContextActions;
using ServiceCenterAPI.Infrastructure.Persistence.Models;
using ServiceCenterAPI.Infrastructure.Repositories;

namespace ServiceCenterAPI.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ServiceCenterDbContext _context;
        private IDbContextTransaction? _transaction;

        // Main Entities
        private IRepository<Customer>? _customers;
        private IRepository<Employee>? _employees;
        private IRepository<ServiceOrder>? _serviceOrders;
        private IRepository<Job>? _jobs;
        private IRepository<Invoice>? _invoices;
        private IRepository<Part>? _parts;
        private IRepository<Supplier>? _suppliers;
        private IRepository<Payment>? _payments;
        private IRepository<Complaint>? _complaints;
        private IRepository<Warranty>? _warranties;
        private IRepository<Inspection>? _inspections;
        private IRepository<StockTransaction>? _stockTransactions;
        private IRepository<ReturnAuthorization>? _returnAuthorizations;

        // User & Authentication
        private IRepository<User>? _users;
        private IRepository<UserRole>? _userRoles;
        private IRepository<Role>? _roles;

        // Reference Data
        private IRepository<Status>? _statuses;
        private IRepository<ServiceType>? _serviceTypes;
        private IRepository<JobStatus>? _jobStatuses;

        public UnitOfWork(ServiceCenterDbContext context)
        {
            _context = context;
        }

        // Main Entities Properties
        public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);
        public IRepository<Employee> Employees => _employees ??= new Repository<Employee>(_context);
        public IRepository<ServiceOrder> ServiceOrders => _serviceOrders ??= new Repository<ServiceOrder>(_context);
        public IRepository<Job> Jobs => _jobs ??= new Repository<Job>(_context);
        public IRepository<Invoice> Invoices => _invoices ??= new Repository<Invoice>(_context);
        public IRepository<Part> Parts => _parts ??= new Repository<Part>(_context);
        public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
        public IRepository<Payment> Payments => _payments ??= new Repository<Payment>(_context);
        public IRepository<Complaint> Complaints => _complaints ??= new Repository<Complaint>(_context);
        public IRepository<Warranty> Warranties => _warranties ??= new Repository<Warranty>(_context);
        public IRepository<Inspection> Inspections => _inspections ??= new Repository<Inspection>(_context);
        public IRepository<StockTransaction> StockTransactions => _stockTransactions ??= new Repository<StockTransaction>(_context);
        public IRepository<ReturnAuthorization> ReturnAuthorizations => _returnAuthorizations ??= new Repository<ReturnAuthorization>(_context);

        // User & Authentication Properties
        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<UserRole> UserRoles => _userRoles ??= new Repository<UserRole>(_context);
        public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);

        // Reference Data Properties
        public IRepository<Status> Statuses => _statuses ??= new Repository<Status>(_context);
        public IRepository<ServiceType> ServiceTypes => _serviceTypes ??= new Repository<ServiceType>(_context);
        public IRepository<JobStatus> JobStatuses => _jobStatuses ??= new Repository<JobStatus>(_context);

        /// <summary>
        /// Save all changes from all repositories in one transaction
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction?.CommitAsync()!;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            try
            {
                await _transaction?.RollbackAsync()!;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
