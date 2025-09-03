"""
Data Transfer Objects (DTOs) for Order Service
Equivalent to the .NET OrderService.Models.DTOs
"""

from datetime import datetime
from decimal import Decimal
from typing import List, Optional
from pydantic import BaseModel, EmailStr, Field, validator


# Customer DTOs
class CustomerDto(BaseModel):
    """Customer data transfer object"""
    id: int
    first_name: str
    last_name: str
    email: str
    phone: Optional[str] = None
    address: Optional[str] = None
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class CreateCustomerDto(BaseModel):
    """Create customer data transfer object"""
    first_name: str = Field(..., min_length=1, max_length=100, alias="firstName")
    last_name: str = Field(..., min_length=1, max_length=100, alias="lastName")
    email: EmailStr = Field(..., max_length=255)
    phone: Optional[str] = Field(None, max_length=20)
    address: Optional[str] = Field(None, max_length=500)
    
    model_config = {"populate_by_name": True}


class UpdateCustomerDto(BaseModel):
    """Update customer data transfer object"""
    first_name: str = Field(..., min_length=1, max_length=100, alias="firstName")
    last_name: str = Field(..., min_length=1, max_length=100, alias="lastName")
    email: EmailStr = Field(..., max_length=255)
    phone: Optional[str] = Field(None, max_length=20)
    address: Optional[str] = Field(None, max_length=500)
    
    model_config = {"populate_by_name": True}


# Product DTOs
class ProductDto(BaseModel):
    """Product data transfer object"""
    id: int
    name: str
    description: Optional[str] = None
    price: Decimal
    sku: Optional[str] = None
    stock_quantity: int
    category: Optional[str] = None
    is_active: bool
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True


class CreateProductDto(BaseModel):
    """Create product data transfer object"""
    name: str = Field(..., min_length=1, max_length=200)
    description: Optional[str] = Field(None, max_length=1000)
    price: Decimal = Field(..., gt=0)
    sku: Optional[str] = Field(None, max_length=50)
    stock_quantity: int = Field(default=0, ge=0)
    category: Optional[str] = Field(None, max_length=100)


class UpdateProductDto(BaseModel):
    """Update product data transfer object"""
    name: str = Field(..., min_length=1, max_length=200)
    description: Optional[str] = Field(None, max_length=1000)
    price: Decimal = Field(..., gt=0)
    sku: Optional[str] = Field(None, max_length=50)
    stock_quantity: int = Field(..., ge=0)
    category: Optional[str] = Field(None, max_length=100)
    is_active: bool = True


# OrderItem DTOs
class OrderItemDto(BaseModel):
    """Order item data transfer object"""
    id: int
    order_id: int
    product_id: int
    quantity: int
    unit_price: Decimal
    total_price: Decimal
    created_at: datetime
    updated_at: datetime
    product: ProductDto

    class Config:
        from_attributes = True


class CreateOrderItemDto(BaseModel):
    """Create order item data transfer object"""
    product_id: int = Field(..., gt=0)
    quantity: int = Field(..., ge=1)


# Order DTOs
class OrderDto(BaseModel):
    """Order data transfer object"""
    id: int
    customer_id: int
    order_number: str
    order_date: datetime
    status: str
    sub_total: Decimal
    tax_amount: Decimal
    shipping_amount: Decimal
    total_amount: Decimal
    notes: Optional[str] = None
    shipping_address: Optional[str] = None
    created_at: datetime
    updated_at: datetime
    customer: CustomerDto
    order_items: List[OrderItemDto] = []

    class Config:
        from_attributes = True


class CreateOrderDto(BaseModel):
    """Create order data transfer object"""
    customer_id: int = Field(..., gt=0)
    notes: Optional[str] = Field(None, max_length=500)
    shipping_address: Optional[str] = Field(None, max_length=200)
    order_items: List[CreateOrderItemDto] = Field(..., min_items=1)


class UpdateOrderDto(BaseModel):
    """Update order data transfer object"""
    status: str = Field(..., min_length=1, max_length=50)
    notes: Optional[str] = Field(None, max_length=500)
    shipping_address: Optional[str] = Field(None, max_length=200)

    @validator('status')
    def validate_status(cls, v):
        valid_statuses = ["Pending", "Processing", "Shipped", "Delivered", "Cancelled"]
        if v not in valid_statuses:
            raise ValueError(f'Status must be one of: {", ".join(valid_statuses)}')
        return v
