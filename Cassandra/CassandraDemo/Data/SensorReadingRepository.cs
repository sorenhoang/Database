using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;
using CassandraDemo.Models;

namespace CassandraDemo.Data
{
    /// <summary>
    /// Repository for sensor reading data
    /// </summary>
    public class SensorReadingRepository
    {
        private readonly CassandraContext _context;
        private readonly IMapper _mapper;

        public SensorReadingRepository(CassandraContext context)
        {
            _context = context;
            _mapper = context.GetMapper();

            // Create the table if it doesn't exist
            CreateTableIfNotExists();
        }

        /// <summary>
        /// Creates the sensor readings table if it doesn't exist
        /// </summary>
        private void CreateTableIfNotExists()
        {
            _context.ExecuteCql(@"
                CREATE TABLE IF NOT EXISTS sensor_readings (
                    sensor_id text,
                    timestamp timestamp,
                    temperature double,
                    humidity double,
                    co2_level double,
                    location text,
                    PRIMARY KEY (sensor_id, timestamp)
                ) WITH CLUSTERING ORDER BY (timestamp DESC);
            ");

            // Create index for location-based queries
            _context.ExecuteCql("CREATE INDEX IF NOT EXISTS ON sensor_readings(location);");
        }

        /// <summary>
        /// Insert a new sensor reading
        /// </summary>
        public async Task InsertAsync(SensorReading reading)
        {
            // Use string table name since CqlIdentifier is not available
            await _mapper.InsertAsync(reading, "sensor_readings");
        }

        /// <summary>
        /// Insert multiple sensor readings in batch
        /// </summary>
        public async Task InsertBatchAsync(List<SensorReading> readings)
        {
            var batch = new BatchStatement();
            ISession session = _context.GetSession();

            var preparedStatement = await session.PrepareAsync(
                "INSERT INTO sensor_readings (sensor_id, timestamp, temperature, humidity, co2_level, location) " +
                "VALUES (?, ?, ?, ?, ?, ?)"
            );

            foreach (var reading in readings)
            {
                var boundStatement = preparedStatement.Bind(
                    reading.SensorId,
                    reading.Timestamp,
                    reading.Temperature,
                    reading.Humidity,
                    reading.CO2Level,
                    reading.Location
                );

                batch.Add(boundStatement);
            }

            await session.ExecuteAsync(batch);
        }

        /// <summary>
        /// Get readings for a specific sensor within a time range
        /// </summary>
        public async Task<IEnumerable<SensorReading>> GetReadingsForSensorAsync(
            string sensorId, DateTime startTime, DateTime endTime)
        {
            string cql = "SELECT * FROM sensor_readings WHERE sensor_id = ? AND timestamp >= ? AND timestamp <= ?";

            return await _mapper.FetchAsync<SensorReading>(cql, sensorId, startTime, endTime);
        }

        /// <summary>
        /// Get the latest reading for each sensor
        /// </summary>
        public async Task<Dictionary<string, SensorReading>> GetLatestReadingsForAllSensorsAsync(IEnumerable<string> sensorIds)
        {
            var results = new Dictionary<string, SensorReading>();

            foreach (var sensorId in sensorIds)
            {
                // Get the latest reading for this sensor
                var latestReadings = await _mapper.FetchAsync<SensorReading>(
                    "SELECT * FROM sensor_readings WHERE sensor_id = ? LIMIT 1",
                    sensorId);

                var latestReading = latestReadings.FirstOrDefault();
                if (latestReading != null)
                {
                    results.Add(sensorId, latestReading);
                }
            }

            return results;
        }

        /// <summary>
        /// Get readings by location
        /// </summary>
        public async Task<IEnumerable<SensorReading>> GetReadingsByLocationAsync(string location)
        {
            string cql = "SELECT * FROM sensor_readings WHERE location = ? ALLOW FILTERING";
            return await _mapper.FetchAsync<SensorReading>(cql, location);
        }
    }
}
