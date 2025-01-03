# Smart Light System

## Structuur van de applicatie

De applicatie bestaat uit de volgende klassen:
- `LightController`: Bevat de beslissingslogica voor het in- en uitschakelen van het licht.
- `TimeProviderReal`: Haalt de huidige tijd op via een API.
- `LightElement` en `LightElementStub`: Implementeren de `ILightElement` interface en voeren acties uit zoals het in- en uitschakelen van een licht.

## Klassendiagram

![Klassendiagram](path/to/your/class-diagram.png)

## Testen

De testen zijn geschreven met behulp van TDD en maken gebruik van mocking waar nodig. De testen voldoen aan de ZOMBIES criteria:

- **Zero**: Geen specifieke test voor nul, maar de `Work_WhenTimeFailsAndNotInSafeMode_DoNothing` test behandelt een soortgelijk scenario.
- **One**: `Work_WhenTimeEqualsStartTime_EnablesLight` en `Work_WhenTimeEqualsEndTime_DisablesLight` testen de grenswaarden.
- **Many**: `Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode` test meerdere opeenvolgende fouten.
- **Boundary**: `Work_WhenTimeEqualsStartTime_EnablesLight` en `Work_WhenTimeEqualsEndTime_DisablesLight` testen de grenswaarden.
- **Interface**: De tests maken gebruik van de `ITimeProvider` en `ILightElement` interfaces.
- **Exception**: `Work_WhenTimeFailsAndNotInSafeMode_DoNothing` en `Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode` testen exception handling.
- **Simple**: De tests zijn eenvoudig en gericht op specifieke scenario's.

## Hoe de testen zijn opgesteld

De testen zijn opgesteld door eerst de beslissingslogica in de `LightController` klasse te definiëren en vervolgens tests te schrijven voor verschillende scenario's. Hierbij is gebruik gemaakt van mocking om de afhankelijkheden te isoleren en de tests te focussen op de logica van de `LightController`.
