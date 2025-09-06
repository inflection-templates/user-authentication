"""
Customer API handlers
Contains the business logic for customer endpoints
"""

from typing import List
from fastapi import HTTPException, Depends
from database.db_context import get_db_session
from models.dtos import CustomerDto, CreateCustomerDto, UpdateCustomerDto
from services.customer_service import CustomerService


async def get_all_customers_handler(
    db_session=Depends(get_db_session)
) -> List[CustomerDto]:
    """Get all customers"""
    service = CustomerService(db_session)
    return await service.get_all_customers()


async def get_customer_by_id_handler(
    customer_id: int,
    db_session=Depends(get_db_session)
) -> CustomerDto:
    """Get customer by ID"""
    service = CustomerService(db_session)
    customer = await service.get_customer_by_id(customer_id)
    if not customer:
        raise HTTPException(status_code=404, detail="Customer not found")
    return customer


async def create_customer_handler(
    customer_data: CreateCustomerDto,
    db_session=Depends(get_db_session)
) -> CustomerDto:
    """Create a new customer"""
    try:
        service = CustomerService(db_session)
        return await service.create_customer(customer_data)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_customer_handler(
    customer_id: int,
    customer_data: UpdateCustomerDto,
    db_session=Depends(get_db_session)
) -> CustomerDto:
    """Update a customer"""
    try:
        service = CustomerService(db_session)
        customer = await service.update_customer(customer_id, customer_data)
        if not customer:
            raise HTTPException(status_code=404, detail="Customer not found")
        return customer
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_customer_handler(
    customer_id: int,
    db_session=Depends(get_db_session)
) -> None:
    """Delete a customer"""
    try:
        service = CustomerService(db_session)
        success = await service.delete_customer(customer_id)
        if not success:
            raise HTTPException(status_code=404, detail="Customer not found")
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))
