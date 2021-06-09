﻿using ReportPortal.Client.Abstractions.Models;
using ReportPortal.Client.Abstractions.Responses;
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

            ReportPortalAddin.AfterFeatureFinished += ReportPortalAddin_AfterFeatureFinished;
        }

        private static void ReportPortalAddin_BeforeRunStarted(object sender, RunStartedEventArgs e)
        {
            e.StartLaunchRequest.Description = $"OS: {Environment.OSVersion.VersionString}";
        }

        private static void ReportPortalAddin_BeforeScenarioFinished(object sender, TestItemFinishedEventArgs e)
        {
            if (e.ScenarioContext.TestError != null && e.ScenarioContext.ScenarioInfo.Title == "System Error")
            {
                e.FinishTestItemRequest.Issue = new Issue
                {
                    Type = WellKnownIssueType.SystemIssue,
                    Comment = "my custom system error comment"
                };
            }
            // put scenario failure reason into RP defect comment
            else if (e.ScenarioContext.TestError != null)
            {
                e.FinishTestItemRequest.Issue = new Issue
                {
                    Type = WellKnownIssueType.ToInvestigate,
                    Comment = e.ScenarioContext.TestError.Message
                };
            }
        }

        private static void ReportPortalAddin_BeforeFeatureStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding feature tag on runtime
            e.StartTestItemRequest.Attributes.Add(new ItemAttribute { Value = "runtime_feature_tag" });
        }

        private static void ReportPortalAddin_BeforeScenarioStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding scenario tag on runtime
            e.StartTestItemRequest.Attributes.Add(new ItemAttribute { Value = "runtime_scenario_tag" });
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext context)
        {
            if (context.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
            {
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\cat.png";
                Log.Debug("This cat came from AfterScenario hook {rp#file#" + filePath + "}");
            }
        }

        private static void ReportPortalAddin_AfterFeatureFinished(object sender, TestItemFinishedEventArgs e)
        {
#if NETCOREAPP
            // Workaround how to avoid issue https://github.com/techtalk/SpecFlow/issues/1348 (launch doesn't finish on .netcore tests)
            e.TestReporter.FinishTask.Wait();
#endif
        }
    }
}
