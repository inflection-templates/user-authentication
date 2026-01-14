"""
Main database seeder that orchestrates seeding of all tables.
Similar to the Node.js DatabaseSeeder implementation.
"""
from typing import Dict, Optional
from .user_seeder import UserSeeder
from .tenant_seeder import TenantSeeder
from .role_seeder import RoleSeeder
from .client_app_seeder import ClientAppSeeder


class DatabaseSeeder:
    """Main database seeder orchestrator."""
    
    def __init__(self, data_source, user_repository, tenant_repository, role_repository, client_app_repository):
        """
        Initialize database seeder.
        
        Args:
            data_source: Database connection/session
            user_repository: Repository for User model
            tenant_repository: Repository for Tenant model
            role_repository: Repository for Role model
            client_app_repository: Repository for ClientApp model
        """
        self.data_source = data_source
        self.user_repository = user_repository
        self.tenant_repository = tenant_repository
        self.role_repository = role_repository
        self.client_app_repository = client_app_repository
    
    def check_database_connection(self) -> bool:
        """
        Check if database connection is working.
        
        Returns:
            True if connection is successful, False otherwise
        """
        try:
            # For SQLAlchemy engine
            from sqlalchemy import text
            with self.data_source.connect() as conn:
                conn.execute(text("SELECT 1"))
            print('[OK] Database connection successful')
            return True
        except Exception as error:
            print(f'[ERROR] Database connection failed: {str(error)}')
            return False
    
    def seed_all(self, counts: Dict[str, int], session) -> int:
        """
        Seed all tables.
        
        Args:
            counts: Dictionary with counts for users, roles, tenants, clientApps
            
        Returns:
            Total number of records created
        """
        print('[INFO] Using B2B mode')
        print(f"[INFO] Will create roles: {', '.join(RoleSeeder.ROLE_OPTIONS)}")
        
        total_created = 0
        
        from database.models.client_app import ClientApp
        
        # Seed tenants
        tenant_count = counts.get('tenants', 1)
        print(f'\n[SEED] Seeding tenants...')
        tenant_seeder = TenantSeeder(session, batch_size=100)
        created_tenants = tenant_seeder.seed(tenant_count, session)
        total_created += created_tenants
        
        # Seed users
        user_count = counts.get('users', 100)
        print(f'\n[SEED] Seeding users...')
        user_seeder = UserSeeder(session, batch_size=100)
        created_users = user_seeder.seed(user_count, session)
        total_created += created_users
        
        # Seed roles
        role_count = counts.get('roles', 3)
        print(f'\n[SEED] Seeding roles...')
        role_seeder = RoleSeeder(session, batch_size=100)
        created_roles = role_seeder.seed(role_count, session)
        total_created += created_roles
        
        # Seed client apps
        client_app_count = counts.get('clientApps', 2)
        print(f'\n[SEED] Seeding client apps...')
        client_app_seeder = ClientAppSeeder(session, batch_size=100)
        created_client_apps = client_app_seeder.seed(client_app_count, session)
        total_created += created_client_apps
        
        print(f'\n[OK] Seeding completed!')
        return total_created
    
    async def seed_specific(self, table_name: str, count: int) -> int:
        """
        Seed a specific table.
        
        Args:
            table_name: Name of table to seed (users, roles, tenants, client-apps)
            count: Number of records to create
            
        Returns:
            Number of records created
        """
        if table_name == 'roles':
            print('[INFO] Using B2B mode')
            print(f"[INFO] Will create roles: {', '.join(RoleSeeder.ROLE_OPTIONS)}")
        
        if table_name == 'users':
            print(f'[SEED] Seeding users with {count} records...')
            user_seeder = UserSeeder(self.user_repository, self.tenant_repository)
            created = await user_seeder.seed(count if count > 0 else 100)
            print(f'[OK] Created {created} records in users')
            return created
        
        elif table_name == 'roles':
            print(f'[SEED] Seeding roles with {count} records...')
            role_seeder = RoleSeeder(self.role_repository, self.user_repository)
            created = await role_seeder.seed(count if count > 0 else 3)
            print(f'[OK] Created {created} records in roles')
            return created
        
        elif table_name == 'tenants':
            print(f'[SEED] Seeding tenants with {count} records...')
            tenant_seeder = TenantSeeder(self.tenant_repository)
            created = await tenant_seeder.seed(count if count > 0 else 1)
            print(f'[OK] Created {created} records in tenants')
            return created
        
        elif table_name == 'client-apps' or table_name == 'client_apps':
            print(f'[SEED] Seeding client apps with {count} records...')
            client_app_seeder = ClientAppSeeder(self.client_app_repository, self.user_repository)
            created = await client_app_seeder.seed(count if count > 0 else 2)
            print(f'[OK] Created {created} records in client apps')
            return created
        
        else:
            print(f'[ERROR] Unknown table: {table_name}')
            print('Available tables: users, roles, tenants, client-apps')
            return 0
    
    async def clear_table(self, table_name: str) -> bool:
        """
        Clear all records from a table.
        
        Args:
            table_name: Name of table to clear
            
        Returns:
            True if successful, False otherwise
        """
        try:
            deleted_count = 0
            
            if table_name == 'users':
                if hasattr(self.user_repository, 'delete'):
                    deleted_count = await self.user_repository.delete({})
                elif hasattr(self.user_repository, 'query'):
                    deleted_count = self.user_repository.query.delete()
            elif table_name == 'roles':
                if hasattr(self.role_repository, 'delete'):
                    deleted_count = await self.role_repository.delete({})
                elif hasattr(self.role_repository, 'query'):
                    deleted_count = self.role_repository.query.delete()
            elif table_name == 'tenants':
                if hasattr(self.tenant_repository, 'delete'):
                    deleted_count = await self.tenant_repository.delete({})
                elif hasattr(self.tenant_repository, 'query'):
                    deleted_count = self.tenant_repository.query.delete()
            else:
                print(f'[ERROR] Unknown table: {table_name}')
                return False
            
            print(f'[OK] Cleared {deleted_count} records from {table_name}')
            return True
        except Exception as error:
            print(f'[ERROR] Error clearing table {table_name}: {str(error)}')
            return False

