using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Services.Features.Agent.Commands.SendMessage
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator(IStringLocalizer<Domain.Resources.Messages> localizer)
        {
            RuleFor(x => x.AgentName)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(SendMessageCommand.AgentName)]);

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage(localizer["RequiredField", nameof(SendMessageCommand.Message)])
                .MaximumLength(32000).WithMessage(localizer["MaxLength", nameof(SendMessageCommand.Message), 32000]);
        }
    }
}
