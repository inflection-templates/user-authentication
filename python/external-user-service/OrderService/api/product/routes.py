"""
Product API routes
Defines the FastAPI route endpoints for product operations
"""

from typing import List
from fastapi import APIRouter, Depends
from models.dtos import ProductDto, CreateProductDto, UpdateProductDto, ProductResponse, ProductsListResponse, SuccessResponse
from database.db_context import get_db_session
from auth.dependencies import verify_token
from .handlers import (
    get_all_products_handler,
    get_product_by_id_handler,
    create_product_handler,
    update_product_handler,
    delete_product_handler
)

# Create router for product endpoints
router = APIRouter(prefix="/api/v1/products", tags=["products"])


@router.get("", response_model=ProductsListResponse)
async def get_all_products(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all products"""
    return await get_all_products_handler(db_session)


@router.get("/{id}", response_model=ProductResponse)
async def get_product_by_id(
    id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get product by ID"""
    return await get_product_by_id_handler(id, db_session)


@router.post("", response_model=ProductResponse)
async def create_product(
    product_data: CreateProductDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new product"""
    return await create_product_handler(product_data, db_session)


@router.put("/{id}", response_model=ProductResponse)
async def update_product(
    id: int,
    product_data: UpdateProductDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update a product"""
    return await update_product_handler(id, product_data, db_session)


@router.delete("/{id}", response_model=SuccessResponse)
async def delete_product(
    id: int,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete a product"""
    await delete_product_handler(id, db_session)
