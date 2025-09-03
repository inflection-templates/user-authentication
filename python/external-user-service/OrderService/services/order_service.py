"""
Order service implementation
Equivalent to the .NET OrderService.Services.OrderService
"""

import logging
from datetime import datetime
from decimal import Decimal
from typing import List, Optional
import uuid
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, and_
from sqlalchemy.orm import selectinload

from models.entities import Order, OrderItem, Customer, Product
from models.dtos import OrderDto, CreateOrderDto, UpdateOrderDto, CustomerDto, OrderItemDto, ProductDto

logger = logging.getLogger(__name__)


class OrderService:
    """Order service for managing order operations"""
    
    def __init__(self, db_session: AsyncSession):
        self.db_session = db_session
    
    async def get_all_orders(self) -> List[OrderDto]:
        """Get all orders with customer and order items"""
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .order_by(Order.order_date.desc())
        )
        orders = result.scalars().all()
        return [self._map_to_dto(order) for order in orders]
    
    async def get_order_by_id(self, order_id: int) -> Optional[OrderDto]:
        """Get order by ID with customer and order items"""
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .where(Order.id == order_id)
        )
        order = result.scalar_one_or_none()
        return self._map_to_dto(order) if order else None
    
    async def create_order(self, create_dto: CreateOrderDto) -> OrderDto:
        """Create a new order"""
        # Validate customer exists
        customer_result = await self.db_session.execute(
            select(Customer).where(Customer.id == create_dto.customer_id)
        )
        customer = customer_result.scalar_one_or_none()
        
        if not customer:
            raise ValueError("Customer not found")
        
        # Validate products and calculate totals
        sub_total = Decimal("0.00")
        order_items_data = []
        
        for item_dto in create_dto.order_items:
            # Get product
            product_result = await self.db_session.execute(
                select(Product).where(Product.id == item_dto.product_id)
            )
            product = product_result.scalar_one_or_none()
            
            if not product:
                raise ValueError(f"Product with ID {item_dto.product_id} not found")
            
            if not product.is_active:
                raise ValueError(f"Product {product.name} is not active")
            
            if product.stock_quantity < item_dto.quantity:
                raise ValueError(f"Insufficient stock for product {product.name}")
            
            # Calculate item total
            item_total = product.price * item_dto.quantity
            sub_total += item_total
            
            order_items_data.append({
                "product": product,
                "quantity": item_dto.quantity,
                "unit_price": product.price,
                "total_price": item_total
            })
        
        # Calculate tax and shipping (simplified calculation)
        tax_amount = sub_total * Decimal("0.08")  # 8% tax
        shipping_amount = Decimal("10.00") if sub_total < Decimal("100.00") else Decimal("0.00")
        total_amount = sub_total + tax_amount + shipping_amount
        
        # Generate order number
        order_number = f"ORD-{datetime.utcnow().strftime('%Y%m%d')}-{str(uuid.uuid4())[:8].upper()}"
        
        # Create order
        order = Order(
            customer_id=create_dto.customer_id,
            order_number=order_number,
            order_date=datetime.utcnow(),
            status="Pending",
            sub_total=sub_total,
            tax_amount=tax_amount,
            shipping_amount=shipping_amount,
            total_amount=total_amount,
            notes=create_dto.notes,
            shipping_address=create_dto.shipping_address,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        self.db_session.add(order)
        await self.db_session.flush()  # Get the order ID
        
        # Create order items and update stock
        for item_data in order_items_data:
            order_item = OrderItem(
                order_id=order.id,
                product_id=item_data["product"].id,
                quantity=item_data["quantity"],
                unit_price=item_data["unit_price"],
                total_price=item_data["total_price"],
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            self.db_session.add(order_item)
            
            # Update product stock
            item_data["product"].stock_quantity -= item_data["quantity"]
            item_data["product"].updated_at = datetime.utcnow()
        
        await self.db_session.commit()
        
        # Refresh and return
        await self.db_session.refresh(order)
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .where(Order.id == order.id)
        )
        order = result.scalar_one()
        
        logger.info(f"Order created with ID: {order.id}, Number: {order.order_number}")
        return self._map_to_dto(order)
    
    async def update_order(self, order_id: int, update_dto: UpdateOrderDto) -> Optional[OrderDto]:
        """Update an existing order"""
        # Get existing order
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .where(Order.id == order_id)
        )
        order = result.scalar_one_or_none()
        
        if not order:
            return None
        
        # Update order
        order.status = update_dto.status
        order.notes = update_dto.notes
        order.shipping_address = update_dto.shipping_address
        order.updated_at = datetime.utcnow()
        
        await self.db_session.commit()
        await self.db_session.refresh(order)
        
        logger.info(f"Order updated with ID: {order_id}")
        return self._map_to_dto(order)
    
    async def update_order_status(self, order_id: int, status: str) -> Optional[OrderDto]:
        """Update order status"""
        valid_statuses = ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"]
        if status not in valid_statuses:
            raise ValueError(f"Invalid status. Must be one of: {', '.join(valid_statuses)}")
        
        # Get existing order
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .where(Order.id == order_id)
        )
        order = result.scalar_one_or_none()
        
        if not order:
            return None
        
        # Update status
        order.status = status
        order.updated_at = datetime.utcnow()
        
        await self.db_session.commit()
        await self.db_session.refresh(order)
        
        logger.info(f"Order status updated to '{status}' for order ID: {order_id}")
        return self._map_to_dto(order)
    
    async def delete_order(self, order_id: int) -> bool:
        """Delete an order"""
        # Get order
        result = await self.db_session.execute(
            select(Order)
            .options(selectinload(Order.order_items))
            .where(Order.id == order_id)
        )
        order = result.scalar_one_or_none()
        
        if not order:
            return False
        
        # Check if order can be deleted (only pending orders)
        if order.status not in ["Pending"]:
            raise RuntimeError("Cannot delete order that is not in Pending status")
        
        # Restore product stock
        for order_item in order.order_items:
            product_result = await self.db_session.execute(
                select(Product).where(Product.id == order_item.product_id)
            )
            product = product_result.scalar_one_or_none()
            if product:
                product.stock_quantity += order_item.quantity
                product.updated_at = datetime.utcnow()
        
        # Delete order (order items will be deleted due to cascade)
        await self.db_session.delete(order)
        await self.db_session.commit()
        
        logger.info(f"Order deleted with ID: {order_id}")
        return True
    
    async def get_orders_by_customer(self, customer_id: int) -> List[OrderDto]:
        """Get orders by customer ID"""
        result = await self.db_session.execute(
            select(Order)
            .options(
                selectinload(Order.customer),
                selectinload(Order.order_items).selectinload(OrderItem.product)
            )
            .where(Order.customer_id == customer_id)
            .order_by(Order.order_date.desc())
        )
        orders = result.scalars().all()
        return [self._map_to_dto(order) for order in orders]
    
    def _map_to_dto(self, order: Order) -> OrderDto:
        """Map Order entity to OrderDto"""
        return OrderDto(
            id=order.id,
            customer_id=order.customer_id,
            order_number=order.order_number,
            order_date=order.order_date,
            status=order.status,
            sub_total=order.sub_total,
            tax_amount=order.tax_amount,
            shipping_amount=order.shipping_amount,
            total_amount=order.total_amount,
            notes=order.notes,
            shipping_address=order.shipping_address,
            created_at=order.created_at,
            updated_at=order.updated_at,
            customer=CustomerDto(
                id=order.customer.id,
                first_name=order.customer.first_name,
                last_name=order.customer.last_name,
                email=order.customer.email,
                phone=order.customer.phone,
                address=order.customer.address,
                created_at=order.customer.created_at,
                updated_at=order.customer.updated_at
            ),
            order_items=[
                OrderItemDto(
                    id=item.id,
                    order_id=item.order_id,
                    product_id=item.product_id,
                    quantity=item.quantity,
                    unit_price=item.unit_price,
                    total_price=item.total_price,
                    created_at=item.created_at,
                    updated_at=item.updated_at,
                    product=ProductDto(
                        id=item.product.id,
                        name=item.product.name,
                        description=item.product.description,
                        price=item.product.price,
                        sku=item.product.sku,
                        stock_quantity=item.product.stock_quantity,
                        category=item.product.category,
                        is_active=item.product.is_active,
                        created_at=item.product.created_at,
                        updated_at=item.product.updated_at
                    )
                )
                for item in order.order_items
            ]
        )
