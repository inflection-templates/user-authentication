"""
Database entities for Order Service
Equivalent to the .NET OrderService.Models.Entities
"""

from datetime import datetime
from typing import List, Optional
from sqlalchemy import Column, Integer, String, DateTime, Boolean, ForeignKey, Text
from sqlalchemy.types import DECIMAL
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship

Base = declarative_base()


class Customer(Base):
    """Customer entity"""
    __tablename__ = "customers"
    
    id = Column(Integer, primary_key=True, index=True)
    first_name = Column(String(100), nullable=False)
    last_name = Column(String(100), nullable=False)
    email = Column(String(255), nullable=False, unique=True, index=True)
    phone = Column(String(20), nullable=True)
    address = Column(String(500), nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    orders = relationship("Order", back_populates="customer", cascade="all, delete-orphan")


class Product(Base):
    """Product entity"""
    __tablename__ = "products"
    
    id = Column(Integer, primary_key=True, index=True)
    name = Column(String(200), nullable=False)
    description = Column(String(1000), nullable=True)
    price = Column(DECIMAL(18, 2), nullable=False)
    sku = Column(String(50), nullable=True, unique=True, index=True)
    stock_quantity = Column(Integer, default=0, nullable=False)
    category = Column(String(100), nullable=True)
    is_active = Column(Boolean, default=True, nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    order_items = relationship("OrderItem", back_populates="product")


class Order(Base):
    """Order entity"""
    __tablename__ = "orders"
    
    id = Column(Integer, primary_key=True, index=True)
    customer_id = Column(Integer, ForeignKey("customers.id"), nullable=False)
    order_number = Column(String(50), nullable=False, unique=True, index=True)
    order_date = Column(DateTime, default=datetime.utcnow, nullable=False)
    status = Column(String(50), default="Pending", nullable=False)  # Pending, Processing, Shipped, Delivered, Cancelled
    sub_total = Column(DECIMAL(18, 2), default=0, nullable=False)
    tax_amount = Column(DECIMAL(18, 2), default=0, nullable=False)
    shipping_amount = Column(DECIMAL(18, 2), default=0, nullable=False)
    total_amount = Column(DECIMAL(18, 2), default=0, nullable=False)
    notes = Column(String(500), nullable=True)
    shipping_address = Column(String(200), nullable=True)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    customer = relationship("Customer", back_populates="orders")
    order_items = relationship("OrderItem", back_populates="order", cascade="all, delete-orphan")


class OrderItem(Base):
    """Order item entity"""
    __tablename__ = "order_items"
    
    id = Column(Integer, primary_key=True, index=True)
    order_id = Column(Integer, ForeignKey("orders.id"), nullable=False)
    product_id = Column(Integer, ForeignKey("products.id"), nullable=False)
    quantity = Column(Integer, nullable=False)
    unit_price = Column(DECIMAL(18, 2), nullable=False)
    total_price = Column(DECIMAL(18, 2), nullable=False)
    created_at = Column(DateTime, default=datetime.utcnow, nullable=False)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow, nullable=False)
    
    # Relationships
    order = relationship("Order", back_populates="order_items")
    product = relationship("Product", back_populates="order_items")
