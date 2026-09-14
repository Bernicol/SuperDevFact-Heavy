# SUPER DEV FACT — Client lourd (WPF)

Application de devis/facturation pour un artisan (Solaris Installation, installateur de
panneaux solaires) — client desktop natif WPF, construit sur un socle métier partagé
avec la version web/hybride du même produit (voir le dépôt `SuperDevFact-Hybrid`).

> POC réalisé dans un contexte de démonstration technique, en environ 8 heures de
> travail. Ce délai reflète la contrainte de l'exercice (aller vite vers un résultat
> démontrable), pas la vélocité visée en conditions de production réelles.

## Architecture

```
src/
  SuperDevFact.Domain          → règles métier pures (aucune dépendance externe)
  SuperDevFact.Application     → cas d'usage, DTOs, orchestration
  SuperDevFact.Infrastructure  → EF Core / PostgreSQL, repositories, unité de travail
  SuperDevFact.Desktop         → client WPF (MVVM, XAML, styles)
tests/
  SuperDevFact.Domain.Tests
  SuperDevFact.Application.Tests
  SuperDevFact.IntegrationTests
installer/
  SuperDevFact-Heavy.iss       → script Inno Setup (packaging Windows)
```

Le Domain et l'Application ne dépendent d'aucune techno d'interface : c'est ce même
socle qui alimente à l'identique le client hybride (Web + WebView2) livré séparément.

## Prérequis

- .NET 9 SDK
- Une base PostgreSQL accessible (locale ou distante)

## Configuration

Créer `src/SuperDevFact.Desktop/appsettings.Secrets.json` (ignoré par Git) :

```json
{
  "ConnectionStrings": {
    "Default": "Host=...;Port=5432;Database=...;Username=...;Password=..."
  }
}
```

## Lancer le projet

```bash
dotnet ef database update --project src/SuperDevFact.Infrastructure --connection "<votre chaîne de connexion>"
dotnet run --project src/SuperDevFact.Desktop
```

Le jeu de données de démonstration (clients, devis, factures, paiements) se crée
automatiquement au premier lancement si la base est vide.

## Tests

```bash
dotnet test
```

## Publier / packager

```bash
dotnet publish src/SuperDevFact.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/desktop
```

Puis compiler l'installeur avec [Inno Setup](https://jrsoftware.org/isinfo.php) :

```bash
ISCC.exe installer/SuperDevFact-Heavy.iss
```
