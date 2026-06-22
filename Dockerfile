FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Unstapp/Unstapp.API/Unstapp.API.csproj"
RUN dotnet publish "Unstapp/Unstapp.API/Unstapp.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Unstapp.API.dll"]