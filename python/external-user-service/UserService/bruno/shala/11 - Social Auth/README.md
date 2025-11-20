# 🔐 Social Auth - Bruno API Tests

This folder contains Bruno API test collections for testing OAuth social authentication providers in the Python User Service.

## 📋 Available OAuth Providers

### ✅ GitHub OAuth
- **Files:**
  - `GitHub OAuth - Step 1 Get Login URL.bru`
  - `GitHub OAuth - Step 2 Test Callback.bru`
- **OAuth Version:** 2.0
- **Scopes:** `user:email`
- **Endpoints:**
  - GET `/api/v1/users/oauth/github/login`
  - GET `/api/v1/users/oauth/github`

### ✅ Google OAuth
- **Files:**
  - `Google OAuth - Step 1 Get Login URL.bru`
  - `Google OAuth - Step 2 Test Callback.bru`
- **OAuth Version:** 2.0
- **Scopes:** `openid email profile`
- **Endpoints:**
  - GET `/api/v1/users/oauth/google/login`
  - GET `/api/v1/users/oauth/google`

### ✅ Twitter/X OAuth
- **Files:**
  - `Twitter OAuth - Step 1 Get Login URL.bru`
  - `Twitter OAuth - Step 2 Test Callback.bru`
- **OAuth Version:** 2.0 with PKCE
- **Scopes:** `tweet.read users.read offline.access`
- **Endpoints:**
  - GET `/api/v1/users/oauth/twitter/login`
  - GET `/api/v1/users/oauth/twitter`

## 🚀 How to Test OAuth Providers

### Step-by-Step Testing Process

1. **Start the Python Service:**
   ```bash
   cd python/external-user-service/UserService
   python -m uvicorn app:app --reload --port 5000
   ```

2. **Configure Environment Variables:**
   ```bash
   # For GitHub
   export OAUTH_GITHUB_ENABLED=true
   export OAUTH_GITHUB_CLIENT_ID=your_github_client_id
   export OAUTH_GITHUB_CLIENT_SECRET=your_github_client_secret
   
   # For Google
   export OAUTH_GOOGLE_ENABLED=true
   export OAUTH_GOOGLE_CLIENT_ID=your_google_client_id
   export OAUTH_GOOGLE_CLIENT_SECRET=your_google_client_secret
   
   # For Twitter
   export OAUTH_TWITTER_ENABLED=true
   export OAUTH_TWITTER_CLIENT_ID=your_twitter_client_id
   export OAUTH_TWITTER_CLIENT_SECRET=your_twitter_client_secret
   ```

3. **Run Step 1 Test:**
   - Execute the "Step 1 Get Login URL" test
   - Copy the authorization URL from the response
   - For Twitter: Also save the `code_verifier` and `state` values

4. **Manual Authorization:**
   - Open the authorization URL in your browser
   - Sign in with your social account
   - Authorize the application
   - Copy the `code` parameter from the callback URL

5. **Run Step 2 Test:**
   - Replace placeholder values with real OAuth code
   - For Twitter: Also replace the state parameter
   - Execute the "Step 2 Test Callback" test
   - Verify successful authentication

## 🔧 Configuration Requirements

### GitHub OAuth Setup
- Create GitHub App at https://github.com/settings/developers
- Set Authorization callback URL: `http://localhost:5000/api/v1/users/oauth/github`
- Enable "Request user authorization (OAuth) during installation"

### Google OAuth Setup
- Create project at https://console.developers.google.com/
- Enable Google+ API
- Create OAuth 2.0 Client ID
- Set Redirect URI: `http://localhost:5000/api/v1/users/oauth/google`

### Twitter OAuth Setup
- Create app at https://developer.twitter.com/
- Enable OAuth 2.0
- Set Callback URL: `http://localhost:5000/api/v1/users/oauth/twitter`
- Request email permissions (optional, requires approval)

## 🧪 Test Features

### Common Test Validations
- ✅ HTTP status codes (200/302)
- ✅ Response structure validation
- ✅ Authorization URL format checking
- ✅ Provider-specific URL validation
- ✅ Detailed logging for debugging

### Twitter-Specific Tests
- ✅ PKCE code_verifier validation
- ✅ State parameter validation
- ✅ Code challenge parameter checking
- ✅ OAuth 2.0 compliance validation

### Expected Responses

#### Step 1 Response (Get Login URL)
```json
{
  "success": true,
  "message": "OAuth login initiated",
  "data": {
    "authorization_url": "https://provider.com/oauth/authorize?...",
    "provider": "GitHub|Google|Twitter",
    // Twitter only:
    "code_verifier": "abc123...",
    "state": "xyz789..."
  }
}
```

#### Step 2 Response (Callback)
- **Status:** 302 (Redirect) or 200 (Success)
- **Redirect:** `http://localhost:5000?success=true&token=...&user_id=...`
- **Creates:** New user account (if first login)
- **Generates:** JWT authentication tokens

## 🚨 Common Issues & Solutions

### Issue: "OAuth not configured"
**Solution:** Check environment variables are set correctly

### Issue: "Invalid redirect URI"
**Solution:** Verify callback URLs in provider app settings

### Issue: "Invalid authorization code"
**Solution:** OAuth codes expire quickly, get a fresh code

### Issue: "PKCE validation failed" (Twitter only)
**Solution:** Ensure using OAuth 2.0 credentials, not OAuth 1.0a

## 📊 Test Results

### Successful OAuth Flow
1. ✅ Step 1: Authorization URL generated
2. ✅ Manual: User authorizes application
3. ✅ Step 2: Callback handled successfully
4. ✅ Result: User authenticated with JWT tokens

### What Happens on Success
- New user account created (if first login)
- Existing user logged in (if account exists)
- JWT access token generated
- Login session created
- User redirected to frontend with tokens

## 🔗 Related Documentation

- [GitHub OAuth Setup Guide](../../GITHUB_OAUTH_SETUP.md) (if exists)
- [Google OAuth Setup Guide](../../GOOGLE_OAUTH_SETUP.md) (if exists)
- [Twitter OAuth Setup Guide](../../TWITTER_X_OAUTH_SETUP.md)
- [FastAPI OAuth Documentation](https://fastapi.tiangolo.com/advanced/security/)

## 🎯 Testing Tips

1. **Use Incognito/Private browsing** for clean OAuth sessions
2. **Check browser network tab** for detailed request/response info
3. **Monitor server logs** for debugging OAuth flow issues
4. **Test with different social accounts** to verify user creation
5. **Verify JWT tokens** using the JWKS endpoints in folder "10 - JWKS"

---

**Happy Testing!** 🚀✨
