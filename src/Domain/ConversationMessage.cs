using Domain.Core;
using System;

namespace Domain
{
    public class ConversationMessage : EntityBase<ConversationMessage>
    {
        public Guid ConversationId { get; set; }
        public virtual Conversation Conversation { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
