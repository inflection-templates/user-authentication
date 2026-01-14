"""
Client app seeder that creates client applications, optionally loading from internal.clients.seed.json.
Similar to the Node.js ClientAppSeeder implementation.
"""
import json
import os
from typing import List, Dict, Optional
from database.models.client_app import ClientApp
from database.models.user import User
from .base_seeder import BaseSeeder


class ClientAppSeeder(BaseSeeder):
    """Seeder for creating client applications."""
    
    def __init__(self, session, user_repository=None, batch_size: int = 1000, verbose: bool = True):
        """
        Initialize client app seeder.
        
        Args:
            session: SQLAlchemy session
            user_repository: Optional user repository (not used)
            batch_size: Batch size for inserts
            verbose: Whether to print progress
        """
        super().__init__(session, batch_size, verbose)
        self.session = session
    
    def _load_seed_data(self) -> Optional[List[Dict]]:
        """Load seed data from internal.clients.seed.json."""
        primary_seed_path = os.path.join(os.getcwd(), 'seed.data', 'internal.clients.seed.json')
        
        apps_from_file = None
        
        try:
            if os.path.exists(primary_seed_path):
                with open(primary_seed_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    if isinstance(data, list):
                        apps_from_file = []
                        for x in data:
                            app = {
                                'name': x.get('ClientName', 'Client App'),
                                'code': str(x.get('ClientCode', x.get('ClientName', 'client-app'))).strip().lower(),
                                'description': x.get('Description', 'Client application'),
                                'api_key': x.get('ApiKey') if x.get('ApiKey') else None,
                                'is_verified': True,
                                'is_active': True,
                                'status': 'active',
                            }
                            apps_from_file.append(app)
        except Exception as e:
            if self.verbose:
                print(f"[WARN] Could not load seed data: {e}")
        
        return apps_from_file
    
    def seed(self, count: int = 2, session=None) -> int:
        """
        Seed client apps.
        
        Args:
            count: Number of client apps to create
            session: SQLAlchemy session
            
        Returns:
            Number of client apps created
        """
        if session is None:
            session = self.session
            
        print(f"Seeding {count} client app(s)...")
        
        # Get users for owner
        users = session.query(User).all()
        
        if len(users) == 0:
            print('[ERROR] No users found. Seed users before client apps.')
            return 0
        
        owner = users[0]
        
        # Load seed data
        apps_from_file = self._load_seed_data()
        
        # Default client apps
        defaults = apps_from_file if apps_from_file else [
            {
                'name': 'Default',
                'code': 'default',
                'description': 'Primary web client application',
                'is_verified': True,
                'is_active': True,
                'status': 'active',
            }
        ]
        
        # Ensure ownerUserId is set on each app
        normalized = []
        for app in defaults:
            normalized_app = {
                **app,
                'owner_user_id': owner.id,
            }
            normalized.append(normalized_app)
        
        # Limit to requested count
        to_create = normalized[:max(1, min(count, len(normalized)))]
        
        self.batch_insert(to_create, session, model_class=ClientApp)
        print(f"Successfully seeded {self.created_count} client app(s)")
        return self.created_count
