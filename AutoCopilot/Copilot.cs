using System.Text.Json;

namespace AutoCopilot
{
    public interface ICopilot
    {
        IInstructionGroup Instructions { get; }

        void SetInstructions(IInstructionGroup instructions);
        string SerializeInstructions();
    }

    public class Copilot : ICopilot
    {
        public IInstructionGroup Instructions { get; private set; }

        public Copilot()
        {
            Instructions = new InstructionGroup("Root");
        }

        public void SetInstructions(IInstructionGroup instructions)
        {
            Instructions = instructions;
        }

        public string SerializeInstructions()
        {
            return JsonSerializer.Serialize(Instructions);
        }
    }
}