# Smart Light System

## Structure of the Application

The application consists of the following classes:
- `LightController`: Contains the decision logic for turning the light on and off.
- `TimeProviderReal`: Fetches the current time via an API.
- `LightElement` and `LightElementStub`: Implement the `ILightElement` interface and perform actions such as turning a light on and off.

## Class Diagram

![Class Diagram](path/to/your/class-diagram.png)

## Tests

The tests are written using TDD and make use of mocking where necessary. The tests adhere to the ZOMBIES criteria:

- **Zero**: The `Work_WhenTimeFailsAndNotInSafeMode_DoNothing` test handles a similar scenario.
- **One**: `Work_WhenTimeEqualsStartTime_EnablesLight` and `Work_WhenTimeEqualsEndTime_DisablesLight` test the boundary values.
- **Many**: `Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode` tests multiple consecutive failures.
- **Boundary**: `Work_WhenTimeEqualsStartTime_EnablesLight` and `Work_WhenTimeEqualsEndTime_DisablesLight` test the boundary values.
- **Interface**: The tests use the `ITimeProvider` and `ILightElement` interfaces.
- **Exception**: `Work_WhenTimeFailsAndNotInSafeMode_DoNothing` and `Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode` test exception handling.
- **Simple**: The tests are simple and focused on specific scenarios.

### Test Descriptions

1. **Work_WhenTimeIsDuringActiveHours_EnablesLight**:
    - Checks if the light is turned on during active hours.

2. **Work_WhenTimeIsOutsideActiveHours_DisablesLight**:
    - Checks if the light is turned off outside active hours.

3. **Work_WhenTimeEqualsStartTime_EnablesLight**:
    - Checks if the light is turned on when the current time equals the start time.

4. **Work_WhenTimeEqualsEndTime_DisablesLight**:
    - Checks if the light is turned off when the current time equals the end time.

5. **Work_WhenTimeFailsAndNotInSafeMode_DoNothing**:
    - Checks if no action is taken when time retrieval fails and the system is not in safe mode.

6. **Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode**:
    - Checks if the system enters safe mode after the maximum number of time retrieval failures.

7. **Work_WhenInSafeModeAndTimeSucceeds_ResetsSafeMode**:
    - Checks if the system exits safe mode after a successful time retrieval.

8. **Work_WhenStartTimeEqualsEndTime_LightStaysDisabled**:
    - Checks if the light remains off when the start time equals the end time.

9. **Work_WhenTimeSpansOvernight_EnablesLightAfterStartTime**:
    - Checks if the light is turned on when the current time is after the start time, even if the period spans overnight.

10. **Work_WhenTimeSpansOvernight_EnablesLightBeforeEndTime**:
    - Checks if the light is turned on when the current time is before the end time, even if the period spans overnight.

11. **Work_AfterFailure_ResetsFailureCountOnSuccess**:
    - Checks if the failure count is reset after a successful time retrieval.

12. **Work_WhenOneMinuteBeforeEndTime_StillEnabled**:
    - Checks if the light remains on when the current time is one minute before the end time.
