using Domain.Core;
using System.Collections.Generic;

namespace Domain
{
    public class Conversation : EntityBase<Conversation>
    {
        public string UserId { get; set; }
        public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
        public ICollection<ConversationTurn> Turns { get; set; } = new List<ConversationTurn>();
    }
}
