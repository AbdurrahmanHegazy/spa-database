using Microsoft.Extensions.Configuration;
using Npgsql;
using IndustrialMonitoring.Collector.OpcUa;

namespace IndustrialMonitoring.Collector.Storage;

public class OpcUaDiscoveryPersistenceService
{
    private readonly string _connectionString;

    public OpcUaDiscoveryPersistenceService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public async Task SaveSectionAsync(DiscoveredOpcUaSection section, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sectionId = await UpsertSectionAsync(connection, section, cancellationToken);

        foreach (var tag in section.Tags)
        {
            await UpsertTagAsync(connection, sectionId, tag, cancellationToken);
        }
    }

    private static async Task<int> UpsertSectionAsync(
        NpgsqlConnection connection,
        DiscoveredOpcUaSection section,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO public.opcua_sections (name, node_id, display_name, child_count, is_enabled, discovered_at_utc)
            VALUES (@name, @nodeId, @displayName, @childCount, FALSE, NOW())
            ON CONFLICT (node_id)
            DO UPDATE SET
                name = EXCLUDED.name,
                display_name = EXCLUDED.display_name,
                child_count = EXCLUDED.child_count,
                discovered_at_utc = NOW()
            RETURNING id;
            """;

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("name", section.Name);
        cmd.Parameters.AddWithValue("nodeId", section.NodeId);
        cmd.Parameters.AddWithValue("displayName", section.DisplayName);
        cmd.Parameters.AddWithValue("childCount", section.ChildCount);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static async Task UpsertTagAsync(
        NpgsqlConnection connection,
        int sectionId,
        DiscoveredOpcUaTag tag,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO public.opcua_tags (section_id, tag_name, node_id, display_name, data_type, is_enabled, discovered_at_utc)
            VALUES (@sectionId, @tagName, @nodeId, @displayName, @dataType, FALSE, NOW())
            ON CONFLICT (node_id)
            DO UPDATE SET
                section_id = EXCLUDED.section_id,
                tag_name = EXCLUDED.tag_name,
                display_name = EXCLUDED.display_name,
                data_type = EXCLUDED.data_type,
                discovered_at_utc = NOW();
            """;

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("sectionId", sectionId);
        cmd.Parameters.AddWithValue("tagName", tag.TagName);
        cmd.Parameters.AddWithValue("nodeId", tag.NodeId);
        cmd.Parameters.AddWithValue("displayName", tag.DisplayName);
        cmd.Parameters.AddWithValue("dataType", (object?)tag.DataType ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}