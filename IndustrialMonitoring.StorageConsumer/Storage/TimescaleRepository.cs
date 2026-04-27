using IndustrialMonitoring.Shared.Models;
using IndustrialMonitoring.StorageConsumer.Configurations;
using Microsoft.Extensions.Options;
using Npgsql;

namespace IndustrialMonitoring.StorageConsumer.Storage;

public class TimescaleRepository : IReadingRepository
{
    private readonly DatabaseSettings _databaseSettings;
    private bool _tableEnsured = false;

    public TimescaleRepository(IOptions<DatabaseSettings> databaseOptions)
    {
        _databaseSettings = databaseOptions.Value;
    }

    public async Task SaveAsync(TagReading reading, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_databaseSettings.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            if (!_tableEnsured)
            {
                const string createTableSql = """
                    CREATE TABLE IF NOT EXISTS tag_readings (
                        id SERIAL,
                        tag_name TEXT NOT NULL,
                        value TEXT NOT NULL,
                        timestamp TIMESTAMP NOT NULL,
                        quality TEXT NOT NULL,
                        source TEXT NOT NULL,
                        PRIMARY KEY (id, timestamp)
                    );
                    """;

                await using var createCommand = new NpgsqlCommand(createTableSql, connection);
                await createCommand.ExecuteNonQueryAsync(cancellationToken);

                const string createHypertableSql = """
                    SELECT create_hypertable(
                        'tag_readings'::regclass,
                        'timestamp'::name,
                        if_not_exists => TRUE
                    );
                    """;

                await using var hypertableCommand = new NpgsqlCommand(createHypertableSql, connection);
                await hypertableCommand.ExecuteNonQueryAsync(cancellationToken);

                Console.WriteLine("[DB] Table and hypertable are ready.");
                _tableEnsured = true;
            }

            const string insertSql = """
                INSERT INTO tag_readings (tag_name, value, timestamp, quality, source)
                VALUES (@tagName, @value, @timestamp, @quality, @source);
                """;

            await using var insertCommand = new NpgsqlCommand(insertSql, connection);
            insertCommand.Parameters.AddWithValue("tagName", reading.TagName);
            insertCommand.Parameters.AddWithValue("value", reading.Value);
            insertCommand.Parameters.AddWithValue("timestamp", reading.Timestamp);
            insertCommand.Parameters.AddWithValue("quality", reading.Quality);
            insertCommand.Parameters.AddWithValue("source", reading.Source);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            Console.WriteLine($"[DB INSERT] Saved reading for tag: {reading.TagName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB ERROR] {ex.Message}");
        }
    }
}