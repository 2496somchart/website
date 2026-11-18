FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props EasySchedule.sln ./
COPY src/EasySchedule.Domain/EasySchedule.Domain.csproj src/EasySchedule.Domain/
COPY src/EasySchedule.Infrastructure/EasySchedule.Infrastructure.csproj src/EasySchedule.Infrastructure/
COPY src/EasySchedule.Solver/EasySchedule.Solver.csproj src/EasySchedule.Solver/
COPY src/EasySchedule.Api/EasySchedule.Api.csproj src/EasySchedule.Api/
RUN dotnet restore src/EasySchedule.Api/EasySchedule.Api.csproj
COPY src/EasySchedule.Domain/ src/EasySchedule.Domain/
COPY src/EasySchedule.Infrastructure/ src/EasySchedule.Infrastructure/
COPY src/EasySchedule.Solver/ src/EasySchedule.Solver/
COPY src/EasySchedule.Api/ src/EasySchedule.Api/
RUN dotnet publish src/EasySchedule.Api/EasySchedule.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    TZ=Asia/Bangkok
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "EasySchedule.Api.dll"]
