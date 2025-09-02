namespace OrderService.Models;

// Customer DTOs
public class CustomerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class UpdateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

// Product DTOs
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}

// Order DTOs
public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public CustomerDto Customer { get; set; } = null!;
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderDto
{
    public int CustomerId { get; set; }
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class UpdateOrderDto
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ShippingAddress { get; set; }
}

// OrderItem DTOs
public class OrderItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ProductDto Product { get; set; } = null!;
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
