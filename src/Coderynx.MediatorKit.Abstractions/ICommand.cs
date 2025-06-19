using Coderynx.Functional.Results;

namespace Coderynx.MediatorKit.Abstractions;

public interface ICommand<TResponse> : IRequest<TResponse> where TResponse : Result;