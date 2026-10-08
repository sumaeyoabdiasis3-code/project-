FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV ConnectionStrings__DefaultConnection="Data Source=/data/PFDETBM.db"
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PFDETBM.csproj", "./"]
RUN dotnet restore "PFDETBM.csproj"
COPY . .
RUN dotnet publish "PFDETBM.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["sh", "-c", "exec dotnet PFDETBM.dll --urls http://0.0.0.0:${PORT:-8080}"]