# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# On copie TOUT le dossier BotPoe dans le conteneur
COPY BotPoe/ . 

# Maintenant le .csproj est bien présent dans le dossier courant du conteneur
RUN dotnet restore "BotPoe.csproj"
RUN dotnet publish "BotPoe.csproj" -c Release -o /app/out

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "BotPoe.dll"]