using System.Management.Automation;

namespace B2Powershell
{
    [Cmdlet("Export", "B2State")]
    public class ExportStateCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true)]
        public string File { get; set; }

        protected override void ProcessRecordInternal()
        {
            Client.SaveState(File);
        }
    }
}