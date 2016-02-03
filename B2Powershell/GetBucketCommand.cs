using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Bucket")]
    public class GetBucketCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "by_name", Mandatory = true, Position = 1)]
        public string Name { get; set; }

        [Parameter(ParameterSetName = "by_id", Mandatory = true, Position = 1)]
        public string Id { get; set; }

        protected override void ProcessRecordInternal()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                B2Bucket res = Client.GetBucketByName(Name);

                WriteObject(res);
            }
            else if (!string.IsNullOrEmpty(Id))
            {
                B2Bucket res = Client.GetBucketById(Id);

                WriteObject(res);
            }
        }
    }
}