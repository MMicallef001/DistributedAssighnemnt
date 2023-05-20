FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY ECommerceApp/ECommerceApp.csproj ECommerceApp/
COPY Common/Common.csproj Common/
RUN dotnet restore ECommerceApp/ECommerceApp.csproj

COPY . ./
RUN dotnet publish ECommerceApp -c Release -o out
 
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ECommerceApp.dll"]