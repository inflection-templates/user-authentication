"""
Order API handlers
Contains the business logic for order operations
"""

from typing import List, Optional
from fastapi import HTTPException
from models.dtos import OrderDto, CreateOrderDto, UpdateOrderDto, OrderResponse, OrdersListResponse, SuccessResponse
from services.order_service import OrderService


async def get_all_orders_handler(db_session) -> OrdersListResponse:
    """Handler for getting all orders"""
    service = OrderService(db_session)
    orders = await service.get_all_orders()
    return OrdersListResponse(
        success=True,
        message="Orders retrieved successfully",
        httpcode=200,
        orders=orders
    )


async def get_order_by_id_handler(order_id: int, db_session) -> OrderResponse:
    """Handler for getting an order by ID"""
    service = OrderService(db_session)
    order = await service.get_order_by_id(order_id)
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    return OrderResponse(
        success=True,
        message="Order retrieved successfully",
        httpcode=200,
        order=order
    )


async def create_order_handler(order_data: CreateOrderDto, db_session) -> OrderResponse:
    """Handler for creating a new order"""
    try:
        service = OrderService(db_session)
        order = await service.create_order(order_data)
        return OrderResponse(
            success=True,
            message="Order created successfully",
            httpcode=201,
            order=order
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_order_handler(order_id: int, order_data: UpdateOrderDto, db_session) -> OrderResponse:
    """Handler for updating an order"""
    try:
        service = OrderService(db_session)
        order = await service.update_order(order_id, order_data)
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return OrderResponse(
            success=True,
            message="Order updated successfully",
            httpcode=200,
            order=order
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_order_handler(order_id: int, db_session) -> SuccessResponse:
    """Handler for deleting an order"""
    try:
        service = OrderService(db_session)
        success = await service.delete_order(order_id)
        if not success:
            raise HTTPException(status_code=404, detail="Order not found")
        return SuccessResponse(
            success=True,
            message="Order deleted successfully",
            httpcode=204
        )
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_order_status_handler(order_id: int, status: str, db_session) -> OrderResponse:
    """Handler for updating order status"""
    try:
        service = OrderService(db_session)
        order = await service.update_order_status(order_id, status)
        if not order:
            raise HTTPException(status_code=404, detail="Order not found")
        return OrderResponse(
            success=True,
            message="Order status updated successfully",
            httpcode=200,
            order=order
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
