"""
Tenant seeder that creates tenants.
Similar to the Node.js TenantSeeder implementation.
"""
from datetime import datetime
from database.models.tenant import Tenant
from .base_seeder import BaseSeeder


class TenantSeeder(BaseSeeder):
    """Seeder for creating tenants."""
    
    def __init__(self, session=None, user_repository=None, batch_size: int = 1000, verbose: bool = True):
        """
        Initialize tenant seeder.
        
        Args:
            session: SQLAlchemy session
            user_repository: Optional user repository (not used)
            batch_size: Batch size for inserts
            verbose: Whether to print progress
        """
        super().__init__(session, batch_size, verbose)
        self.session = session
        self.model_class = Tenant
    
    def seed(self, count: int = 1, session=None) -> int:
        """
        Seed tenants.
        
        Args:
            count: Number of tenants to create (typically 1 for default tenant)
            session: SQLAlchemy session
            
        Returns:
            Number of tenants created
        """
        print(f"Seeding {count} tenant(s)...")
        
        # Check if tenants already exist
        existing_count = session.query(Tenant).count()
        
        if existing_count > 0:
            print(f"[INFO] {existing_count} tenant(s) already exist, skipping seed")
            return 0
        
        tenants = []
        
        # Create default tenant
        tenant = {
            'name': 'Default',
            'code': 'default',
            'is_active': True,
            'status': 'active',
            'is_verified': True,
        }
        
        tenants.append(tenant)
        
        self.batch_insert(tenants, session, model_class=Tenant)
        print(f"Successfully seeded {self.created_count} tenant(s)")
        return self.created_count
