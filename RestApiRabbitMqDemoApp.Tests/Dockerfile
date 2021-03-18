#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["RestApiRabbitMqDemoApp.Tests/RestApiRabbitMqDemoApp.Tests.csproj", "RestApiRabbitMqDemoApp.Tests/"]
COPY ["RestApiRabbitMqDemoApp/RestApiRabbitMqDemoApp.csproj", "RestApiRabbitMqDemoApp/"]
RUN dotnet restore "RestApiRabbitMqDemoApp.Tests/RestApiRabbitMqDemoApp.Tests.csproj"
COPY . .
WORKDIR "/src/RestApiRabbitMqDemoApp.Tests"
RUN dotnet build "RestApiRabbitMqDemoApp.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RestApiRabbitMqDemoApp.Tests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RestApiRabbitMqDemoApp.Tests.dll"]