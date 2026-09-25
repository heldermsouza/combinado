# Build a partir da raiz do repositório (precisa de Directory.*.props e global.json):
#   docker build -f src/Combinado.Api/Dockerfile -t combinado-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore em camada separada: só invalida quando csproj/props mudam.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Combinado.Domain/Combinado.Domain.csproj                 src/Combinado.Domain/
COPY src/Combinado.Application/Combinado.Application.csproj       src/Combinado.Application/
COPY src/Combinado.Infrastructure/Combinado.Infrastructure.csproj src/Combinado.Infrastructure/
COPY src/Combinado.Api/Combinado.Api.csproj                       src/Combinado.Api/
RUN dotnet restore src/Combinado.Api/Combinado.Api.csproj

COPY src/ src/
RUN dotnet publish src/Combinado.Api/Combinado.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=0 \
    TZ=America/Sao_Paulo
EXPOSE 8080
USER $APP_UID
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Combinado.Api.dll"]
