# BotPoe

## Description
BotPoe est un bot Discord développé en C# utilisant .NET 9. Son objectif principal est le monitoring en temps réel de l'économie du jeu Path of Exile.

Le bot surveille les variations de prix des objets de valeur (comme les Divine Orbs) et envoie des alertes automatiques sur un canal dédié afin d'informer les utilisateurs des fluctuations du marché.

## Fonctionnalités
- Monitoring automatique des prix via des services d'arrière-plan.
- Alertes configurables en cas de variation brutale de la monnaie.
- Rapport quotidien automatique à heure fixe.
- Commandes d'administration pour activer ou mettre en veille le monitoring.
- Architecture basée sur l'injection de dépendances pour une extension facile (Beasts, Essences, Belts).

## Configuration
Le projet nécessite un fichier appsettings.json à la racine avec les informations suivantes :
- BotToken : Le jeton d'accès API Discord.
- PriceAlertChannelId : L'identifiant du salon Discord pour les alertes.

## Technologies
- C# / .NET 9
- Discord.Net
- Microsoft.Extensions.Hosting