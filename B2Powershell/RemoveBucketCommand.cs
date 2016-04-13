using System.Management.Automation;
using B2Lib.Client;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Remove, "B2Bucket")]
    public class RemoveBucketCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "by_name", ValueFromPipeline = true, Mandatory = true, Position = 0)]
        public string[] BucketNames { get; set; }

        [Parameter(ParameterSetName = "by_bucket", ValueFromPipeline = true, Mandatory = true, Position = 0)]
        public B2Bucket[] Buckets { get; set; }

        protected override void ProcessRecordInternal()
        {
            if (ParameterSetName == "by_name")
            {
                foreach (string bucketName in BucketNames)
                {
                    B2Bucket bucket = Client.GetBucketByName(bucketName);
                    bool res = bucket.Delete();

                    if (res)
                        WriteObject(bucket);
                }
            }
            else if (ParameterSetName == "by_bucket")
            {
                foreach (B2Bucket bucket in Buckets)
                {
                    bool res = bucket.Delete();

                    if (res)
                        WriteObject(bucket);
                }
            }
            else
            {
                throw new PSArgumentException("Invalid set of values provided");
            }
        }
    }
}