FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 9024

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY ["asa-manager/asa-manager.sln", "asa-manager/"]
COPY ["asa-manager/DeviceGroupsAgent/DeviceGroupsAgent.csproj", "asa-manager/DeviceGroupsAgent/"]
COPY ["asa-manager/Services/Services.csproj", "asa-manager/Services/"]
COPY ["asa-manager/SetupAgent/SetupAgent.csproj", "asa-manager/SetupAgent/"]
COPY ["asa-manager/TelemetryRulesAgent/TelemetryRulesAgent.csproj", "asa-manager/TelemetryRulesAgent/"]
COPY ["asa-manager/WebService/WebService.csproj", "asa-manager/WebService/"]

RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR "/src/asa-manager/WebService"
RUN dotnet build "WebService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebService.csproj" -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Microsoft.Azure.IoTSolutions.AsaManager.WebService.dll"]
