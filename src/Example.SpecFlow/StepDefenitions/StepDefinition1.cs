using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Example.SpecFlow.StepDefenitions
{
    [Binding]
    public sealed class StepDefinition1
    {
        // For additional details on SpecFlow step definitions see http://go.specflow.org/doc-stepdef

        [Given("I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredSomethingIntoTheCalculator(int number)
        {
            
        }

        [When("I press add")]
        public void WhenIPressAdd()
        {
            
        }

        [Then("the result should be (.*) on the screen")]
        public void ThenTheResultShouldBe(int result)
        {
            
        }
    }
}
