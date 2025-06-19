using Coderynx.Functional.Results;

namespace Coderynx.MediatorKit.Abstractions;

public interface IQuery<TResult> : IRequest<TResult> where TResult : Result;