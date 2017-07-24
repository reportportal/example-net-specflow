using ReportPortal.SpecFlowPlugin;
using ReportPortal.SpecFlowPlugin.EventArguments;
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
            ReportPortalAddin.BeforeFeatureStarted += ReportPortalAddin_BeforeFeatureStarted;
            ReportPortalAddin.BeforeScenarioStarted += ReportPortalAddin_BeforeScenarioStarted;
        }

        private static void ReportPortalAddin_BeforeFeatureStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding feature tag on runtime
            e.TestItem.Tags.Add("runtime_feature_tag");
        }

        private static void ReportPortalAddin_BeforeScenarioStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding scenario tag on runtime
            e.TestItem.Tags.Add("runtime_scenario_tag");
        }
    }
}
