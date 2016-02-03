using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Remove, "B2Files")]
    public class RemoveFilesCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }
        
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public B2FileBase[] Files { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            B2Client client = new B2Client();
            client.LoadState(State);

            foreach (B2FileBase file in Files)
            {
                bool result = client.DeleteFile(file);
                WriteObject(result);
            }
        }
    }
}