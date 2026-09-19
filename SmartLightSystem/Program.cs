using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartLightSystem.Configuration;
using SmartLightSystem.Controllers;
using SmartLightSystem.HostedServices;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services
    .AddOptions<SmartLightOptions>()
    .Bind(builder.Configuration.GetSection(SmartLightOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<SmartLightOptions>, SmartLightOptionsValidator>();

builder.Services.AddSingleton(services =>
{
    var options = services.GetRequiredService<IOptions<SmartLightOptions>>().Value;
    return new HttpClient { Timeout = options.HttpTimeout };
});
builder.Services.AddSingleton<ITimeProvider, HttpTimeProvider>();
builder.Services.AddSingleton<ILightOutput, ConsoleLightOutput>();
builder.Services.AddSingleton<LightController>();
builder.Services.AddHostedService<LightControllerWorker>();

await builder.Build().RunAsync();
