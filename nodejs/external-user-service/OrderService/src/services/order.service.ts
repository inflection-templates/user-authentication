/**
 * Order service for Order Service
 * TypeScript equivalent to Python order service
 */

import { Order, OrderItem, Customer, Product } from '../models/entities';
import { CreateOrderDto, UpdateOrderDto, OrderDto, VALID_ORDER_STATUSES, OrderStatus } from '../models/dtos';
import { ProductService } from './product.service';
import { logger } from '../common/logger';
import { v4 as uuidv4 } from 'uuid';

export class OrderService {
    private productService: ProductService;

    constructor() {
        this.productService = new ProductService();
    }

    async getAllOrders(): Promise<OrderDto[]> {
        try {
            const orders = await Order.findAll({
                include: [
                    {
                        model: Customer,
                        as: 'customer'
                    },
                    {
                        model: OrderItem,
                        as: 'order_items',
                        include: [{
                            model: Product,
                            as: 'product'
                        }]
                    }
                ],
                order: [['created_at', 'DESC']]
            });
            return orders.map(order => this.mapToDto(order));
        } catch (error) {
            logger.error('Error getting all orders:', error);
            throw error;
        }
    }

    async getOrderById(id: number): Promise<OrderDto> {
        try {
            const order = await Order.findByPk(id, {
                include: [
                    {
                        model: Customer,
                        as: 'customer'
                    },
                    {
                        model: OrderItem,
                        as: 'order_items',
                        include: [{
                            model: Product,
                            as: 'product'
                        }]
                    }
                ]
            });
            
            if (!order) {
                const error = new Error(`Order with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }
            
            return this.mapToDto(order);
        } catch (error) {
            logger.error(`Error getting order by ID ${id}:`, error);
            throw error;
        }
    }

    async createOrder(orderData: CreateOrderDto): Promise<OrderDto> {
        try {
            // Verify customer exists
            const customer = await Customer.findByPk(orderData.customer_id);
            if (!customer) {
                const error = new Error(`Customer with ID ${orderData.customer_id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            // Calculate totals and validate products
            let subTotal = 0;
            const orderItems = [];

            for (const item of orderData.order_items) {
                const product = await Product.findByPk(item.product_id);
                if (!product || !product.is_active) {
                    const error = new Error(`Product with ID ${item.product_id} not found or inactive`);
                    (error as any).statusCode = 404;
                    throw error;
                }

                if (product.stock_quantity < item.quantity) {
                    const error = new Error(`Insufficient stock for product ${product.name}. Available: ${product.stock_quantity}, Required: ${item.quantity}`);
                    (error as any).statusCode = 400;
                    throw error;
                }

                const unitPrice = product.price;
                const totalPrice = unitPrice * item.quantity;
                subTotal += totalPrice;

                orderItems.push({
                    product_id: item.product_id,
                    quantity: item.quantity,
                    unit_price: unitPrice,
                    total_price: totalPrice
                });
            }

            // Calculate tax and shipping (simplified)
            const taxAmount = subTotal * 0.08; // 8% tax
            const shippingAmount = subTotal > 100 ? 0 : 10; // Free shipping over $100
            const totalAmount = subTotal + taxAmount + shippingAmount;

            // Generate order number
            const orderNumber = `ORD-${Date.now()}-${Math.random().toString(36).substr(2, 9).toUpperCase()}`;

            // Create order
            const order = await Order.create({
                customer_id: orderData.customer_id,
                order_number: orderNumber,
                status: 'Pending',
                sub_total: subTotal,
                tax_amount: taxAmount,
                shipping_amount: shippingAmount,
                total_amount: totalAmount,
                notes: orderData.notes,
                shipping_address: orderData.shipping_address
            });

            // Create order items and update product stock
            for (const itemData of orderItems) {
                await OrderItem.create({
                    order_id: order.id,
                    ...itemData
                });

                // Update product stock
                await this.productService.updateStock(itemData.product_id, itemData.quantity);
            }

            logger.info(`Order created with ID: ${order.id}`);
            return await this.getOrderById(order.id);
        } catch (error) {
            logger.error('Error creating order:', error);
            throw error;
        }
    }

    async updateOrder(id: number, orderData: UpdateOrderDto): Promise<OrderDto> {
        try {
            const order = await Order.findByPk(id);
            if (!order) {
                const error = new Error(`Order with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            // Validate status
            if (!VALID_ORDER_STATUSES.includes(orderData.status as OrderStatus)) {
                const error = new Error(`Invalid status. Valid statuses: ${VALID_ORDER_STATUSES.join(', ')}`);
                (error as any).statusCode = 400;
                throw error;
            }

            await order.update({
                status: orderData.status,
                notes: orderData.notes,
                shipping_address: orderData.shipping_address
            });

            logger.info(`Order updated with ID: ${id}`);
            return await this.getOrderById(id);
        } catch (error) {
            logger.error(`Error updating order with ID ${id}:`, error);
            throw error;
        }
    }

    async updateOrderStatus(id: number, status: string): Promise<OrderDto> {
        try {
            const order = await Order.findByPk(id);
            if (!order) {
                const error = new Error(`Order with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            // Validate status
            if (!VALID_ORDER_STATUSES.includes(status as OrderStatus)) {
                const error = new Error(`Invalid status. Valid statuses: ${VALID_ORDER_STATUSES.join(', ')}`);
                (error as any).statusCode = 400;
                throw error;
            }

            await order.update({ status });
            logger.info(`Order status updated for ID: ${id} to ${status}`);
            return await this.getOrderById(id);
        } catch (error) {
            logger.error(`Error updating order status for ID ${id}:`, error);
            throw error;
        }
    }

    async deleteOrder(id: number): Promise<void> {
        try {
            const order = await Order.findByPk(id, {
                include: [{
                    model: OrderItem,
                    as: 'order_items'
                }]
            });
            
            if (!order) {
                const error = new Error(`Order with ID ${id} not found`);
                (error as any).statusCode = 404;
                throw error;
            }

            // Only allow deletion of pending orders
            if (order.status !== 'Pending') {
                const error = new Error(`Cannot delete order with status: ${order.status}`);
                (error as any).statusCode = 400;
                throw error;
            }

            // Restore product stock
            for (const item of order.order_items) {
                const product = await Product.findByPk(item.product_id);
                if (product) {
                    await product.update({
                        stock_quantity: product.stock_quantity + item.quantity
                    });
                }
            }

            await order.destroy();
            logger.info(`Order deleted with ID: ${id}`);
        } catch (error) {
            logger.error(`Error deleting order with ID ${id}:`, error);
            throw error;
        }
    }

    private mapToDto(order: Order): OrderDto {
        return {
            id: order.id,
            customer_id: order.customer_id,
            order_number: order.order_number,
            order_date: order.order_date,
            status: order.status,
            sub_total: order.sub_total,
            tax_amount: order.tax_amount,
            shipping_amount: order.shipping_amount,
            total_amount: order.total_amount,
            notes: order.notes,
            shipping_address: order.shipping_address,
            created_at: order.created_at,
            updated_at: order.updated_at,
            customer: {
                id: order.customer.id,
                first_name: order.customer.first_name,
                last_name: order.customer.last_name,
                email: order.customer.email,
                phone: order.customer.phone,
                address: order.customer.address,
                created_at: order.customer.created_at,
                updated_at: order.customer.updated_at
            },
            order_items: order.order_items.map(item => ({
                id: item.id,
                order_id: item.order_id,
                product_id: item.product_id,
                quantity: item.quantity,
                unit_price: item.unit_price,
                total_price: item.total_price,
                created_at: item.created_at,
                updated_at: item.updated_at,
                product: {
                    id: item.product.id,
                    name: item.product.name,
                    description: item.product.description,
                    price: item.product.price,
                    sku: item.product.sku,
                    stock_quantity: item.product.stock_quantity,
                    category: item.product.category,
                    is_active: item.product.is_active,
                    created_at: item.product.created_at,
                    updated_at: item.product.updated_at
                }
            }))
        };
    }
}
