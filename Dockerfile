FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY productCatalougeMicroservice/productCatalougeMicroservice.csproj productCatalougeMicroservice/
COPY Common/Common.csproj Common/
RUN dotnet restore productCatalougeMicroservice/productCatalougeMicroservice.csproj

COPY . ./
RUN dotnet publish productCatalougeMicroservice -c Release -o out
 
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "productCatalougeMicroservice.dll"]