"""
User routes.
Similar to the Node.js user.routes.ts implementation.
"""
from fastapi import APIRouter, Request
from api.user.user_controller import UserController

router = APIRouter(prefix="/users", tags=["users"])
controller = UserController()


@router.post("")
async def create_user(request: Request):
    """
    Create a new user.
    
    @route   POST /api/v1/users
    @desc    Create a new user
    @access  Public or System Admin
    """
    return await controller.create_user(request)


@router.get("/profile")
async def get_profile(request: Request):
    """
    Get current user's profile.
    
    @route   GET /api/v1/users/profile
    @desc    Get current user's profile
    @access  Authenticated users
    """
    return await controller.get_profile(request)


@router.put("/profile")
async def update_profile(request: Request):
    """
    Update current user's profile.
    
    @route   PUT /api/v1/users/profile
    @desc    Update current user's profile
    @access  Authenticated users
    """
    return await controller.update_profile(request)


@router.get("/search")
async def search_users(request: Request):
    """
    Search users.
    
    @route   GET /api/v1/users/search
    @desc    Search users
    @access  System Admin, System User
    """
    return await controller.search_users(request)


@router.get("/{user_id}")
async def get_user_by_id(request: Request, user_id: str):
    """
    Get user by ID.
    
    @route   GET /api/v1/users/:id
    @desc    Get user by ID
    @access  System Admin, System User, or the user themselves
    """
    return await controller.get_user_by_id(request, user_id)


@router.put("/{user_id}")
async def update_user_by_id(request: Request, user_id: str):
    """
    Update user by ID.
    
    @route   PUT /api/v1/users/:id
    @desc    Update user by ID
    @access  System Admin or the user themselves
    """
    return await controller.update_user_by_id(request, user_id)


@router.delete("/{user_id}")
async def delete_user_by_id(request: Request, user_id: str):
    """
    Delete user by ID.
    
    @route   DELETE /api/v1/users/:id
    @desc    Delete user by ID
    @access  System Admin only
    """
    return await controller.delete_user_by_id(request, user_id)

