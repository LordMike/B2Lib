using System.Management.Automation;
using B2Lib;

namespace B2Powershell
{
    [Cmdlet("Import", "B2State")]
    public class ImportStateCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string File { get; set; }

        protected override void ProcessRecord()
        {
            B2Client client = new B2Client();
            client.LoadState(File);

            WriteObject(client.SaveState());
        }
    }
}