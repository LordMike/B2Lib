using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Bucket")]
    public class GetBucketCommand : B2CommandWithSaveState
    {
        [Parameter]
        public string Name { get; set; }

        [Parameter]
        public string Id { get; set; }

        protected override void ProcessRecordInternal()
        {
            IEnumerable<B2Bucket> toShow = Client.ListBuckets();
            if (!string.IsNullOrEmpty(Name))
            {
                toShow = toShow.Where(s => s.BucketName.Equals(Name.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(Id))
            {
                toShow = toShow.Where(s => s.BucketName.Equals(Id.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }

            WriteObject(toShow, true);
        }
    }
}