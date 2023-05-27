FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY Samples/Panama.Samples.TestConsole/ ./Samples/Panama.Samples.TestConsole/
COPY Panama.Canal/ ./Panama.Canal/
RUN dotnet restore ./Samples/Panama.Samples.TestConsole/Panama.Samples.TestConsole.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "Panama.Samples.TestConsole.dll"]