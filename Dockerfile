FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TaskFlow.API/*.csproj ./TaskFlow.API/
RUN dotnet restore ./TaskFlow.API/TaskFlow.API.csproj

COPY . .
WORKDIR /src/TaskFlow.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]