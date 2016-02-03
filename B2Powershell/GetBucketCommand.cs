using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Bucket")]
    public class GetBucketCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        [Parameter(ParameterSetName = "by_name", Mandatory = true, Position = 1)]
        public string Name { get; set; }

        [Parameter(ParameterSetName = "by_id", Mandatory = true, Position = 1)]
        public string Id { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            B2Client client = new B2Client();
            client.LoadState(State);

            if (!string.IsNullOrEmpty(Name))
            {
                B2Bucket res = client.GetBucketByName(Name);

                WriteObject(res);
            }
            else if (!string.IsNullOrEmpty(Id))
            {
                B2Bucket res = client.GetBucketById(Id);

                WriteObject(res);
            }
        }
    }
}