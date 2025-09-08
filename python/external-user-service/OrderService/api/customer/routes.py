"""
Customer API routes
Defines the FastAPI route endpoints for customer operations
"""

from typing import List
from uuid import UUID
from fastapi import APIRouter, Depends
from models.dtos import (
    CustomerDto, CreateCustomerDto, UpdateCustomerDto,
    CustomerResponse, CustomersListResponse, SuccessResponse
)
from database.db_context import get_db_session
from auth.dependencies import verify_token
from .handlers import (
    get_all_customers_handler,
    get_customer_by_id_handler,
    create_customer_handler,
    update_customer_handler,
    delete_customer_handler
)

# Create router for customer endpoints
router = APIRouter(prefix="/api/v1/customers", tags=["customers"])


@router.get("", response_model=CustomersListResponse)
async def get_all_customers(
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get all customers"""
    return await get_all_customers_handler(db_session)


@router.get("/{id}", response_model=CustomerResponse)
async def get_customer_by_id(
    id: UUID,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Get customer by ID"""
    return await get_customer_by_id_handler(id, db_session)


@router.post("", response_model=CustomerResponse, status_code=201)
async def create_customer(
    customer_data: CreateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Create a new customer"""
    return await create_customer_handler(customer_data, db_session)


@router.put("/{id}", response_model=CustomerResponse)
async def update_customer(
    id: UUID,
    customer_data: UpdateCustomerDto,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Update a customer"""
    return await update_customer_handler(id, customer_data, db_session)


@router.delete("/{id}", response_model=SuccessResponse, status_code=200)
async def delete_customer(
    id: UUID,
    current_user=Depends(verify_token),
    db_session=Depends(get_db_session)
):
    """Delete a customer"""
    return await delete_customer_handler(id, db_session)
