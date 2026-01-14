"""
User seeder that creates users, optionally loading from system.admin.seed.json.
Similar to the Node.js UserSeeder implementation.
"""
import json
import os
from typing import Dict
from datetime import datetime

from .base_seeder import BaseSeeder
from database.models.user import User
from database.models.tenant import Tenant


class UserSeeder(BaseSeeder):
    """Seeder for creating users."""
    
    def __init__(self, session, tenant_repository=None, batch_size: int = 1000, verbose: bool = True):
        """
        Initialize user seeder.
        
        Args:
            session: SQLAlchemy session
            tenant_repository: Optional tenant repository (not used in SQLAlchemy version)
            batch_size: Batch size for inserts
            verbose: Whether to print progress
        """
        super().__init__(session, batch_size, verbose)
        self.session = session
    
    def _hash_password(self, password: str) -> str:
        """Hash password using bcrypt."""
        try:
            import bcrypt
            return bcrypt.hashpw(password.encode('utf-8'), bcrypt.gensalt()).decode('utf-8')
        except ImportError:
            # Fallback for development
            import hashlib
            return hashlib.sha256(password.encode()).hexdigest()
    
    def _load_seed_data(self) -> Dict:
        """Load seed data from system.admin.seed.json."""
        primary_seed_path = os.path.join(os.getcwd(), 'seed.data', 'system.admin.seed.json')
        
        try:
            if os.path.exists(primary_seed_path):
                with open(primary_seed_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    return {
                        'username': data.get('UserName', 'admin'),
                        'firstName': data.get('FirstName', 'System'),
                        'lastName': 'Admin',
                        'email': data.get('Email', 'sys.admin@example.com'),
                        'password': data.get('Password', 'ChangeMe123!'),
                        'phoneNumber': data.get('PhoneNumber', None),
                        'phoneCode': data.get('PhoneCode', '+91'),
                    }
        except Exception as e:
            if self.verbose:
                print(f"[WARN] Could not load seed data: {e}")
        
        return None
    
    def seed(self, count: int = 2, session=None) -> int:
        """
        Seed users.
        
        Args:
            count: Number of users to create (default: 2)
            session: SQLAlchemy session
            
        Returns:
            Number of users created
        """
        if session is None:
            session = self.session
            
        print(f"Seeding {count} users...")
        
        # Get all tenants
        tenants = session.query(Tenant).all()
        
        if len(tenants) > 0:
            print(f"[INFO] Assigning users to {len(tenants)} tenant(s)")
        else:
            print("[WARN] No tenants found. Users will be created without tenantId.")
        
        # Load seed data
        seed_data = self._load_seed_data()
        
        default_password = self._hash_password('password123')
        
        # Default values
        usernames = ['john.smith', 'sarah.brown', 'mike.johnson', 'jane.williams', 'david.davis']
        first_names = ['John', 'Jane', 'Mike', 'Sarah', 'David']
        last_names = ['Smith', 'Johnson', 'Williams', 'Brown', 'Davis']
        email_domain = 'example.com'
        
        # Override with seed data if available
        if seed_data:
            if seed_data.get('username'):
                usernames = [seed_data['username']]
            if seed_data.get('firstName'):
                first_names = [seed_data['firstName']]
            if seed_data.get('email') and '@' in seed_data['email']:
                email_domain = seed_data['email'].split('@')[1]
        
        users = []
        
        for i in range(count):
            # Use seed data for first user if available
            if i == 0 and seed_data:
                user = {
                    'username': seed_data.get('username', f'user{i}'),
                    'email': seed_data.get('email', f'user{i}@{email_domain}'),
                    'password': self._hash_password(seed_data.get('password', 'ChangeMe123!')),
                    'first_name': seed_data.get('firstName', self.get_random_choice(first_names)),
                    'last_name': seed_data.get('lastName', 'Admin'),
                    'is_active': True,
                    'is_email_verified': True,
                    'is_phone_verified': False,
                }
                
                # Add phone number if available
                if seed_data.get('phoneNumber'):
                    user['phone_number'] = seed_data['phoneNumber']
                if seed_data.get('phoneCode'):
                    user['country_code'] = seed_data['phoneCode']
            else:
                user = {
                    'username': self.get_random_choice(usernames) if i < len(usernames) else f'user{i}',
                    'email': f'user{i}@{email_domain}',
                    'password': default_password,
                    'first_name': self.get_random_choice(first_names),
                    'last_name': self.get_random_choice(last_names),
                    'is_active': self.get_random_boolean(0.9),
                    'is_email_verified': self.get_random_boolean(0.8),
                    'is_phone_verified': self.get_random_boolean(0.5),
                }
            
            # Assign tenant if available
            if tenants and len(tenants) > 0:
                tenant = self.get_random_choice(tenants)
                user['tenant_id'] = tenant.id
            
            users.append(user)
        
        from database.models.user import User
        self.batch_insert(users, session, model_class=User)
        print(f"Successfully seeded {self.created_count} users")
        return self.created_count
