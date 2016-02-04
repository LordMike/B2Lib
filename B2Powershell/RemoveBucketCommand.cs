using System.Collections.Generic;
using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Remove, "B2Bucket")]
    public class RemoveBucketCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "by_name", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public string[] BucketNames { get; set; }

        [Parameter(ParameterSetName = "by_bucket", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public B2Bucket[] Buckets { get; set; }

        protected override void ProcessRecordInternal()
        {
            if (ParameterSetName == "by_name")
            {
                foreach (string bucketName in BucketNames)
                {
                    B2Bucket bucket = Client.GetBucketByName(bucketName);
                    B2Bucket res = Client.DeleteBucket(bucket);

                    WriteObject(res);
                }
            }
            else if (ParameterSetName == "by_bucket")
            {
                foreach (B2Bucket bucket in Buckets)
                {
                    B2Bucket res = Client.DeleteBucket(bucket);

                    WriteObject(res);
                }
            }
            else
            {
                throw new PSArgumentException("Invalid set of values provided");
            }
        }
    }
}