Feature: Abnormal working of the light controller - getting time gives an exception

  Abnormal working of the Light Controller. We have 3 scenarios:
  Scenario 1: The light is on and number of failures less than maximum: Do Nothing
  Scenario 2: Number of failures reaches maximum: Enter safe mode
  Scenario 3: Recovery from safe mode: Return to normal operation

  Scenario: 1 - light stays on when failures are below maximum
    Given the light is on
    And number of failures is less than maximum
    When getting the time fails
    Then do nothing - light stays on

  Scenario: 2 - entering safe mode on maximum failures
    Given the light is on
    And number of failures is maximum failures minus one
    When getting the time fails
    Then turn light off
    And set controller in safe mode

  Scenario: 3 - recovery from safe mode
    Given controller in safe mode
    When the time service recovers
    Then set controller in normal mode
    And light state updates according to time
