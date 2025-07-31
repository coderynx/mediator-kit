using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.RequestTests;

public sealed class TestRequest : IRequest<int>;

public class TestRequestHandler : IRequestHandler<TestRequest, int>
{
    public Task<int> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }
}