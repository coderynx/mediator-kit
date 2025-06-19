using Coderynx.Functional.Results;

namespace Coderynx.MediatorKit.Abstractions;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult> where TResult : Result
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}