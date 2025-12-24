FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5087

ENV ASPNETCORE_URLS=http://+:5087

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["dailycue-api.csproj", "./"]
RUN dotnet tool restore
RUN dotnet restore "dailycue-api.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "dailycue-api.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "dailycue-api.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dailycue-api.dll"]
