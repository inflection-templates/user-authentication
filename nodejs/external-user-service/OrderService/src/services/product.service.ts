/**
 * Product service for Order Service
 * TypeScript equivalent to Python product service
 */

import { Product } from '../models/entities';
import { CreateProductDto, UpdateProductDto, ProductDto } from '../models/dtos';
import { logger } from '../common/logger';

export class ProductService {
    async getAllProducts(): Promise<ProductDto[]> {
        try {
            const products = await Product.findAll({
                where: { is_active: true },
                order: [['created_at', 'DESC']]
            });
            return products.map(product => this.mapToDto(product));
        } catch (error) {
            logger.error('Error getting all products:', error);
            throw error;
        }
    }

    async getProductById(id: number): Promise<ProductDto> {
        try {
            const product = await Product.findByPk(id);
            if (!product) {
                const error = new Error(`Product with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }
            return this.mapToDto(product);
        } catch (error) {
            logger.error(`Error getting product by ID ${id}:`, error);
            throw error;
        }
    }

    async createProduct(productData: CreateProductDto): Promise<ProductDto> {
        try {
            const product = await Product.create({
                name: productData.name,
                description: productData.description,
                price: productData.price,
                sku: productData.sku,
                stock_quantity: productData.stock_quantity || 0,
                category: productData.category,
                is_active: true
            });
            logger.info(`Product created with ID: ${product.id}`);
            return this.mapToDto(product);
        } catch (error) {
            logger.error('Error creating product:', error);
            throw error;
        }
    }

    async updateProduct(id: number, productData: UpdateProductDto): Promise<ProductDto> {
        try {
            const product = await Product.findByPk(id);
            if (!product) {
                const error = new Error(`Product with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            await product.update({
                name: productData.name,
                description: productData.description,
                price: productData.price,
                sku: productData.sku,
                stock_quantity: productData.stock_quantity,
                category: productData.category,
                is_active: productData.is_active ?? product.is_active
            });

            logger.info(`Product updated with ID: ${id}`);
            return this.mapToDto(product);
        } catch (error) {
            logger.error(`Error updating product with ID ${id}:`, error);
            throw error;
        }
    }

    async deleteProduct(id: number): Promise<void> {
        try {
            const product = await Product.findByPk(id);
            if (!product) {
                const error = new Error(`Product with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            // Soft delete by setting is_active to false
            await product.update({ is_active: false });
            logger.info(`Product soft deleted with ID: ${id}`);
        } catch (error) {
            logger.error(`Error deleting product with ID ${id}:`, error);
            throw error;
        }
    }

    async updateStock(id: number, quantity: number): Promise<ProductDto> {
        try {
            const product = await Product.findByPk(id);
            if (!product) {
                const error = new Error(`Product with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            if (product.stock_quantity < quantity) {
                const error = new Error(`Insufficient stock. Available: ${product.stock_quantity}, Required: ${quantity}`);
                (error as any).statusCode = 400;
                throw error;
            }

            await product.update({
                stock_quantity: product.stock_quantity - quantity
            });

            logger.info(`Product stock updated for ID: ${id}, Quantity reduced by: ${quantity}`);
            return this.mapToDto(product);
        } catch (error) {
            logger.error(`Error updating stock for product ID ${id}:`, error);
            throw error;
        }
    }

    private mapToDto(product: Product): ProductDto {
        return {
            id: product.id,
            name: product.name,
            description: product.description,
            price: product.price,
            sku: product.sku,
            stock_quantity: product.stock_quantity,
            category: product.category,
            is_active: product.is_active,
            created_at: product.created_at,
            updated_at: product.updated_at
        };
    }
}
