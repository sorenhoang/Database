# Cassandra C# Demo Application

This application demonstrates the use of Apache Cassandra with C# for two common use cases:
1. **IoT Sensor Data (Time Series)** - Storing and querying time-based sensor readings
2. **Product Catalog** - Building a product catalog with search capabilities

## Prerequisites

1. **Apache Cassandra**: You need to have Cassandra installed and running on your local machine
   - Download from: https://cassandra.apache.org/download/ or use Docker
   - Default port: 9042
   - Default datacenter: datacenter1

2. **.NET SDK**: Make sure you have .NET SDK installed (6.0 or later)

## Running with Docker (recommended)

If you don't have Cassandra installed, you can easily run it using Docker:

```powershell
# Pull the Cassandra image
docker pull cassandra:latest

# Run Cassandra container
docker run --name cassandra-demo -d -p 9042:9042 cassandra:latest
```

## Project Structure

- **Models/** - Contains the data models (SensorReading, Product)
- **Data/** - Contains Cassandra connectivity and repositories
- **UseCases/** - Contains the implementation of the demonstration use cases

## Running the Application

1. Build the project:
```powershell
dotnet build
```

2. Run the application:
```powershell
dotnet run
```

3. Follow the on-screen menu to select a use case to run

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

## Notes on the Implementation

- The application uses the DataStax C# Driver for Apache Cassandra
- The demo connects to a local Cassandra instance by default
- The schema is created automatically when the application runs
- Simple console UI demonstrates the functionality

## Additional Use Cases

1. **User Sessions and Authentication**
   - Fast write/read for session data
   - TTL for automatic expiration

2. **Messaging Systems**
   - High throughput message storage
   - Time-ordered message retrieval

3. **Real-time Analytics**
   - Storing and querying analytics events
   - Counters for real-time stats
