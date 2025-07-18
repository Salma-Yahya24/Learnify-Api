# ===== Base image =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080    # غيرنا من 80 إلى 8080

# ===== Build image =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

WORKDIR /src/Learnify
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ===== Final image =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Learnify.dll"]
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
