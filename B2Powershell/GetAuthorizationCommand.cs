using System.Management.Automation;
using B2Lib.Client;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet(VerbsCommon.Get, "B2Authorization")]
    public class GetAuthorizationCommand : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string AccountId { get; set; }

        [Parameter(Mandatory = true)]
        public string ApplicationKey { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("Logging in with " + AccountId);

            B2Client client = new B2Client();
            client.Login(AccountId, ApplicationKey);

            B2SaveState state = client.SaveState();
            this.SaveState(state);

            WriteObject(state);
        }
    }
}
