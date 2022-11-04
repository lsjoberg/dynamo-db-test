FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DynamoDbTest/DynamoDbTest.csproj", "DynamoDbTest/"]
RUN dotnet restore "DynamoDbTest/DynamoDbTest.csproj"
COPY . .
WORKDIR "/src/DynamoDbTest"
RUN dotnet build "DynamoDbTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DynamoDbTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DynamoDbTest.dll"]
