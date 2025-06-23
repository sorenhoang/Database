using Cassandra;
using Cassandra.Mapping;
using CassandraDemo.Models;

namespace CassandraDemo.Data
{
    /// <summary>
    /// Repository for messaging system operations
    /// </summary>
    public class MessageRepository
    {
        private readonly CassandraContext _context;
        private readonly ISession _session;
        private readonly IMapper _mapper;
        private bool _tableInitialized = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Cassandra context</param>
        public MessageRepository(CassandraContext context)
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

            // Create table for messages
            _context.ExecuteCql(@"
                CREATE TABLE IF NOT EXISTS messages (
                    conversation_id uuid,
                    timestamp timeuuid,
                    sender_id uuid,
                    message_type text,
                    message_content text,
                    attachments list<text>,
                    read_status map<uuid, boolean>,
                    PRIMARY KEY (conversation_id, timestamp)
                ) WITH CLUSTERING ORDER BY (timestamp DESC)");

            _tableInitialized = true;
        }        /// <summary>
                 /// Send a new message
                 /// </summary>
                 /// <param name="message">Message</param>
        public async Task SendMessageAsync(Message message)
        {
            Initialize();

            var statement = new SimpleStatement(@"
                INSERT INTO messages (
                    conversation_id, timestamp, sender_id, message_type,
                    message_content, attachments, read_status
                ) VALUES (?, ?, ?, ?, ?, ?, ?)",
                message.ConversationId,
                message.Timestamp,
                message.SenderId,
                message.MessageType,
                message.MessageContent,
                message.Attachments,
                message.ReadStatus);

            await _session.ExecuteAsync(statement);
        }        /// <summary>
                 /// Get messages from a conversation with pagination
                 /// </summary>
                 /// <param name="conversationId">Conversation ID</param>
                 /// <param name="limit">Maximum number of messages</param>
                 /// <param name="pagingState">Paging state from a previous query</param>
                 /// <returns>Messages and paging state for next page</returns>
        public async Task<(List<Message> Messages, byte[]? PagingState)> GetMessagesAsync(
            Guid conversationId, int limit = 20, byte[]? pagingState = null)
        {
            Initialize();

            var statement = new SimpleStatement(@"
                SELECT * FROM messages
                WHERE conversation_id = ?
                LIMIT ?",
                conversationId, limit);

            if (pagingState != null)
                statement.SetPagingState(pagingState);

            var result = await _session.ExecuteAsync(statement);
            var messages = new List<Message>();

            foreach (var row in result)
            {
                messages.Add(new Message
                {
                    ConversationId = row.GetValue<Guid>("conversation_id"),
                    Timestamp = row.GetValue<Guid>("timestamp"),
                    SenderId = row.GetValue<Guid>("sender_id"),
                    MessageType = row.GetValue<string>("message_type"),
                    MessageContent = row.GetValue<string>("message_content"),
                    Attachments = row.GetValue<List<string>>("attachments"),
                    ReadStatus = row.GetValue<Dictionary<Guid, bool>>("read_status")
                });
            }

            return (messages, result.PagingState);
        }        /// <summary>
                 /// Mark messages as read for a user
                 /// </summary>
                 /// <param name="conversationId">Conversation ID</param>
                 /// <param name="userId">User ID</param>
                 /// <param name="timestamp">Message timestamp</param>
        public async Task MarkMessageAsReadAsync(Guid conversationId, Guid userId, Guid timestamp)
        {
            Initialize();

            var statement = new SimpleStatement(@"
                UPDATE messages
                SET read_status[?] = true
                WHERE conversation_id = ? AND timestamp = ?",
                userId, conversationId, timestamp);

            await _session.ExecuteAsync(statement);
        }

        /// <summary>
        /// Get messages within a specific time range
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="startTime">Start timestamp (exclusive)</param>
        /// <param name="endTime">End timestamp (inclusive)</param>
        /// <param name="limit">Maximum number of messages</param>
        /// <returns>Messages in the specified time range</returns>
        public async Task<List<Message>> GetMessagesByTimeRangeAsync(
            Guid conversationId, Guid startTime, Guid endTime, int limit = 100)
        {
            Initialize(); var statement = new SimpleStatement(@"
                SELECT * FROM messages
                WHERE conversation_id = ? 
                AND timestamp > ? 
                AND timestamp <= ?
                LIMIT ?", conversationId, startTime, endTime, limit);

            var result = await _session.ExecuteAsync(statement);
            var messages = new List<Message>();

            foreach (var row in result)
            {
                messages.Add(new Message
                {
                    ConversationId = row.GetValue<Guid>("conversation_id"),
                    Timestamp = row.GetValue<Guid>("timestamp"),
                    SenderId = row.GetValue<Guid>("sender_id"),
                    MessageType = row.GetValue<string>("message_type"),
                    MessageContent = row.GetValue<string>("message_content"),
                    Attachments = row.GetValue<List<string>>("attachments"),
                    ReadStatus = row.GetValue<Dictionary<Guid, bool>>("read_status")
                });
            }

            return messages;
        }

        /// <summary>
        /// Mark all messages in a conversation as read for a user
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <param name="userId">User ID</param>
        public async Task MarkAllMessagesAsReadAsync(Guid conversationId, Guid userId)
        {
            Initialize();            // First, get all unread messages for this conversation
            var statement = new SimpleStatement(@"
                SELECT timestamp FROM messages
                WHERE conversation_id = ? AND read_status[?] = false",
                conversationId, userId);

            var result = await _session.ExecuteAsync(statement);

            // For each message, mark it as read
            var batch = new BatchStatement();
            foreach (var row in result)
            {
                var timestamp = row.GetValue<Guid>("timestamp");
                var updateStatement = new SimpleStatement(@"
                    UPDATE messages
                    SET read_status[?] = true
                    WHERE conversation_id = ? AND timestamp = ?",
                    userId, conversationId, timestamp);

                batch.Add(updateStatement);
            }

            // Execute the batch if it has any statements
            if (batch.Statements.Any())
            {
                await _session.ExecuteAsync(batch);
            }
        }
    }
}
