using Domain.Interfaces;
using MediatR;
using System.Threading.Tasks;

namespace Services.Infrastructure
{
    public class MediatorHandler : IMediatorHandler
    {
        private readonly IMediator _mediator;

        public MediatorHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendCommand<T>(T comando) where T : IRequest
        {
            await _mediator.Send(comando);
        }

        public async Task<TResponse> SendCommand<TResponse>(IRequest<TResponse> comando)
        {
            return await _mediator.Send(comando);
        }

        public async Task PublishEvent<T>(T evento) where T : INotification
        {
            await _mediator.Publish(evento);
        }
    }
}
