using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Shared.Models;
using Microsoft.Extensions.Options;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Text;
using System.IO;


namespace IndustrialMonitoring.Collector.OpcUa;

public class OpcUaClientService : IOpcUaClient
{
    private readonly ILogger<OpcUaClientService> _logger;
    private readonly OpcUaSettings _settings;
    private Session? _session;


    public async Task<DiscoveredOpcUaProject?> DiscoverProjectAsync(string rootNodeId, CancellationToken cancellationToken)
    {
        if (_session is null)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var rootNode = ExpandedNodeId.ToNodeId(rootNodeId, _session.NamespaceUris);

        if (rootNode is null)
        {
            _logger.LogWarning("Could not parse root node id {RootNodeId}", rootNodeId);
            return null;
        }

        var project = new DiscoveredOpcUaProject
        {
            Name = "Discovered OPC UA Project",
            EndpointUrl = _settings.EndpointUrl ?? string.Empty,
            RootNodeId = rootNodeId
        };

        var rootDiscoveredNode = BuildDiscoveredNode(_session, rootNode, depth: 0);

        await DiscoverChildrenRecursiveAsync(
            _session,
            rootNode,
            rootDiscoveredNode,
            currentDepth: 0,
            maxDepth: 4,
            cancellationToken);

        project.Nodes.Add(rootDiscoveredNode);

        return project;
    }

    private async Task DiscoverChildrenRecursiveAsync(
    Session session,
    NodeId parentNodeId,
    DiscoveredOpcUaNode parentNode,
    int currentDepth,
    int maxDepth,
    CancellationToken cancellationToken)
    {
        if (currentDepth >= maxDepth)
        {
            return;
        }

        var references = BrowseNode(session, parentNodeId);

        foreach (var reference in references)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var childNodeId = ExpandedNodeId.ToNodeId(reference.NodeId, session.NamespaceUris);

            if (childNodeId is null)
            {
                continue;
            }

            var child = BuildDiscoveredNode(session, childNodeId, currentDepth + 1);
            parentNode.Children.Add(child);

            await DiscoverChildrenRecursiveAsync(
                session,
                childNodeId,
                child,
                currentDepth + 1,
                maxDepth,
                cancellationToken);
        }
    }

    private DiscoveredOpcUaNode BuildDiscoveredNode(Session session, NodeId nodeId, int depth)
    {
        var displayName = nodeId.ToString();
        string? browseName = null;
        string? nodeClass = null;
        string? dataType = null;

        try
        {
            var node = session.ReadNode(nodeId);

            if (node is not null)
            {
                displayName = node.DisplayName?.Text ?? displayName;
                browseName = node.BrowseName?.Name;
                nodeClass = node.NodeClass.ToString();

                if (node is VariableNode variableNode && variableNode.DataType is not null)
                {
                    dataType = variableNode.DataType.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read node metadata for {NodeId}", nodeId);
        }

        return new DiscoveredOpcUaNode
        {
            NodeId = nodeId.ToString(),
            BrowseName = browseName,
            DisplayName = displayName,
            NodeClass = nodeClass,
            Depth = depth,
            Kind = ClassifyNodeKind(displayName, browseName, nodeClass, depth),
            DataType = dataType,
            IsSelectable = IsSelectableNode(displayName, browseName, nodeClass, depth),
            IsEnabled = false
        };
    }

    private List<ReferenceDescription> BrowseNode(Session session, NodeId nodeId)
    {
        var results = new List<ReferenceDescription>();

        session.Browse(
            null,
            null,
            nodeId,
            0u,
            BrowseDirection.Forward,
            ReferenceTypeIds.HierarchicalReferences,
            true,
            (uint)(NodeClass.Object | NodeClass.Variable),
            out var continuationPoint,
            out var references);

        if (references is not null)
        {
            results.AddRange(references);
        }

        while (continuationPoint is not null && continuationPoint.Length > 0)
        {
            session.BrowseNext(
                null,
                false,
                continuationPoint,
                out continuationPoint,
                out var nextReferences);

            if (nextReferences is not null)
            {
                results.AddRange(nextReferences);
            }
        }

        return results;
    }


    private string ClassifyNodeKind(string displayName, string? browseName, string? nodeClass, int depth)
    {
        var display = displayName.ToLowerInvariant();
        var browse = browseName?.ToLowerInvariant() ?? string.Empty;

        if (depth == 0)
        {
            return "project_root";
        }

        if (display.Contains("icon") || browse.Contains("icon"))
        {
            return "metadata";
        }

        if (depth == 1 && nodeClass?.Equals("Object", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "section_candidate";
        }

        if (nodeClass?.Equals("Object", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "tag_object";
        }

        if (nodeClass?.Equals("Variable", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "tag_field";
        }

        return "unknown";
    }

    private bool IsSelectableNode(string displayName, string? browseName, string? nodeClass, int depth)
    {
        var display = displayName.ToLowerInvariant();
        var browse = browseName?.ToLowerInvariant() ?? string.Empty;

        if (display.Contains("icon") || browse.Contains("icon"))
        {
            return false;
        }

        if (depth == 1 && nodeClass?.Equals("Object", StringComparison.OrdinalIgnoreCase) == true)
        {
            return true;
        }

        return false;
    }
    public OpcUaClientService(
    IOptions<OpcUaSettings> options,
    ILogger<OpcUaClientService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    private void SearchNode(NodeId nodeId, string searchText, int level = 0)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var references = BrowseAll(nodeId);

        foreach (var reference in references)
        {
            var displayName = reference.DisplayName.Text ?? string.Empty;
            var browseName = reference.BrowseName.Name ?? string.Empty;

            if (displayName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                browseName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    $"MATCH -> DisplayName: {displayName} | BrowseName: {browseName} | NodeId: {reference.NodeId}");
            }

            try
            {
                var childNodeId = ExpandedNodeId.ToNodeId(reference.NodeId, _session.NamespaceUris);

                if (childNodeId != null && level < 8)
                {
                    SearchNode(childNodeId, searchText, level + 1);
                }
            }
            catch
            {
            }
        }
    }

    private void BrowseNode(NodeId nodeId, int level = 0)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var references = BrowseAll(nodeId);

        foreach (var reference in references)
        {
            var indent = new string(' ', level * 2);

            Console.WriteLine(
                $"{indent}- DisplayName: {reference.DisplayName.Text} | BrowseName: {reference.BrowseName.Name} | NodeId: {reference.NodeId}");

            try
            {
                var childNodeId = ExpandedNodeId.ToNodeId(reference.NodeId, _session.NamespaceUris);

                if (childNodeId != null && level < 6)
                {
                    BrowseNode(childNodeId, level + 1);
                }
            }
            catch
            {
            }
        }
    }
    public Task BrowseNodeAsync(string nodeIdText, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var nodeId = NodeId.Parse(nodeIdText);

        Console.WriteLine($"===== OPC UA BROWSE NODE START: {nodeIdText} =====");
        BrowseNode(nodeId, 0);
        Console.WriteLine($"===== OPC UA BROWSE NODE END: {nodeIdText} =====");

        return Task.CompletedTask;
    }

    public Task SearchAsync(string searchText, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        Console.WriteLine($"===== OPC UA SEARCH START: {searchText} =====");
        SearchNode(ObjectIds.ObjectsFolder, searchText, 0);
        Console.WriteLine($"===== OPC UA SEARCH END: {searchText} =====");

        return Task.CompletedTask;
    }
    

    public Task BrowseRootAsync(CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        Console.WriteLine("===== OPC UA BROWSE START =====");
        BrowseNode(ObjectIds.ObjectsFolder, 0);
        Console.WriteLine("===== OPC UA BROWSE END =====");

        return Task.CompletedTask;
    }

    public Task<DiscoveredOpcUaSection?> DiscoverSectionAsync(string sectionNodeId, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var rootNodeId = NodeId.Parse(sectionNodeId);

        var rootReferences = BrowseAll(rootNodeId);
        var rootDisplayName = GetNodeDisplayName(rootNodeId);

        var section = new DiscoveredOpcUaSection
        {
            Name = rootDisplayName,
            DisplayName = rootDisplayName,
            NodeId = sectionNodeId,
            ChildCount = rootReferences.Count
        };

        foreach (var reference in rootReferences)
        {
            var childNodeId = ExpandedNodeId.ToNodeId(reference.NodeId, _session.NamespaceUris);
            if (childNodeId is null)
            {
                continue;
            }

            var childReferences = BrowseAll(childNodeId);

            var valueNode = childReferences.FirstOrDefault(x =>
                string.Equals(x.DisplayName.Text, "valore attuale", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.BrowseName.Name, "valore attuale", StringComparison.OrdinalIgnoreCase));

            if (valueNode is null)
            {
                continue;
            }

            section.Tags.Add(new DiscoveredOpcUaTag
            {
                TagName = reference.DisplayName.Text ?? reference.BrowseName.Name ?? "Unnamed",
                DisplayName = reference.DisplayName.Text ?? reference.BrowseName.Name ?? "Unnamed",
                NodeId = valueNode.NodeId.ToString(),
                DataType = "Real"
            });
        }

        return Task.FromResult<DiscoveredOpcUaSection?>(section);
    }

    private List<ReferenceDescription> BrowseAll(NodeId nodeId)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var results = new List<ReferenceDescription>();

        _session.Browse(
            null,
            null,
            nodeId,
            0u,
            BrowseDirection.Forward,
            ReferenceTypeIds.HierarchicalReferences,
            true,
            (uint)(NodeClass.Object | NodeClass.Variable),
            out var continuationPoint,
            out var references);

        results.AddRange(references);

        while (continuationPoint != null && continuationPoint.Length > 0)
        {
            _session.BrowseNext(
                null,
                false,
                continuationPoint,
                out continuationPoint,
                out var nextReferences);

            results.AddRange(nextReferences);
        }

        return results;
    }

    private string GetNodeDisplayName(NodeId nodeId)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        var node = _session.ReadNode(nodeId);
        return node?.DisplayName?.Text ?? nodeId.ToString();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (_session is { Connected: true })
        {
            return;
        }

        var config = new ApplicationConfiguration
        {
            ApplicationName = "IndustrialMonitoringCollector",
            ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:IndustrialMonitoringCollector",
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "IndustrialMonitoringCollector",
                        "pki",
                        "own"),
                    SubjectName = "CN=IndustrialMonitoringCollector"
                },
                TrustedIssuerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "IndustrialMonitoringCollector",
                        "pki",
                        "issuer")
                },
                TrustedPeerCertificates = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "IndustrialMonitoringCollector",
                        "pki",
                        "trusted")
                },
                RejectedCertificateStore = new CertificateTrustList
                {
                    StoreType = "Directory",
                    StorePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "IndustrialMonitoringCollector",
                        "pki",
                        "rejected")
                },
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false,
                MinimumCertificateKeySize = 1024
            },
            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 15000
            },
            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 60000
            }
        };

        await config.Validate(ApplicationType.Client);

        EndpointDescription selectedEndpoint;

        using (var discoveryClient = DiscoveryClient.Create(new Uri(_settings.EndpointUrl)))
        {
            var endpoints = await discoveryClient.GetEndpointsAsync(null);

            selectedEndpoint = endpoints
                .Where(e =>
                    e.EndpointUrl.StartsWith(_settings.EndpointUrl, StringComparison.OrdinalIgnoreCase) &&
                    e.SecurityMode == MessageSecurityMode.None &&
                    e.SecurityPolicyUri == SecurityPolicies.None)
                .FirstOrDefault()
                ?? throw new Exception("No matching OPC UA endpoint found for Security=None.");
        }

        var endpointConfiguration = EndpointConfiguration.Create(config);
        var configuredEndpoint = new ConfiguredEndpoint(
            null,
            selectedEndpoint,
            endpointConfiguration);

        IUserIdentity identity;

        if (!string.IsNullOrWhiteSpace(_settings.Username))
        {
            identity = new UserIdentity(
                _settings.Username,
                Encoding.UTF8.GetBytes(_settings.Password ?? string.Empty));
        }
        else
        {
            identity = new UserIdentity(new AnonymousIdentityToken());
        }

        _session = await Session.Create(
            config,
            configuredEndpoint,
            false,
            "IndustrialMonitoringCollectorSession",
            60000,
            identity,
            null);

        Console.WriteLine($"Connected to OPC UA endpoint: {selectedEndpoint.EndpointUrl}");
    }
    public Task<TagReading?> ReadTagAsync(string tagName, CancellationToken cancellationToken)
    {
        if (_session is null || !_session.Connected)
        {
            throw new InvalidOperationException("OPC UA session is not connected.");
        }

        try
        {
            var nodeId = NodeId.Parse(tagName);
            var dataValue = _session.ReadValue(nodeId);

            var reading = new TagReading
            {
                TagName = tagName,
                Value = dataValue.Value?.ToString() ?? "null",
                Timestamp = dataValue.SourceTimestamp == DateTime.MinValue
                    ? DateTime.UtcNow
                    : dataValue.SourceTimestamp.ToUniversalTime(),
                Quality = StatusCode.IsGood(dataValue.StatusCode) ? "Good" : dataValue.StatusCode.ToString(),
                Source = "OPC UA"
            };

            return Task.FromResult<TagReading?>(reading);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to read OPC UA tag '{tagName}': {ex.Message}");
            return Task.FromResult<TagReading?>(null);
        }
    }
}