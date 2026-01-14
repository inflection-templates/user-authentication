# JWKS (JSON Web Key Set) Testing Collection

This folder contains Bruno requests for testing the JWKS functionality in your User Service.

## 🔐 What is JWKS?

JWKS (JSON Web Key Set) is a standard way to expose public keys for JWT token validation. This allows other services to validate JWT tokens without sharing private keys.

## 📋 Available Requests

### 1. **Get JWKS (Public Key)** - `Get JWKS (Public Key).bru`
- **Purpose**: Retrieve the public key for JWT validation
- **Endpoint**: `GET /.well-known/jwks.json`
- **Authentication**: None required
- **What it does**: 
  - Fetches the public key from your service
  - Stores key information in environment variables
  - Logs key details for debugging

### 2. **JWKS Health Check** - `JWKS Health Check.bru`
- **Purpose**: Check the health status of the JWKS service
- **Endpoint**: `GET /.well-known/jwks/health`
- **Authentication**: None required
- **What it does**:
  - Verifies JWKS service is running
  - Shows key information (type, ID, algorithm)
  - Stores health status in environment

### 3. **Test JWT Token Validation** - `Test JWT Token Validation.bru`
- **Purpose**: Generate a JWT token and extract its claims
- **Endpoint**: `POST /api/users/auth/login-with-password`
- **Authentication**: User credentials
- **What it does**:
  - Logs in a user to get JWT token
  - Decodes the token to show claims
  - Stores tokens in environment variables
  - Prepares for JWT validation testing

### 4. **Validate JWT with JWKS** - `Validate JWT with JWKS.bru`
- **Purpose**: Test JWT token validation using the generated token
- **Endpoint**: `GET /api/users/users/profile`
- **Authentication**: JWT Bearer token
- **What it does**:
  - Uses the JWT token to access a protected endpoint
  - Proves the token is valid and properly signed
  - Demonstrates asymmetric key validation

## 🚀 How to Use

### **Step 1: Test JWKS Endpoint**
1. Run **"Get JWKS (Public Key)"** to verify the endpoint works
2. Check the console for key information
3. Verify environment variables are set

### **Step 2: Check JWKS Health**
1. Run **"JWKS Health Check"** to verify service health
2. Confirm the service is running and keys are available

### **Step 3: Generate JWT Token**
1. Set environment variables for test user credentials:
   - `TEST_USER_EMAIL`: Your test user's email
   - `TEST_USER_PASSWORD`: Your test user's password
2. Run **"Test JWT Token Validation"** to get a JWT token
3. Check console for token details and claims

### **Step 4: Validate JWT Token**
1. Run **"Validate JWT with JWKS"** to test token validation
2. This proves the asymmetric key system is working
3. Check console for validation results

## 🔧 Environment Variables

These requests will set the following environment variables:

- `JWKS_KEY_ID`: The key ID from JWKS
- `JWKS_ALGORITHM`: The algorithm (should be RS256)
- `JWKS_KEY_TYPE`: The key type (should be RSA)
- `JWKS_DATA`: Complete JWKS response
- `JWKS_HEALTH_STATUS`: Health check status
- `JWT_ACCESS_TOKEN`: Generated JWT access token
- `JWT_REFRESH_TOKEN`: Generated JWT refresh token
- `JWT_USER_ID`: User ID from token claims
- `JWT_USERNAME`: Username from token claims
- `JWT_ROLE`: User role from token claims
- `JWT_VALIDATION_STATUS`: Token validation result

## 🎯 Expected Results

### **JWKS Endpoint Response:**
```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "abc12345",
      "alg": "RS256",
      "n": "base64-encoded-modulus",
      "e": "base64-encoded-exponent"
    }
  ]
}
```

### **JWKS Health Response:**
```json
{
  "Status": "Healthy",
  "KeyType": "RsaSecurityKey",
  "KeyId": "abc12345",
  "Algorithm": "RS256",
  "Timestamp": "2024-01-XX..."
}
```

## 🔍 Troubleshooting

### **JWKS Endpoint Not Working:**
- Check if User Service is running
- Verify the `.well-known/jwks.json` route is configured
- Check service logs for errors

### **JWT Token Generation Fails:**
- Verify user credentials are correct
- Check if JWT service is properly configured
- Ensure asymmetric keys are generated

### **Token Validation Fails:**
- Verify the JWT token was generated successfully
- Check if the token has expired
- Ensure the service is using the same keys for validation

## 🌟 Benefits of This Setup

1. **Security**: Asymmetric keys instead of shared secrets
2. **Interoperability**: Standard JWKS endpoint for other services
3. **Scalability**: Multiple services can validate tokens independently
4. **Maintainability**: Centralized key management
5. **Standards Compliance**: RFC 7517 compliant JWKS

## 🔄 Integration with Demo Service

Once these endpoints are working, your Demo Service can:
1. Fetch the public key from `/.well-known/jwks.json`
2. Use it to validate JWT tokens from this User Service
3. Extract user information from validated tokens
4. Provide secure access to resources

This creates a complete JWT authentication delegation system! 🚀
