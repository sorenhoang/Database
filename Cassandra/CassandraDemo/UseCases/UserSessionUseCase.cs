using CassandraDemo.Data;
using CassandraDemo.Models;

namespace CassandraDemo.UseCases
{
    /// <summary>
    /// Use case that demonstrates the user session and authentication features
    /// </summary>
    public class UserSessionUseCase
    {
        private readonly CassandraContext _context;
        private readonly UserSessionRepository _repository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Cassandra context</param>
        public UserSessionUseCase(CassandraContext context)
        {
            _context = context;
            _repository = new UserSessionRepository(context);
            _repository.Initialize();
        }

        /// <summary>
        /// Create sample user sessions
        /// </summary>
        public async Task CreateSampleSessionsAsync()
        {
            Console.WriteLine("\nCreating sample user sessions...");

            // Create a few user sessions
            var sessions = new List<UserSession>
            {
                new UserSession
                {
                    UserId = Guid.NewGuid(),
                    Username = "john.doe",
                    IpAddress = "192.168.1.100",
                    DeviceInfo = "Chrome 100.0.4896.127 on Windows 10",
                    Permissions = new Dictionary<string, bool>
                    {
                        { "read_profile", true },
                        { "edit_profile", true },
                        { "admin_access", false }
                    },
                    TimeToLive = 3600 // 1 hour
                },
                
                new UserSession
                {
                    UserId = Guid.NewGuid(),
                    Username = "jane.smith",
                    IpAddress = "192.168.1.101",
                    DeviceInfo = "Safari 15.4 on MacOS",
                    Permissions = new Dictionary<string, bool>
                    {
                        { "read_profile", true },
                        { "edit_profile", true },
                        { "admin_access", true }
                    },
                    TimeToLive = 86400 // 24 hours
                },
                
                new UserSession
                {
                    UserId = Guid.NewGuid(),
                    Username = "guest.user",
                    IpAddress = "192.168.1.102",
                    DeviceInfo = "Firefox 99.0 on Linux",
                    Permissions = new Dictionary<string, bool>
                    {
                        { "read_profile", true },
                        { "edit_profile", false },
                        { "admin_access", false }
                    },
                    TimeToLive = 1800 // 30 minutes
                }
            };

            // Save the sessions to Cassandra
            foreach (var session in sessions)
            {
                await _repository.CreateSessionAsync(session);
                Console.WriteLine($"Created session for {session.Username} with ID: {session.SessionId}");
            }

            return; // Return the session IDs for use in demonstrations
        }

        /// <summary>
        /// Demonstrate user session operations
        /// </summary>
        public async Task DemonstrateSessionOperationsAsync()
        {
            Console.WriteLine("\nCreating sample user sessions...");

            // Create a sample session
            var sessionId = Guid.NewGuid();
            var session = new UserSession
            {
                SessionId = sessionId,
                UserId = Guid.NewGuid(),
                Username = "demo.user",
                IpAddress = "192.168.1.103",
                DeviceInfo = "Edge 100.0.1185.50 on Windows 11",
                Permissions = new Dictionary<string, bool>
                {
                    { "read_profile", true },
                    { "edit_profile", true },
                    { "admin_access", false }
                },
                TimeToLive = 3600 // 1 hour
            };
            
            await _repository.CreateSessionAsync(session);
            Console.WriteLine($"Created session for {session.Username} with ID: {sessionId}");

            // Retrieve the session
            Console.WriteLine("\nRetrieving the session...");
            var retrievedSession = await _repository.GetSessionByIdAsync(sessionId);
            
            if (retrievedSession != null)
            {
                Console.WriteLine($"Retrieved session for {retrievedSession.Username}");
                Console.WriteLine($"Login time: {retrievedSession.LoginTime}");
                Console.WriteLine($"Last activity: {retrievedSession.LastActivity}");
                Console.WriteLine("Permissions:");
                foreach (var permission in retrievedSession.Permissions)
                {
                    Console.WriteLine($"  {permission.Key}: {permission.Value}");
                }
            }

            // Update session activity
            Console.WriteLine("\nUpdating session activity...");
            await _repository.UpdateSessionActivityAsync(sessionId);
            Console.WriteLine("Session activity updated");

            // Retrieve the updated session
            Console.WriteLine("\nRetrieving the updated session...");
            retrievedSession = await _repository.GetSessionByIdAsync(sessionId);
            
            if (retrievedSession != null)
            {
                Console.WriteLine($"Retrieved session for {retrievedSession.Username}");
                Console.WriteLine($"Login time: {retrievedSession.LoginTime}");
                Console.WriteLine($"Last activity: {retrievedSession.LastActivity} (updated)");
            }

            // Delete the session (logout)
            Console.WriteLine("\nDeleting session (logout)...");
            await _repository.DeleteSessionAsync(sessionId);
            Console.WriteLine("Session deleted");

            // Try to retrieve the deleted session
            Console.WriteLine("\nAttempting to retrieve the deleted session...");
            retrievedSession = await _repository.GetSessionByIdAsync(sessionId);
            
            if (retrievedSession == null)
            {
                Console.WriteLine("Session not found (as expected)");
            }
            else
            {
                Console.WriteLine("Session was unexpectedly found");
            }

            Console.WriteLine("\nNote: In a real application, TTL would automatically expire sessions");
            Console.WriteLine("after the specified period without requiring explicit deletion.");
        }
    }
}
