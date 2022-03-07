using Microsoft.Data.SqlClient;

namespace Coordinator.App;

public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> SelectUser(long telegramId)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        var command = new SqlCommand($"SELECT TOP 1 telegram_id, first_name FROM Users WHERE telegram_id = {telegramId}",
            connection);

        var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? new User(reader.GetInt64(0), reader.GetString(1))
            : null;
    }

    public async Task<bool> InsertUser(long telegramId, string name)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        var command = new SqlCommand($"INSERT INTO Users (telegram_id, first_name) VALUES ({telegramId}, N'{name}')",
            connection);

        var count = await command.ExecuteNonQueryAsync();

        return count == 1; //TODO Не отказоустойчиво
    }

    public async Task<User?> SelectUserByMessage(int telegramMessageId)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync();

        var command = new SqlCommand(
            $"SELECT Users.telegram_id, Users.first_name FROM Messages INNER JOIN Users on Messages.[user] = Users.id WHERE Messages.telegram_id ={telegramMessageId}",
            connection);

        var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? new User(reader.GetInt64(0), reader.GetString(1))
            : null;
    }

    public async Task<bool> InsertMessage(long telegramUserId, int telegramMessageId)
    {
        int userId;
        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();


            var selectCommand = new SqlCommand($"SELECT Users.id FROM Users WHERE telegram_id = {telegramUserId}",
                connection);

            var reader = await selectCommand.ExecuteReaderAsync();

            await reader.ReadAsync();

            userId = reader.GetInt32(0);
        }

        if (userId == 0)
            return false;


        await using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var insertCommand =
                new SqlCommand($"INSERT INTO Messages ([user], telegram_id) VALUES ({userId}, {telegramMessageId})",
                    connection);

            var count = await insertCommand.ExecuteNonQueryAsync();
            return count == 1; //TODO Не отказоустойчиво
        }
    }
}