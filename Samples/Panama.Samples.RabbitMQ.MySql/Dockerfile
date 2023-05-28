FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY Samples/Panama.Samples.RabbitMQ.MySql/ ./Samples/Panama.Samples.RabbitMQ.MySql/
COPY Panama.Canal/ ./Panama.Canal/
COPY Panama.Canal.MySQL/ ./Panama.Canal.MySQL/
COPY Panama.Canal.RabbitMQ/ ./Panama.Canal.RabbitMQ/
RUN dotnet restore ./Samples/Panama.Samples.RabbitMQ.MySql/Panama.Samples.RabbitMQ.MySql.csproj

COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

RUN apt-get update 
RUN apt-get --yes install curl

EXPOSE 5296

ENTRYPOINT ["dotnet", "Panama.Samples.RabbitMQ.MySql.dll"]