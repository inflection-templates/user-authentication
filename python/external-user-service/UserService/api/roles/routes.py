"""
Role management routes
Equivalent to the .NET shala.api.api.roles.RoleRoutes
"""

from typing import Optional
from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from database.db_context import get_db_session
from .handlers import RoleHandler

roles_router = APIRouter()

# Dependencies
def get_role_handler(db_session: AsyncSession = Depends(get_db_session)) -> RoleHandler:
    return RoleHandler(db_session)

@roles_router.get("/")
async def get_roles(
    tenant_id: Optional[str] = None,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Get all roles"""
    return await handler.get_roles(tenant_id)

@roles_router.post("/")
async def create_role(
    role_data: dict,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Create new role"""
    return await handler.create_role(role_data)

@roles_router.get("/{role_id}")
async def get_role_by_id(
    role_id: str,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Get role by ID"""
    return await handler.get_role_by_id(role_id)

@roles_router.put("/{role_id}")
async def update_role(
    role_id: str,
    role_data: dict,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Update existing role"""
    return await handler.update_role(role_id, role_data)

@roles_router.delete("/{role_id}")
async def delete_role(
    role_id: str,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Delete role"""
    return await handler.delete_role(role_id)

@roles_router.post("/users/{user_id}/roles/{role_id}")
async def assign_role_to_user(
    user_id: str,
    role_id: str,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Assign role to user"""
    return await handler.assign_role_to_user(user_id, role_id)

@roles_router.delete("/users/{user_id}/roles/{role_id}")
async def remove_role_from_user(
    user_id: str,
    role_id: str,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Remove role from user"""
    return await handler.remove_role_from_user(user_id, role_id)

@roles_router.get("/users/{user_id}")
async def get_user_roles(
    user_id: str,
    handler: RoleHandler = Depends(get_role_handler)
):
    """Get all roles assigned to a user"""
    return await handler.get_user_roles(user_id)