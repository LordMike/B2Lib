using System.Management.Automation;
using B2Lib.Client;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Remove, "B2File")]
    public class RemoveFileCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public B2File[] Files { get; set; }

        protected override void ProcessRecordInternal()
        {
            foreach (B2File file in Files)
            {
                bool result = file.Delete();
                WriteObject(result);
            }
        }
    }
}