using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ReportPortal.Client.Models;
using ReportPortal.Client.Requests;
using ReportPortal.SpecFlowPlugin;
using ReportPortal.SpecFlowPlugin.EventArguments;
using TechTalk.SpecFlow;

namespace Example.SpecFlow.Hooks
{
    [Binding]
    public sealed class Hooks1
    {
        [BeforeTestRun(Order = 0)]
        public static void BeforeTestRunPart()
        {
            ReportPortalAddin.BeforeFeatureStarted += ReportPortalAddin_BeforeFeatureStarted;

            ReportPortalAddin.BeforeScenarioStarted += ReportPortalAddin_BeforeScenarioStarted;
            ReportPortalAddin.BeforeScenarioFinished += ReportPortalAddin_BeforeScenarioFinished;
        }

        private static void ReportPortalAddin_BeforeFeatureStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding feature tag on runtime
            e.TestItem.Tags.Add("runtime_feature_tag");
        }

        private static void ReportPortalAddin_BeforeScenarioFinished(object sender, TestItemFinishedEventArgs e)
        {
            // Attaching screenshot at the end of the scenario
            using (var bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
                using (var g = Graphics.FromImage(bmpScreenCapture))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                     Screen.PrimaryScreen.Bounds.Y,
                                     0, 0,
                                     bmpScreenCapture.Size,
                                     CopyPixelOperation.SourceCopy);
                }

                using (var stream = new MemoryStream())
                {
                    bmpScreenCapture.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    var attach = new Attach("attach", "image/png", stream.ToArray());

                    e.Service.AddLogItem(new AddLogItemRequest
                    {
                        TestItemId = ReportPortalAddin.CurrentScenarioId,
                        Level = LogLevel.None,
                        Time = DateTime.UtcNow,
                        Text = "Screenshot " + DateTime.Now,
                        Attach = attach
                    });
                }
            }
        }

        private static void ReportPortalAddin_BeforeScenarioStarted(object sender, TestItemStartedEventArgs e)
        {
            // Adding scenario tag on runtime
            e.TestItem.Tags.Add("runtime_scenario_tag");

            var nunitTestName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            const string pattern = @"^[^\(]+\((.*)\)$";

            // Adding test case parameters to scenario name
            if (Regex.IsMatch(nunitTestName, pattern))
            {
                var nunitTestParameters = Regex.Match(nunitTestName, pattern).Groups.Cast<Group>().ElementAt(1).Value;
                e.TestItem.Name += ": " + Regex.Replace(nunitTestParameters, @"\,(null|System\.String\[\])$", "");
            }

            // Workaround for 1024 characters restriction on the test item description
            if (string.IsNullOrEmpty(e.TestItem.Description) == false && e.TestItem.Description.Length > 1024)
            {
                e.TestItem.Description = e.TestItem.Description.Substring(0, 1024);
            }

            // Workaround for 256 characters restriction on the test item name
            if (string.IsNullOrEmpty(e.TestItem.Name) == false && e.TestItem.Name.Length > 256)
            {
                e.TestItem.Name = e.TestItem.Name.Substring(0, 256);
            }
        }
    }
}
