The guide goes throught steps required to setup Specflow-based test automation project in order to work with Report Portal. To keep this guide focus on Report Portal integration with Specflow, info needed to setup adjacent tools is rather provided by references to a dedicated resources than reviewed directly with steps needed.

# Setting up Report Portal

- install Report Portal

Follow steps here to install the service on Docker: https://reportportal.io/download.
Allocate Docker with resources enough to satisfy Report Portal needs: set RAM >= 5Gb; make drive C available for containers.

- check installation

Once the service is up, refer to the 3rd step of the following page for login instructions: https://reportportal.io/download.

# Test automation project configuration

## Setting up Specflow

The section is mainly based on official setup guide for Specflow. Code under test was slightly modified to make more space for several test scenarios to take place, in order to demonstrate different aspects of Report Portal integration with Specflow framework. The official guide is located at: https://specflow.org/getting-started.

Follow the official guide to create a basic project with the test framework applied, finishing after "Adding a SpecFlow Profile" section done. Once steps completed the project should fit these conditions:

1. The following packages are installed:
- Microsoft.NETCore.App (pre-installed for .NET Core apps)
- Microsoft.NET.Test.Sdk (pre-installed for test projectqs)
- MSTest.TestFramework (pre-installed for MSTest Test Project)
- SpecFlow
- SpecFlow.Tools.MsBuild.Generation
- SpecRun.SpecFlow

Note each test project created with VS wizard has a test runner pre-installed (MSTest.TestAdapter in the case of MSTest project). Uninstall it since Specflow provided one will be used instead in the guide.

2. Specflow+ test runner config file added

Make sure the file is named as "Default.srprofile", otherwise the runner may fail to pick up the file. In order to take into account changes made on the file, set its property `Copy to Output Directory` to `Copy if newer` .

Modify/add the following values in the `Execution` section of the file to override defaults for the test runner:
- set `stopAfterFailures` to `0`

As result all scenarios are executed, instead skipping them on several preceding scenario failures (by default)
- set `retryCount` to `0`

Disables failed tests rerun

To learn more about Specflow+ runner configuration visit: https://specflow.org/plus/documentation/SpecFlowPlus-Runner-Profiles .

## Adding test scenarios

Imagine we have got a buggy calculator but we don't know it yet, so we are going to test it. 
Below are our scenarios, add them into the project being configured.

*screen.feature*

```Gherkin
Feature: Calculator screen
    As a happy owner of a new calculator
    I want to make sure its screen works
    So the digits I type are mirorred on the screen immediately

Scenario: Typing on device is on
    Given I turn on a calculator
    When I have entered "94320324" into the calculator
    Then The result should be "94320324" on the screen

Scenario: Typing on device is off
    Given I turn off a calculator
    When I have entered "94320324" into the calculator
    Then The screen does not display anything
```

*operations.feature*
```Gherkin
Feature: Calculator operations
    As a happy owner of a new calculator
    I want to be sure "+-" buttons are not fake
    And they represents sum/substract operations as it is usually customary

Background:
    Given I turn on a calculator

@operation @sum
Scenario Outline: Add two numbers
    When I have entered "<expr>" into the calculator
    And I have entered "=" into the calculator
    Then The result should be "<result>" on the screen

    Examples:
        | expr  | result |
        | 2+2   | 4      |
        | 50+70 | 120    |

@operation @sub
Scenario: Substract two numbers
    When I have entered "9-2=" into the calculator
    Then The result should be "7" on the screen
```

And our dummy calculator implementation: *Calculator.cs*
```C#
using System;

public class Calculator
{
    public Calculator()
    {
        isTurnedOn = false;
    }
    public string screenContent
    {
        get;
        private set;
    }

    private bool _state;
    public bool isTurnedOn
    {
        get => _state;
        private set
        {
            _state = value;
            screenContent = value ? "0" : "";
        }
    }

    public void IO()
    {
        isTurnedOn = !isTurnedOn;
    }
    private void compute()
    {
        screenContent = "4";
    }
    private void executeOperation(char opSign)
    {
        switch (opSign)
        {
            default:
                throw new NotSupportedException();
            case '+':
                break;
            case '=':
                compute();
                break;
        }
    }

    private void processKey(char key)
    {
        if (char.IsDigit(key))
        {
            if (screenContent == "0")
                screenContent = "";
            screenContent += key;
        }
        else
            executeOperation(key);
    }

    public void press(string keysPressed)
    {
        if (!isTurnedOn)
            return;
        try
        {
            foreach (var key in keysPressed)
            {
                processKey(key);
            }
        }
        catch (NotSupportedException)
        {
            screenContent = "unsupported operation. Restart is required";
        }
    }
}
```

Gherkin scenarios implementation: *stepsImpl.cs*
```C#
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

[Binding]
public class CalculatorSpecSteps
{
    private Calculator calculator = new Calculator();

    [Given(@"I turn (on|off) a calculator")]
    public void switchCalculatorPower(string neededState)
    {
        bool expectedState = neededState == "on";
        if (calculator.isTurnedOn != expectedState)
            calculator.IO();
    }
    [When(@"I have entered ""(\S*)"" into the calculator")]
    public void calculatorInput(string number)
    {
        calculator.press(number);
    }

    [Then(@"The result should be ""(\S*)"" on the screen")]
    public void screenContentCheck(string expectedResult)
    {
        Assert.AreEqual(expectedResult, calculator.screenContent);
    }
    [Then(@"The screen does not display anything")]
    public void screenIsClear()
    {
        Assert.AreEqual("", calculator.screenContent);
    }

}
```

Now tests are ready to be executed. Use `Test Explorer` IDE feature to `Run All` of them.
![Test run result](https://github.com/reportportal/reportportal/blob/master/screenshots/testRun.png)

 In the case of issues, make sure each .feature file has property `Custom Tool` set to an empty field. More info about it here: https://specflow.org/documentation/Generate-Tests-from-MsBuild

## Preparing Report Portal

In order to report test execution results to Report Portal, test automation project needs to provide the following info:
- Report Portal instance address the results will be reported to
- user identity the results will be uploaded by
- project name

Project is an organizational unit of test launches in Report Portal. Each test launch has a project assigned.
- test launch properties

---

Login to a service instance and perform the next steps:

1. Go through the projects list and pick the one where test results are expected to come into

If there no such project, go to `Administrate->Add project` and create it

![Settings menu](https://github.com/reportportal/reportportal/blob/master/screenshots/rpSettingsMenu.png)

2. Take uuid from `Profile` screen

The value will be used to authorize test automation project to the service

## Setting up Report Portal integration

Follow these steps to connect the service with the test framework:

1. Install a package `ReportPortal.SpecFlow`

2. Create and put Report Portal config file at a root directory, naming it as "ReportPortal.config.json". Config file template is located at: https://github.com/reportportal/agent-net-specflow#configuration

3. Set the file property `Copy to Output Directory` to `Copy if newer`

4. Modificate the config file according the Report Portal instance in use

Change `URL`, `uuid`, `project` to actual values. Use values obtained during the previous section. Notice that `uuid` may change between service restarts, so if test results will not be coming to the service, make sure the specified `uuid` is still actual.

For a simple local hosted experiments use HTTP for connection when specifying `URL` value. To learn about SSL setup visit: https://reportportal.io/docs/Setup-SSL .

Here is the config used for this guide:

```json
{
  "$schema": "https://raw.githubusercontent.com/reportportal/agent-net-specflow/master/ReportPortal.SpecFlowPlugin/ReportPortal.config.schema",
  "enabled": true,
  "server": {
    "url": "http://localhost:8080/api/v1/",
    "project": "demo_project",
    "authentication": {
      "uuid": "f4d24900-4a38-477a-996a-1d992d9017f3"
    }
  },
  "launch": {
    "name": "SpecFlow Demo Launch",
    "description": "demo tests for a calculator",
    "debugMode": true,
    "tags": [ "calculator", "fake" ]
  }
}
```

# Test the integration

Configuration is complete, so it is the time to make sure integration works.
Run all tests again and go to Report Portal.

1. Pick the project you pointed to report test results to. In this case it is "demo_project"
![Report Portal project selection](https://github.com/reportportal/reportportal/blob/master/screenshots/rpProjectSelection.png)

2. Open launches/debug tab to see test results

There 2 screens where test runs info can be displayed on the platform: debug and launches. By default test results appear in `launches` screen, but if test execution was done with Report Portal config property `debug` set to `true`, the results will appear on the debug screen instead. Read here about the screens: https://reportportal.io/docs/View-launches .

Since `debug` is `true` for the current project, test runs appear on `debug` screen.
![Report Portal. Test runs screen (debug)](https://github.com/reportportal/reportportal/blob/master/screenshots/rpTestRuns.png)
Pay attention how the test run's info reflects configuration set in the test automation project (ReportPortal.config.json):
- test run title is equal to `launch.name`
- tags are those specified in `launch.tags`
- user specified below title is the one used to authenticate test project by. His key is used in `server.authentication.uuid`

3. Look inside a test run

![Report Portal. Scenarios within a test run](https://github.com/reportportal/reportportal/blob/master/screenshots/rpTestRunScenarios.png)

Pay attention each clause represents a feature with its title and description displayed.

Looking inside a feature, "Calculator screen" on example, one can see list of scenarios executed for the feature:
![Report Portal. Scenarios for 'Calculator screen' feature](screenshots/rpFeatureScenariosList_calcScreen.png)

And the same for "Calculator operations" feature:
![Report Portal. Scenarios for 'Calculator operations' feature](https://github.com/reportportal/reportportal/blob/master/screenshots/rpFeatureScenariosList_calcOperations.png)


Going one step deeper test steps are displayed. Here steps for "Substract two numbers" scenario:
![Report Portal. Steps of "Substract two numbers" scenario](https://github.com/reportportal/reportportal/blob/master/screenshots/rpScenarioSteps_2numbSubstraction.png)

# Next steps

The guide finishes at this point. To learn more about the tools used throughout the guide visit the corresponding resources:

- Report Portal official site: https://reportportal.io
- Specflow official site: https://specflow.org
- Docker official site: https://www.docker.com