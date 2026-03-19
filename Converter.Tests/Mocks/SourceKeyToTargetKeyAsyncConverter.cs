using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Mocks;

public class SourceKeyToTargetKeyAsyncConverter : IAsyncConverter<SourceKeyAsync, TargetKeyAsync>
{
    public Task<TargetKeyAsync> ConvertAsync(SourceKeyAsync from, CancellationToken cancellationToken)
    {
        var result = new TargetKeyAsync
        {
            Integer = from.Integer
        };

        return Task.FromResult(result);
    }
}
