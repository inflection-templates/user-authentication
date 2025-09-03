# Code Organization

This section provides an overview of the code organization in the project.

The code organization supports the development of a modular and maintainable codebase. It is possible that whole modules can be replaceable with the alternative provider implementations.
For example, the EFCore based database module can be replaced with a MongoDB or NHibernate module. But the repository interfaces and the service classes will remain the same. This allows the code to be more maintainable and testable.

The code organization can be depicted as follows:

```plaintext
project
├── shala.api/ (Main API folder)
│   ├── shala.api.csproj
│   ├── api/
│   │   ├── books/
│   │   │   ├── BookController.cs
│   │   │   ├── BookRoutes.cs
│   │   │   ├── BookValidator.cs
│   │   ├── authors/
│   │   │   ├── AuthorController.cs
│   │   │   ├── AuthorRoutes.cs
│   │   │   ├── AuthorValidator.cs
│   │   ├── ...
│   │   ├── ControllerInjector.cs
│   │   ├── Router.cs
│   │   ├── Validator.cs
│   ├── domain.types/
│   │   ├── enums
│   │   ├── types
│   │   ├── ...
│   ├── cache/
│   │   ├── ...
│   ├── event.messaging/
│   │   ├── ...
│   ├── modules
│   │   ├── <module-name>/
│   │   │   ├── providers/
│   │   │   │   ├── ProviderAService.cs
│   │   │   │   ├── ProviderBService.cs
│   │   │   ├── interfaces/
│   │   │   │   ├── ...
│   │   │   ├── <module-name>Service.cs
│   │   ├── ...
│   ├── services/
│   │   ├── ...
│   ├── database/
│   │   ├── interfaces/
│   │   │   ├── models/
│   │   │   ├── repositories/
│   │   ├── mappers/
│   │   ├── <orm>/
│   │   │   ├── models/
│   │   │   ├── repositories/
│   │   │   ├── migrations/
│   │   │   ├── <orm>Injector.cs
│   │   │   ├── DatabaseInitializer.cs
│   │   │   ├── <orm>Context.cs
│   │   ├── repositories/
│   │   │   ├── ...
│   ├── startup/
│   │   ├── configurations/
│   │   ├── middlewares/
│   │   ├── scheduler/
│   │   ├── Seeder.cs
│   │   ├── ...
│   ├── common/
│   │   ├── ...
│   ├── static.content/
│   │   ├── ...
│   ├── appsettings.Development.json
│   ├── appsettings.json
├── shala.api.integration.tests/ (Integration tests folder)
│   ├── shala.api.integration.tests.csproj
│   ├── common/
│   ├── tests/
│   │   ├── ...
├── api.clients/ (API clients folder)
│   ├── bruno (Bruno collection folder)
│   ├── postman (Postman collection and environment)
├── docs/ (Documentation folder)
├── .gitignore
├── Dockerfile
|── README.md
```
