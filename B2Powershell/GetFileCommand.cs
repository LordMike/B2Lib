using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;
using B2Lib.Utilities;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2File")]
    public class GetFileCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "by_name", Mandatory = true, Position = 1)]
        public string BucketName { get; set; }

        [Parameter(ParameterSetName = "by_bucket", Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public B2Bucket Bucket { get; set; }

        [Parameter]
        public int? Count { get; set; }

        protected override void ProcessRecordInternal()
        {
            B2Bucket bucket;

            switch (ParameterSetName)
            {
                case "by_name":
                    bucket = Client.GetBucketByName(BucketName);
                    break;
                case "by_bucket":
                    bucket = Bucket;
                    break;
                default:
                    throw new PSArgumentException("Invalid set of values provided");
            }
            
            B2FilesIterator files = Client.ListFiles(bucket);
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