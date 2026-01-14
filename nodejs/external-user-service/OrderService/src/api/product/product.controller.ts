/**
 * Product controller for Order Service
 * TypeScript equivalent to Python product handlers
 */

import { Request, Response, NextFunction } from 'express';
import { ProductService } from '../../services/product.service';
import { logger } from '../../common/logger';

export class ProductController {
    private productService: ProductService;

    constructor() {
        this.productService = new ProductService();
    }

    getAll = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Getting all products');
            const products = await this.productService.getAllProducts();
            
            res.json({
                success: true,
                message: 'Products retrieved successfully',
                httpcode: 200,
                products
            });
        } catch (error) {
            next(error);
        }
    };

    getById = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Getting product by ID: ${id}`);
            
            const product = await this.productService.getProductById(parseInt(id));
            
            res.json({
                success: true,
                message: 'Product retrieved successfully',
                httpcode: 200,
                product
            });
        } catch (error) {
            next(error);
        }
    };

    create = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Creating new product');
            const product = await this.productService.createProduct(req.body);
            
            res.status(201).json({
                success: true,
                message: 'Product created successfully',
                httpcode: 201,
                product
            });
        } catch (error) {
            next(error);
        }
    };

    update = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Updating product ID: ${id}`);
            
            const product = await this.productService.updateProduct(parseInt(id), req.body);
            
            res.json({
                success: true,
                message: 'Product updated successfully',
                httpcode: 200,
                product
            });
        } catch (error) {
            next(error);
        }
    };

    delete = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Deleting product ID: ${id}`);
            
            await this.productService.deleteProduct(parseInt(id));
            
            res.json({
                success: true,
                message: 'Product deleted successfully',
                httpcode: 200
            });
        } catch (error) {
            next(error);
        }
    };
}
