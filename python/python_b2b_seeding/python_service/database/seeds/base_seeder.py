"""
Base seeder class providing common functionality for all seeders.
Similar to the Node.js BaseSeeder implementation.
"""
import random
from datetime import datetime, timedelta
from typing import List, TypeVar, Type
from abc import ABC, abstractmethod

T = TypeVar('T')


class BaseSeeder(ABC):
    """Abstract base class for all seeders."""
    
    def __init__(self, repository, batch_size: int = 1000, verbose: bool = True):
        """
        Initialize base seeder.
        
        Args:
            repository: Database repository/Session to use
            batch_size: Number of records to insert per batch
            verbose: Whether to print progress messages
        """
        self.repository = repository
        self.batch_size = batch_size
        self.verbose = verbose
        self.created_count = 0
    
    @abstractmethod
    def seed(self, count: int, session) -> int:
        """
        Abstract method that must be implemented by each seeder.
        
        Args:
            count: Number of records to seed
            
        Returns:
            Number of records created
        """
        pass
    
    def batch_insert(self, objects: List[dict], session, model_class=None) -> None:
        """
        Insert objects in batches to improve performance.
        Works with SQLAlchemy sessions.
        
        Args:
            objects: List of dictionaries/objects to insert
            session: SQLAlchemy session
            model_class: Explicit model class to use (if None, will infer from object structure)
        """
        from database.models.user import User
        from database.models.tenant import Tenant
        from database.models.role import Role
        from database.models.client_app import ClientApp
        
        # Determine model class
        if model_class is None:
            if hasattr(self, 'model_class'):
                model_class = self.model_class
            elif hasattr(self.repository, 'entity'):
                model_class = self.repository.entity
        
        for i in range(0, len(objects), self.batch_size):
            batch = objects[i:i + self.batch_size]
            try:
                # Convert dicts to model instances
                instances = []
                for obj_dict in batch:
                    # Use explicit model_class if provided
                    if model_class:
                        instance = model_class(**obj_dict)
                    else:
                        # Infer model class from object structure
                        if 'username' in obj_dict or 'email' in obj_dict:
                            instance = User(**obj_dict)
                        elif 'owner_user_id' in obj_dict or ('name' in obj_dict and 'code' in obj_dict and 'api_key' in obj_dict):
                            # ClientApp has owner_user_id field
                            instance = ClientApp(**obj_dict)
                        elif 'role_name' in obj_dict:
                            instance = Role(**obj_dict)
                        elif 'name' in obj_dict and 'code' in obj_dict and ('is_verified' in obj_dict or 'status' in obj_dict):
                            # Tenant has is_verified/status but not owner_user_id
                            instance = Tenant(**obj_dict)
                        else:
                            continue
                    instances.append(instance)
                
                session.add_all(instances)
                session.commit()
                
                self.created_count += len(batch)
                
                if self.verbose:
                    print(f"Inserted batch of {len(batch)} records. Total created: {self.created_count}")
                    
            except Exception as error:
                session.rollback()
                # Handle duplicate key errors gracefully
                error_str = str(error).lower()
                if 'duplicate' in error_str or 'unique' in error_str or 'integrity' in error_str:
                    print(f"[WARN] Duplicate entry detected in batch. Attempting individual inserts...")
                    
                    success_count = 0
                    for obj_dict in batch:
                        try:
                            # Use explicit model_class if provided
                            if model_class:
                                instance = model_class(**obj_dict)
                            else:
                                # Determine model class for this object
                                if 'username' in obj_dict or 'email' in obj_dict:
                                    instance = User(**obj_dict)
                                elif 'owner_user_id' in obj_dict or ('name' in obj_dict and 'code' in obj_dict and 'api_key' in obj_dict):
                                    instance = ClientApp(**obj_dict)
                                elif 'role_name' in obj_dict:
                                    instance = Role(**obj_dict)
                                elif 'name' in obj_dict and 'code' in obj_dict:
                                    instance = Tenant(**obj_dict)
                                else:
                                    continue
                            
                            session.merge(instance)  # Use merge to handle duplicates
                            session.commit()
                            success_count += 1
                            self.created_count += 1
                        except Exception as individual_error:
                            session.rollback()
                            error_str_ind = str(individual_error).lower()
                            if 'duplicate' in error_str_ind or 'unique' in error_str_ind:
                                if self.verbose:
                                    print(f"[WARN] Skipping duplicate entry")
                            else:
                                # Log but continue
                                if self.verbose:
                                    print(f"[WARN] Error inserting record: {str(individual_error)}")
                    
                    if self.verbose:
                        print(f"Inserted {success_count} of {len(batch)} records from batch. Total created: {self.created_count}")
                else:
                    raise error
    
    def get_random_date(self, start_date: datetime = None, end_date: datetime = None) -> datetime:
        """
        Get a random date between start_date and end_date.
        
        Args:
            start_date: Start date (default: 1 year ago)
            end_date: End date (default: now)
            
        Returns:
            Random datetime
        """
        if start_date is None:
            start_date = datetime.now() - timedelta(days=365)
        if end_date is None:
            end_date = datetime.now()
        
        time_between = end_date - start_date
        days_between = time_between.days
        random_number_of_days = random.randrange(days_between)
        random_date = start_date + timedelta(days=random_number_of_days)
        
        # Add random time
        random_seconds = random.randint(0, 86400)
        return random_date + timedelta(seconds=random_seconds)
    
    def get_random_choice(self, choices: List) -> any:
        """
        Get a random choice from an array.
        
        Args:
            choices: List of choices
            
        Returns:
            Random choice from the list
        """
        return random.choice(choices) if choices else None
    
    def get_random_boolean(self, true_probability: float = 0.5) -> bool:
        """
        Get a random boolean value.
        
        Args:
            true_probability: Probability of returning True (0.0 to 1.0)
            
        Returns:
            Random boolean
        """
        return random.random() < true_probability
    
    def get_created_count(self) -> int:
        """Get the number of records created."""
        return self.created_count

