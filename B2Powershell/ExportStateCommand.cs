using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;

namespace B2Powershell
{
    [Cmdlet("Export", "B2State")]
    public class ExportStateCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        [Parameter(Mandatory = true)]
        public string File { get; set; }

        protected override void ProcessRecord()
        {
            B2Client client = new B2Client();
            client.LoadState(State);

            client.SaveState(File);
        }
    }
}