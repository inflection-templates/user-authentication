"""
Customer API handlers
Contains the business logic for customer endpoints
"""

from typing import List
from uuid import UUID
from fastapi import HTTPException, Depends
from database.db_context import get_db_session
from models.dtos import (
    CustomerDto, CreateCustomerDto, UpdateCustomerDto,
    CustomerResponse, CustomersListResponse, SuccessResponse
)
from services.customer_service import CustomerService


async def get_all_customers_handler(
    db_session=Depends(get_db_session)
) -> CustomersListResponse:
    """Get all customers"""
    service = CustomerService(db_session)
    customers = await service.get_all_customers()
    return CustomersListResponse(
        success=True,
        message="Customers retrieved successfully",
        httpcode=200,
        customers=customers
    )


async def get_customer_by_id_handler(
    customer_id: UUID,
    db_session=Depends(get_db_session)
) -> CustomerResponse:
    """Get customer by ID"""
    service = CustomerService(db_session)
    customer = await service.get_customer_by_id(customer_id)
    if not customer:
        raise HTTPException(status_code=404, detail="Customer not found")
    return CustomerResponse(
        success=True,
        message="Customer retrieved successfully",
        httpcode=200,
        customer=customer
    )


async def create_customer_handler(
    customer_data: CreateCustomerDto,
    db_session=Depends(get_db_session)
) -> CustomerResponse:
    """Create a new customer"""
    try:
        service = CustomerService(db_session)
        customer = await service.create_customer(customer_data)
        return CustomerResponse(
            success=True,
            message="Customer created successfully",
            httpcode=201,
            customer=customer
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def update_customer_handler(
    customer_id: UUID,
    customer_data: UpdateCustomerDto,
    db_session=Depends(get_db_session)
) -> CustomerResponse:
    """Update a customer"""
    try:
        service = CustomerService(db_session)
        customer = await service.update_customer(customer_id, customer_data)
        if not customer:
            raise HTTPException(status_code=404, detail="Customer not found")
        return CustomerResponse(
            success=True,
            message="Customer updated successfully",
            httpcode=200,
            customer=customer
        )
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


async def delete_customer_handler(
    customer_id: UUID,
    db_session=Depends(get_db_session)
) -> SuccessResponse:
    """Delete a customer"""
    try:
        service = CustomerService(db_session)
        success = await service.delete_customer(customer_id)
        if not success:
            raise HTTPException(status_code=404, detail="Customer not found")
        return SuccessResponse(
            success=True,
            message="Customer deleted successfully",
            httpcode=204
        )
    except RuntimeError as e:
        raise HTTPException(status_code=400, detail=str(e))
