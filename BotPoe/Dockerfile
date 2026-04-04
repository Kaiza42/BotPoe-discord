FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BotPoe/ . 

RUN dotnet restore "BotPoe.csproj"
RUN dotnet publish "BotPoe.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "BotPoe.dll"]