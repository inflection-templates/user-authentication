"""
Customer API routes
Defines the FastAPI route endpoints for customer operations
"""

from typing import List
from fastapi import APIRouter, Depends
from models.dtos import CustomerDto, CreateCustomerDto, UpdateCustomerDto
from .handlers import (
    get_all_customers_handler,
    get_customer_by_id_handler,
    create_customer_handler,
    update_customer_handler,
    delete_customer_handler
)

# Create router for customer endpoints
router = APIRouter(prefix="/customers", tags=["customers"])


@router.get("", response_model=List[CustomerDto])
async def get_all_customers(
    current_user=None,  # Will be replaced with Depends(verify_token) in main app
    db_session=None  # Will be replaced with Depends(get_db_session) in main app
):
    """Get all customers"""
    return await get_all_customers_handler(db_session)


@router.get("/{customer_id}", response_model=CustomerDto)
async def get_customer_by_id(
    customer_id: int,
    current_user=None,  # Will be replaced with Depends(verify_token) in main app
    db_session=None  # Will be replaced with Depends(get_db_session) in main app
):
    """Get customer by ID"""
    return await get_customer_by_id_handler(customer_id, db_session)


@router.post("", response_model=CustomerDto, status_code=201)
async def create_customer(
    customer_data: CreateCustomerDto,
    current_user=None,  # Will be replaced with Depends(verify_token) in main app
    db_session=None  # Will be replaced with Depends(get_db_session) in main app
):
    """Create a new customer"""
    return await create_customer_handler(customer_data, db_session)


@router.put("/{customer_id}", response_model=CustomerDto)
async def update_customer(
    customer_id: int,
    customer_data: UpdateCustomerDto,
    current_user=None,  # Will be replaced with Depends(verify_token) in main app
    db_session=None  # Will be replaced with Depends(get_db_session) in main app
):
    """Update a customer"""
    return await update_customer_handler(customer_id, customer_data, db_session)


@router.delete("/{customer_id}", status_code=204)
async def delete_customer(
    customer_id: int,
    current_user=None,  # Will be replaced with Depends(verify_token) in main app
    db_session=None  # Will be replaced with Depends(get_db_session) in main app
):
    """Delete a customer"""
    await delete_customer_handler(customer_id, db_session)
