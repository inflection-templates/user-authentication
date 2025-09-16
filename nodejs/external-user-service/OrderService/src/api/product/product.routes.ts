/**
 * Product routes for Order Service
 * TypeScript equivalent to Python product routes
 */

import { Router } from 'express';
import { ProductController } from './product.controller';

export class ProductRoutes {
    public router: Router;
    private controller: ProductController;

    constructor() {
        this.router = Router();
        this.controller = new ProductController();
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
