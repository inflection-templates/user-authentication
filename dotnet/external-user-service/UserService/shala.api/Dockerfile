# Use the official .NET Core SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY shala.api.csproj ./
RUN dotnet restore

# Run dotnet build to compile the code
COPY . ./
RUN dotnet build -c Release

# Copy the rest of the application code and build the app
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET Core runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 5000

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

# Make docker container run on custom port
# ENV ASPNETCORE_URLS=http://0.0.0.0:5089

# Run the application
ENTRYPOINT ["dotnet", "shala.api.dll"]
