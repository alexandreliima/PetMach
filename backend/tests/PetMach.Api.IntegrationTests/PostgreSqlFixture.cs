using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PetMach.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PetMach.Api.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PostgreSqlTestCollectionDefinition : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private const string PostgreSqlImage = "postgres:18.0-alpine";
    private static readonly TimeSpan InitializationTimeout = TimeSpan.FromMinutes(3);
    private readonly string password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
    private PostgreSqlContainer? container;

    public string ConnectionString => container?.GetConnectionString()
        ?? throw new InvalidOperationException("O container PostgreSQL de testes ainda não foi inicializado.");

    public async Task InitializeAsync()
    {
        using CancellationTokenSource timeout = new(InitializationTimeout);
        try
        {
            container = new PostgreSqlBuilder(PostgreSqlImage)
                .WithDatabase("petmach_tests")
                .WithUsername("petmach_tests")
                .WithPassword(password)
                .Build();

            await container.StartAsync(timeout.Token);
            await using PetMachDbContext dbContext = CreateDbContext();
            await dbContext.Database.MigrateAsync(timeout.Token);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"Não foi possível iniciar o PostgreSQL de testes com a imagem '{PostgreSqlImage}'. " +
                "Os testes de persistência exigem um daemon Docker acessível e não são considerados aprovados sem ele.",
                exception);
        }
    }

    public async Task DisposeAsync()
    {
        if (container is not null)
        {
            await container.DisposeAsync();
        }
    }

    public PetMachDbContext CreateDbContext()
    {
        DbContextOptions<PetMachDbContext> options = new DbContextOptionsBuilder<PetMachDbContext>()
            .UseNpgsql(
                ConnectionString,
                npgsql => npgsql.MigrationsAssembly(typeof(PetMachDbContext).Assembly.FullName))
            .Options;

        return new PetMachDbContext(options);
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            DO $$
            DECLARE
                tables_to_truncate text;
            BEGIN
                SELECT string_agg(format('%I.%I', schemaname, tablename), ', ')
                INTO tables_to_truncate
                FROM pg_tables
                WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
                  AND tablename <> '__EFMigrationsHistory';

                IF tables_to_truncate IS NOT NULL THEN
                    EXECUTE 'TRUNCATE TABLE ' || tables_to_truncate || ' RESTART IDENTITY CASCADE';
                END IF;
            END
            $$;
            """;

        await using NpgsqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
