using System.Collections.Generic;
using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Buckets")]
    public class GetBucketsCommand : B2CommandWithSaveState
    {
        protected override void ProcessRecordInternal()
        {
            List<B2Bucket> res = Client.ListBuckets();

            WriteObject(res, true);
        }
    }
}