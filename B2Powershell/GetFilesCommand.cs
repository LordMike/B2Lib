using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;
using B2Lib.SyncExtensions;
using B2Lib.Utilities;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Files")]
    public class GetFilesCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        [Parameter(ParameterSetName = "by_name", Mandatory = true, Position = 1)]
        public string BucketName { get; set; }

        [Parameter(ParameterSetName = "by_bucket", Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public B2Bucket Bucket { get; set; }

        [Parameter]
        public int? Count { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            B2Client client = new B2Client();
            client.LoadState(State);

            B2Bucket bucket;
            if (!string.IsNullOrEmpty(BucketName))
            {
                bucket = client.GetBucketByName(BucketName);
            }
            else
            {
                bucket = Bucket;
            }

            B2FilesIterator files = client.ListFiles(bucket);
            IEnumerable<B2FileBase> iterator = files;

            if (Count.HasValue)
            {
                if (Count.Value < files.PageSize)
                    files.PageSize = Count.Value;

                iterator = files.Take(Count.Value);
            }

            WriteObject(iterator, true);
        }
    }
}