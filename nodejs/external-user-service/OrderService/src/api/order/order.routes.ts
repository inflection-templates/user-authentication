/**
 * Order routes for Order Service
 * TypeScript equivalent to Python order routes
 */

import { Router } from 'express';
import { OrderController } from './order.controller';

export class OrderRoutes {
    public router: Router;
    private controller: OrderController;

    constructor() {
        this.router = Router();
        this.controller = new OrderController();
        this.initializeRoutes();
    }

    private initializeRoutes(): void {
        this.router.get('/', this.controller.getAll);
        this.router.get('/:id', this.controller.getById);
        this.router.post('/', this.controller.create);
        this.router.put('/:id', this.controller.update);
        this.router.delete('/:id', this.controller.delete);
        this.router.put('/:id/status', this.controller.updateStatus);
    }
}
