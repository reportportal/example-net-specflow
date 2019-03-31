using ReportPortal.Shared;
using ReportPortal.SpecFlowPlugin;
using ReportPortal.SpecFlowPlugin.EventArguments;
using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Example.SpecFlow.Hooks
{
    [Binding]
    public sealed class Hooks1
    {
        // BeforeTestRun hook order should be set to the value that is lower than -20000
        // if you plan to use ReportPortalAddin.BeforeRunStarted event.
        [BeforeTestRun(Order = -30000)]
        public static void BeforeTestRunPart()
        {
            ReportPortalAddin.BeforeRunStarted += ReportPortalAddin_BeforeRunStarted;
            ReportPortalAddin.BeforeFeatureStarted += ReportPortalAddin_BeforeFeatureStarted;
            ReportPortalAddin.BeforeScenarioStarted += ReportPortalAddin_BeforeScenarioStarted;
            ReportPortalAddin.BeforeScenarioFinished += ReportPortalAddin_BeforeScenarioFinished;
        }

        private static void ReportPortalAddin_BeforeRunStarted(object sender, RunStartedEventArgs e)
        {
            e.StartLaunchRequest.Description = $"OS: {Environment.OSVersion.VersionString}";
        }

        private static void ReportPortalAddin_BeforeScenarioFinished(object sender, TestItemFinishedEventArgs e)
        {
            if (e.ScenarioContext.TestError != null && e.ScenarioContext.ScenarioInfo.Title == "System Error")
            {
                e.FinishTestItemRequest.Issue = new ReportPortal.Client.Models.Issue
                {
                    Type = ReportPortal.Client.Models.WellKnownIssueType.SystemIssue,
                    Comment = "my custom system error comment"
                };
            }
        }

        private static void ReportPortalAddin_BeforeFeatureStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding feature tag on runtime
            e.StartTestItemRequest.Tags.Add("runtime_feature_tag");
        }

        private static void ReportPortalAddin_BeforeScenarioStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding scenario tag on runtime
            e.StartTestItemRequest.Tags.Add("runtime_scenario_tag");
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext context)
        {
            if (context.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
            {
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cat.png";
                Bridge.LogMessage(ReportPortal.Client.Models.LogLevel.Debug, "This cat came from AfterScenario hook {rp#file#" + filePath + "}");
            }
        }
    }
}
