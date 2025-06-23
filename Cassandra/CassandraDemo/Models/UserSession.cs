using System;
using System.Collections.Generic;

namespace CassandraDemo.Models
{
    /// <summary>
    /// Represents a user session for the authentication use case
    /// </summary>
    public class UserSession
    {
        // Unique session identifier
        public Guid SessionId { get; set; } = Guid.NewGuid();
        
        // User identifier
        public Guid UserId { get; set; }
        
        // Username
        public required string Username { get; set; }
        
        // When the session was created
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        
        // Last activity time
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        // IP address of the user
        public required string IpAddress { get; set; }
        
        // Device information
        public required string DeviceInfo { get; set; }
        
        // User permissions stored as key-value pairs
        public Dictionary<string, bool> Permissions { get; set; } = new Dictionary<string, bool>();
        
        // TTL (Time To Live) in seconds - default 24 hours
        public int TimeToLive { get; set; } = 86400;
    }
}
