"""
Startup seeder that runs on application startup.
Similar to the dotnet Startup/Seeder.cs implementation.
"""
import json
import os
import uuid
from pathlib import Path
from typing import Optional, Dict, List, Any
from datetime import datetime

from config.config import Config
from database.database_connector import DatabaseConnector
from database.models.user import User
from database.models.tenant import Tenant
from database.models.role import Role
from database.models.client_app import ClientApp
from startup.platform_info_handler import PlatformInfoHandler
import bcrypt


def _get_seed_data_folder_path() -> str:
    """
    Get the path to the seed data folder.
    
    Returns:
        Path to static.content/seed.data folder
    """
    # Try to get path from current working directory or executable location
    cwd = os.getcwd()
    
    # Check if we're in python_service directory
    static_content_path = os.path.join(cwd, 'static.content', 'seed.data')
    if os.path.exists(static_content_path):
        return static_content_path
    
    # Check parent directory
    parent_static_content = os.path.join(os.path.dirname(cwd), 'static.content', 'seed.data')
    if os.path.exists(parent_static_content):
        return parent_static_content
    
    # Try relative to this file
    current_file = Path(__file__)
    project_root = current_file.parent.parent
    static_path = project_root / 'static.content' / 'seed.data'
    if static_path.exists():
        return str(static_path)
    
    return os.path.join(cwd, 'static.content', 'seed.data')


class Seeder:
    """Seeder class for startup seeding."""
    
    @staticmethod
    def seed(is_dev_environment: bool = False, is_test_environment: bool = False):
        """
        Run seeding synchronously.
        
        Args:
            is_dev_environment: Whether running in development
            is_test_environment: Whether running in test environment
        """
        # For async compatibility, we'll run synchronously
        Seeder.seed_async(is_dev_environment, is_test_environment)
    
    @staticmethod
    def seed_async(is_dev_environment: bool = False, is_test_environment: bool = False):
        """
        Run seeding asynchronously.
        Similar to dotnet SeedAsync method.
        
        Args:
            is_dev_environment: Whether running in development
            is_test_environment: Whether running in test environment
        """
        platform_info = PlatformInfoHandler.get_platform_info()
        if platform_info:
            print(f'Extracted Platform Info: {platform_info.platform}')
        
        session = DatabaseConnector.get_session()
        try:
            tenant = Seeder._seed_default_tenant(session)
            admin_user = Seeder._create_system_admin(session, tenant.id if tenant else None)
            Seeder._add_system_roles(session, tenant.id if tenant else None, admin_user.id if admin_user else None)
            Seeder._seed_default_client_apps(session, is_dev_environment, is_test_environment)
        finally:
            session.close()
    
    @staticmethod
    def _add_system_roles(session, tenant_id: Optional[str], admin_user_id: Optional[str]):
        """
        Add system roles to admin user.
        Similar to dotnet AddSystemRoles method.
        
        Args:
            session: Database session
            tenant_id: Optional tenant ID
            admin_user_id: Optional admin user ID
        """
        try:
            print('Seeding system roles...')
            
            # Default system roles: User, Organization, Tenant
            system_roles = ['User', 'Organization', 'Tenant']
            
            # Get admin user to assign roles to
            if admin_user_id:
                admin_user = session.query(User).filter(User.id == admin_user_id).first()
            else:
                admin_user = session.query(User).filter(User.username == 'admin').first()
            
            if admin_user is None:
                print('Warning: Admin user not found. Cannot seed roles without a user.')
                return
            
            roles_to_add = []
            
            for role_name in system_roles:
                # Check if this specific role already exists for the admin user
                existing_role = session.query(Role).filter(
                    Role.role_name == role_name,
                    Role.user_id == admin_user.id
                ).first()
                
                if existing_role is None:
                    role = Role(
                        id=uuid.uuid4(),
                        user_id=admin_user.id,
                        role_name=role_name,
                        created_at=datetime.utcnow(),
                        updated_at=datetime.utcnow()
                    )
                    roles_to_add.append(role)
                    print(f'Adding role: {role_name}')
                else:
                    print(f'Role {role_name} already exists for admin user')
            
            if roles_to_add:
                session.add_all(roles_to_add)
                session.commit()
                print(f'Successfully seeded {len(roles_to_add)} system roles')
            else:
                print('All system roles already exist')
        except Exception as ex:
            session.rollback()
            print(f'Error seeding system roles: {str(ex)}')
    
    @staticmethod
    def _create_system_admin(session, tenant_id: Optional[str]) -> Optional[User]:
        """
        Create system admin user.
        Similar to dotnet CreateSystemAdmin method.
        
        Args:
            session: Database session
            tenant_id: Optional tenant ID
            
        Returns:
            Created or existing admin User
        """
        try:
            print('Seeding System admin...')
            
            # Ensure we have a tenant ID
            if tenant_id is None:
                print('Tenant ID is null or empty. Attempting to retrieve default tenant.')
                default_tenant = session.query(Tenant).filter(Tenant.code == 'default').first()
                if default_tenant and default_tenant.id:
                    tenant_id = default_tenant.id
                    print(f'Retrieved default tenant ID: {tenant_id}')
                else:
                    print('Could not retrieve default tenant. User will be created without tenant ID.')
            
            base_path = _get_seed_data_folder_path()
            admin_seed_path = os.path.join(base_path, 'system.admin.seed.json')
            
            if not os.path.exists(admin_seed_path):
                print(f'Warning: Admin seed file not found at {admin_seed_path}')
                return None
            
            with open(admin_seed_path, 'r', encoding='utf-8') as f:
                admin_model = json.load(f)
            
            if admin_model is None:
                return None
            
            user_name = admin_model.get('UserName')
            password = admin_model.get('Password')
            
            if not user_name or not password:
                return None
            
            existing_user = session.query(User).filter(User.username == user_name).first()
            
            if existing_user is None:
                hashed_password = bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
                
                user = User(
                    id=uuid.uuid4(),
                    username=user_name,
                    email=admin_model.get('Email', 'admin@example.com'),
                    password=hashed_password,
                    first_name=admin_model.get('FirstName'),
                    last_name=admin_model.get('LastName'),
                    country_code=admin_model.get('CountryCode'),
                    phone_number=admin_model.get('PhoneNumber'),
                    tenant_id=tenant_id,
                    is_active=True,
                    is_email_verified=True,
                    created_at=datetime.utcnow(),
                    updated_at=datetime.utcnow()
                )
                
                session.add(user)
                session.commit()
                
                # Add User role for system admin
                user_role = session.query(Role).filter(
                    Role.role_name == 'User',
                    Role.user_id == user.id
                ).first()
                
                if user_role is None:
                    user_role = Role(
                        id=uuid.uuid4(),
                        user_id=user.id,
                        role_name='User',
                        created_at=datetime.utcnow(),
                        updated_at=datetime.utcnow()
                    )
                    session.add(user_role)
                    session.commit()
                
                print('System admin account seeded successfully')
                return user
            else:
                print(f'System admin account already exists with ID: {existing_user.id}, TenantId: {existing_user.tenant_id}')
                
                # Ensure User role is assigned
                has_user_role = session.query(Role).filter(
                    Role.user_id == existing_user.id,
                    Role.role_name == 'User'
                ).first()
                
                if has_user_role is None:
                    user_role = Role(
                        id=uuid.uuid4(),
                        user_id=existing_user.id,
                        role_name='User',
                        created_at=datetime.utcnow(),
                        updated_at=datetime.utcnow()
                    )
                    session.add(user_role)
                    session.commit()
                
                return existing_user
        except Exception as ex:
            session.rollback()
            print(f'Error seeding system admin: {str(ex)}')
            return None
    
    @staticmethod
    def _seed_default_tenant(session) -> Optional[Tenant]:
        """
        Seed default tenant.
        Similar to dotnet SeedDefaultTenant method.
        
        Args:
            session: Database session
            
        Returns:
            Created or existing Tenant
        """
        try:
            print('Seeding default tenant...')
            
            base_path = _get_seed_data_folder_path()
            tenant_seed_path = os.path.join(base_path, 'default.tenant.seed.json')
            
            if not os.path.exists(tenant_seed_path):
                print(f'Warning: Tenant seed file not found at {tenant_seed_path}')
                return None
            
            with open(tenant_seed_path, 'r', encoding='utf-8') as f:
                model = json.load(f)
            
            if model is None:
                print('Tenant model is invalid or null for seeding.')
                return None
            
            tenant_code = model.get('Code', 'default')
            
            default_tenant = session.query(Tenant).filter(Tenant.code == tenant_code).first()
            
            if default_tenant is not None:
                print(f'Default tenant already exists with ID: {default_tenant.id}')
                return default_tenant
            
            tenant = Tenant(
                id=uuid.uuid4(),
                name=model.get('Name', 'default'),
                code=tenant_code,
                description=model.get('Description'),
                contact_email=model.get('Email'),
                contact_phone=model.get('PhoneNumber'),
                is_active=True,
                is_verified=True,
                status='active',
                created_at=datetime.utcnow(),
                updated_at=datetime.utcnow()
            )
            
            session.add(tenant)
            session.commit()
            
            print(f'Default tenant seeded successfully with ID: {tenant.id}')
            return tenant
        except Exception as ex:
            session.rollback()
            print(f'Error seeding default tenant: {str(ex)}')
            # Try to retrieve existing tenant as fallback
            try:
                existing_tenant = session.query(Tenant).filter(Tenant.code == 'default').first()
                if existing_tenant:
                    print(f'Retrieved existing default tenant as fallback with ID: {existing_tenant.id}')
                    return existing_tenant
            except Exception as fallback_ex:
                print(f'Failed to retrieve existing tenant as fallback: {str(fallback_ex)}')
            return None
    
    @staticmethod
    def _seed_default_client_apps(session, is_dev_environment: bool = False, is_test_environment: bool = False) -> bool:
        """
        Seed default client apps.
        Similar to dotnet SeedDefaultClientApps method.
        
        Args:
            session: Database session
            is_dev_environment: Whether running in development
            is_test_environment: Whether running in test environment
            
        Returns:
            True if successful
        """
        try:
            print('Seeding default client apps...')
            
            base_path = _get_seed_data_folder_path()
            client_apps_seed_path = os.path.join(base_path, 'default.clientapps.seed.json')
            
            if not os.path.exists(client_apps_seed_path):
                print(f'Warning: Client apps seed file not found at {client_apps_seed_path}')
                return False
            
            with open(client_apps_seed_path, 'r', encoding='utf-8') as f:
                models = json.load(f)
            
            if models is None or len(models) == 0:
                return False
            
            owner = session.query(User).filter(User.username == 'admin').first()
            
            if owner is None:
                print('System admin not found')
                return False
            
            for client_app_model in models:
                if client_app_model is None:
                    continue
                
                app_code = client_app_model.get('Code')
                if not app_code:
                    continue
                
                existing_client_app = session.query(ClientApp).filter(ClientApp.code == app_code).first()
                
                if existing_client_app is None:
                    # Read all fields from seed data, matching .NET implementation
                    client_app = ClientApp(
                        id=uuid.uuid4(),
                        code=app_code,
                        name=client_app_model.get('Name') or 'Default Client App',
                        description=client_app_model.get('Description') or '',
                        owner_user_id=owner.id,
                        redirect_uri=client_app_model.get('RedirectUri'),
                        logo_url=client_app_model.get('LogoUrl'),
                        website_url=client_app_model.get('WebsiteUrl'),
                        privacy_policy_url=client_app_model.get('PrivacyPolicyUrl'),
                        terms_of_service_url=client_app_model.get('TermsOfServiceUrl'),
                        is_active=True,
                        is_verified=False,
                        status='active',
                        created_at=datetime.utcnow(),
                        updated_at=datetime.utcnow()
                    )
                    
                    session.add(client_app)
                    session.commit()
                    
                    # Read API key from seed data, or generate if not provided
                    api_key = client_app_model.get('ApiKey')
                    if not api_key:
                        # Generate API key if not provided in seed data
                        api_key = str(uuid.uuid4()).replace('-', '')[:32]
                    
                    client_app.api_key = api_key
                    session.commit()
                    
                    print(f'Client app {client_app.name} created with default API key')
            
            return True
        except Exception as ex:
            session.rollback()
            print(f'Error seeding default client apps: {str(ex)}')
            return False
