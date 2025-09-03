"""
Product service implementation
Equivalent to the .NET OrderService.Services.ProductService
"""

import logging
from datetime import datetime
from typing import List, Optional
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, and_
from sqlalchemy.orm import selectinload

from models.entities import Product, OrderItem
from models.dtos import ProductDto, CreateProductDto, UpdateProductDto

logger = logging.getLogger(__name__)


class ProductService:
    """Product service for managing product operations"""
    
    def __init__(self, db_session: AsyncSession):
        self.db_session = db_session
    
    async def get_all_products(self) -> List[ProductDto]:
        """Get all products ordered by name"""
        result = await self.db_session.execute(
            select(Product).order_by(Product.name)
        )
        products = result.scalars().all()
        return [self._map_to_dto(product) for product in products]
    
    async def get_product_by_id(self, product_id: int) -> Optional[ProductDto]:
        """Get product by ID"""
        result = await self.db_session.execute(
            select(Product).where(Product.id == product_id)
        )
        product = result.scalar_one_or_none()
        return self._map_to_dto(product) if product else None
    
    async def create_product(self, create_dto: CreateProductDto) -> ProductDto:
        """Create a new product"""
        # Check if product with SKU already exists (if SKU is provided)
        if create_dto.sku:
            existing_result = await self.db_session.execute(
                select(Product).where(Product.sku == create_dto.sku)
            )
            existing_product = existing_result.scalar_one_or_none()
            
            if existing_product:
                raise ValueError("Product with this SKU already exists")
        
        # Create new product
        product = Product(
            name=create_dto.name,
            description=create_dto.description,
            price=create_dto.price,
            sku=create_dto.sku,
            stock_quantity=create_dto.stock_quantity,
            category=create_dto.category,
            is_active=True,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        self.db_session.add(product)
        await self.db_session.commit()
        await self.db_session.refresh(product)
        
        logger.info(f"Product created with ID: {product.id}, Name: {product.name}")
        return self._map_to_dto(product)
    
    async def update_product(self, product_id: int, update_dto: UpdateProductDto) -> Optional[ProductDto]:
        """Update an existing product"""
        # Get existing product
        result = await self.db_session.execute(
            select(Product).where(Product.id == product_id)
        )
        product = result.scalar_one_or_none()
        
        if not product:
            return None
        
        # Check if SKU is being changed and if new SKU already exists
        if update_dto.sku and product.sku != update_dto.sku:
            existing_result = await self.db_session.execute(
                select(Product).where(
                    and_(Product.sku == update_dto.sku, Product.id != product_id)
                )
            )
            existing_product = existing_result.scalar_one_or_none()
            
            if existing_product:
                raise ValueError("Product with this SKU already exists")
        
        # Update product
        product.name = update_dto.name
        product.description = update_dto.description
        product.price = update_dto.price
        product.sku = update_dto.sku
        product.stock_quantity = update_dto.stock_quantity
        product.category = update_dto.category
        product.is_active = update_dto.is_active
        product.updated_at = datetime.utcnow()
        
        await self.db_session.commit()
        await self.db_session.refresh(product)
        
        logger.info(f"Product updated with ID: {product_id}")
        return self._map_to_dto(product)
    
    async def delete_product(self, product_id: int) -> bool:
        """Delete a product"""
        # Get product
        result = await self.db_session.execute(
            select(Product).where(Product.id == product_id)
        )
        product = result.scalar_one_or_none()
        
        if not product:
            return False
        
        # Check if product has order items
        order_items_result = await self.db_session.execute(
            select(OrderItem).where(OrderItem.product_id == product_id)
        )
        has_order_items = order_items_result.first() is not None
        
        if has_order_items:
            raise RuntimeError("Cannot delete product with existing order items")
        
        # Delete product
        await self.db_session.delete(product)
        await self.db_session.commit()
        
        logger.info(f"Product deleted with ID: {product_id}")
        return True
    
    async def get_product_by_sku(self, sku: str) -> Optional[ProductDto]:
        """Get product by SKU"""
        result = await self.db_session.execute(
            select(Product).where(Product.sku == sku)
        )
        product = result.scalar_one_or_none()
        return self._map_to_dto(product) if product else None
    
    async def get_products_by_category(self, category: str) -> List[ProductDto]:
        """Get products by category"""
        result = await self.db_session.execute(
            select(Product).where(Product.category == category).order_by(Product.name)
        )
        products = result.scalars().all()
        return [self._map_to_dto(product) for product in products]
    
    async def get_active_products(self) -> List[ProductDto]:
        """Get only active products"""
        result = await self.db_session.execute(
            select(Product).where(Product.is_active == True).order_by(Product.name)
        )
        products = result.scalars().all()
        return [self._map_to_dto(product) for product in products]
    
    def _map_to_dto(self, product: Product) -> ProductDto:
        """Map Product entity to ProductDto"""
        return ProductDto(
            id=product.id,
            name=product.name,
            description=product.description,
            price=product.price,
            sku=product.sku,
            stock_quantity=product.stock_quantity,
            category=product.category,
            is_active=product.is_active,
            created_at=product.created_at,
            updated_at=product.updated_at
        )
