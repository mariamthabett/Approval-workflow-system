# ============================================================
# Backend container — build + run the ASP.NET Core API.
# Works on Render, Railway, Fly.io, Azure Container Apps, etc.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore MyProject/MyProject.csproj
RUN dotnet publish MyProject/MyProject.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

# Zero-install SQLite provider: the schema is created and demo data seeded on boot.
# (Swap to SqlServer + a real connection string for a persistent production database.)
ENV Database__Provider=Sqlite
ENV ASPNETCORE_ENVIRONMENT=Production

# Hosts like Render/Railway inject $PORT; default to 8080 for a plain `docker run`.
ENV PORT=8080
EXPOSE 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT} dotnet MyProject.dll"]
