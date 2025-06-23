using Cassandra;
using Cassandra.Mapping;
using CassandraDemo.Models;

namespace CassandraDemo.Data
{
    /// <summary>
    /// Repository for user session operations
    /// </summary>
    public class UserSessionRepository
    {
        private readonly CassandraContext _context;
        private readonly ISession _session;
        private readonly IMapper _mapper;
        private bool _tableInitialized = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Cassandra context</param>
        public UserSessionRepository(CassandraContext context)
        {
            _context = context;
            _session = context.GetSession();
            _mapper = context.GetMapper();
        }

        /// <summary>
        /// Initialize the table schema
        /// </summary>
        public void Initialize()
        {
            if (_tableInitialized)
                return;

            // Create table for user sessions
            _context.ExecuteCql(@"
                CREATE TABLE IF NOT EXISTS user_sessions (
                    session_id uuid,
                    user_id uuid,
                    username text,
                    login_time timestamp,
                    last_activity timestamp,
                    ip_address text,
                    device_info text,
                    permissions map<text, boolean>,
                    PRIMARY KEY (session_id)
                ) WITH default_time_to_live = 86400");

            _tableInitialized = true;
        }

        /// <summary>
        /// Create a new user session
        /// </summary>
        /// <param name="session">User session</param>
        public async Task CreateSessionAsync(UserSession session)
        {
            Initialize(); var statement = new SimpleStatement(@"
                INSERT INTO user_sessions (
                    session_id, user_id, username, login_time, last_activity,
                    ip_address, device_info, permissions
                ) VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                USING TTL ?",
                session.SessionId,
                session.UserId,
                session.Username,
                session.LoginTime,
                session.LastActivity,
                session.IpAddress,
                session.DeviceInfo,
                session.Permissions,
                session.TimeToLive);

            await _session.ExecuteAsync(statement);
        }

        /// <summary>
        /// Get a user session by session ID
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>User session or null if not found</returns>
        public async Task<UserSession?> GetSessionByIdAsync(Guid sessionId)
        {
            Initialize(); var statement = new SimpleStatement("SELECT * FROM user_sessions WHERE session_id = ?", sessionId);

            var result = await _session.ExecuteAsync(statement);
            var row = result.FirstOrDefault();

            if (row == null)
                return null;

            return new UserSession
            {
                SessionId = row.GetValue<Guid>("session_id"),
                UserId = row.GetValue<Guid>("user_id"),
                Username = row.GetValue<string>("username"),
                LoginTime = row.GetValue<DateTime>("login_time"),
                LastActivity = row.GetValue<DateTime>("last_activity"),
                IpAddress = row.GetValue<string>("ip_address"),
                DeviceInfo = row.GetValue<string>("device_info"),
                Permissions = row.GetValue<Dictionary<string, bool>>("permissions")
            };
        }

        /// <summary>
        /// Update the activity timestamp for a session
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="timeToLive">New TTL in seconds</param>
        public async Task UpdateSessionActivityAsync(Guid sessionId, int timeToLive = 86400)
        {
            Initialize(); var statement = new SimpleStatement(@"
                UPDATE user_sessions 
                USING TTL ? 
                SET last_activity = ? 
                WHERE session_id = ?",
                timeToLive,
                DateTime.UtcNow,
                sessionId);

            await _session.ExecuteAsync(statement);
        }

        /// <summary>
        /// Delete a user session
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public async Task DeleteSessionAsync(Guid sessionId)
        {
            Initialize(); var statement = new SimpleStatement("DELETE FROM user_sessions WHERE session_id = ?", sessionId);

            await _session.ExecuteAsync(statement);
        }
    }
}
