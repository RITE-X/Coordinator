# Based on https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Coordinator/*.csproj ./Coordinator/
RUN dotnet restore -r linux-musl-x64

# copy everything else and build app
COPY Coordinator/. ./Coordinator/
WORKDIR /source/Coordinator
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained false --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["./Coordinator"]