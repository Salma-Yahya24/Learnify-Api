# ===== Base image =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# ===== Build image =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ننسخ كل ملفات السوليوشن بالكامل (DAL و Entities و Web API)
COPY . .

# نروح لمجلد المشروع الأساسي
WORKDIR /src/Learnify
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# ===== Final image =====
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Learnify.dll"]
