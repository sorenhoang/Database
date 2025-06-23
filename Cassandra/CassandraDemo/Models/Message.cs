using System;
using System.Collections.Generic;

namespace CassandraDemo.Models
{
    /// <summary>
    /// Represents a message in the messaging system use case
    /// </summary>
    public class Message
    {
        // Conversation identifier (partition key)
        public Guid ConversationId { get; set; }
        
        // Message timestamp (clustering key)
        public Guid Timestamp { get; set; } = Cassandra.TimeUuid.NewId();
        
        // Sender identifier
        public Guid SenderId { get; set; }
        
        // Message type (text, image, etc.)
        public required string MessageType { get; set; }
        
        // Message content
        public required string MessageContent { get; set; }
        
        // Attachments (URLs or references)
        public List<string> Attachments { get; set; } = new List<string>();
        
        // Read status by user
        public Dictionary<Guid, bool> ReadStatus { get; set; } = new Dictionary<Guid, bool>();
    }
}
