using FluentAssertions;
using Npgsql;

namespace PetMach.Api.IntegrationTests;

public sealed class PostgreSqlConcurrencyTests
{
    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task ActiveReservationsShouldRejectConcurrentDuplicateAvailability()
    {
        string? connectionString = Environment.GetEnvironmentVariable("PETMACH_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        Guid availabilityId = Guid.NewGuid();
        await using NpgsqlConnection first = new(connectionString);
        await using NpgsqlConnection second = new(connectionString);
        await first.OpenAsync();
        await second.OpenAsync();
        await using NpgsqlTransaction firstTransaction = await first.BeginTransactionAsync();
        await using NpgsqlTransaction secondTransaction = await second.BeginTransactionAsync();
        await SetReplicaRoleAsync(first, firstTransaction);
        await SetReplicaRoleAsync(second, secondTransaction);

        await InsertReservationAsync(first, firstTransaction, availabilityId);
        Task<int> competingInsert = InsertReservationAsync(second, secondTransaction, availabilityId);
        await Task.Delay(100);
        await firstTransaction.CommitAsync();

        Func<Task> act = async () => _ = await competingInsert;
        PostgresException exception = (await act.Should().ThrowAsync<PostgresException>()).Which;
        exception.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        await secondTransaction.RollbackAsync();
        await DeleteReservationAsync(connectionString, availabilityId);
    }

    [Fact]
    [Trait("Category", "PostgreSQL")]
    public async Task AdoptionProfileShouldRejectConcurrentMultipleApprovals()
    {
        string? connectionString = Environment.GetEnvironmentVariable("PETMACH_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        Guid profileId = Guid.NewGuid();
        await using NpgsqlConnection first = new(connectionString);
        await using NpgsqlConnection second = new(connectionString);
        await first.OpenAsync();
        await second.OpenAsync();
        await using NpgsqlTransaction firstTransaction = await first.BeginTransactionAsync();
        await using NpgsqlTransaction secondTransaction = await second.BeginTransactionAsync();
        await SetReplicaRoleAsync(first, firstTransaction);
        await SetReplicaRoleAsync(second, secondTransaction);

        await InsertApplicationAsync(first, firstTransaction, profileId);
        Task<int> competingInsert = InsertApplicationAsync(second, secondTransaction, profileId);
        await Task.Delay(100);
        await firstTransaction.CommitAsync();

        Func<Task> act = async () => _ = await competingInsert;
        PostgresException exception = (await act.Should().ThrowAsync<PostgresException>()).Which;
        exception.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
        await secondTransaction.RollbackAsync();
        await DeleteApplicationAsync(connectionString, profileId);
    }

    private static async Task SetReplicaRoleAsync(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        await using NpgsqlCommand command = new("SET LOCAL session_replication_role = replica", connection, transaction);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> InsertReservationAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid availabilityId)
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
        return await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> InsertApplicationAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, Guid profileId)
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
        return await command.ExecuteNonQueryAsync();
    }

    private static async Task DeleteReservationAsync(string connectionString, Guid availabilityId)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand role = new("SET session_replication_role = replica", connection);
        await role.ExecuteNonQueryAsync();
        await using NpgsqlCommand delete = new("DELETE FROM reservations.\"Reservations\" WHERE \"AvailabilityId\" = @id", connection);
        delete.Parameters.AddWithValue("id", availabilityId);
        await delete.ExecuteNonQueryAsync();
    }

    private static async Task DeleteApplicationAsync(string connectionString, Guid profileId)
    {
        await using NpgsqlConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using NpgsqlCommand role = new("SET session_replication_role = replica", connection);
        await role.ExecuteNonQueryAsync();
        await using NpgsqlCommand delete = new("DELETE FROM adoption.\"Applications\" WHERE \"ProfileId\" = @id", connection);
        delete.Parameters.AddWithValue("id", profileId);
        await delete.ExecuteNonQueryAsync();
    }
}
