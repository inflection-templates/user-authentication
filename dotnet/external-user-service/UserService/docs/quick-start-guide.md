# Quick Start Guide

This guide will help you to get started with the Shala Learning Management Service.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL](https://www.mysql.com/) or any other relational database
- [Visual Studio Code](https://code.visualstudio.com/) or any other code editor
- [Git](https://git-scm.com/)
- [Postman](https://www.postman.com/) or [Bruno](https://www.usebruno.com/) or any other API testing tool

## Setup

1. Clone the repository OR unzip the downloaded the source code

```bash
    git clone https://github.com/<your-github-acccount>/shala.git
    cd shala
```

2. Open `appsettings.json` and update the relevant fields, options, secrets and connection strings. Save the file as `appsettings.local.json`.

3. Go to the main project directory (shala.api) and run the following.

```bash
    dotnet restore
    dotnet build
    dotnet run
```

4. If you are using postman, open Postman and import the collection from the `api.clients/postman` folder. If you are using Bruno, open Bruno and import the `bruno` folder from the `api.clients` folder.

5. Start testing the API endpoints.

## API Documentation

Based on what has been selected as API documentation tool (Swagger or Scalar), the API documentation can be accessed at the following URLs.

- Swagger: `https://localhost:<port>/swagger/index.html`
- Scalar: `https://localhost:<port>/scalar/v1`

## Run Integration Tests

1. Copy the `appsettings.json` file to `appsettings.test.json` and update the database schema name for your test database.

2. To run the integration tests, go to the `shala.tests` directory and run the following command.

```bash
    dotnet test
```
