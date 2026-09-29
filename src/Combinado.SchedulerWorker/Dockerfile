# Build a partir da raiz do repositório:
#   docker build -f src/Combinado.SchedulerWorker/Dockerfile -t combinado-scheduler-worker .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/Combinado.Domain/Combinado.Domain.csproj                     src/Combinado.Domain/
COPY src/Combinado.Application/Combinado.Application.csproj           src/Combinado.Application/
COPY src/Combinado.Infrastructure/Combinado.Infrastructure.csproj     src/Combinado.Infrastructure/
COPY src/Combinado.SchedulerWorker/Combinado.SchedulerWorker.csproj   src/Combinado.SchedulerWorker/
RUN dotnet restore src/Combinado.SchedulerWorker/Combinado.SchedulerWorker.csproj

COPY src/ src/
RUN dotnet publish src/Combinado.SchedulerWorker/Combinado.SchedulerWorker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
ENV TZ=America/Sao_Paulo
USER $APP_UID
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Combinado.SchedulerWorker.dll"]
