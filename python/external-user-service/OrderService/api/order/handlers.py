"""
Order API handlers
Contains the business logic for order operations
"""

from typing import List, Optional
from fastapi import HTTPException
from models.dtos import OrderDto, CreateOrderDto, UpdateOrderDto
from services.order_service import OrderService


async def get_all_orders_handler(db_session) -> List[OrderDto]:
    """Handler for getting all orders"""
    service = OrderService(db_session)
    return await service.get_all_orders()


async def get_order_by_id_handler(order_id: int, db_session) -> Optional[OrderDto]:
    """Handler for getting an order by ID"""
    service = OrderService(db_session)
    order = await service.get_order_by_id(order_id)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return order


async def create_order_handler(order_data: CreateOrderDto, db_session) -> OrderDto:
    """Handler for creating a new order"""
    try:
        service = OrderService(db_session)
        return await service.create_order(order_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_order_handler(order_id: int, order_data: UpdateOrderDto, db_session) -> Optional[OrderDto]:
    """Handler for updating an order"""
    try:
        service = OrderService(db_session)
        order = await service.update_order(order_id, order_data)
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return order
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_order_handler(order_id: int, db_session) -> bool:
    """Handler for deleting an order"""
    try:
        service = OrderService(db_session)
        success = await service.delete_order(order_id)
        if not success:
            raise HTTPException(status_code=404, detail="Order not found")
        return success
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_order_status_handler(order_id: int, status: str, db_session) -> Optional[OrderDto]:
    """Handler for updating order status"""
    try:
        service = OrderService(db_session)
        order = await service.update_order_status(order_id, status)
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return order
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
