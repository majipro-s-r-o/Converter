using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Mocks;

public class SourceValueToTargetValueAsyncConverter : IAsyncConverter<SourceValueAsync, TargetValueAsync>
{
    public Task<TargetValueAsync> ConvertAsync(SourceValueAsync from, CancellationToken cancellationToken)
    {
        var result = new TargetValueAsync
        {
            Integer = from.Integer
        };

        return Task.FromResult(result);
    }
}
