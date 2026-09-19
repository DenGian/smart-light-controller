using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartLightSystem.Configuration;
using SmartLightSystem.Exceptions;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Models;

namespace SmartLightSystem.Services;

public sealed class HttpTimeProvider : ITimeProvider
{
    private readonly HttpClient _httpClient;
    private readonly Uri _requestUri;

    public HttpTimeProvider(HttpClient httpClient, IOptions<SmartLightOptions> options)
    {
        _httpClient = httpClient;
        var settings = options.Value;
        var escapedTimeZone = string.Join(
            '/',
            settings.TimeZoneId.Split('/').Select(Uri.EscapeDataString));
        _requestUri = new Uri(settings.TimeServiceBaseUri, escapedTimeZone);
    }

    public async Task<DateTimeOffset> GetCurrentTimeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                _requestUri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<TimeResponse>(
                cancellationToken: cancellationToken);

            if (payload?.DateTime is not { } currentTime)
            {
                throw new TimeProviderException("The time service response did not contain a valid 'datetime' value.");
            }

            return currentTime;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException exception)
        {
            throw new TimeProviderException("The time service request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new TimeProviderException("The time service request failed.", exception);
        }
        catch (JsonException exception)
        {
            throw new TimeProviderException("The time service returned invalid JSON.", exception);
        }
    }
}
