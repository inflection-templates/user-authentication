/**
 * Customer controller for Order Service
 * TypeScript equivalent to Python customer handlers
 */

import { Request, Response, NextFunction } from 'express';
import { CustomerService } from '../../services/customer.service';
import { logger } from '../../common/logger';

export class CustomerController {
    private customerService: CustomerService;

    constructor() {
        this.customerService = new CustomerService();
    }

    getAll = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Getting all customers');
            const customers = await this.customerService.getAllCustomers();
            
            res.json({
                success: true,
                message: 'Customers retrieved successfully',
                httpcode: 200,
                customers
            });
        } catch (error) {
            next(error);
        }
    };

    getById = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Getting customer by ID: ${id}`);
            
            const customer = await this.customerService.getCustomerById(id);
            
            res.json({
                success: true,
                message: 'Customer retrieved successfully',
                httpcode: 200,
                customer
            });
        } catch (error) {
            next(error);
        }
    };

    create = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Creating new customer');
            const customer = await this.customerService.createCustomer(req.body);
            
            res.status(201).json({
                success: true,
                message: 'Customer created successfully',
                httpcode: 201,
                customer
            });
        } catch (error) {
            next(error);
        }
    };

    update = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Updating customer ID: ${id}`);
            
            const customer = await this.customerService.updateCustomer(id, req.body);
            
            res.json({
                success: true,
                message: 'Customer updated successfully',
                httpcode: 200,
                customer
            });
        } catch (error) {
            next(error);
        }
    };

    delete = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Deleting customer ID: ${id}`);
            
            await this.customerService.deleteCustomer(id);
            
            res.json({
                success: true,
                message: 'Customer deleted successfully',
                httpcode: 200
            });
        } catch (error) {
            next(error);
        }
    };
}
