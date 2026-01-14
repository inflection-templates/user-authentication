# Social Authentication (OAuth) Testing Collection

This folder contains Bruno requests for testing OAuth authentication flows in your .NET User Service.

## 🔐 Available OAuth Providers

Your service supports the following OAuth providers:
- **GitHub** - GitHub OAuth 2.0
- **Google** - Google OAuth 2.0  
- **GitLab** - GitLab OAuth 2.0

## 📋 Available Requests

### 1. **Get OAuth Providers** - `Get OAuth Providers.bru`
- **Purpose**: List all available OAuth providers
- **Endpoint**: `GET /api/v1/oauth/providers`
- **Authentication**: None required
- **What it does**: Returns a list of supported OAuth providers

### 2. **GitHub OAuth Flow**

#### **Step 1: Get GitHub Login URL** - `GitHub OAuth - Step 1 Get Login URL.bru`
- **Purpose**: Get GitHub authorization URL
- **Endpoint**: `GET /api/v1/oauth/github/challenge`
- **Authentication**: None required
- **What it does**: Returns GitHub OAuth authorization URL

#### **Step 2: Test GitHub Callback** - `GitHub OAuth - Step 2 Test Callback.bru`
- **Purpose**: Complete GitHub OAuth flow with authorization code
- **Endpoint**: `GET /api/v1/oauth/github/callback`
- **Authentication**: None required (uses authorization code)
- **What it does**: Exchanges authorization code for access token and user info

### 3. **Google OAuth Flow**

#### **Step 1: Get Google Login URL** - `Google OAuth - Step 1 Get Login URL.bru`
- **Purpose**: Get Google authorization URL
- **Endpoint**: `GET /api/v1/oauth/google/challenge`
- **Authentication**: None required
- **What it does**: Returns Google OAuth authorization URL

#### **Step 2: Test Google Callback** - `Google OAuth - Step 2 Test Callback.bru`
- **Purpose**: Complete Google OAuth flow with authorization code
- **Endpoint**: `GET /api/v1/oauth/google/callback`
- **Authentication**: None required (uses authorization code)
- **What it does**: Exchanges authorization code for access token and user info

### 4. **GitLab OAuth Flow**

#### **Step 1: Get GitLab Login URL** - `GitLab OAuth - Step 1 Get Login URL.bru`
- **Purpose**: Get GitLab authorization URL
- **Endpoint**: `GET /api/v1/oauth/gitlab/challenge`
- **Authentication**: None required
- **What it does**: Returns GitLab OAuth authorization URL

#### **Step 2: Test GitLab Callback** - `GitLab OAuth - Step 2 Test Callback.bru`
- **Purpose**: Complete GitLab OAuth flow with authorization code
- **Endpoint**: `GET /api/v1/oauth/gitlab/callback`
- **Authentication**: None required (uses authorization code)
- **What it does**: Exchanges authorization code for access token and user info

## 🚀 How to Test OAuth Flows

### **Step 1: Check Available Providers**
1. Run **"Get OAuth Providers"** to see all supported providers
2. Verify the response contains the providers you expect

### **Step 2: Test Provider Authorization URL**
1. Run the **"Step 1"** request for your chosen provider (e.g., GitHub)
2. Copy the authorization URL from the response
3. Verify the URL contains the correct provider domain

### **Step 3: Complete OAuth Flow (Manual)**
1. Open the authorization URL in your browser
2. Sign in with the OAuth provider
3. Grant permissions when prompted
4. Copy the `code` parameter from the redirect URL
5. Replace `YOUR_REAL_*_CODE_HERE` in the **"Step 2"** request
6. Run the **"Step 2"** request to complete the flow

### **Step 4: Verify Token Response**
1. Check that the response contains user data and tokens
2. Verify tokens are stored in environment variables
3. Use the tokens for subsequent authenticated requests

## 🔧 Configuration Required

Before testing, ensure your service is configured with OAuth credentials:

### **OAuth Configuration in appsettings.json**
```json
{
  "OAuth": {
    "Google": {
      "Enabled": true,
      "ClientId": "your_google_client_id",
      "ClientSecret": "your_google_client_secret"
    },
    "GitHub": {
      "Enabled": true,
      "ClientId": "your_github_client_id",
      "ClientSecret": "your_github_client_secret"
    },
    "GitLab": {
      "Enabled": true,
      "ClientId": "your_gitlab_client_id",
      "ClientSecret": "your_gitlab_client_secret"
    }
  }
}
```

**Note**: The redirect URIs are hardcoded in the controllers:
- GitHub: `http://localhost:5000/api/v1/oauth/github/callback`
- Google: `http://localhost:5000/api/v1/oauth/google/callback`
- GitLab: `http://localhost:5000/api/v1/oauth/gitlab/callback`

## 🎯 Expected Results

### **Authorization URL Response:**
```json
{
  "success": true,
  "message": "Github login link generated successfully!",
  "data": {
    "Url": "https://github.com/login/oauth/authorize?client_id=...&redirect_uri=...&scope=user:email%20read:user%20repo%20read:org%20read:public_key%20read:gpg_key&state=...&allow_signup=true"
  }
}
```

### **OAuth Callback Response:**
```json
{
  "success": true,
  "message": "User with email user@example.com logged in successfully!",
  "data": {
    "Token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "RefreshToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
    "Expiration": "2024-01-XX...",
    "UserId": "guid",
    "Username": "username",
    "SessionId": "guid",
    "ValidTill": "2024-01-XX..."
  }
}
```

### **Providers List Response:**
```json
{
  "success": true,
  "message": "Providers fetched successfully",
  "data": ["Google", "GitHub", "GitLab"]
}
```

## 🔍 Troubleshooting

### **Authorization URL Not Working:**
- Check if OAuth credentials are configured correctly
- Verify the service is running and accessible
- Ensure the redirect URI is properly configured

### **OAuth Callback Fails:**
- Verify the authorization code is valid and not expired
- Check if the OAuth provider credentials are correct
- Ensure the redirect URI matches what's configured in the OAuth provider

### **Token Generation Fails:**
- Verify user creation/login logic is working
- Check if JWT configuration is properly set up
- Ensure the OAuth provider returned valid user information

## 🌟 Benefits of OAuth Integration

1. **Enhanced User Experience**: Users can sign in with existing accounts
2. **Reduced Friction**: No need to create new accounts
3. **Trusted Authentication**: Leverages established OAuth providers
4. **Security**: OAuth 2.0 standard for secure authentication
5. **User Profile Data**: Access to user information from OAuth providers

## 🔄 Integration with Your App

Once OAuth is working, your frontend can:
1. Redirect users to the authorization URL
2. Handle the callback with the authorization code
3. Exchange the code for tokens via your API
4. Use the tokens for authenticated requests
5. Provide seamless social login experience

This creates a complete OAuth authentication system! 🚀
