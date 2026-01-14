/**
 * Data Transfer Objects (DTOs) for Order Service
 * TypeScript equivalent to the Python OrderService.Models.DTOs
 */

// Base interfaces and types
export interface BaseDto {
    created_at: Date;
    updated_at: Date;
}

export interface StandardResponse<T = any> {
    success: boolean;
    message: string;
    httpcode: number;
    data?: T;
}

export interface SuccessResponse {
    success: boolean;
    message: string;
    httpcode: number;
}

// Customer DTOs
export interface CustomerDto extends BaseDto {
    id: string;
    first_name: string;
    last_name: string;
    email: string;
    phone?: string;
    address?: string;
}

export interface CreateCustomerDto {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    address?: string;
}

export interface UpdateCustomerDto {
    firstName: string;
    lastName: string;
    email: string;
    phone?: string;
    address?: string;
}

// Product DTOs
export interface ProductDto extends BaseDto {
    id: number;
    name: string;
    description?: string;
    price: number;
    sku?: string;
    stock_quantity: number;
    category?: string;
    is_active: boolean;
}

export interface CreateProductDto {
    name: string;
    description?: string;
    price: number;
    sku?: string;
    stock_quantity?: number;
    category?: string;
}

export interface UpdateProductDto {
    name: string;
    description?: string;
    price: number;
    sku?: string;
    stock_quantity: number;
    category?: string;
    is_active?: boolean;
}

// OrderItem DTOs
export interface OrderItemDto extends BaseDto {
    id: number;
    order_id: number;
    product_id: number;
    quantity: number;
    unit_price: number;
    total_price: number;
    product: ProductDto;
}

export interface CreateOrderItemDto {
    product_id: number;
    quantity: number;
}

// Order DTOs
export interface OrderDto extends BaseDto {
    id: number;
    customer_id: string;
    order_number: string;
    order_date: Date;
    status: string;
    sub_total: number;
    tax_amount: number;
    shipping_amount: number;
    total_amount: number;
    notes?: string;
    shipping_address?: string;
    customer: CustomerDto;
    order_items: OrderItemDto[];
}

export interface CreateOrderDto {
    customer_id: string;
    notes?: string;
    shipping_address?: string;
    order_items: CreateOrderItemDto[];
}

export interface UpdateOrderDto {
    status: string;
    notes?: string;
    shipping_address?: string;
}

// Response Models
export interface CustomerResponse {
    success: boolean;
    message: string;
    httpcode: number;
    customer: CustomerDto;
}

export interface CustomersListResponse {
    success: boolean;
    message: string;
    httpcode: number;
    customers: CustomerDto[];
}

export interface ProductResponse {
    success: boolean;
    message: string;
    httpcode: number;
    product: ProductDto;
}

export interface ProductsListResponse {
    success: boolean;
    message: string;
    httpcode: number;
    products: ProductDto[];
}

export interface OrderResponse {
    success: boolean;
    message: string;
    httpcode: number;
    order: OrderDto;
}

export interface OrdersListResponse {
    success: boolean;
    message: string;
    httpcode: number;
    orders: OrderDto[];
}

// Validation types
export type OrderStatus = 'Pending' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled';

export const VALID_ORDER_STATUSES: OrderStatus[] = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

// Utility type for partial updates
export type PartialUpdate<T> = Partial<T> & { id: string | number };
