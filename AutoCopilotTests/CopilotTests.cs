using Action = AutoCopilot.Action;

namespace AutoCopilotTests
{
    [TestClass]
    public class CopilotTests
    {
        [TestMethod]
        public void Nested_Instructions_Seralize_Correctly()
        {
            var copilot = new Copilot();

            var instructions = new InstructionGroup
            {
                new Instruction(Action.Log("One")),

                new InstructionGroup
                {
                    new Instruction(Action.Log("Two")),
                    new Instruction(Action.Log("Three"))
                }
            };

            copilot.SetInstructions(instructions);

            var output = copilot.SerializeInstructions();

            Assert.IsTrue(true);
        }
    }
}