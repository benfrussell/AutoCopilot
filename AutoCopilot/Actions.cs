using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCopilot
{
    public class Action
    {
        public enum ActionType
        {
            Log,
            SetMavlinkServo,
            SetRPIPWM,
            SetRPIGPIO,
            StartTimer,
            StopTimer,
            ResetTimer,
            InitiateReturnHome,
            FinishInstruction,
            SetFlag,
            SetCopilotStage
        }

        public ActionType Type { get; private set; }
        public List<object> Parameters { get; private set; }

        private Action()
        {
            Parameters = new List<object>();
        }

        public static Action Log(string text)
        {
            return new Action() { Type = ActionType.Log, Parameters = { text } };
        }
    }
}
