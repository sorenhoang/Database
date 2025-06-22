using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using CassandraDemo.Models;

namespace CassandraDemo.Data
{
    /// <summary>
    /// Repository for product catalog data
    /// </summary>
    public class ProductRepository
    {
        private readonly CassandraContext _context;
        private readonly IMapper _mapper;

        public ProductRepository(CassandraContext context)
        {
            _context = context;
            _mapper = context.GetMapper();

            // Create the table if it doesn't exist
            CreateTableIfNotExists();
        }

        /// <summary>
        /// Creates the products table if it doesn't exist
        /// </summary>
        private void CreateTableIfNotExists()
        {
            _context.ExecuteCql(@"
                CREATE TABLE IF NOT EXISTS products (
                    id uuid,
                    category text,
                    name text,
                    description text,
                    price decimal,
                    stock_level int,
                    last_updated timestamp,
                    attributes map<text, text>,
                    PRIMARY KEY ((category), id)
                );
            ");

            // Create secondary indices for common queries
            _context.ExecuteCql("CREATE INDEX IF NOT EXISTS ON products(name);");
        }

        /// <summary>
        /// Insert or update a product
        /// </summary>
        public async Task UpsertAsync(Product product)
        {
            // Ensure we have a valid GUID
            if (product.Id == Guid.Empty)
            {
                product.Id = Guid.NewGuid();
            }

            // Set last updated time to now
            product.LastUpdated = DateTime.UtcNow;

            // Use Mapper to insert the product
            // Use string table name instead of CqlIdentifier
            await _mapper.InsertAsync(product, "products");
        }

        /// <summary>
        /// Get a product by ID
        /// </summary>
        public async Task<Product?> GetByIdAsync(Guid id)
        {
            string cql = "SELECT * FROM products WHERE id = ?";
            var products = await _mapper.FetchAsync<Product>(cql, id);
            return products.FirstOrDefault();
        }

        /// <summary>
        /// Get all products in a category
        /// </summary>
        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            string cql = "SELECT * FROM products WHERE category = ?";
            return await _mapper.FetchAsync<Product>(cql, category);
        }

        /// <summary>
        /// Search products by name (case-insensitive)
        /// </summary>
        public async Task<IEnumerable<Product>> SearchByNameAsync(string namePattern)
        {
            // Note: LIKE queries in Cassandra are limited compared to SQL databases
            // In a real application, consider using a dedicated search engine like Elasticsearch
            string cql = "SELECT * FROM products WHERE name LIKE ? ALLOW FILTERING";
            return await _mapper.FetchAsync<Product>(cql, $"%{namePattern}%");
        }
        /// <summary>
        /// Update product stock level
        /// </summary>
        public async Task UpdateStockLevelAsync(Guid id, int newStockLevel)
        {
            string cql = "UPDATE products SET stock_level = ?, last_updated = ? WHERE id = ?";
            var statement = _context.GetSession().Prepare(cql);
            var boundStatement = statement.Bind(newStockLevel, DateTime.UtcNow, id);
            await _context.GetSession().ExecuteAsync(boundStatement);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            string cql = "DELETE FROM products WHERE id = ?";
            var statement = _context.GetSession().Prepare(cql);
            var boundStatement = statement.Bind(id);
            await _context.GetSession().ExecuteAsync(boundStatement);
        }
    }
}
