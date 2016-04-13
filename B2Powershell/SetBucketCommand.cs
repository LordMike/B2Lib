using System.Management.Automation;
using B2Lib.Client;
using B2Lib.Enums;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Set, "B2Bucket")]
    public class SetBucketCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true)]
        public B2BucketType NewType { get; set; }

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
                    bool res = bucket.Update(NewType);

                    if (res)
                        WriteObject(bucket);
                }
            }
            else if (ParameterSetName == "by_bucket")
            {
                foreach (B2Bucket bucket in Buckets)
                {
                    bool res = bucket.Update(NewType);

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