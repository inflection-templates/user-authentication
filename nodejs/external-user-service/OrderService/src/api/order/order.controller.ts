/**
 * Order controller for Order Service
 * TypeScript equivalent to Python order handlers
 */

import { Request, Response, NextFunction } from 'express';
import { OrderService } from '../../services/order.service';
import { logger } from '../../common/logger';

export class OrderController {
    private orderService: OrderService;

    constructor() {
        this.orderService = new OrderService();
    }

    getAll = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Getting all orders');
            const orders = await this.orderService.getAllOrders();
            
            res.json({
                success: true,
                message: 'Orders retrieved successfully',
                httpcode: 200,
                orders
            });
        } catch (error) {
            next(error);
        }
    };

    getById = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Getting order by ID: ${id}`);
            
            const order = await this.orderService.getOrderById(parseInt(id));
            
            res.json({
                success: true,
                message: 'Order retrieved successfully',
                httpcode: 200,
                order
            });
        } catch (error) {
            next(error);
        }
    };

    create = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            logger.info('Creating new order');
            const order = await this.orderService.createOrder(req.body);
            
            res.status(201).json({
                success: true,
                message: 'Order created successfully',
                httpcode: 201,
                order
            });
        } catch (error) {
            next(error);
        }
    };

    update = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Updating order ID: ${id}`);
            
            const order = await this.orderService.updateOrder(parseInt(id), req.body);
            
            res.json({
                success: true,
                message: 'Order updated successfully',
                httpcode: 200,
                order
            });
        } catch (error) {
            next(error);
        }
    };

    updateStatus = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            const { status } = req.body;
            logger.info(`Updating order status for ID: ${id} to ${status}`);
            
            const order = await this.orderService.updateOrderStatus(parseInt(id), status);
            
            res.json({
                success: true,
                message: 'Order status updated successfully',
                httpcode: 200,
                order
            });
        } catch (error) {
            next(error);
        }
    };

    delete = async (req: Request, res: Response, next: NextFunction): Promise<void> => {
        try {
            const { id } = req.params;
            logger.info(`Deleting order ID: ${id}`);
            
            await this.orderService.deleteOrder(parseInt(id));
            
            res.json({
                success: true,
                message: 'Order deleted successfully',
                httpcode: 200
            });
        } catch (error) {
            next(error);
        }
    };
}
