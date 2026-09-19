using SmartLightSystem.Configuration;

namespace SmartLightSystem.UnitTests;

public sealed class SmartLightOptionsValidatorTests
{
    [Fact]
    public void ValidConfiguration_Succeeds()
    {
        var result = new SmartLightOptionsValidator().Validate(null, new SmartLightOptions());

        Assert.False(result.Failed);
    }

    [Fact]
    public void InvalidConfiguration_ReportsEveryProblem()
    {
        var options = new SmartLightOptions
        {
            MaxConsecutiveFailures = 0,
            PollingInterval = TimeSpan.Zero,
            HttpTimeout = TimeSpan.Zero,
            TimeZoneId = " ",
            TimeServiceBaseUri = new Uri("http://example.test/"),
        };

        var result = new SmartLightOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Equal(5, result.Failures.Count());
    }

    [Fact]
    public void BaseUriWithoutTrailingSlash_FailsValidation()
    {
        var options = new SmartLightOptions
        {
            TimeServiceBaseUri = new Uri("https://example.test/api"),
        };

        var result = new SmartLightOptionsValidator().Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains("end with '/'", StringComparison.Ordinal));
    }
}
