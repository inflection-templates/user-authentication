"""
Product API handlers
Contains the business logic for product operations
"""

from typing import List, Optional
from fastapi import HTTPException
from models.dtos import ProductDto, CreateProductDto, UpdateProductDto
from services.product_service import ProductService


async def get_all_products_handler(db_session) -> List[ProductDto]:
    """Handler for getting all products"""
    service = ProductService(db_session)
    return await service.get_all_products()


async def get_product_by_id_handler(product_id: int, db_session) -> Optional[ProductDto]:
    """Handler for getting a product by ID"""
    service = ProductService(db_session)
    product = await service.get_product_by_id(product_id)
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    return product


async def create_product_handler(product_data: CreateProductDto, db_session) -> ProductDto:
    """Handler for creating a new product"""
    try:
        service = ProductService(db_session)
        return await service.create_product(product_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_product_handler(product_id: int, product_data: UpdateProductDto, db_session) -> Optional[ProductDto]:
    """Handler for updating a product"""
    try:
        service = ProductService(db_session)
        product = await service.update_product(product_id, product_data)
        if not product:
            raise HTTPException(status_code=404, detail="Product not found")
        return product
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_product_handler(product_id: int, db_session) -> bool:
    """Handler for deleting a product"""
    try:
        service = ProductService(db_session)
        success = await service.delete_product(product_id)
        if not success:
            raise HTTPException(status_code=404, detail="Product not found")
        return success
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))
