# Build a partir da raiz do repositório:
#   docker build -f src/Combinado.WhatsAppWorker/Dockerfile -t combinado-whatsapp-worker .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Combinado.Domain/Combinado.Domain.csproj                     src/Combinado.Domain/
COPY src/Combinado.Application/Combinado.Application.csproj           src/Combinado.Application/
COPY src/Combinado.Infrastructure/Combinado.Infrastructure.csproj     src/Combinado.Infrastructure/
COPY src/Combinado.WhatsAppWorker/Combinado.WhatsAppWorker.csproj     src/Combinado.WhatsAppWorker/
RUN dotnet restore src/Combinado.WhatsAppWorker/Combinado.WhatsAppWorker.csproj

COPY src/ src/
RUN dotnet publish src/Combinado.WhatsAppWorker/Combinado.WhatsAppWorker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
ENV TZ=America/Sao_Paulo
USER $APP_UID
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Combinado.WhatsAppWorker.dll"]
