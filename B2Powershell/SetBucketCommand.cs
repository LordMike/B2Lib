using System.Management.Automation;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Set, "B2Bucket")]
    public class SetBucketCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true)]
        public B2BucketType NewType { get; set; }

        [Parameter(ParameterSetName = "by_name", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public string[] BucketNames { get; set; }

        [Parameter(ParameterSetName = "by_bucket", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public B2Bucket[] Buckets { get; set; }
        
        protected override void ProcessRecordInternal()
        {
            if (BucketNames != null)
            {
                foreach (string bucketName in BucketNames)
                {
                    B2Bucket bucket = Client.GetBucketByName(bucketName);
                    B2Bucket res = Client.UpdateBucket(bucket, NewType);

                    WriteObject(res);
                }
            }

            if (Buckets != null)
            {
                foreach (B2Bucket bucket in Buckets)
                {
                    B2Bucket res = Client.UpdateBucket(bucket, NewType);

                    WriteObject(res);
                }
            }
        }
    }
}