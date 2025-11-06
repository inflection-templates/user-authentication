# Local Machine Docker Deployment

This guide will help you to deploy and test shala-api service in docker container on local machine.

## Prerequisites

1. [Docker Desktop](https://docs.docker.com/desktop/setup/install/windows-install/)

## Setup

1. Clone the repository OR unzip the downloaded the source code

```bash
    git clone https://github.com/<your-github-acccount>/shala.git
    cd shala
```

2. Open `appsettings.docker.json` and update the relevant fields, options, secrets and connection strings. Save the file as `appsettings.json`.

3. Open Dockerfile and update `ENV ASPNETCORE_URLS=http://0.0.0.0:5000` if want to run application on any specific port.

>**Note**: If you don't configure ASPNETCORE_URLS in your Dockerfile, the .NET application will default to using Kestrel's built-in behavior, which typically listens on http://localhost:5000 inside the container by default. This effectively makes the application accessible only from inside the container itself. It won't be accessible from the host machine or other containers.

4. Build docker image. Go to the main project directory (shala.api) and run the following.

```bash
    docker build -t shala-api .
```

5. Run Docker Container.

```bash
    docker run -p 5000:5000 -d shala-api
```

6. Open Bruno and import the `bruno` folder from the `api.clients` folder.

7. Start testing the API endpoints.
