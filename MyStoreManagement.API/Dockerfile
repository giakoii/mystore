FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 3000
EXPOSE 3001

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:3000;https://+:3001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyStoreManagement.API/MyStoreManagement.API.csproj", "MyStoreManagement.API/"]
COPY ["MyStoreManagement.Application/MyStoreManagement.Application.csproj", "MyStoreManagement.Application/"]
COPY ["MyStoreManagement.Domain/MyStoreManagement.Domain.csproj", "MyStoreManagement.Domain/"]
COPY ["MyStoreManagement.Infrastructure/MyStoreManagement.Infrastructure.csproj", "MyStoreManagement.Infrastructure/"]
RUN dotnet restore "MyStoreManagement.API/MyStoreManagement.API.csproj"
COPY . .
WORKDIR "/src/MyStoreManagement.API"
RUN dotnet build "./MyStoreManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyStoreManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyStoreManagement.API.dll"]
