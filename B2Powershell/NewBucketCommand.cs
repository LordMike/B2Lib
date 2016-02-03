using System.Management.Automation;
using B2Lib;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.New, "B2Bucket")]
    public class NewBucketCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public B2BucketType Type { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            B2Client client = new B2Client();
            client.LoadState(State);

            B2Bucket res = client.CreateBucket(Name, Type);

            WriteObject(res);
        }
    }
}