# SmartLight System Design Documentation

## Table of Contents
* [Overview](#overview)
* [System Design Process](#system-design-process)
* [Architecture](#architecture)
* [Test Design & Implementation](#test-design--implementation)
* [Implementation Details](#implementation-details)
* [Future Enhancements](#future-enhancements)
* [Development Approach](#development-approach)
* [Conclusion](#conclusion)

## Overview
The SmartLight System is designed to provide automated light control based on time of day. It emphasizes reliability, safety, and maintainability through robust error handling and clear separation of concerns.

### Key Features
* Time-based light control
* Automatic overnight handling
* Fault tolerance with safe mode
* Configurable time windows
* API-based time synchronization

## System Design Process

### Initial Analysis
The system was designed through a systematic approach:

1. **Requirements Analysis**
* Need for time-based control
* Requirement for fault tolerance
* API integration necessity
* Testing requirements

2. **Design Decisions**
* Separation into three main components
* Use of interfaces for dependency injection
* Implementation of safe mode
* Error threshold mechanism

3. **Pattern Selection**
* Repository pattern for time retrieval
* Strategy pattern for light control
* Factory pattern potential for future extensions

## Architecture

### Core Components

1. LightController (Decision Module)
* **Purpose**: Central decision-making component
* **Responsibilities**:
    * Time-based light state management
    * Failure handling and recovery
    * Safe mode management
* **Key Methods**:
    * `Work()`: Main control loop
    * Property accessors for configuration

2. ITimeProvider (Information Provider)
* **Purpose**: Time information abstraction
* **Responsibilities**:
    * Time retrieval from external sources
    * URL configuration
* **Implementations**:
    * `TimeProviderReal`: Production implementation
    * `TimeProviderStub`: Testing implementation

3. ILightElement (Action Module)
* **Purpose**: Light control abstraction
* **Responsibilities**:
    * Light state management
    * Hardware interaction abstraction
* **Implementations**:
    * `LightElementStub`: Testing implementation
    * Future real hardware implementations

### Class Diagram

```mermaid
classDiagram
    class ITimeProvider {
        +string Url
        +DateTime GetCurrentTime()
    }

    class ILightElement {
        +bool IsEnabled
        +void Enable()
        +void Disable()
    }

    class LightController {
        -ITimeProvider _timeProvider
        -ILightElement _lightElement
        -int _failures
        -TimeSpan _startTime
        -TimeSpan _endTime  
        -int _maxFailures
        +TimeSpan StartTime
        +TimeSpan EndTime
        +int MaxFailures
        +bool InSafeMode
        +LightController(ITimeProvider, ILightElement)
        +void Work()
    }

    class TimeProviderReal {
        -string _url
        +string Url
        +DateTime GetCurrentTime()
    }

    class TimeProvider {
        +string Url
        +DateTime GetCurrentTime()
    }

    class LightElement {
        +bool IsEnabled
        +void Enable()
        +void Disable()
    }

    class LightElementStub {
        -bool _isEnabled
        +bool IsEnabled
        +void Enable()
        +void Disable()
    }

    class TimeResponse {
        +string datetime
        +int day_of_week
        +int day_of_year
        +int week_number
        +string timezone
    }

    LightController --> ITimeProvider
    LightController --> ILightElement
    TimeProviderReal ..|> ITimeProvider
    TimeProvider ..|> ITimeProvider
    LightElement ..|> ILightElement 
    LightElementStub ..|> ILightElement
    TimeProviderReal --> TimeResponse : uses
```

## Test Design & Implementation

### ZOMBIES Testing Strategy

#### Zero Tests

##### 1. Work_WhenStartTimeEqualsEndTime_LightStaysDisabled
* **Purpose**: Verifies system behavior with empty time window
* **Scenario**: Start time equals end time
* **Expected**: Light remains disabled
* **Importance**: Ensures system safety in edge cases

#### One Tests

##### 1. Work_WhenTimeIsDuringActiveHours_EnablesLight
* **Purpose**: Verifies basic active period functionality
* **Scenario**: Single time check during active hours
* **Expected**: Light enables correctly
* **Implementation**: Uses mock time provider

##### 2. Work_WhenTimeIsOutsideActiveHours_DisablesLight
* **Purpose**: Verifies basic inactive period functionality
* **Scenario**: Single time check during inactive hours
* **Expected**: Light disables correctly
* **Implementation**: Uses mock time provider

#### Many Tests

##### 1. Work_WhenTimeSpansOvernight_EnablesLightAfterStartTime
* **Purpose**: Verifies overnight functionality
* **Scenario**: Time after start time but before midnight
* **Expected**: Light enables correctly
* **Edge Case**: Handles day boundary

##### 2. Work_WhenTimeSpansOvernight_EnablesLightBeforeEndTime
* **Purpose**: Verifies early morning functionality
* **Scenario**: Time after midnight but before end time
* **Expected**: Light remains enabled
* **Edge Case**: Handles day boundary

#### Boundary Tests

##### 1. Work_WhenTimeEqualsStartTime_EnablesLight
* **Purpose**: Verifies exact start time behavior
* **Scenario**: Time exactly matches start time
* **Expected**: Light enables precisely at start

##### 2. Work_WhenTimeEqualsEndTime_DisablesLight
* **Purpose**: Verifies exact end time behavior
* **Scenario**: Time exactly matches end time
* **Expected**: Light disables precisely at end

##### 3. Work_WhenOneMinuteBeforeEndTime_StillEnabled
* **Purpose**: Verifies boundary precision
* **Scenario**: One minute before end time
* **Expected**: Light remains enabled
* **Importance**: Ensures no premature disabling

##### 4. Work_WhenTimeIsExactlyMidnight_HandlesOvernightPeriodCorrectly
* **Purpose**: Verifies midnight boundary
* **Scenario**: Time is exactly 00:00
* **Expected**: Maintains correct state across days

#### Interface Tests
* Mock implementations verify interface contracts
* Stub implementations provide controlled test scenarios
* Real implementations tested in integration tests

#### Exception Tests

##### 1. Work_WhenTimeFailsAndNotInSafeMode_DoNothing
* **Purpose**: Verifies initial failure handling
* **Scenario**: Single time service failure
* **Expected**: Maintains current state
* **Implementation**: Uses exception-throwing mock

##### 2. Work_WhenTimeFailsAndMaxFailuresReached_EntersSafeMode
* **Purpose**: Verifies safe mode transition
* **Scenario**: Multiple consecutive failures
* **Expected**: Enters safe mode and disables light
* **Implementation**: Uses exception-throwing mock

##### 3. Work_WhenInSafeModeAndTimeSucceeds_ResetsSafeMode
* **Purpose**: Verifies recovery behavior
* **Scenario**: Success after safe mode
* **Expected**: Exits safe mode and resumes normal operation
* **Implementation**: Uses state-changing mock

##### 4. Work_AfterFailure_ResetsFailureCount_OnSuccess
* **Purpose**: Verifies failure count reset
* **Scenario**: Success after partial failures
* **Expected**: Resets failure count
* **Implementation**: Uses state-changing mock

#### Simple Scenarios
* Basic time-based control verification
* State transition verification
* Normal operation flow testing

### Integration Testing
The integration tests verify:
* Real API interaction
* Time format handling
* System recovery capabilities
* Long-running stability

### Test Coverage
* Unit tests: Core logic and edge cases
* Integration tests: External dependencies
* Acceptance tests: User scenarios

## Implementation Details

### Error Handling Strategy

#### Gradual Degradation
* Counts failures before safe mode
* Maintains operation during intermittent failures

#### Recovery Mechanism
* Automatic safe mode exit on success
* Failure count reset on success

#### Safety Measures
* Light disabled in safe mode
* Conservative time window handling

### Performance Considerations

#### Time Complexity
* O(1) decision-making
* Minimal memory usage

#### Resource Usage
* Efficient time comparisons
* Minimal state storage

## Future Enhancements

### Potential Extensions

#### Additional Features
* Multiple time windows per day
* Gradual dimming
* Motion sensor integration
* Weather condition integration

#### Technical Improvements
* Caching layer for time service
* Configuration persistence
* Real-time monitoring
* Health metrics

### Scaling Considerations

#### Multiple Light Support
* Light group management
* Zone-based control
* Hierarchical configuration

#### Integration Capabilities
* Smart home systems
* Building automation
* Energy management systems

## Development Approach

The system was developed using Test-Driven Development (TDD):

### First Phase: Core structure and interfaces
* Defined basic interfaces for time and light control
* Implemented basic controller structure
* Created initial tests for basic functionality

### Second Phase: Basic functionality
* Implemented time-based control
* Added basic error handling
* Created tests for normal operation

### Third Phase: Error handling and edge cases
* Added safe mode functionality
* Implemented failure counting
* Created tests for error scenarios

### Fourth Phase: Refinement
* Added boundary condition handling
* Improved overnight period handling
* Added tests for edge cases

## Conclusion
The SmartLight System demonstrates robust design through:
* Clear separation of concerns
* Comprehensive testing strategy
* Effective error handling
* Future-proof architecture

The system is production-ready with:
* Reliable operation
* Maintainable codebase
* Extensible design
* Comprehensive documentation
