/**
 * Customer routes for Order Service
 * TypeScript equivalent to Python customer routes
 */

import { Router } from 'express';
import { CustomerController } from './customer.controller';

export class CustomerRoutes {
    public router: Router;
    private controller: CustomerController;

    constructor() {
        this.router = Router();
        this.controller = new CustomerController();
        this.initializeRoutes();
    }

    private initializeRoutes(): void {
        this.router.get('/', this.controller.getAll);
        this.router.get('/:id', this.controller.getById);
        this.router.post('/', this.controller.create);
        this.router.put('/:id', this.controller.update);
        this.router.delete('/:id', this.controller.delete);
    }
}
