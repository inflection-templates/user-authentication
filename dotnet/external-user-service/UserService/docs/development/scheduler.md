# Scheduler and Background Tasks

This document provides information on how to schedule and run background tasks in the application.

In many API services, it is common to have tasks that run in the background. These tasks can be scheduled to run at specific intervals or can be triggered by certain events. For example, sending emails, updating data, or processing data in the background at certain intervals.

The scheduling could be distributed across multiple instances of the application or could be in-memory in a single instance. The choice of the scheduler depends on the requirements of the application.

In this service, we are using the `Hangfire` library for scheduling background tasks. Hangfire is an open-source library that allows you to perform background processing in .NET applications. It is easy to set up and use and provides a dashboard to monitor the background tasks.
Hangfire supports multiple storage options like SQL Server, Redis, MongoDB, major SQL databases and in-memory. For local development and testing, we are using the in-memory storage option.

## Configuration

There are no specific configurations required for Hangfire. The Hangfire dashboard is available at the `/hangfire` endpoint. The dashboard provides information on the background jobs, queues, servers, and performance.

## Injections

The Hangfire is injected into the builder's service collection in the file `BuilderSchedulerExtensions.cs` in the `/startup/configurations/builder.extensions` folder.
The hangfire dashboard is enabled through

```csharp
    // Hangfire dashboard setup for background jobs
    app.UseHangfireDashboard("/hangfire");
```

You can access the Hangfire dashboard at `http://localhost:<port>/hangfire`.

## Scheduling Background Tasks

The background tasks are scheduled through `Scheduler` class defined in the `startup/scheduler` folder. The `Scheduler` class has methods to schedule
    1. Recurrent background tasks.
    2. Fire-and-Forget background tasks.
    3. Delayed background tasks.

You can add your custom scheduled tasks in the `schedules` folder within the `startup/scheduler` folder. There are examples provided for each type of background task.
