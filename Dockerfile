FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY O_Dzhumyk_MefistoTheatre/O_Dzhumyk_MefistoTheatre.csproj O_Dzhumyk_MefistoTheatre/
RUN dotnet restore O_Dzhumyk_MefistoTheatre/O_Dzhumyk_MefistoTheatre.csproj
COPY O_Dzhumyk_MefistoTheatre/ O_Dzhumyk_MefistoTheatre/
RUN dotnet publish O_Dzhumyk_MefistoTheatre/O_Dzhumyk_MefistoTheatre.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "O_Dzhumyk_MefistoTheatre.dll"]
