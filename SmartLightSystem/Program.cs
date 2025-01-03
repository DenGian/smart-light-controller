using SmartLightSystem.Controllers;
using SmartLightSystem.Interfaces;
using SmartLightSystem.Services;

ITimeProvider timeProvider = new TimeProviderReal();
ILightElement lightElement = new LightElementStub();

LightController controller = new LightController(timeProvider, lightElement);

while (true)
{
    controller.Work();
    Thread.Sleep(5000);
}
