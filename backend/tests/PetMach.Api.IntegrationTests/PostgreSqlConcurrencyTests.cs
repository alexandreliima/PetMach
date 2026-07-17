using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PetMach.Api.IntegrationTests;

[Collection(PostgreSqlTestCollectionDefinition.Name)]
public sealed class PostgreSqlConcurrencyTests(PostgreSqlFixture fixture) : IAsyncLifetime
{
    private static readonly TimeSpan ConcurrentWriteDelay = TimeSpan.FromMilliseconds(100);

    public Task InitializeAsync() => fixture.ResetDatabaseAsync(CancellationToken.None);

    public Task DisposeAsync() => fixture.ResetDatabaseAsync(CancellationToken.None);

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task ContainerShouldProvideARealPostgreSqlConnection()
    {
        await using NpgsqlConnection connection = new(fixture.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);
        await using NpgsqlCommand command = new("SELECT version()", connection);

        string? version = (string?)await command.ExecuteScalarAsync(CancellationToken.None);

        version.Should().StartWith("PostgreSQL 18");
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task AllMigrationsShouldBeAppliedFromScratch()
    {
        await using var dbContext = fixture.CreateDbContext();

        string[] knownMigrations = dbContext.Database.GetMigrations().ToArray();
        string[] appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync(CancellationToken.None)).ToArray();
        string[] pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(CancellationToken.None)).ToArray();

        knownMigrations.Should().HaveCount(18);
        appliedMigrations.Should().Equal(knownMigrations);
        pendingMigrations.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task ResetShouldRemoveTestDataAndPreserveMigrations()
    {
        Guid availabilityId = Guid.NewGuid();
        await using NpgsqlConnection connection = new(fixture.ConnectionString);
        await connection.OpenAsync(CancellationToken.None);
        await SetReplicaRoleAsync(connection);
        await InsertReservationAsync(connection, null, availabilityId);

        (await CountReservationsAsync(connection)).Should().Be(1);

        await fixture.ResetDatabaseAsync(CancellationToken.None);

        (await CountReservationsAsync(connection)).Should().Be(0);
        await using var dbContext = fixture.CreateDbContext();
        (await dbContext.Database.GetAppliedMigrationsAsync(CancellationToken.None)).Should().HaveCount(18);
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task ActiveReservationsShouldRejectConcurrentDuplicateAvailability()
    {
        Guid availabilityId = Guid.NewGuid();
        await using NpgsqlConnection first = new(fixture.ConnectionString);
        await using NpgsqlConnection second = new(fixture.ConnectionString);
        await first.OpenAsync(CancellationToken.None);
        await second.OpenAsync(CancellationToken.None);
        await using NpgsqlTransaction firstTransaction = await first.BeginTransactionAsync(CancellationToken.None);
        await using NpgsqlTransaction secondTransaction = await second.BeginTransactionAsync(CancellationToken.None);
        await SetReplicaRoleAsync(first, firstTransaction);
        await SetReplicaRoleAsync(second, secondTransaction);

        await InsertReservationAsync(first, firstTransaction, availabilityId);
        Task<int> competingInsert = InsertReservationAsync(second, secondTransaction, availabilityId);
        await Task.Delay(ConcurrentWriteDelay);
        await firstTransaction.CommitAsync(CancellationToken.None);

        Func<Task> act = async () => _ = await competingInsert;
        PostgresException exception = (await act.Should().ThrowAsync<PostgresException>()).Which;
        exception.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        exception.ConstraintName.Should().Be("IX_Reservations_AvailabilityId");
        await secondTransaction.RollbackAsync(CancellationToken.None);
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task AdoptionProfileShouldRejectConcurrentMultipleApprovals()
    {
        Guid profileId = Guid.NewGuid();
        await using NpgsqlConnection first = new(fixture.ConnectionString);
        await using NpgsqlConnection second = new(fixture.ConnectionString);
        await first.OpenAsync(CancellationToken.None);
        await second.OpenAsync(CancellationToken.None);
        await using NpgsqlTransaction firstTransaction = await first.BeginTransactionAsync(CancellationToken.None);
        await using NpgsqlTransaction secondTransaction = await second.BeginTransactionAsync(CancellationToken.None);
        await SetReplicaRoleAsync(first, firstTransaction);
        await SetReplicaRoleAsync(second, secondTransaction);

        await InsertApplicationAsync(first, firstTransaction, profileId);
        Task<int> competingInsert = InsertApplicationAsync(second, secondTransaction, profileId);
        await Task.Delay(ConcurrentWriteDelay);
        await firstTransaction.CommitAsync(CancellationToken.None);

        Func<Task> act = async () => _ = await competingInsert;
        PostgresException exception = (await act.Should().ThrowAsync<PostgresException>()).Which;
        exception.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        exception.ConstraintName.Should().Be("IX_Applications_ProfileId");
        await secondTransaction.RollbackAsync(CancellationToken.None);
    }

    private static async Task SetReplicaRoleAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction = null)
    {
        await using NpgsqlCommand command = new(
            "SET session_replication_role = replica",
            connection,
            transaction);
        await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private static async Task<int> InsertReservationAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        Guid availabilityId)
    {
        const string sql = """
            INSERT INTO reservations."Reservations"
                ("Id", "AvailabilityId", "RequesterUserId", "DogId", "Status", "PaymentStatus", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES (@id, @availabilityId, @userId, @dogId, 'Pending', 'AwaitingOnSite', now(), now())
            """;
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("availabilityId", availabilityId);
        command.Parameters.AddWithValue("userId", Guid.NewGuid());
        command.Parameters.AddWithValue("dogId", Guid.NewGuid());
        return await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private static async Task<int> InsertApplicationAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid profileId)
    {
        const string sql = """
            INSERT INTO adoption."Applications"
                ("Id", "ProfileId", "ApplicantUserId", "Motivation", "Experience", "HousingContext", "TermsVersion",
                 "TermsAcceptedAtUtc", "Status", "CreatedAtUtc", "UpdatedAtUtc")
            VALUES (@id, @profileId, @userId, 'Motivação', 'Experiência', 'Lar seguro', '2026-07-16',
                    now(), 'Approved', now(), now())
            """;
        await using NpgsqlCommand command = new(sql, connection, transaction);
        command.Parameters.AddWithValue("id", Guid.NewGuid());
        command.Parameters.AddWithValue("profileId", profileId);
        command.Parameters.AddWithValue("userId", Guid.NewGuid());
        return await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private static async Task<long> CountReservationsAsync(NpgsqlConnection connection)
    {
        await using NpgsqlCommand command = new(
            "SELECT count(*) FROM reservations.\"Reservations\"",
            connection);
        return (long)(await command.ExecuteScalarAsync(CancellationToken.None))!;
    }
}
