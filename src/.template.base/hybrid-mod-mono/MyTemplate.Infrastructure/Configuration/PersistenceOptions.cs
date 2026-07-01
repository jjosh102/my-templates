using Microsoft.Extensions.Configuration;

namespace MyTemplate.Infrastructure.Configuration;

public sealed class PersistenceOptions
{
    public const string SectionName = "Infrastructure:Persistence";
    public const string DefaultConnectionStringName = "database";
    public const string DefaultInMemoryDatabaseName = "MyTemplateDb";

    public string ConnectionStringName { get; init; } = DefaultConnectionStringName;
    public string InMemoryDatabaseName { get; init; } = DefaultInMemoryDatabaseName;

    public static PersistenceOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        return new PersistenceOptions
        {
            ConnectionStringName = section[nameof(ConnectionStringName)] ?? DefaultConnectionStringName,
            InMemoryDatabaseName = section[nameof(InMemoryDatabaseName)] ?? DefaultInMemoryDatabaseName
        };
    }
}
