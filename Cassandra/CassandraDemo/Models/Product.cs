namespace CassandraDemo.Models
{
    /// <summary>
    /// Represents a product in our catalog use case
    /// </summary>
    public class Product
    {
        // Unique product identifier
        public Guid Id { get; set; }

        // Product category (used for partitioning)
        public required string Category { get; set; }

        // Product name
        public required string Name { get; set; }

        // Product description
        public required string Description { get; set; }

        // Price in USD
        public decimal Price { get; set; }

        // Current stock level
        public int StockLevel { get; set; }

        // When the product was added/updated
        public DateTime LastUpdated { get; set; }

        // Product attributes stored as key-value pairs
        // Demonstrates Cassandra's ability to store collections
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
