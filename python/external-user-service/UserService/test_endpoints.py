#!/usr/bin/env python3
"""
Simple endpoint testing script for User Service
This script helps verify that the service is running and endpoints are accessible
"""

import requests
import json
import sys
from typing import Dict, Any

# Service base URL
BASE_URL = "http://localhost:5000"

def test_endpoint(method: str, url: str, data: Dict[Any, Any] = None, headers: Dict[str, str] = None) -> Dict[str, Any]:
    """Test an endpoint and return the result"""
    try:
        full_url = f"{BASE_URL}{url}"
        print(f"Testing {method} {full_url}")
        
        if method.upper() == "GET":
            response = requests.get(full_url, headers=headers)
        elif method.upper() == "POST":
            response = requests.post(full_url, json=data, headers=headers)
        elif method.upper() == "PUT":
            response = requests.put(full_url, json=data, headers=headers)
        else:
            return {"error": f"Unsupported method: {method}"}
        
        result = {
            "status_code": response.status_code,
            "url": full_url,
            "success": response.status_code < 400
        }
        
        try:
            result["response"] = response.json()
        except:
            result["response"] = response.text
            
        return result
        
    except requests.exceptions.ConnectionError:
        return {
            "error": f"Connection failed to {BASE_URL}",
            "url": full_url,
            "success": False
        }
    except Exception as e:
        return {
            "error": str(e),
            "url": full_url,
            "success": False
        }

def main():
    """Test key endpoints"""
    print("=" * 60)
    print("User Service Endpoint Testing")
    print("=" * 60)
    
    # Test basic endpoints
    endpoints_to_test = [
        ("GET", "/"),
        ("GET", "/health"),
        ("GET", "/.well-known/jwks.json"),
        ("GET", "/.well-known/openid_configuration"),
        ("GET", "/api/v1/roles/"),
        ("POST", "/api/v1/auth/register", {
            "email": "test@example.com",
            "password": "TestPassword123!",
            "first_name": "Test",
            "last_name": "User"
        }),
        ("POST", "/api/v1/auth/login", {
            "email": "test@example.com", 
            "password": "TestPassword123!"
        })
    ]
    
    results = []
    for method, url, *data in endpoints_to_test:
        payload = data[0] if data else None
        result = test_endpoint(method, url, payload)
        results.append(result)
        
        if result["success"]:
            print(f"✅ {method} {url} - Status: {result['status_code']}")
        else:
            print(f"❌ {method} {url} - {result.get('error', f'Status: {result.get(\"status_code\", \"Unknown\")}')}") 
        
        print()
    
    # Summary
    successful = sum(1 for r in results if r["success"])
    total = len(results)
    
    print("=" * 60)
    print(f"Test Summary: {successful}/{total} endpoints working")
    print("=" * 60)
    
    if successful == 0:
        print("❌ Service appears to be down or not accessible")
        print("   Make sure to start the service with: python app.py")
        print("   Or: uvicorn app:app --host 0.0.0.0 --port 5000 --reload")
    elif successful < total:
        print("⚠️  Some endpoints are not working - check the logs")
    else:
        print("✅ All tested endpoints are working!")
        
    return 0 if successful > 0 else 1

if __name__ == "__main__":
    sys.exit(main())
