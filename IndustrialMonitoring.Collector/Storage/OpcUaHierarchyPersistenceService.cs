using IndustrialMonitoring.Collector.OpcUa;
using Npgsql;

namespace IndustrialMonitoring.Collector.Storage;

public class OpcUaHierarchyPersistenceService
{
    private readonly string _connectionString;
    private readonly ILogger<OpcUaHierarchyPersistenceService> _logger;

    public OpcUaHierarchyPersistenceService(
        IConfiguration configuration,
        ILogger<OpcUaHierarchyPersistenceService> logger)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");

        _logger = logger;
    }

    public async Task SaveProjectAsync(
        DiscoveredOpcUaProject project,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var projectId = await UpsertProjectAsync(connection, transaction, project, cancellationToken);

            foreach (var rootNode in project.Nodes)
            {
                await UpsertNodeRecursiveAsync(
                    connection,
                    transaction,
                    projectId,
                    parentNodeDbId: null,
                    node: rootNode,
                    cancellationToken: cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Hierarchical OPC UA project persisted successfully. ProjectId: {ProjectId}, RootNodes: {Count}",
                projectId,
                project.Nodes.Count);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<int> UpsertProjectAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        DiscoveredOpcUaProject project,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO public.opcua_projects
            (
                name,
                endpoint_url,
                root_node_id,
                is_active,
                discovered_at_utc
            )
            VALUES
            (
                @name,
                @endpointUrl,
                @rootNodeId,
                TRUE,
                NOW()
            )
            ON CONFLICT (name, endpoint_url)
            DO UPDATE SET
                root_node_id = EXCLUDED.root_node_id,
                is_active = TRUE,
                discovered_at_utc = NOW()
            RETURNING id;
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("name", project.Name);
        cmd.Parameters.AddWithValue("endpointUrl", project.EndpointUrl);
        cmd.Parameters.AddWithValue("rootNodeId", (object?)project.RootNodeId ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }

    private async Task<int> UpsertNodeRecursiveAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int projectId,
        int? parentNodeDbId,
        DiscoveredOpcUaNode node,
        CancellationToken cancellationToken)
    {
        var currentNodeDbId = await UpsertNodeAsync(
            connection,
            transaction,
            projectId,
            parentNodeDbId,
            node,
            cancellationToken);

        foreach (var child in node.Children)
        {
            await UpsertNodeRecursiveAsync(
                connection,
                transaction,
                projectId,
                currentNodeDbId,
                child,
                cancellationToken);
        }

        return currentNodeDbId;
    }

    private static async Task<int> UpsertNodeAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        int projectId,
        int? parentNodeDbId,
        DiscoveredOpcUaNode node,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO public.opcua_nodes
            (
                project_id,
                parent_node_id,
                node_id,
                browse_name,
                display_name,
                node_class,
                depth,
                kind,
                data_type,
                is_selectable,
                is_enabled,
                discovered_at_utc
            )
            VALUES
            (
                @projectId,
                @parentNodeId,
                @nodeId,
                @browseName,
                @displayName,
                @nodeClass,
                @depth,
                @kind,
                @dataType,
                @isSelectable,
                @isEnabled,
                NOW()
            )
            ON CONFLICT (project_id, node_id)
            DO UPDATE SET
                parent_node_id = EXCLUDED.parent_node_id,
                browse_name = EXCLUDED.browse_name,
                display_name = EXCLUDED.display_name,
                node_class = EXCLUDED.node_class,
                depth = EXCLUDED.depth,
                kind = EXCLUDED.kind,
                data_type = EXCLUDED.data_type,
                is_selectable = EXCLUDED.is_selectable,
                discovered_at_utc = NOW()
            RETURNING id;
            """;

        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("projectId", projectId);
        cmd.Parameters.AddWithValue("parentNodeId", (object?)parentNodeDbId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("nodeId", node.NodeId);
        cmd.Parameters.AddWithValue("browseName", (object?)node.BrowseName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("displayName", node.DisplayName);
        cmd.Parameters.AddWithValue("nodeClass", (object?)node.NodeClass ?? DBNull.Value);
        cmd.Parameters.AddWithValue("depth", node.Depth);
        cmd.Parameters.AddWithValue("kind", node.Kind);
        cmd.Parameters.AddWithValue("dataType", (object?)node.DataType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("isSelectable", node.IsSelectable);
        cmd.Parameters.AddWithValue("isEnabled", node.IsEnabled);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(result);
    }
}