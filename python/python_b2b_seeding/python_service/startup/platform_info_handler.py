"""
Platform info handler for reading platform information from JSON file.
Similar to the dotnet PlatformInfoHandler.cs implementation.
"""
import json
import os
from pathlib import Path
from typing import Optional, Dict, Any


class PlatformInfo:
    """Platform information model."""
    def __init__(self, data: Dict[str, Any]):
        self.platform = data.get('Platform', '')
        self.version = data.get('Version', '')
        self.website = data.get('Website', '')
        self.support_email = data.get('SupportEmail', '')
        self.support_phone = data.get('SupportPhone', '')
        self.company_address = data.get('CompanyAddress', '')
        self.platform_logo = data.get('PlatformLogo', '')
        self.social_media_links = data.get('SocialMediaLinks', {})
        self.social_media_icons = data.get('SocialMediaIcons', {})
        self.about_us = data.get('AboutUs', '')
        self.privacy_policy = data.get('PrivacyPolicy', '')
        self.terms_and_conditions = data.get('TermsAndConditions', '')
        self.unsubscribe = data.get('Unsubscribe', '')
        self.contact_us = data.get('ContactUs', '')
        self.faq = data.get('Faq', '')


class PlatformInfoHandler:
    """Handler for reading platform information."""
    
    @staticmethod
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
    
    @staticmethod
    def get_platform_info() -> Optional[PlatformInfo]:
        """
        Get platform information from JSON file.
        
        Returns:
            PlatformInfo object or None if file not found
        """
        try:
            base_path = PlatformInfoHandler._get_seed_data_folder_path()
            platform_info_json = os.path.join(base_path, 'platform.info.json')
            
            if not os.path.exists(platform_info_json):
                return None
            
            with open(platform_info_json, 'r', encoding='utf-8') as f:
                data = json.load(f)
                
            return PlatformInfo(data)
        except Exception as exception:
            print(f'Error getting platform info: {str(exception)}')
            return None

