namespace NetEvolve.Http.Correlation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NetEvolve.Http.Correlation.Generators;

internal sealed class SequentialGuidConfigure : IConfigureOptions<SequentialGuidOptions>
{
    private readonly IConfiguration _configuration;

    public SequentialGuidConfigure(IConfiguration configuration) => _configuration = configuration;

    public void Configure(SequentialGuidOptions options) => _configuration.Bind($"HttpCorralation:Sequential", options);
}
