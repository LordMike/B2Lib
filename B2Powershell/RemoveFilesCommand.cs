using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Remove, "B2Files")]
    public class RemoveFilesCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public B2FileBase[] Files { get; set; }

        protected override void ProcessRecordInternal()
        {
            foreach (B2FileBase file in Files)
            {
                bool result = Client.DeleteFile(file);
                WriteObject(result);
            }
        }
    }
}