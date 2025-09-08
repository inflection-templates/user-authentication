"""
Product API handlers
Contains the business logic for product operations
"""

from typing import List, Optional
from fastapi import HTTPException
from models.dtos import ProductDto, CreateProductDto, UpdateProductDto, ProductResponse, ProductsListResponse, SuccessResponse
from services.product_service import ProductService


async def get_all_products_handler(db_session) -> ProductsListResponse:
    """Handler for getting all products"""
    service = ProductService(db_session)
    products = await service.get_all_products()
    return ProductsListResponse(
        success=True,
        message="Products retrieved successfully",
        httpcode=200,
        products=products
    )


async def get_product_by_id_handler(product_id: int, db_session) -> ProductResponse:
    """Handler for getting a product by ID"""
    service = ProductService(db_session)
    product = await service.get_product_by_id(product_id)
    if not product:
        raise HTTPException(status_code=404, detail="Product not found")
    return ProductResponse(
        success=True,
        message="Product retrieved successfully",
        httpcode=200,
        product=product
    )


async def create_product_handler(product_data: CreateProductDto, db_session) -> ProductResponse:
    """Handler for creating a new product"""
    try:
        service = ProductService(db_session)
        product = await service.create_product(product_data)
        return ProductResponse(
            success=True,
            message="Product created successfully",
            httpcode=201,
            product=product
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_product_handler(product_id: int, product_data: UpdateProductDto, db_session) -> ProductResponse:
    """Handler for updating a product"""
    try:
        service = ProductService(db_session)
        product = await service.update_product(product_id, product_data)
        if not product:
            raise HTTPException(status_code=404, detail="Product not found")
        return ProductResponse(
            success=True,
            message="Product updated successfully",
            httpcode=200,
            product=product
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_product_handler(product_id: int, db_session) -> SuccessResponse:
    """Handler for deleting a product"""
    try:
        service = ProductService(db_session)
        success = await service.delete_product(product_id)
        if not success:
            raise HTTPException(status_code=404, detail="Product not found")
        return SuccessResponse(
            success=True,
            message="Product deleted successfully",
            httpcode=204
        )
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))
