using System;
using System.Collections.Generic;

namespace Services.Contracts.Results
{
    public record ConversationResult(
        Guid Id,
        string UserId,
        List<ConversationMessageResult> Messages);
}
