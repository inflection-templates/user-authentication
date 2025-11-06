# Integration Tests Documentation

## Introduction

This document provides an overview of the structure and configuration of integration tests for the `shala.api` project. It covers the application factory setup, xUnit runner configuration, test fixtures, collection structures, and global fixtures. These components are essential for ensuring that the integration tests are reliable, maintainable, and scalable.

## 1. Application Factory Structure

The application factory is responsible for setting up the test server and configuring the services required for integration tests. It typically inherits from `WebApplicationFactory<TStartup>` and overrides the `ConfigureWebHost` method to customize the test environment.

```csharp
public class ShalaApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Customize the services for testing
        });
    }
}
```

Here, the `ShalaApplicationFactory` class is used to create a test server for the `shala.api` project. It allows us to customize the services and configuration settings for integration tests. `TStartup` is the type of the startup class for the application which in this case is `Program` class from `shala.api`.

__Important__
To make the `Program` class accessible to the test project, the `shala.api` project must be referenced in the test project.

1. This can be done by adding a project `shala.api` reference in the test project file (`shala.api.integration.tests.csproj`).

    ```xml
    <ItemGroup>
        <ProjectReference Include="..\shala.api\shala.api.csproj" />
    </ItemGroup>
    ```

2. The `InternalsVisibleTo` attribute allows the test project to access internal classes and members of the `shala.api` project. The `Program` class is marked as internal in the `shala.api` project.

    ```xml
    <ItemGroup>
        <InternalsVisibleTo Include="shala.api.integration.tests"/>
    </ItemGroup>
    ```

## 2. xUnit Runner JSON

The `xunit.runner.json` file is used to configure the xUnit test runner. It can be placed in the root of the test project and typically includes settings like parallelism and diagnostic messages. We are not parallelizing the tests as these workflow tests are dependent on each other.

```json
{
  "parallelizeTestCollections": false,
  "diagnosticMessages": true,
  //...Other settings
}
```

## 3. Test Fixtures and Collection Structure

Test fixtures are used to share setup and cleanup code across multiple test classes. They are defined by implementing the `IClassFixture<T>` or `ICollectionFixture<T>` interfaces. Collections group related test classes together.
In this case, for all basic workflows, we have created a collection named `BasicWorkflowsCollection` with attribute named "Basic workflows" and a fixture named `BasicWorkflowsFixture` which is shared across all tests in the collection. The fixture contains the setup and cleanup code for the tests, which also stores instances for `ShalaWebApplicationFactory` and `HttpClient`.

```csharp
public class BasicWorkflowsFixture : IDisposable
{
    internal ShalaWebApplicationFactory Factory { get; private set; }
    internal HttpClient Client { get; private set; }

    public BasicWorkflowsFixture()
    {
        Factory = new ShalaWebApplicationFactory();
        Client = Factory.CreateClient();
        //...Other setup code
    }

    public void Dispose()
    {
        // Cleanup
        Client.Dispose();
        Factory.Dispose();
    }
}

[CollectionDefinition("Basic workflows")]
public class BasicWorkflowsCollection : ICollectionFixture<BasicWorkflowsFixture>
{
    // No implementation needed, just marks the collection
}
```

## 4. Global Fixture

A global fixture is used to set up resources that are shared across all tests in the assembly. Here we are using a global fixture to initialize and cleanup global resources that are required for all tests in the assembly. The global fixture implements the `IDisposable` interface to ensure proper cleanup of resources.

```csharp
public class GlobalTestFixture : IDisposable
{
    public GlobalTestFixture()
    {
        // Initialize global resources
        // We also cleanup the test database before running the tests
        //dropExistingDatabase();
    }

    public void Dispose()
    {
        // Cleanup global resources
    }
}

```

### 5. Test Class Structure

The test classes are organized based on the workflow they are testing. Each test class contains multiple test methods that cover different scenarios of the workflow. The test methods are annotated with `[Fact]` attribute to indicate that they are test methods.

```csharp
    [Collection("Basic workflows")]
    public class AdminTests
    {
        private readonly BasicWorkflowsFixture _fixture;

        public AdminTests(BasicWorkflowsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task T01_some_test()
        {
            // Test setup
            // Any test-specific data initialization

            // Use HttpClient to make requests as follows
            var response = await _fixture.Client.GetAsync("/api/...");

            // Assert response structure and status code
        }

        [Fact]
        public async Task T02_some_other_test()
        {
            // ...
        }

        // Other test methods
    }
```
