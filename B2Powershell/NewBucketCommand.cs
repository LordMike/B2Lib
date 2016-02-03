using System.Management.Automation;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.New, "B2Bucket")]
    public class NewBucketCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public B2BucketType Type { get; set; }

        protected override void ProcessRecordInternal()
        {
            B2Bucket res = Client.CreateBucket(Name, Type);

            WriteObject(res);
        }
    }
}