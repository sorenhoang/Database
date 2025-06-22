using CassandraDemo.Data;
using CassandraDemo.Models;

namespace CassandraDemo.UseCases
{
    /// <summary>
    /// Implements a IoT sensor data use case with Cassandra
    /// </summary>
    public class IoTSensorUseCase
    {
        private readonly SensorReadingRepository _repository;
        private readonly Random _random = new Random();

        public IoTSensorUseCase(CassandraContext context)
        {
            _repository = new SensorReadingRepository(context);
        }

        /// <summary>
        /// Simulate a stream of sensor data and store it in Cassandra
        /// </summary>
        public async Task SimulateSensorDataAsync(int numSensors, int readingsPerSensor)
        {
            var sensorsIds = new List<string>();
            var locations = new[] { "Kitchen", "Living Room", "Bedroom", "Bathroom", "Garage" };

            // Generate sensor IDs
            for (int i = 1; i <= numSensors; i++)
            {
                sensorsIds.Add($"sensor-{i}");
            }

            Console.WriteLine($"Simulating data for {numSensors} sensors with {readingsPerSensor} readings each");

            foreach (var sensorId in sensorsIds)
            {
                var readings = new List<SensorReading>();
                var location = locations[_random.Next(locations.Length)];

                for (int i = 0; i < readingsPerSensor; i++)
                {
                    var reading = new SensorReading
                    {
                        SensorId = sensorId,
                        Timestamp = DateTime.UtcNow.AddMinutes(-i),  // Stagger timestamps
                        Temperature = Math.Round(18 + (_random.NextDouble() * 10), 1),  // Temperature between 18-28
                        Humidity = Math.Round(30 + (_random.NextDouble() * 60), 1),     // Humidity between 30-90%
                        CO2Level = Math.Round(400 + (_random.NextDouble() * 1000), 0),  // CO2 between 400-1400 ppm
                        Location = location
                    };

                    readings.Add(reading);
                }

                // Insert batch
                await _repository.InsertBatchAsync(readings);

                Console.WriteLine($"Inserted {readings.Count} readings for sensor {sensorId} in {location}");
            }
        }

        /// <summary>
        /// Demonstrate how to query time-series data
        /// </summary>
        public async Task DemonstrateQueriesAsync()
        {
            // Get list of all sensor IDs (for a real app, you'd store this separately)
            var allSensors = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                allSensors.Add($"sensor-{i}");
            }

            // Demo 1: Get latest readings for all sensors
            Console.WriteLine("\n=== Latest Readings for All Sensors ===");
            var latestReadings = await _repository.GetLatestReadingsForAllSensorsAsync(allSensors);
            foreach (var entry in latestReadings)
            {
                Console.WriteLine($"Sensor {entry.Key} - Temperature: {entry.Value.Temperature}°C, " +
                                  $"Humidity: {entry.Value.Humidity}%, CO2: {entry.Value.CO2Level} ppm, " +
                                  $"Location: {entry.Value.Location}, Time: {entry.Value.Timestamp}");
            }

            // Demo 2: Get readings for a specific time range
            var sensorId = "sensor-1";
            var startTime = DateTime.UtcNow.AddMinutes(-30);
            var endTime = DateTime.UtcNow;

            Console.WriteLine($"\n=== Readings for Sensor {sensorId} from {startTime} to {endTime} ===");
            var timeRangeReadings = await _repository.GetReadingsForSensorAsync(sensorId, startTime, endTime);
            foreach (var reading in timeRangeReadings)
            {
                Console.WriteLine($"Time: {reading.Timestamp}, Temp: {reading.Temperature}°C, " +
                                  $"Humidity: {reading.Humidity}%, CO2: {reading.CO2Level} ppm");
            }

            // Demo 3: Get readings by location
            var location = "Living Room";
            Console.WriteLine($"\n=== Readings for Location: {location} ===");
            var locationReadings = await _repository.GetReadingsByLocationAsync(location);
            foreach (var reading in locationReadings)
            {
                Console.WriteLine($"Sensor: {reading.SensorId}, Time: {reading.Timestamp}, " +
                                  $"Temp: {reading.Temperature}°C, Humidity: {reading.Humidity}%");
            }
        }
    }
}
