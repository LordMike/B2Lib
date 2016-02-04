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
            B2Bucket res;

            switch (ParameterSetName)
            {
                case "by_name":
                    res = Client.GetBucketByName(Name);
                    break;
                case "by_id":
                    res = Client.GetBucketById(Id);
                    break;
                default:
                    throw new PSArgumentException("Invalid set of values provided");
            }

            WriteObject(res);
        }
    }
}