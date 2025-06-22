using CassandraDemo.Data;
using CassandraDemo.Models;

namespace CassandraDemo.UseCases
{
    /// <summary>
    /// Implements a product catalog use case with Cassandra
    /// </summary>
    public class ProductCatalogUseCase
    {
        private readonly ProductRepository _repository;

        public ProductCatalogUseCase(CassandraContext context)
        {
            _repository = new ProductRepository(context);
        }

        /// <summary>
        /// Populate the catalog with sample products
        /// </summary>
        public async Task PopulateSampleProductsAsync()
        {
            // Create sample products for different categories
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Category = "Electronics",
                    Name = "Smartphone X1",
                    Description = "High-end smartphone with amazing features",
                    Price = 899.99m,
                    StockLevel = 120,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "TechCorp" },
                        { "Color", "Black" },
                        { "Storage", "128GB" },
                        { "RAM", "8GB" }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Category = "Electronics",
                    Name = "Laptop Pro",
                    Description = "Powerful laptop for professionals",
                    Price = 1499.99m,
                    StockLevel = 50,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "TechCorp" },
                        { "Processor", "Intel i7" },
                        { "RAM", "16GB" },
                        { "Storage", "512GB SSD" },
                        { "Screen", "15.6 inch" }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Category = "Books",
                    Name = "Cassandra: The Definitive Guide",
                    Description = "Learn all about Cassandra database",
                    Price = 45.99m,
                    StockLevel = 200,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Author", "Jeff Carpenter" },
                        { "Format", "Paperback" },
                        { "Pages", "400" }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Category = "Books",
                    Name = "C# Programming Masterclass",
                    Description = "Complete C# programming guide",
                    Price = 39.99m,
                    StockLevel = 150,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Author", "John Smith" },
                        { "Format", "Hardcover" },
                        { "Pages", "550" }
                    }
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Category = "Home",
                    Name = "Smart Thermostat",
                    Description = "Control your home temperature from your phone",
                    Price = 129.99m,
                    StockLevel = 75,
                    Attributes = new Dictionary<string, string>
                    {
                        { "Brand", "HomeComfort" },
                        { "Color", "White" },
                        { "Wireless", "Yes" },
                        { "App Available", "iOS, Android" }
                    }
                }
            };

            // Insert all products
            foreach (var product in products)
            {
                await _repository.UpsertAsync(product);
                Console.WriteLine($"Added product: {product.Name} in category {product.Category}");
            }
        }

        /// <summary>
        /// Demonstrate various product catalog operations
        /// </summary>
        public async Task DemonstrateProductOperationsAsync()
        {
            // Demo 1: List products by category
            var category = "Electronics";
            Console.WriteLine($"\n=== Products in {category} category ===");
            var electronicProducts = await _repository.GetByCategoryAsync(category);
            foreach (var product in electronicProducts)
            {
                Console.WriteLine($"{product.Name} - {product.Description} - ${product.Price} - Stock: {product.StockLevel}");
                Console.WriteLine("Attributes:");
                foreach (var attr in product.Attributes)
                {
                    Console.WriteLine($"  {attr.Key}: {attr.Value}");
                }
                Console.WriteLine();
            }

            // Demo 2: Search for products by name
            var searchTerm = "Pro";
            Console.WriteLine($"\n=== Products matching '{searchTerm}' ===");
            var matchingProducts = await _repository.SearchByNameAsync(searchTerm);
            foreach (var product in matchingProducts)
            {
                Console.WriteLine($"{product.Name} - {product.Category} - ${product.Price}");
            }

            // Demo 3: Update stock levels
            if (electronicProducts.Any())
            {
                var productToUpdate = electronicProducts.First();
                int newStockLevel = productToUpdate.StockLevel - 5;

                Console.WriteLine($"\n=== Updating stock level for {productToUpdate.Name} ===");
                Console.WriteLine($"Old stock level: {productToUpdate.StockLevel}");

                await _repository.UpdateStockLevelAsync(productToUpdate.Id, newStockLevel);

                // Verify update
                var updatedProduct = await _repository.GetByIdAsync(productToUpdate.Id);
                Console.WriteLine($"New stock level: {updatedProduct?.StockLevel}");
            }
        }
    }
}
