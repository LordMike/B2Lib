using System.Collections.Generic;
using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Buckets")]
    public class GetBucketsCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            B2Client client = new B2Client();
            client.LoadState(State);

            List<B2Bucket> res = client.ListBuckets();

            WriteObject(res, true);
        }
    }
}