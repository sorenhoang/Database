using Cassandra;
using Cassandra.Mapping;
using System.Net;

namespace CassandraDemo.Data
{
    /// <summary>
    /// Manages the connection to Cassandra and provides basic setup functionality
    /// </summary>
    public class CassandraContext : IDisposable
    {
        private readonly string[] _contactPoints;
        private readonly string _dataCenter;
        private readonly string _keyspaceName;
        private Cluster? _cluster;
        private ISession? _session;
        private IMapper? _mapper;

        /// <summary>
        /// Initializes a new Cassandra context
        /// </summary>
        /// <param name="contactPoints">Contact points/hosts for Cassandra cluster</param>
        /// <param name="dataCenter">Data center name (usually "datacenter1" for local setup)</param>
        /// <param name="keyspaceName">Keyspace name to use/create</param>
        public CassandraContext(string[] contactPoints, string dataCenter, string keyspaceName)
        {
            _contactPoints = contactPoints;
            _dataCenter = dataCenter;
            _keyspaceName = keyspaceName;
        }

        /// <summary>
        /// Connect to the Cassandra cluster and initialize the session and mapper
        /// </summary>
        public void Connect()
        {
            _cluster = Cluster.Builder()
                .AddContactPoints(_contactPoints)
                .WithLoadBalancingPolicy(new DCAwareRoundRobinPolicy(_dataCenter))
                .Build();

            // Create or verify keyspace existence
            EnsureKeyspaceExists();

            // Connect to the keyspace
            _session = _cluster.Connect(_keyspaceName);

            // Setup mapping configurations
            _mapper = new Mapper(_session);

            Console.WriteLine($"Connected to cluster: {_cluster.Metadata.ClusterName}");
            Console.WriteLine("Connected to datacenter: {0}", _dataCenter);
        }        /// <summary>
                 /// Creates the keyspace if it doesn't exist
                 /// </summary>
        private void EnsureKeyspaceExists()
        {
            if (_cluster == null)
                throw new InvalidOperationException("Cluster is not initialized");

            var session = _cluster.Connect();

            // Create keyspace with SimpleStrategy and replication factor of 1 (for development)
            session.Execute($@"
                CREATE KEYSPACE IF NOT EXISTS {_keyspaceName} 
                WITH replication = {{'class': 'SimpleStrategy', 'replication_factor': 1}}");

            session.Dispose();
        }

        /// <summary>
        /// Execute CQL directly
        /// </summary>
        /// <param name="cql">CQL statement to execute</param>
        public void ExecuteCql(string cql)
        {
            if (_session == null)
                throw new InvalidOperationException("Session is not initialized. Call Connect() first.");

            _session.Execute(cql);
        }

        /// <summary>
        /// Get the Cassandra session
        /// </summary>
        public ISession GetSession()
        {
            if (_session == null)
                throw new InvalidOperationException("Session is not initialized. Call Connect() first.");

            return _session;
        }

        /// <summary>
        /// Get the Cassandra mapper
        /// </summary>
        public IMapper GetMapper()
        {
            if (_mapper == null)
                throw new InvalidOperationException("Mapper is not initialized. Call Connect() first.");

            return _mapper;
        }

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
            _session?.Dispose();
            _cluster?.Dispose();
        }
    }
}
