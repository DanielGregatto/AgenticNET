using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentInfrastructure.Conversations
{
    public interface IConversationStore
    {
        ChatHistory GetOrCreate(string conversationId, string systemPrompt);
    }
}
