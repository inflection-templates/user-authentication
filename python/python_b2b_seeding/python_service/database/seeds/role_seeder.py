"""
Role seeder that creates roles.
Similar to the Node.js RoleSeederB2B implementation.
"""
from database.models.role import Role
from database.models.user import User
from .base_seeder import BaseSeeder


class RoleSeeder(BaseSeeder):
    """Seeder for creating roles."""
    
    # The three roles as specified (B2B mode)
    ROLE_OPTIONS = ['Tenant', 'Organization', 'User']
    
    def __init__(self, session, user_repository=None, batch_size: int = 1000, verbose: bool = True):
        """
        Initialize role seeder.
        
        Args:
            session: SQLAlchemy session
            user_repository: Optional user repository (not used)
            batch_size: Batch size for inserts
            verbose: Whether to print progress
        """
        super().__init__(session, batch_size, verbose)
        self.session = session
    
    def seed(self, count: int = 3, session=None) -> int:
        """
        Seed roles.
        
        Args:
            count: Number of roles to create (typically 3 for B2B mode)
            session: SQLAlchemy session
            
        Returns:
            Number of roles created
        """
        if session is None:
            session = self.session
            
        print(f"Seeding {count} roles...")
        
        # Get all existing users
        users = session.query(User).all()
        
        if len(users) == 0:
            print('[ERROR] No users found in database. Please seed users first.')
            return 0
        
        roles = []
        
        # Create exactly one role of each type, assigned to different users
        for i, role_name in enumerate(self.ROLE_OPTIONS):
            # Assign role to available user; reuse first if limited users
            user = users[min(i, len(users) - 1)]
            
            role = {
                'user_id': user.id,
                'role_name': role_name,
            }
            
            roles.append(role)
        
        from database.models.role import Role
        self.batch_insert(roles, session, model_class=Role)
        print(f"Successfully seeded {self.created_count} roles")
        return self.created_count
