using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartLightSystem.Configuration;
using SmartLightSystem.Controllers;
using SmartLightSystem.Exceptions;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;

namespace SmartLightSystem.IntegrationTests;

public sealed class HttpTimeProviderTests
{
    [Fact]
    public async Task SuccessfulResponse_ReturnsParsedOffsetTime()
    {
        using var client = CreateClient((_, _) => Json(HttpStatusCode.OK, """{"datetime":"2026-01-02T21:15:00+01:00"}"""));
        var provider = CreateProvider(client);

        var result = await provider.GetCurrentTimeAsync(CancellationToken.None);

        Assert.Equal(new DateTimeOffset(2026, 1, 2, 21, 15, 0, TimeSpan.FromHours(1)), result);
    }

    [Fact]
    public async Task Request_AppendsConfiguredTimeZoneToBaseUri()
    {
        Uri? requestedUri = null;
        using var client = CreateClient((request, _) =>
        {
            requestedUri = request.RequestUri;
            return Json(HttpStatusCode.OK, """{"datetime":"2026-01-02T21:15:00+01:00"}""");
        });
        var provider = CreateProvider(client);

        await provider.GetCurrentTimeAsync(CancellationToken.None);

        Assert.Equal("https://time.example/api/Europe/Brussels", requestedUri?.AbsoluteUri);
    }

    [Fact]
    public async Task NonSuccessStatus_ThrowsMeaningfulProviderException()
    {
        using var client = CreateClient((_, _) => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var provider = CreateProvider(client);

        var exception = await Assert.ThrowsAsync<TimeProviderException>(
            () => provider.GetCurrentTimeAsync(CancellationToken.None));

        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("{\"datetime\":\"not-a-date\"}")]
    [InlineData("{}")]
    public async Task InvalidPayload_ThrowsMeaningfulProviderException(string payload)
    {
        using var client = CreateClient((_, _) => Json(HttpStatusCode.OK, payload));
        var provider = CreateProvider(client);

        await Assert.ThrowsAsync<TimeProviderException>(
            () => provider.GetCurrentTimeAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Timeout_IsReportedAsProviderFailure()
    {
        using var client = CreateClient(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return Json(HttpStatusCode.OK, "{}");
        });
        client.Timeout = TimeSpan.FromMilliseconds(20);
        var provider = CreateProvider(client);

        var exception = await Assert.ThrowsAsync<TimeProviderException>(
            () => provider.GetCurrentTimeAsync(CancellationToken.None));

        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CallerCancellation_RemainsCancellation()
    {
        using var client = CreateClient(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return Json(HttpStatusCode.OK, "{}");
        });
        var provider = CreateProvider(client);
        using var source = new CancellationTokenSource(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetCurrentTimeAsync(source.Token));
    }

    [Fact]
    public async Task Controller_RecoversAfterTransientHttpFailures()
    {
        var call = 0;
        using var client = CreateClient((_, _) => ++call <= 2
            ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            : Json(HttpStatusCode.OK, """{"datetime":"2026-01-02T21:15:00+01:00"}"""));
        var options = Options.Create(CreateOptions());
        var provider = new HttpTimeProvider(client, options);
        var light = new InMemoryLightOutput();
        var controller = new LightController(provider, light, options, NullLogger<LightController>.Instance);

        await controller.RunOnceAsync();
        await controller.RunOnceAsync();
        Assert.True(controller.IsInSafeMode);

        await controller.RunOnceAsync();

        Assert.False(controller.IsInSafeMode);
        Assert.True(light.IsEnabled);
        Assert.Equal(0, controller.ConsecutiveFailures);
    }

    private static HttpTimeProvider CreateProvider(HttpClient client) =>
        new(client, Options.Create(CreateOptions()));

    private static SmartLightOptions CreateOptions() => new()
    {
        ActiveStart = new TimeOnly(20, 0),
        ActiveEnd = new TimeOnly(6, 0),
        MaxConsecutiveFailures = 2,
        TimeZoneId = "Europe/Brussels",
        TimeServiceBaseUri = new Uri("https://time.example/api/"),
    };

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory) =>
        new(new StubHttpMessageHandler((request, token) => Task.FromResult(responseFactory(request, token))));

    private static HttpClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) =>
        new(new StubHttpMessageHandler(responseFactory));

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string content) => new(statusCode)
    {
        Content = new StringContent(content, Encoding.UTF8, "application/json"),
    };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => responseFactory(request, cancellationToken);
    }

    private sealed class InMemoryLightOutput : ILightOutput
    {
        public bool IsEnabled { get; private set; }

        public void SetEnabled(bool enabled) => IsEnabled = enabled;
    }
}
