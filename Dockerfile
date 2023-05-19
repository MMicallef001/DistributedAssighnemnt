FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY ordersMicroservice/ordersMicroservice.csproj ordersMicroservice/
COPY Common/Common.csproj Common/
RUN dotnet restore ordersMicroservice/ordersMicroservice.csproj

COPY . ./
RUN dotnet publish ordersMicroservice -c Release -o out
 
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ordersMicroservice.dll"]