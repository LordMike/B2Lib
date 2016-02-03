using System.Management.Automation;
using B2Lib;
using B2Lib.Enums;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Set, "B2Bucket")]
    public class SetBucketCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        [Parameter(Mandatory = true)]
        public B2BucketType NewType { get; set; }

        [Parameter(ParameterSetName = "by_name", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public string[] BucketNames { get; set; }

        [Parameter(ParameterSetName = "by_bucket", ValueFromPipeline = true, Mandatory = true, Position = 1)]
        public B2Bucket[] Buckets { get; set; }

        private B2Client _client;

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            _client = new B2Client();
            _client.LoadState(State);
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (BucketNames != null)
            {
                foreach (string bucketName in BucketNames)
                {
                    B2Bucket bucket = _client.GetBucketByName(bucketName);
                    B2Bucket res = _client.UpdateBucket(bucket, NewType);

                    WriteObject(res);
                }
            }

            if (Buckets != null)
            {
                foreach (B2Bucket bucket in Buckets)
                {
                    B2Bucket res = _client.UpdateBucket(bucket, NewType);

                    WriteObject(res);
                }
            }
        }
    }
}