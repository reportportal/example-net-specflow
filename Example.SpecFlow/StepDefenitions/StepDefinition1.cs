using ReportPortal.Shared;
using System;
using System.IO;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Example.SpecFlow.StepDefenitions
{
    [Binding]
    public sealed class StepDefinition1
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef
        [When(@"I upload ""(.*)"" into Report Portal")]
        public void WhenIUploadIntoReportPortal(string fileName)
        {
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + fileName;
            Log.Info("this is my cat {rp#file#" + filePath + "}");
        }


        [Given("I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredSomethingIntoTheCalculator(int number)
        {
            using (var scope = Log.BeginScope("Searching for calculator..."))
            {
                scope.Debug("Where is calculator?");
                Log.Info("Yeah, found it.");
                scope.Debug($"Typing '{number}'..");

                using (var scope2 = scope.BeginScope($"Searching '{number}' button.."))
                {
                    Log.Error("I lost my button :(");
                    scope2.Warn("lucky next time.");

                    scope2.Status = ReportPortal.Shared.Execution.Logging.LogScopeStatus.Skipped;
                }
            }
        }

        [When("I press add")]
        public void WhenIPressAdd()
        {

        }

        [Then("the result should be (.*) on the screen")]
        public void ThenTheResultShouldBe(int result)
        {
            if (result == 666)
            {
                throw new Exception("Daemon here.");
            }
        }

        [Then(@"I execute failed test")]
        public void ThenIExecuteFailedTest()
        {
            throw new Exception("This step raises an exception.");
        }

        [When(@"I make a note")]
        public void WhenIMakeANote(string multilineText)
        {

        }

        [Then(@"I should buy the following")]
        public void ThenIShouldBuyTheFollowing(Table table)
        {

        }

    }
}
