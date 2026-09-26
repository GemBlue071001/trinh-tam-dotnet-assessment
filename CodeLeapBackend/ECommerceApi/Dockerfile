FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CodeLeapBackend/Domain/Domain.csproj CodeLeapBackend/Domain/
COPY CodeLeapBackend/Application/Application.csproj CodeLeapBackend/Application/
COPY CodeLeapBackend/Infrastructure/Infrastructure.csproj CodeLeapBackend/Infrastructure/
COPY CodeLeapBackend/ECommerceApi/ECommerceApi.csproj CodeLeapBackend/ECommerceApi/
RUN dotnet restore CodeLeapBackend/ECommerceApi/ECommerceApi.csproj

COPY CodeLeapBackend/ CodeLeapBackend/
RUN dotnet publish CodeLeapBackend/ECommerceApi/ECommerceApi.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerceApi.dll"]
