using IndustrialMonitoring.Api.Models.OpcUa;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IndustrialMonitoring.Api.Services;

public class OpcUaDiscoveryService : IOpcUaDiscoveryService
{
    private readonly string _connectionString;

    public OpcUaDiscoveryService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public List<OpcUaSectionDto> GetSections()
    {
        var results = new List<OpcUaSectionDto>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT id, name, node_id, display_name, child_count, is_enabled, discovered_at_utc
            FROM public.opcua_sections
            ORDER BY display_name;
            """;

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new OpcUaSectionDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                NodeId = reader.GetString(reader.GetOrdinal("node_id")),
                DisplayName = reader.GetString(reader.GetOrdinal("display_name")),
                ChildCount = reader.GetInt32(reader.GetOrdinal("child_count")),
                IsEnabled = reader.GetBoolean(reader.GetOrdinal("is_enabled")),
                DiscoveredAtUtc = DateTime.SpecifyKind(
                    reader.GetDateTime(reader.GetOrdinal("discovered_at_utc")),
                    DateTimeKind.Utc)
            });
        }

        return results;
    }

    public List<OpcUaTagDto> GetTagsBySectionId(int sectionId)
    {
        var results = new List<OpcUaTagDto>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT id, section_id, tag_name, node_id, display_name, data_type, is_enabled, discovered_at_utc
            FROM public.opcua_tags
            WHERE section_id = @sectionId
            ORDER BY display_name;
            """;

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("sectionId", sectionId);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new OpcUaTagDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                SectionId = reader.GetInt32(reader.GetOrdinal("section_id")),
                TagName = reader.GetString(reader.GetOrdinal("tag_name")),
                NodeId = reader.GetString(reader.GetOrdinal("node_id")),
                DisplayName = reader.GetString(reader.GetOrdinal("display_name")),
                DataType = reader.IsDBNull(reader.GetOrdinal("data_type"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("data_type")),
                IsEnabled = reader.GetBoolean(reader.GetOrdinal("is_enabled")),
                DiscoveredAtUtc = DateTime.SpecifyKind(
                    reader.GetDateTime(reader.GetOrdinal("discovered_at_utc")),
                    DateTimeKind.Utc)
            });
        }


        return results;
    }

    public void UpdateTagsEnabledStateBySection(int sectionId, bool isEnabled)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
        UPDATE public.opcua_tags
        SET is_enabled = @isEnabled
        WHERE section_id = @sectionId;
        """;

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("isEnabled", isEnabled);
        cmd.Parameters.AddWithValue("sectionId", sectionId);

        cmd.ExecuteNonQuery();
    }

    public void UpdateTagEnabledState(int tagId, bool isEnabled)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
        UPDATE public.opcua_tags
        SET is_enabled = @isEnabled
        WHERE id = @tagId;
        """;

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("isEnabled", isEnabled);
        cmd.Parameters.AddWithValue("tagId", tagId);

        cmd.ExecuteNonQuery();
    }
    public void UpdateSectionEnabledState(int sectionId, bool isEnabled)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        const string updateSectionSql = """
        UPDATE public.opcua_sections
        SET is_enabled = @isEnabled
        WHERE id = @sectionId;
        """;

        using (var sectionCmd = new NpgsqlCommand(updateSectionSql, connection, transaction))
        {
            sectionCmd.Parameters.AddWithValue("isEnabled", isEnabled);
            sectionCmd.Parameters.AddWithValue("sectionId", sectionId);
            sectionCmd.ExecuteNonQuery();
        }

        const string updateTagsSql = """
        UPDATE public.opcua_tags
        SET is_enabled = @isEnabled
        WHERE section_id = @sectionId;
        """;

        using (var tagsCmd = new NpgsqlCommand(updateTagsSql, connection, transaction))
        {
            tagsCmd.Parameters.AddWithValue("isEnabled", isEnabled);
            tagsCmd.Parameters.AddWithValue("sectionId", sectionId);
            tagsCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}