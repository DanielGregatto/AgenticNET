using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;

namespace AgentInfrastructure.Conversations
{
    public class InMemoryConversationStore : IConversationStore
    {
        private readonly ConcurrentDictionary<string, ChatHistory> _histories = new();

        public ChatHistory GetOrCreate(string conversationId, string systemPrompt) =>
            _histories.GetOrAdd(conversationId, _ => new ChatHistory(systemPrompt));
    }
}
