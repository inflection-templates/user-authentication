# Seting Up OAuth2 Providers

To enable OAuth2 authentication in your application, you need to register your application with the desired OAuth2 provider(s) and obtain the necessary credentials. Here are the general steps to set up OAuth2 providers for common services:

## Google

1. Go to the [Google Cloud Console](https://console.cloud.google.com/).
2. Create a new project or select an existing one.
3. Navigate to **APIs & Services > Credentials**.
4. Click **Create Credentials > OAuth client ID**.
5. Select **Web application** and set authorized redirect URIs (e.g., `https://localhost:<port>/api/v1/oauth/google/callback`).
6. Note the **Client ID** and **Client Secret**, which needs to be added to your application configuration.

## GitHub

- Reference:
    [GitHub: Creating an OAuth app](https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/creating-an-oauth-app)

1. Go to 'Settings' in your GitHub account.
2. In the left sidebar, click 'Developer settings'.
3. Click **New OAuth App** or **Register a new application**.
4. Fill in the application details and set the Authorization callback URL (e.g., `https://localhost:<port>/api/v1/oauth/github/callback`).
5. Register the application and note the **Client ID** and **Client Secret**.

## GitLab

- References:
    A. [GitLab: OAuth2 applications.](https://docs.gitlab.com/ee/api/oauth2.html)
    B. [Configure GitLab as an OAuth 2.0 authentication identity provider.](https://docs.gitlab.com/ee/integration/oauth_provider.html)

1. Log in to your GitLab account.
2. To create a new application for your user:
   - Go to **Settings > Applications**.
   - Click **New application**.
   - Fill in the application details and set the Redirect URI (e.g., `https://localhost:<port>/api/v1/oauth/gitlab/callback`).
   - Select required Scopes and register the application to obtain the **Application ID** and **Secret**.
   - Note the **Application ID** and **Secret**.
   - Save the application.

## Okta

1. Sign up or log in to [Okta Developer Console](https://developer.okta.com/).
2. Create a new application and choose **Web** as the platform.
3. Set the Login redirect URI (e.g., `https://localhost:<port>/api/v1/oauth/okta/callback`).
4. Note the **Client ID**, **Client Secret**, and **Issuer URL**.
5. Save the application.
6. Add the **Client ID**, **Client Secret**, and **Issuer URL** to your application configuration.
7. Enable the required scopes and grant types.
8. Save the changes.
9. Test the OAuth2 flow.

## AWS Cognito

- References:
    A. [AWS Cognito: Getting started with user pools](https://docs.aws.amazon.com/cognito/latest/developerguide/getting-started-with-cognito-user-pools.html)
    B. [How to use OAuth 2.0 in Amazon Cognito](https://aws.amazon.com/blogs/security/how-to-use-oauth-2-0-in-amazon-cognito-learn-about-the-different-oauth-2-0-grants/)

1. Go to the AWS Cognito Console.
2. Create a new User Pool or select an existing one.
3. Navigate to App clients and create a new app client.
4. Configure OAuth2 settings with callback URLs (e.g., `https://localhost:<port>/api/v1/oauth/cognito/callback`).
5. Note the App client ID, App client secret, and User Pool Domain.

## Azure AD

- References:
    A. [Azure AD: Quickstart](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
    B. [Register apps in Azure Active Directory B2C](https://learn.microsoft.com/en-us/azure/active-directory-b2c/register-apps)
    C. [Add a web API application to your Azure Active Directory B2C tenant](https://learn.microsoft.com/en-us/azure/active-directory-b2c/add-web-api-application)

1. Go to the Azure Portal and sign in.
2. Navigate to Azure Active Directory > App registrations.
3. Click New registration.
4. Create an App Registration for the Client App
5. Set the Redirect URI (e.g., `https://localhost:<port>/api/v1/oauth/azuread/callback`).
6. Register the application and note the scopes, the Application (client) ID, Directory (tenant) ID, and Client Secret.

## Facebook

- Reference:
    [Facebook for Developers: Getting Started](https://developers.facebook.com/docs/development/create-an-app)

1. Go to the [Facebook for Developers](https://developers.facebook.com/) portal.
2. Create a new app.
3. Set the OAuth redirect URI (e.g., `https://localhost:<port>/api/v1/oauth/facebook/callback`).
4. Note the App ID and App Secret.
5. Save the changes.
6. Add the App ID and App Secret to your application configuration.
7. Enable the required scopes and grant types.
8. Save the changes.
9. Test the OAuth2 flow.
10. Make the app public.
11. Submit the app for review.

## Twitter

- References:
  A. [Log in with X](https://developer.x.com/en/docs/authentication/guides/log-in-with-twitter)
  B. [Twitter Developer: Getting Started](https://developer.x.com/en/docs/authentication/oauth-2-0)
