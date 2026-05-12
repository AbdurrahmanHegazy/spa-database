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
                            SELECT
                                s.id,
                                s.name,
                                s.node_id,
                                s.display_name,
                                s.child_count,
                                s.is_enabled,
                                COALESCE(COUNT(t.id) FILTER (WHERE t.is_enabled = TRUE), 0) AS enabled_tags_count
                            FROM public.opcua_sections s
                            LEFT JOIN public.opcua_tags t
                                ON t.section_id = s.id
                            GROUP BY
                                s.id,
                                s.name,
                                s.node_id,
                                s.display_name,
                                s.child_count,
                                s.is_enabled
                            ORDER BY s.display_name;
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
                EnabledTagsCount = reader.GetInt32(reader.GetOrdinal("enabled_tags_count"))
            });
        }

        return results;
    }
    public async Task<DiscoveredSectionSyncResultDto> SyncDiscoveredSectionsAsync()
    {
        var result = new DiscoveredSectionSyncResultDto();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string readSql = """
        SELECT
            n.display_name,
            n.node_id
        FROM public.opcua_nodes n
        WHERE n.depth = 1
          AND n.node_class = 'Object'
          AND n.is_selectable = TRUE
        ORDER BY n.display_name;
        """;

        var discoveredSections = new List<(string DisplayName, string NodeId)>();

        await using (var readCmd = new NpgsqlCommand(readSql, connection))
        await using (var reader = await readCmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var displayName = reader.GetString(0);
                var nodeId = reader.GetString(1);
                discoveredSections.Add((displayName, nodeId));
            }
        }

        foreach (var section in discoveredSections)
        {
            const string upsertSql = """
            INSERT INTO public.opcua_sections
            (
                name,
                node_id,
                display_name,
                child_count,
                is_enabled,
                discovered_at_utc
            )
            VALUES
            (
                @name,
                @nodeId,
                @displayName,
                0,
                FALSE,
                NOW()
            )
            ON CONFLICT (node_id)
            DO UPDATE SET
                name = EXCLUDED.name,
                display_name = EXCLUDED.display_name,
                discovered_at_utc = NOW()
            RETURNING id;
            """;

            await using var upsertCmd = new NpgsqlCommand(upsertSql, connection);
            upsertCmd.Parameters.AddWithValue("name", section.DisplayName);
            upsertCmd.Parameters.AddWithValue("nodeId", section.NodeId);
            upsertCmd.Parameters.AddWithValue("displayName", section.DisplayName);

            await upsertCmd.ExecuteScalarAsync();

            result.ImportedCount++;
            result.ImportedSections.Add(section.DisplayName);
        }

        return result;
    }

    public async Task<DiscoveredTagSyncResultDto> SyncDiscoveredTagsForSectionAsync(int sectionId)
    {
        var result = new DiscoveredTagSyncResultDto();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        string sectionDisplayName;
        string sectionNodeId;

        const string readSectionSql = """
        SELECT display_name, node_id
        FROM public.opcua_sections
        WHERE id = @sectionId;
        """;

        await using (var readSectionCmd = new NpgsqlCommand(readSectionSql, connection))
        {
            readSectionCmd.Parameters.AddWithValue("sectionId", sectionId);

            await using var reader = await readSectionCmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                result.SectionName = $"Unknown section ({sectionId})";
                return result;
            }

            sectionDisplayName = reader.GetString(0);
            sectionNodeId = reader.GetString(1);
        }

        result.SectionName = sectionDisplayName;

        const string readTagsSql = """
        WITH RECURSIVE node_tree AS
        (
            SELECT
                id,
                parent_node_id,
                display_name,
                node_id,
                node_class,
                kind
            FROM public.opcua_nodes
            WHERE node_id = @sectionNodeId

            UNION ALL

            SELECT
                child.id,
                child.parent_node_id,
                child.display_name,
                child.node_id,
                child.node_class,
                child.kind
            FROM public.opcua_nodes child
            INNER JOIN node_tree parent
                ON child.parent_node_id = parent.id
        )
        SELECT
            display_name,
            node_id,
            kind
        FROM node_tree
        WHERE kind = 'tag_field'
        ORDER BY display_name;
        """;

        var discoveredTags = new List<(string DisplayName, string NodeId)>();

        await using (var readTagsCmd = new NpgsqlCommand(readTagsSql, connection))
        {
            readTagsCmd.Parameters.AddWithValue("sectionNodeId", sectionNodeId);

            await using var reader = await readTagsCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var displayName = reader.GetString(0);
                var nodeId = reader.GetString(1);
                discoveredTags.Add((displayName, nodeId));
            }
        }

        foreach (var tag in discoveredTags)
        {
            const string upsertSql = """
            INSERT INTO public.opcua_tags
            (
                section_id,
                tag_name,
                node_id,
                display_name,
                data_type,
                is_enabled,
                discovered_at_utc
            )
            VALUES
            (
                @sectionId,
                @tagName,
                @nodeId,
                @displayName,
                'Unknown',
                FALSE,
                NOW()
            )
            ON CONFLICT (section_id, node_id)
            DO UPDATE SET
                tag_name = EXCLUDED.tag_name,
                display_name = EXCLUDED.display_name,
                discovered_at_utc = NOW()
            RETURNING id;
            """;

            await using var upsertCmd = new NpgsqlCommand(upsertSql, connection);
            upsertCmd.Parameters.AddWithValue("sectionId", sectionId);
            upsertCmd.Parameters.AddWithValue("tagName", tag.DisplayName);
            upsertCmd.Parameters.AddWithValue("nodeId", tag.NodeId);
            upsertCmd.Parameters.AddWithValue("displayName", tag.DisplayName);

            await upsertCmd.ExecuteScalarAsync();

            result.ImportedCount++;
            result.ImportedTags.Add(tag.DisplayName);
        }

        const string updateChildCountSql = """
        UPDATE public.opcua_sections
        SET child_count = (
            SELECT COUNT(*)
            FROM public.opcua_tags
            WHERE section_id = @sectionId
        )
        WHERE id = @sectionId;
        """;

        await using (var updateCmd = new NpgsqlCommand(updateChildCountSql, connection))
        {
            updateCmd.Parameters.AddWithValue("sectionId", sectionId);
            await updateCmd.ExecuteNonQueryAsync();
        }

        return result;
    }

    public List<EnabledOpcUaTagDto> GetEnabledTags()
    {
        var results = new List<EnabledOpcUaTagDto>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
        SELECT
            t.id,
            t.section_id,
            s.display_name AS section_name,
            t.tag_name,
            t.node_id,
            t.display_name
        FROM public.opcua_tags t
        INNER JOIN public.opcua_sections s
            ON s.id = t.section_id
        WHERE t.is_enabled = TRUE
        ORDER BY s.display_name, t.display_name;
        """;

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            results.Add(new EnabledOpcUaTagDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                SectionId = reader.GetInt32(reader.GetOrdinal("section_id")),
                SectionName = reader.GetString(reader.GetOrdinal("section_name")),
                TagName = reader.GetString(reader.GetOrdinal("tag_name")),
                NodeId = reader.GetString(reader.GetOrdinal("node_id")),
                DisplayName = reader.GetString(reader.GetOrdinal("display_name"))
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