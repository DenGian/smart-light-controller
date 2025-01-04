Feature: Normal working of the Light Controller

  Normal working of the Light Controller. We are testing the following scenarios:
  Scenario 1a: The light is off and time is outside active hours: Do Nothing
  Scenario 1b: The light is on and time is within active hours: Do Nothing
  Scenario 2: Time enters active hours: Light On
  Scenario 3: Time exits active hours: Light Off
  Scenario 4: Time spans overnight: Light On
  Scenario 5: Time at boundaries: Correct state changes

  Scenario: 1a - the light is off and time is outside active hours - do nothing
    Given the light is off
    When the time is outside active hours
    Then do nothing - light is off

  Scenario: 1b - the light is on and time is within active hours - do nothing
    Given the light is on
    When the time is within active hours
    Then do nothing - light is on

  Scenario: 2 - time enters active hours - turn light on
    When the time enters active hours
    Then turn light on

  Scenario: 3 - time exits active hours - turn light off
    Given the light is on
    When the time exits active hours
    Then turn light off

  Scenario: 4 - time spans overnight - light stays on
    Given the time is after start time
    When the time passes midnight
    Then light stays on

  Scenario: 5a - time equals start time - turn light on
    When the time equals start time
    Then turn light on

  Scenario: 5b - time equals end time - turn light off
    Given the light is on
    When the time equals end time
    Then turn light off
