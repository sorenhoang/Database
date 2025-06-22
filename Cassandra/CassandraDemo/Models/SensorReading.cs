namespace CassandraDemo.Models
{
    /// <summary>
    /// Represents a single sensor reading for time-series data use case
    /// </summary>
    public class SensorReading
    {
        // Sensor ID to distinguish different sensors
        public required string SensorId { get; set; }

        // Timestamp when the reading was taken
        public DateTime Timestamp { get; set; }

        // Temperature value
        public double Temperature { get; set; }

        // Humidity value
        public double Humidity { get; set; }

        // CO2 level
        public double CO2Level { get; set; }

        // Optional location information
        public string? Location { get; set; }
    }
}
