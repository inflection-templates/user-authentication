using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
    Task<bool> DeleteCustomerAsync(int id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
}

public class CustomerService : ICustomerService
{
    private readonly OrderDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(OrderDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _context.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
    {
        // Check if customer with email already exists
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == createCustomerDto.Email);

        if (existingCustomer != null)
        {
            throw new ArgumentException("Customer with this email already exists");
        }

        var customer = new Customer
        {
            FirstName = createCustomerDto.FirstName,
            LastName = createCustomerDto.LastName,
            Email = createCustomerDto.Email,
            Phone = createCustomerDto.Phone,
            Address = createCustomerDto.Address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer created with ID: {CustomerId}, Email: {Email}", customer.Id, customer.Email);

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return null;
        }

        // Check if email is being changed and if new email already exists
        if (customer.Email != updateCustomerDto.Email)
        {
            var existingCustomer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == updateCustomerDto.Email && c.Id != id);

            if (existingCustomer != null)
            {
                throw new ArgumentException("Customer with this email already exists");
            }
        }

        customer.FirstName = updateCustomerDto.FirstName;
        customer.LastName = updateCustomerDto.LastName;
        customer.Email = updateCustomerDto.Email;
        customer.Phone = updateCustomerDto.Phone;
        customer.Address = updateCustomerDto.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer updated with ID: {CustomerId}", id);

        return MapToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return false;
        }

        // Check if customer has orders
        var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == id);
        if (hasOrders)
        {
            throw new InvalidOperationException("Cannot delete customer with existing orders");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Customer deleted with ID: {CustomerId}", id);

        return true;
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);

        return customer != null ? MapToDto(customer) : null;
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
