# specify your base image to use for any subsequent instructions that follοw
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory for any subsequent ADD, COPY, CMD, ENTRYPOINT, 
# or RUN instructions that follow it in the Dockerfile
WORKDIR /app

# define the network ports that this container will listen on at runtime
EXPOSE 8080

# copy csproj into working directory
COPY *.csproj ./

# execute restore command as a new layer
RUN dotnet restore

# copy everything into working directory
COPY . ./

# execute publish command
RUN dotnet publish -c Release -o out

# specify the base image without sdk
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Set the working directory again because we set a new image
WORKDIR /app

# with --from we use the previous build stage as the source location instead of the build's context
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "api.dll"]