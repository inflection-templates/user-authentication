using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
    Task<bool> DeleteProductAsync(int id);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<ProductDto?> GetProductBySkuAsync(string sku);
}

public class ProductService : IProductService
{
    private readonly OrderDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(OrderDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
    {
        // Check if product with SKU already exists
        if (!string.IsNullOrEmpty(createProductDto.SKU))
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == createProductDto.SKU);

            if (existingProduct != null)
            {
                throw new ArgumentException("Product with this SKU already exists");
            }
        }

        var product = new Product
        {
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            Price = createProductDto.Price,
            SKU = createProductDto.SKU,
            StockQuantity = createProductDto.StockQuantity,
            Category = createProductDto.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product created with ID: {ProductId}, Name: {Name}", product.Id, product.Name);

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return null;
        }

        // Check if SKU is being changed and if new SKU already exists
        if (product.SKU != updateProductDto.SKU && !string.IsNullOrEmpty(updateProductDto.SKU))
        {
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.SKU == updateProductDto.SKU && p.Id != id);

            if (existingProduct != null)
            {
                throw new ArgumentException("Product with this SKU already exists");
            }
        }

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.Price = updateProductDto.Price;
        product.SKU = updateProductDto.SKU;
        product.StockQuantity = updateProductDto.StockQuantity;
        product.Category = updateProductDto.Category;
        product.IsActive = updateProductDto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Product updated with ID: {ProductId}", id);

        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return false;
        }

        // Check if product has order items
        var hasOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrderItems)
        {
            throw new InvalidOperationException("Cannot delete product with existing order items");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Product deleted with ID: {ProductId}", id);

        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        var products = await _context.Products
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == sku);

        return product != null ? MapToDto(product) : null;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            SKU = product.SKU,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
