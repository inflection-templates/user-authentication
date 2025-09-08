"""
Order API routes
Defines the FastAPI route endpoints for order operations
"""

from typing import List
from fastapi import APIRouter, Depends
from models.dtos import OrderDto, CreateOrderDto, UpdateOrderDto, OrderResponse, OrdersListResponse, SuccessResponse
from database.db_context import get_db_session
from auth.dependencies import verify_token
from .handlers import (
    get_all_orders_handler,
    get_order_by_id_handler,
    create_order_handler,
    update_order_handler,
    delete_order_handler,
    update_order_status_handler
)

# Create router for order endpoints
router = APIRouter(prefix="/api/v1/orders", tags=["orders"])


@router.get("", response_model=OrdersListResponse)
async def get_all_orders(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all orders"""
    return await get_all_orders_handler(db_session)


@router.get("/{id}", response_model=OrderResponse)
async def get_order_by_id(
    id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get order by ID"""
    return await get_order_by_id_handler(id, db_session)


@router.post("", response_model=OrderResponse)
async def create_order(
    order_data: CreateOrderDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new order"""
    return await create_order_handler(order_data, db_session)


@router.put("/{id}", response_model=OrderResponse)
async def update_order(
    id: int,
    order_data: UpdateOrderDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update an order"""
    return await update_order_handler(id, order_data, db_session)


@router.delete("/{id}", response_model=SuccessResponse)
async def delete_order(
    id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete an order"""
    await delete_order_handler(id, db_session)


@router.put("/{id}/status", response_model=OrderResponse)
async def update_order_status(
    id: int,
    status_update: dict,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update order status"""
    return await update_order_status_handler(id, status_update.get("status"), db_session)
