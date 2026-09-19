using Microsoft.Extensions.Options;

namespace SmartLightController.Configuration;

public sealed class SmartLightOptionsValidator : IValidateOptions<SmartLightOptions>
{
    public ValidateOptionsResult Validate(string? name, SmartLightOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (options.MaxConsecutiveFailures < 1)
        {
            errors.Add("SmartLight:MaxConsecutiveFailures must be at least 1.");
        }

        if (options.PollingInterval <= TimeSpan.Zero)
        {
            errors.Add("SmartLight:PollingInterval must be greater than zero.");
        }

        if (options.HttpTimeout <= TimeSpan.Zero)
        {
            errors.Add("SmartLight:HttpTimeout must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.TimeZoneId))
        {
            errors.Add("SmartLight:TimeZoneId is required.");
        }

        if (!options.TimeServiceBaseUri.IsAbsoluteUri ||
            options.TimeServiceBaseUri.Scheme != Uri.UriSchemeHttps)
        {
            errors.Add("SmartLight:TimeServiceBaseUri must be an absolute HTTPS URI.");
        }
        else if (!options.TimeServiceBaseUri.AbsolutePath.EndsWith('/'))
        {
            errors.Add("SmartLight:TimeServiceBaseUri must end with '/'.");
        }

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
