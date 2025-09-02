using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
    Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto);
    Task<bool> DeleteOrderAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId);
    Task<OrderDto?> UpdateOrderStatusAsync(int id, string status);
}

public class OrderService : IOrderService
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(OrderDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order != null ? MapToDto(order) : null;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
    {
        // Validate customer exists
        var customer = await _context.Customers.FindAsync(createOrderDto.CustomerId);
        if (customer == null)
        {
            throw new ArgumentException("Customer not found");
        }

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync();

        // Create order
        var order = new Order
        {
            CustomerId = createOrderDto.CustomerId,
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            Notes = createOrderDto.Notes,
            ShippingAddress = createOrderDto.ShippingAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Calculate totals
        decimal subTotal = 0;
        foreach (var itemDto in createOrderDto.OrderItems)
        {
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new ArgumentException($"Insufficient stock for product {product.Name}");
            }

            var unitPrice = product.Price;
            var totalPrice = unitPrice * itemDto.Quantity;
            subTotal += totalPrice;

            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order.OrderItems.Add(orderItem);

            // Update product stock
            product.StockQuantity -= itemDto.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }

        // Calculate final totals (simplified - no tax/shipping for now)
        order.SubTotal = subTotal;
        order.TaxAmount = 0; // Could be calculated based on location
        order.ShippingAmount = 0; // Could be calculated based on weight/location
        order.TotalAmount = subTotal + order.TaxAmount + order.ShippingAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created with ID: {OrderId}, Order Number: {OrderNumber}", order.Id, order.OrderNumber);

        return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to retrieve created order");
    }

    public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return null;
        }

        order.Status = updateOrderDto.Status;
        order.Notes = updateOrderDto.Notes;
        order.ShippingAddress = updateOrderDto.ShippingAddress;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order updated with ID: {OrderId}", id);

        return await GetOrderByIdAsync(id);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        // Restore product stock
        foreach (var orderItem in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product != null)
            {
                product.StockQuantity += orderItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order deleted with ID: {OrderId}", id);

        return true;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerIdAsync(int customerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(int id, string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return null;
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order status updated to {Status} for Order ID: {OrderId}", status, id);

        return await GetOrderByIdAsync(id);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith($"ORD-{today}"))
            .CountAsync();

        return $"ORD-{today}-{(count + 1):D4}";
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,
            Status = order.Status,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Customer = new CustomerDto
            {
                Id = order.Customer.Id,
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Email = order.Customer.Email,
                Phone = order.Customer.Phone,
                Address = order.Customer.Address,
                CreatedAt = order.Customer.CreatedAt,
                UpdatedAt = order.Customer.UpdatedAt
            },
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice,
                CreatedAt = oi.CreatedAt,
                UpdatedAt = oi.UpdatedAt,
                Product = new ProductDto
                {
                    Id = oi.Product.Id,
                    Name = oi.Product.Name,
                    Description = oi.Product.Description,
                    Price = oi.Product.Price,
                    SKU = oi.Product.SKU,
                    StockQuantity = oi.Product.StockQuantity,
                    Category = oi.Product.Category,
                    IsActive = oi.Product.IsActive,
                    CreatedAt = oi.Product.CreatedAt,
                    UpdatedAt = oi.Product.UpdatedAt
                }
            }).ToList()
        };
    }
}
