Feature: Feature1
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario Outline: Add two numbers
	Given I have entered <First> into the calculator
	And I have entered <Second> into the calculator
	When I press add
	Then the result should be <Result> on the screen
Examples:
    | First | Second | Result |
    | 50    | 70     | 120    |
    | 20    | 50     | 70     |

@mytag @super_super_tag
Scenario: Add three numbers
	Given I have entered 3 into the calculator
	And I have entered 7 into the calculator
	And I have entered 8 into the calculator
	When I press add
	Then the result should be 18 on the screen