using System.Management.Automation;
using B2Lib;
using B2Lib.Objects;

namespace B2Powershell
{
    public abstract class B2CommandWithSaveState : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2SaveState State { get; set; }

        protected B2Client Client;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Beginning outer runner, State: " + State);

            Client = new B2Client();
            Client.LoadState(State);
            
            WriteVerbose("Running ProcessRecordInternal()");

            ProcessRecordInternal();

            WriteVerbose("Finished running ProcessRecordInternal()");

            B2SaveState newState = Client.SaveState();
            newState.CopyTo(State);

            WriteVerbose("Copied new state back to PS");
        }

        protected abstract void ProcessRecordInternal();
    }
}