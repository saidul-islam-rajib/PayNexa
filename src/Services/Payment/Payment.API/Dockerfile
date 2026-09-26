FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo
COPY . .
WORKDIR "/repo/src/Services/Payment/Payment.API"
RUN --mount=type=cache,id=paynexa-nuget,target=/root/.nuget/packages dotnet restore "./Payment.API.csproj"
RUN --mount=type=cache,id=paynexa-nuget,target=/root/.nuget/packages dotnet build "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN --mount=type=cache,id=paynexa-nuget,target=/root/.nuget/packages dotnet publish "./Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.API.dll"]
