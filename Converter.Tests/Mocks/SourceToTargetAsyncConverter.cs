using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Mocks;

public class SourceToTargetAsyncConverter : IAsyncConverter<SourceAsync, TargetAsync>
{
    public Task<TargetAsync> ConvertAsync(SourceAsync from, CancellationToken cancellationToken)
    {
        var result = new TargetAsync
        {
            Integer = from.Integer
        };
        
        return Task.FromResult(result);
    }
}
