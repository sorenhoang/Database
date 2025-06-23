# Cassandra C# Demo Application

This application demonstrates several practical use cases of Apache Cassandra with C#:
1. **IoT Sensor Data (Time Series)** - Storing and querying time-based sensor readings
2. **Product Catalog** - Building a product catalog with search capabilities
3. **User Sessions and Authentication** - Managing session data with TTL for automatic expiration
4. **Messaging System** - Implementing a high-performance messaging platform
5. **Real-time Analytics** - Tracking and analyzing events with counter support

## Setup Guide

### Prerequisites

1. **Docker**: The easiest way to run Cassandra (no installation required)
   - Download from: https://www.docker.com/products/docker-desktop/

2. **.NET SDK 6.0 or later**:
   - Download from: https://dotnet.microsoft.com/download

### Step 1: Start Cassandra with Docker

```powershell
# Pull the Cassandra image
docker pull cassandra:latest

# Run Cassandra container
docker run --name cassandra-demo -d -p 9042:9042 cassandra:latest
```

> **Note**: The first startup of Cassandra may take a minute or two. You can check the logs to see when it's ready:
> ```powershell
> docker logs cassandra-demo -f
> ```
> Look for the message: "Starting listening for CQL clients on /0.0.0.0:9042"

### Step 2: Verify Cassandra is Running

```powershell
# Connect to Cassandra using the interactive CQL shell
docker exec -it cassandra-demo cqlsh

# Try a simple command to verify it's working
cqlsh> DESCRIBE KEYSPACES;

# Exit the shell
cqlsh> EXIT;
```

### Step 3: Build and Run the Application

1. Navigate to the project directory:
```powershell
cd path\to\CassandraDemo
```

2. Build the project:
```powershell
dotnet build
```

3. Run the application:
```powershell
dotnet run
```

4. Follow the on-screen menu to select a use case to run

### Testing Different Use Cases

The application provides an interactive menu where you can select which use case to explore:

1. **IoT Sensor Data**: Demonstrates storing and querying time-series data from sensors
   - Creates sample sensor readings
   - Shows how to query by device and time range
   - Demonstrates aggregation of sensor data

2. **Product Catalog**: Shows how to build a product database
   - Creates sample products with categories and attributes
   - Demonstrates search and filtering capabilities
   - Shows updating and retrieving product information

3. **User Sessions**: Illustrates session management with TTL
   - Creates user sessions with authentication data
   - Shows how TTL automatically expires old sessions
   - Demonstrates updating and validating sessions

4. **Messaging System**: Implements a chat-like messaging platform
   - Organizes messages by conversation
   - Shows time-ordered retrieval of messages
   - Demonstrates read status tracking

5. **Real-time Analytics**: Tracks and analyzes events
   - Records various event types with timestamps
   - Uses counters for real-time aggregation
   - Shows querying by time periods

## Key Cassandra Concepts Demonstrated

1. **Data Modeling**
   - Partition keys for efficient data distribution
   - Clustering columns for data sorting and efficient retrieval
   - Collections (map, list) for complex data types

2. **Efficient Queries**
   - Time-range queries
   - Secondary indices
   - Batch operations

3. **Performance Considerations**
   - Denormalized data model
   - Querying by partition key
   - ALLOW FILTERING considerations

4. **Limitations and Workarounds**
   - **No Transaction Rollbacks**: Cassandra does not support traditional ACID transactions with rollback capabilities
   - **Eventual Consistency**: Implementing compensating transactions for error handling
   - **Idempotent Operations**: Designing operations that can be safely retried
   - **Batch with Caution**: Batches are primarily for ensuring atomicity across partitions, not for performance

## Troubleshooting

### Connection Issues

If you encounter connection issues with Cassandra:

1. **Check if Docker container is running**:
   ```powershell
   docker ps
   ```
   
   If the container is not listed, restart it:
   ```powershell
   docker start cassandra-demo
   ```

2. **Verify port availability**:
   Make sure port 9042 is not being used by another application.

3. **Wait for Cassandra to initialize**:
   Cassandra may take a minute or two to fully initialize on first startup.

4. **Check Docker container logs**:
   ```powershell
   docker logs cassandra-demo
   ```

5. **Restart the container**:
   If all else fails, try recreating the container:
   ```powershell
   docker rm -f cassandra-demo
   docker run --name cassandra-demo -d -p 9042:9042 cassandra:latest
   ```

### Common Cassandra Errors

1. **NoHostAvailableException**: 
   - Verify Cassandra is running and port 9042 is accessible
   - Check firewall settings if applicable

2. **InvalidQueryException**:
   - Check the CQL syntax in the repository classes
   - Verify keyspace and table names

3. **AuthenticationException**:
   - The demo uses default Cassandra authentication (no username/password)
   - If you've enabled authentication in your Cassandra instance, update the connection code

## Advanced Usage

### Persisting Cassandra Data

By default, the Docker container data will be lost when the container is removed. To persist data:

```powershell
docker run --name cassandra-demo -d -p 9042:9042 -v cassandra-data:/var/lib/cassandra cassandra:latest
```

### Using a Cassandra Cluster

For a more realistic setup, you can create a multi-node Cassandra cluster:

```powershell
# Create a Docker network
docker network create cassandra-net

# Create the first node
docker run --name cassandra-1 -d -p 9042:9042 --network cassandra-net cassandra:latest

# Add additional nodes
docker run --name cassandra-2 -d --network cassandra-net -e CASSANDRA_SEEDS=cassandra-1 cassandra:latest
docker run --name cassandra-3 -d --network cassandra-net -e CASSANDRA_SEEDS=cassandra-1 cassandra:latest
```

Update the application connection settings to use all nodes:
```csharp
var contactPoints = new string[] { "cassandra-1", "cassandra-2", "cassandra-3" };
```

### Exploring Cassandra with CQL

You can interact directly with Cassandra using the CQL shell:

```powershell
docker exec -it cassandra-demo cqlsh
```

Useful CQL commands:
```sql
-- List all keyspaces
DESCRIBE KEYSPACES;

-- Use our demo keyspace
USE cassandra_demo;

-- List tables in the keyspace
DESCRIBE TABLES;

-- View table schema
DESCRIBE TABLE user_sessions;

-- Query data
SELECT * FROM user_sessions LIMIT 10;
```

## Notes on the Implementation

- The application uses the DataStax C# Driver for Apache Cassandra
- The demo connects to a local Cassandra instance by default
- The schema is created automatically when the application runs
- Simple console UI demonstrates the functionality

## Additional Use Cases

### 1. User Sessions and Authentication

**Use Case Description:**
Store and manage user session data efficiently, supporting high write/read throughput and automatic expiration of old sessions.

**Cassandra Implementation:**
```cql
CREATE TABLE user_sessions (
  session_id UUID,
  user_id UUID,
  username TEXT,
  login_time TIMESTAMP,
  last_activity TIMESTAMP,
  ip_address TEXT,
  device_info TEXT,
  permissions MAP<TEXT, BOOLEAN>,
  PRIMARY KEY (session_id)
) WITH TTL = 86400; -- 24-hour default TTL
```

**Key Benefits:**
- Fast writes and reads by session ID
- TTL (Time To Live) for automatic session expiration
- No need for separate cleanup processes
- Easily scalable for millions of concurrent sessions

### 2. Messaging Systems

**Use Case Description:**
Implement a high-performance messaging system that can handle millions of messages with time-ordered retrieval and conversation grouping.

**Cassandra Implementation:**
```cql
CREATE TABLE messages (
  conversation_id UUID,
  timestamp TIMEUUID,
  sender_id UUID,
  message_type TEXT,
  message_content TEXT,
  attachments LIST<TEXT>,
  read_status MAP<UUID, BOOLEAN>,
  PRIMARY KEY (conversation_id, timestamp)
) WITH CLUSTERING ORDER BY (timestamp DESC);
```

**Key Benefits:**
- Messages naturally ordered by time within conversation
- Efficient retrieval of recent messages
- Scalable for high message volumes
- Support for real-time chat applications

### 3. Real-time Analytics

**Use Case Description:**
Track and analyze real-time events and metrics, supporting high-volume writes and fast aggregation queries.

**Cassandra Implementation:**
```cql
-- Event tracking table
CREATE TABLE analytics_events (
  event_date DATE,
  event_hour INT,
  event_id TIMEUUID,
  user_id UUID,
  event_type TEXT,
  event_data MAP<TEXT, TEXT>,
  PRIMARY KEY ((event_date, event_hour), event_id)
);

-- Counter table for real-time stats
CREATE TABLE event_counters (
  event_date DATE,
  event_hour INT,
  event_type TEXT,
  counter_value COUNTER,
  PRIMARY KEY ((event_date, event_hour), event_type)
);
```

**Key Benefits:**
- Partitioning by date and hour for efficient time-based queries
- Counters for atomic incrementing without read-before-write
- Designed for high throughput event ingestion
- Supports both detailed event storage and aggregated metrics

### 4. Content Management System

**Use Case Description:**
Store and retrieve content for a CMS with support for versioning, tagging, and efficient retrieval.

**Cassandra Implementation:**
```cql
CREATE TABLE content_items (
  content_id UUID,
  version TIMEUUID,
  title TEXT,
  content TEXT,
  author_id UUID,
  status TEXT,
  tags SET<TEXT>,
  metadata MAP<TEXT, TEXT>,
  PRIMARY KEY (content_id, version)
) WITH CLUSTERING ORDER BY (version DESC);
```

**Key Benefits:**
- Natural versioning with TIMEUUID
- Efficient retrieval of latest version
- Support for tagging and metadata
- Optimized for read-heavy workloads

### 5. Fraud Detection System

**Use Case Description:**
Monitor transactions in real-time to detect potentially fraudulent activity based on patterns and thresholds.

**Cassandra Implementation:**
```cql
-- Transaction history
CREATE TABLE user_transactions (
  user_id UUID,
  transaction_time TIMESTAMP,
  transaction_id UUID,
  amount DECIMAL,
  merchant TEXT,
  location TEXT,
  device_id TEXT,
  ip_address TEXT,
  status TEXT,
  risk_score FLOAT,
  PRIMARY KEY ((user_id), transaction_time, transaction_id)
) WITH CLUSTERING ORDER BY (transaction_time DESC);

-- Suspicious activity alerts
CREATE TABLE fraud_alerts (
  alert_date DATE,
  alert_time TIMESTAMP,
  user_id UUID,
  alert_type TEXT,
  transactions SET<UUID>,
  risk_factors MAP<TEXT, FLOAT>,
  status TEXT,
  PRIMARY KEY ((alert_date), alert_time, user_id)
) WITH CLUSTERING ORDER BY (alert_time DESC);
```

**Key Benefits:**
- Time-based clustering for recent transaction analysis
- Support for complex fraud detection rules
- High write throughput for transaction logging
- Efficient retrieval of user transaction history
