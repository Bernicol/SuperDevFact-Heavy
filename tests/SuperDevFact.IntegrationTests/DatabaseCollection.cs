using Xunit;

namespace SuperDevFact.IntegrationTests;

/// <summary>Empêche l'exécution en parallèle des tests qui partagent la base PostgreSQL de test.</summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>;
