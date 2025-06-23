using CassandraDemo.Data;
using CassandraDemo.Models;

namespace CassandraDemo.UseCases
{
    /// <summary>
    /// Use case that demonstrates the messaging system features
    /// </summary>
    public class MessagingSystemUseCase
    {
        private readonly CassandraContext _context;
        private readonly MessageRepository _repository;
        private Guid _conversationId = Guid.NewGuid(); // Sample conversation ID
        private List<Guid> _userIds = new List<Guid>(); // Sample user IDs

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Cassandra context</param>
        public MessagingSystemUseCase(CassandraContext context)
        {
            _context = context;
            _repository = new MessageRepository(context);
            _repository.Initialize();

            // Create sample users
            for (int i = 0; i < 3; i++)
            {
                _userIds.Add(Guid.NewGuid());
            }
        }

        /// <summary>
        /// Simulate a messaging conversation between users
        /// </summary>
        public async Task SimulateConversationAsync()
        {
            Console.WriteLine("\nSimulating a messaging conversation...");
            Console.WriteLine($"Conversation ID: {_conversationId}");
            Console.WriteLine($"Users in conversation: {_userIds.Count}");

            // Create a series of messages in the conversation
            var messages = new List<Message>
            {
                new Message
                {
                    ConversationId = _conversationId,
                    SenderId = _userIds[0],
                    MessageType = "text",
                    MessageContent = "Hello team! How is everyone doing today?",
                    ReadStatus = new Dictionary<Guid, bool>
                    {
                        { _userIds[0], true }, // The sender has read the message
                        { _userIds[1], false },
                        { _userIds[2], false }
                    }
                },

                new Message
                {
                    ConversationId = _conversationId,
                    SenderId = _userIds[1],
                    MessageType = "text",
                    MessageContent = "Hi! I'm good, working on the Cassandra implementation.",
                    ReadStatus = new Dictionary<Guid, bool>
                    {
                        { _userIds[0], false },
                        { _userIds[1], true }, // The sender has read the message
                        { _userIds[2], false }
                    }
                },

                new Message
                {
                    ConversationId = _conversationId,
                    SenderId = _userIds[2],
                    MessageType = "text",
                    MessageContent = "Hello! I'm reviewing the data model. Check out this document:",
                    ReadStatus = new Dictionary<Guid, bool>
                    {
                        { _userIds[0], false },
                        { _userIds[1], false },
                        { _userIds[2], true } // The sender has read the message
                    },
                    Attachments = new List<string> { "https://example.com/document.pdf" }
                },

                new Message
                {
                    ConversationId = _conversationId,
                    SenderId = _userIds[0],
                    MessageType = "image",
                    MessageContent = "Here's a diagram of the architecture:",
                    ReadStatus = new Dictionary<Guid, bool>
                    {
                        { _userIds[0], true }, // The sender has read the message
                        { _userIds[1], false },
                        { _userIds[2], false }
                    },
                    Attachments = new List<string> { "https://example.com/architecture.png" }
                }
            };

            // Send the messages with a slight delay to ensure ordering
            foreach (var message in messages)
            {
                await _repository.SendMessageAsync(message);
                Console.WriteLine($"Sent message: {message.MessageContent.Substring(0, Math.Min(20, message.MessageContent.Length))}... [{message.MessageType}]");

                // Add a small delay between messages
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Demonstrate messaging operations
        /// </summary>
        public async Task DemonstrateMessagingOperationsAsync()
        {
            // First, create a conversation
            await SimulateConversationAsync();

            Console.WriteLine("\nRetrieving messages from the conversation...");
            var (messages, pagingState) = await _repository.GetMessagesAsync(_conversationId, 10);

            // Display the messages
            Console.WriteLine($"\nFound {messages.Count} messages:");
            foreach (var message in messages)
            {
                Console.WriteLine($"From: User {Array.IndexOf(_userIds.ToArray(), message.SenderId) + 1}");
                Console.WriteLine($"Type: {message.MessageType}");
                Console.WriteLine($"Content: {message.MessageContent}");

                if (message.Attachments.Count > 0)
                {
                    Console.WriteLine($"Attachments: {string.Join(", ", message.Attachments)}");
                }

                var readStatus = message.ReadStatus.Select(rs => $"User {Array.IndexOf(_userIds.ToArray(), rs.Key) + 1}: {(rs.Value ? "Read" : "Unread")}");
                Console.WriteLine($"Read Status: {string.Join(", ", readStatus)}");
                Console.WriteLine();
            }

            // Simulate marking a message as read
            if (messages.Count > 0)
            {
                var messageToMark = messages[0]; // Mark the first message
                await _repository.MarkMessageAsReadAsync(
                    messageToMark.ConversationId,
                    _userIds[1], // User 2 marking as read
                    messageToMark.Timestamp
                );

                Console.WriteLine($"\nMarked message as read by User 2: \"{messageToMark.MessageContent.Substring(0, Math.Min(20, messageToMark.MessageContent.Length))}...\"");

                // Retrieve the updated message
                var (updatedMessages, _) = await _repository.GetMessagesAsync(_conversationId, 1);
                if (updatedMessages.Count > 0)
                {
                    var updatedMessage = updatedMessages[0];
                    var readStatus = updatedMessage.ReadStatus.Select(rs => $"User {Array.IndexOf(_userIds.ToArray(), rs.Key) + 1}: {(rs.Value ? "Read" : "Unread")}");
                    Console.WriteLine($"Updated Read Status: {string.Join(", ", readStatus)}");
                }
            }

            Console.WriteLine("\nNote: In a real application, this messaging system could scale");
            Console.WriteLine("to millions of conversations and messages while maintaining performance");
            Console.WriteLine("thanks to Cassandra's partitioning by conversation_id.");
        }
    }
}
