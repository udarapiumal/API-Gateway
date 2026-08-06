# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ReverseProxy.csproj", "./"]
RUN dotnet restore "ReverseProxy.csproj"
COPY . .
RUN dotnet publish "ReverseProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 7097
EXPOSE 80
ENTRYPOINT ["dotnet", "ReverseProxy.dll"]