using Npgsql;

namespace IndustrialMonitoring.Collector.Services;

public class EnabledTagsProvider
{
    private readonly string _connectionString;

    public EnabledTagsProvider(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public async Task<List<string>> GetEnabledTagNodeIdsAsync(CancellationToken cancellationToken)
    {
        var results = new List<string>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT t.node_id
        FROM public.opcua_tags t
        WHERE t.is_enabled = TRUE
        ORDER BY t.display_name;
        """;

        await using var cmd = new NpgsqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }
}