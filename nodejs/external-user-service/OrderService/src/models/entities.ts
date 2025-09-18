/**
 * Database entities for Order Service
 * TypeScript equivalent to the Python OrderService.Models.Entities
 */

import { Table, Column, Model, DataType, PrimaryKey, AutoIncrement, CreatedAt, UpdatedAt, HasMany, BelongsTo, ForeignKey } from 'sequelize-typescript';

@Table({
    tableName: 'customers',
    timestamps: true,
    underscored: true
})
export class Customer extends Model<Customer> {

    @PrimaryKey
    @Column({
        type: DataType.UUID,
        defaultValue: DataType.UUIDV4
    })
    id!: string;

    @Column({
        type: DataType.STRING(100),
        allowNull: false
    })
    first_name!: string;

    @Column({
        type: DataType.STRING(100),
        allowNull: false
    })
    last_name!: string;

    @Column({
        type: DataType.STRING(255),
        allowNull: false,
        unique: true
    })
    email!: string;

    @Column({
        type: DataType.STRING(20),
        allowNull: true
    })
    phone?: string;

    @Column({
        type: DataType.STRING(500),
        allowNull: true
    })
    address?: string;

    @CreatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    created_at!: Date;

    @UpdatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    updated_at!: Date;

    @HasMany(() => Order)
    orders!: Order[];
}

@Table({
    tableName: 'products',
    timestamps: true,
    underscored: true
})
export class Product extends Model<Product> {

    @PrimaryKey
    @AutoIncrement
    @Column({
        type: DataType.INTEGER
    })
    id!: number;

    @Column({
        type: DataType.STRING(200),
        allowNull: false
    })
    name!: string;

    @Column({
        type: DataType.STRING(1000),
        allowNull: true
    })
    description?: string;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false
    })
    price!: number;

    @Column({
        type: DataType.STRING(50),
        allowNull: true,
        unique: true
    })
    sku?: string;

    @Column({
        type: DataType.INTEGER,
        allowNull: false,
        defaultValue: 0
    })
    stock_quantity!: number;

    @Column({
        type: DataType.STRING(100),
        allowNull: true
    })
    category?: string;

    @Column({
        type: DataType.BOOLEAN,
        allowNull: false,
        defaultValue: true
    })
    is_active!: boolean;

    @CreatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    created_at!: Date;

    @UpdatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    updated_at!: Date;

    @HasMany(() => OrderItem)
    order_items!: OrderItem[];
}

@Table({
    tableName: 'orders',
    timestamps: true,
    underscored: true
})
export class Order extends Model<Order> {

    @PrimaryKey
    @AutoIncrement
    @Column({
        type: DataType.INTEGER
    })
    id!: number;

    @ForeignKey(() => Customer)
    @Column({
        type: DataType.UUID,
        allowNull: false
    })
    customer_id!: string;

    @Column({
        type: DataType.STRING(50),
        allowNull: false,
        unique: true
    })
    order_number!: string;

    @Column({
        type: DataType.DATE,
        allowNull: false,
        defaultValue: DataType.NOW
    })
    order_date!: Date;

    @Column({
        type: DataType.STRING(50),
        allowNull: false,
        defaultValue: 'Pending'
    })
    status!: string; // Pending, Processing, Shipped, Delivered, Cancelled

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false,
        defaultValue: 0
    })
    sub_total!: number;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false,
        defaultValue: 0
    })
    tax_amount!: number;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false,
        defaultValue: 0
    })
    shipping_amount!: number;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false,
        defaultValue: 0
    })
    total_amount!: number;

    @Column({
        type: DataType.STRING(500),
        allowNull: true
    })
    notes?: string;

    @Column({
        type: DataType.STRING(200),
        allowNull: true
    })
    shipping_address?: string;

    @CreatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    created_at!: Date;

    @UpdatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    updated_at!: Date;

    @BelongsTo(() => Customer)
    customer!: Customer;

    @HasMany(() => OrderItem)
    order_items!: OrderItem[];
}

@Table({
    tableName: 'order_items',
    timestamps: true,
    underscored: true
})
export class OrderItem extends Model<OrderItem> {

    @PrimaryKey
    @AutoIncrement
    @Column({
        type: DataType.INTEGER
    })
    id!: number;

    @ForeignKey(() => Order)
    @Column({
        type: DataType.INTEGER,
        allowNull: false
    })
    order_id!: number;

    @ForeignKey(() => Product)
    @Column({
        type: DataType.INTEGER,
        allowNull: false
    })
    product_id!: number;

    @Column({
        type: DataType.INTEGER,
        allowNull: false
    })
    quantity!: number;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false
    })
    unit_price!: number;

    @Column({
        type: DataType.DECIMAL(18, 2),
        allowNull: false
    })
    total_price!: number;

    @CreatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    created_at!: Date;

    @UpdatedAt
    @Column({
        type: DataType.DATE,
        allowNull: false
    })
    updated_at!: Date;

    @BelongsTo(() => Order)
    order!: Order;

    @BelongsTo(() => Product)
    product!: Product;
}
