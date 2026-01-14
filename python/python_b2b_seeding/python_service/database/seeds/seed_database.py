#!/usr/bin/env python3
"""
CLI script for seeding database.
Similar to the Node.js seed-database.ts implementation.

Usage:
    python seed_database.py                    # Seed all tables with default counts
    python seed_database.py --table users --count 50    # Seed specific table
    python seed_database.py --table users --clear --count 100  # Clear and seed
    python seed_database.py --info              # Show database info
"""
import argparse
import asyncio
import os
import sys
from typing import Optional

# Add parent directory to path for imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))

from database.seeds.database_seeder import DatabaseSeeder


# Note: You need to implement these functions based on your ORM/database setup
# This is a template that needs to be adapted to your specific database setup

def get_database_connection():
    """
    Get database connection/session.
    Replace this with your actual database connection logic.
    
    For SQLAlchemy example:
        from sqlalchemy import create_engine
        from sqlalchemy.orm import sessionmaker
        engine = create_engine('postgresql://user:pass@localhost/dbname')
        Session = sessionmaker(bind=engine)
        return Session()
    
    Returns:
        Database connection/session
    """
    # TODO: Implement based on your database setup
    # This is a placeholder
    raise NotImplementedError(
        "You need to implement get_database_connection() based on your ORM.\n"
        "See the docstring for examples."
    )


def get_repositories(data_source):
    """
    Get repositories for all models.
    Replace this with your actual repository logic.
    
    Args:
        data_source: Database connection/session
        
    Returns:
        Tuple of (user_repository, tenant_repository, role_repository, client_app_repository)
    """
    # TODO: Implement based on your ORM setup
    # For SQLAlchemy example:
    #     from models import User, Tenant, Role, ClientApp
    #     return (
    #         data_source.query(User),
    #         data_source.query(Tenant),
    #         data_source.query(Role),
    #         data_source.query(ClientApp),
    #     )
    raise NotImplementedError(
        "You need to implement get_repositories() based on your ORM.\n"
        "See the docstring for examples."
    )


async def main():
    """Main function to run seeding."""
    parser = argparse.ArgumentParser(description='Seed database with sample data')
    parser.add_argument('--table', type=str, help='Specific table to seed (users, roles, tenants, client-apps)')
    parser.add_argument('--count', type=int, default=0, help='Number of records to create')
    parser.add_argument('--clear', action='store_true', help='Clear table before seeding')
    parser.add_argument('--info', action='store_true', help='Show database information')
    parser.add_argument('--auto-setup', action='store_true', help='Auto setup: create tables and seed all data')
    
    args = parser.parse_args()
    
    try:
        print('[START] Starting database seeding...')
        
        # Initialize database connection
        # You need to implement get_database_connection() based on your ORM
        data_source = get_database_connection()
        
        # Get repositories
        # You need to implement get_repositories() based on your ORM
        user_repo, tenant_repo, role_repo, client_app_repo = get_repositories(data_source)
        
        seeder = DatabaseSeeder(
            data_source,
            user_repo,
            tenant_repo,
            role_repo,
            client_app_repo
        )
        
        # Auto setup: create database, tables, and seed all data
        if args.auto_setup:
            print('[INFO] Running automatic setup...')
            default_counts = {
                'users': 5,
                'roles': 3,
                'tenants': 1,
                'clientApps': 1
            }
            await seeder.seed_all(default_counts)
            return
        
        # Check database connection
        if not await seeder.check_database_connection():
            sys.exit(1)
        
        # Show info
        if args.info:
            print('[INFO] Database information:')
            business_mode = os.getenv('BUSINESS_MODE', 'b2c')
            print(f'Mode: {business_mode}')
            return
        
        # Clear table if requested
        if args.clear and args.table:
            await seeder.clear_table(args.table)
            return
        
        # Seed data
        if args.table:
            # Seed specific table
            count = args.count if args.count > 0 else 0
            await seeder.seed_specific(args.table, count)
        else:
            # Seed all tables
            default_counts = {
                'users': 5,
                'roles': 3,
                'tenants': 1,
                'clientApps': 2
            }
            await seeder.seed_all(default_counts)
        
        print('[OK] Seeding completed successfully!')
        sys.exit(0)
    except NotImplementedError as e:
        print(f'[ERROR] {str(e)}')
        sys.exit(1)
    except Exception as error:
        print(f'[ERROR] Error during seeding: {str(error)}')
        sys.exit(1)


if __name__ == '__main__':
    # Run async main
    asyncio.run(main())

