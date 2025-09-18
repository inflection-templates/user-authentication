"""
Customer service implementation
Equivalent to the .NET OrderService.Services.CustomerService
"""

import logging
from datetime import datetime
from typing import List, Optional
from uuid import UUID
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, and_
from sqlalchemy.orm import selectinload

from models.entities import Customer, Order
from models.dtos import CustomerDto, CreateCustomerDto, UpdateCustomerDto

logger = logging.getLogger(__name__)


class CustomerService:
    """Customer service for managing customer operations"""
    
    def __init__(self, db_session: AsyncSession):
        self.db_session = db_session
    
    async def get_all_customers(self) -> List[CustomerDto]:
        """Get all customers ordered by last name, then first name"""
        result = await self.db_session.execute(
            select(Customer).order_by(Customer.last_name, Customer.first_name)
        )
        customers = result.scalars().all()
        return [self._map_to_dto(customer) for customer in customers]
    
    async def get_customer_by_id(self, customer_id: UUID) -> Optional[CustomerDto]:
        """Get customer by ID"""
        result = await self.db_session.execute(
            select(Customer).where(Customer.id == customer_id)
        )
        customer = result.scalar_one_or_none()
        return self._map_to_dto(customer) if customer else None
    
    async def create_customer(self, create_dto: CreateCustomerDto) -> CustomerDto:
        """Create a new customer"""
        # Check if customer with email already exists
        existing_result = await self.db_session.execute(
            select(Customer).where(Customer.email == create_dto.email)
        )
        existing_customer = existing_result.scalar_one_or_none()
        
        if existing_customer:
            raise ValueError("Customer with this email already exists")
        
        # Create new customer
        customer = Customer(
            first_name=create_dto.first_name,
            last_name=create_dto.last_name,
            email=create_dto.email,
            phone=create_dto.phone,
            address=create_dto.address,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        self.db_session.add(customer)
        await self.db_session.commit()
        await self.db_session.refresh(customer)
        
        logger.info(f"Customer created with ID: {customer.id}, Email: {customer.email}")
        return self._map_to_dto(customer)
    
    async def update_customer(self, customer_id: UUID, update_dto: UpdateCustomerDto) -> Optional[CustomerDto]:
        """Update an existing customer"""
        # Get existing customer
        result = await self.db_session.execute(
            select(Customer).where(Customer.id == customer_id)
        )
        customer = result.scalar_one_or_none()
        
        if not customer:
            return None
        
        # Check if email is being changed and if new email already exists
        if customer.email != update_dto.email:
            existing_result = await self.db_session.execute(
                select(Customer).where(
                    and_(Customer.email == update_dto.email, Customer.id != customer_id)
                )
            )
            existing_customer = existing_result.scalar_one_or_none()
            
            if existing_customer:
                raise ValueError("Customer with this email already exists")
        
        # Update customer
        customer.first_name = update_dto.first_name
        customer.last_name = update_dto.last_name
        customer.email = update_dto.email
        customer.phone = update_dto.phone
        customer.address = update_dto.address
        customer.updated_at = datetime.utcnow()
        
        await self.db_session.commit()
        await self.db_session.refresh(customer)
        
        logger.info(f"Customer updated with ID: {customer_id}")
        return self._map_to_dto(customer)
    
    async def delete_customer(self, customer_id: UUID) -> bool:
        """Delete a customer"""
        # Get customer
        result = await self.db_session.execute(
            select(Customer).where(Customer.id == customer_id)
        )
        customer = result.scalar_one_or_none()
        
        if not customer:
            return False
        
        # Check if customer has orders
        orders_result = await self.db_session.execute(
            select(Order).where(Order.customer_id == customer_id)
        )
        has_orders = orders_result.first() is not None
        
        if has_orders:
            raise RuntimeError("Cannot delete customer with existing orders")
        
        # Delete customer
        await self.db_session.delete(customer)
        await self.db_session.commit()
        
        logger.info(f"Customer deleted with ID: {customer_id}")
        return True
    
    async def get_customer_by_email(self, email: str) -> Optional[CustomerDto]:
        """Get customer by email"""
        result = await self.db_session.execute(
            select(Customer).where(Customer.email == email)
        )
        customer = result.scalar_one_or_none()
        return self._map_to_dto(customer) if customer else None
    
    def _map_to_dto(self, customer: Customer) -> CustomerDto:
        """Map Customer entity to CustomerDto"""
        return CustomerDto(
            id=customer.id,
            first_name=customer.first_name,
            last_name=customer.last_name,
            email=customer.email,
            phone=customer.phone,
            address=customer.address,
            created_at=customer.created_at,
            updated_at=customer.updated_at
        )
