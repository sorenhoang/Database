using CassandraDemo.Data;
using CassandraDemo.UseCases;

namespace CassandraDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Cassandra Demo Application");
            Console.WriteLine("=========================\n");

            // Set up connection parameters
            // Note: By default, Cassandra runs on 127.0.0.1:9042
            var contactPoints = new string[] { "127.0.0.1" };
            var dataCenter = "datacenter1"; // Default datacenter name for local setup
            var keyspace = "cassandra_demo";

            // Create Cassandra context
            var context = new CassandraContext(contactPoints, dataCenter, keyspace);

            try
            {
                // Connect to Cassandra
                Console.WriteLine("Connecting to Cassandra...");
                context.Connect();

                // Display menu and run selected use case
                while (true)
                {
                    Console.WriteLine("\nSelect a use case to run:");
                    Console.WriteLine("1. IoT Sensor Data (Time Series)");
                    Console.WriteLine("2. Product Catalog");
                    Console.WriteLine("3. Exit");
                    Console.Write("Enter your choice (1-3): ");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            await RunIoTSensorUseCaseAsync(context);
                            break;

                        case "2":
                            await RunProductCatalogUseCaseAsync(context);
                            break;

                        case "3":
                            Console.WriteLine("Exiting application...");
                            return;

                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Clean up resources
                context.Dispose();
            }
        }

        /// <summary>
        /// Run the IoT sensor time series use case
        /// </summary>
        static async Task RunIoTSensorUseCaseAsync(CassandraContext context)
        {
            Console.WriteLine("\n** IoT Sensor Data Use Case **");

            var useCase = new IoTSensorUseCase(context);

            Console.WriteLine("\nSimulating sensor data...");

            // Generate 5 sensors with 10 readings each
            await useCase.SimulateSensorDataAsync(5, 10);

            // Run demonstration queries
            await useCase.DemonstrateQueriesAsync();
        }

        /// <summary>
        /// Run the product catalog use case
        /// </summary>
        static async Task RunProductCatalogUseCaseAsync(CassandraContext context)
        {
            Console.WriteLine("\n** Product Catalog Use Case **");

            var useCase = new ProductCatalogUseCase(context);

            Console.WriteLine("\nPopulating sample products...");
            await useCase.PopulateSampleProductsAsync();

            // Run demonstration operations
            await useCase.DemonstrateProductOperationsAsync();
        }
    }
}
