using MediatR;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IMediatorHandler
    {
        /// <summary>
        /// Publishes the specified event to all registered handlers asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the event to publish. Must implement <see cref="INotification"/>.</typeparam>
        /// <param name="evento">The event to publish. Must implement <see cref="INotification"/>.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task PublishEvent<T>(T evento) where T : INotification;

        /// <summary>
        /// Sends a command of the specified type for processing.
        /// </summary>
        /// <typeparam name="T">The type of the command to send. Must implement <see cref="IRequest"/>.</typeparam>
        /// <param name="comando">The command instance to be sent. Cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task SendCommand<T>(T comando) where T : IRequest;

        /// <summary>
        /// Sends the specified command to the underlying request handler and asynchronously returns the response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response expected from the command.</typeparam>
        /// <param name="comando">The command to send. Must implement <see cref="IRequest{TResponse}"/> and cannot be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response returned by the
        /// handler.</returns>
        Task<TResponse> SendCommand<TResponse>(IRequest<TResponse> comando);
    }
}
