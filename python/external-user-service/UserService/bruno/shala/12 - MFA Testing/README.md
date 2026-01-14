# 🔐 MFA (Multi-Factor Authentication) Testing Collection

This collection provides **step-by-step testing** for the Multi-Factor Authentication (MFA) system with TOTP (Time-based One-Time Password) support.

## 📋 Test Flow Overview

### **Phase 1: Setup & Registration**
1. **User Registration** - Create a new user account
2. **Check MFA Status** - Verify initial MFA state

### **Phase 2: MFA Setup**
3. **Setup TOTP** - Generate TOTP secret and QR code
4. **Get QR Code Image** - Download QR code as PNG (optional)
5. **Verify TOTP Setup** - Enable MFA with authenticator app

### **Phase 3: Verification**
6. **Confirm MFA Enabled** - Verify MFA is active

### **Phase 4: Login Testing**
7. **Login with MFA Required** - Test login flow requiring MFA
8. **Validate TOTP for Login** - Complete authentication with TOTP

### **Phase 5: Backup Codes**
9. **Test Backup Code** - Validate backup codes (optional)
10. **Generate New Backup Codes** - Create fresh backup codes (optional)
11. **Test New Backup Code** - Validate new backup codes (optional)

### **Phase 6: Cleanup**
12. **Disable MFA** - Turn off MFA (optional)
13. **Verify MFA Disabled** - Confirm MFA is disabled
14. **MFA Health Check** - Service health verification

## 🚀 Quick Start

1. **Set up environment** in `mfa-testing.bru`
2. **Run steps 1-8** for basic MFA flow
3. **Use authenticator app** for TOTP codes
4. **Follow console logs** for next steps

## 📱 Prerequisites

- **Authenticator App**: Google Authenticator, Authy, Microsoft Authenticator
- **Valid API Keys**: Configured in Bruno environment
- **Python Service**: Running on `http://localhost:5000`

## 🔧 Environment Variables

Required in `mfa-testing.bru`:
```
BASE_URL: http://localhost:5000
API_KEY: your-api-key-here
API_SECRET: your-api-secret-here
USER_EMAIL: test@example.com
USER_PASSWORD: password123
```

## 📝 Testing Notes

- **TOTP Codes**: Change every 30 seconds
- **Backup Codes**: Single-use only
- **Console Logs**: Follow step-by-step guidance
- **Error Handling**: Each step includes troubleshooting tips
- **Optional Steps**: Clearly marked for advanced testing

## 🎯 Key Features Tested

✅ User Registration & Authentication  
✅ TOTP Secret Generation & QR Codes  
✅ MFA Enable/Disable Operations  
✅ Login Flow with MFA Requirement  
✅ TOTP Code Validation  
✅ Backup Code Management  
✅ Service Health Monitoring  

## 🔍 Console Output

Each step provides detailed console output with:
- ✅ Success indicators
- ❌ Error messages  
- 💡 Helpful tips
- 🔑 Token storage confirmations
- 📱 Next step guidance
