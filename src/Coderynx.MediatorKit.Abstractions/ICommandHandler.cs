using Coderynx.Functional.Results;

namespace Coderynx.MediatorKit.Abstractions;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult> where TResult : Result
{
    Task<TResult> HandleAsync(TCommand request, CancellationToken cancellationToken = default);
}