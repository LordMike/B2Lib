using System.Management.Automation;
using B2Lib.Client;

namespace B2Powershell
{
    internal static class Constants
    {
        public const string PrivateStateVariable = "_privateB2StateVariable";

        public static B2SaveState GetState(this PSCmdlet cmdlet)
        {
            return cmdlet.SessionState.PSVariable.GetValue(PrivateStateVariable, null) as B2SaveState;
        }

        public static void SaveState(this PSCmdlet cmdlet, B2SaveState state)
        {
            cmdlet.SessionState.PSVariable.Set(PrivateStateVariable, state);
        }
    }
}