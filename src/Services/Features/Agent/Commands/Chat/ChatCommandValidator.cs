using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Agent.Commands.Chat
{
    public class ChatCommandValidator : AbstractValidator<ChatCommand>
    {
        public ChatCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(ChatCommand.Message)])
                .MaximumLength(32000).WithMessage(localizer["MaxLength", nameof(ChatCommand.Message), 32000]);
        }
    }
}
