using System.Management.Automation;
using B2Lib.Client;
using B2Lib.Objects;

namespace B2Powershell
{
    public abstract class B2CommandWithSaveState : PSCmdlet
    {
        [Parameter]
        public B2SaveState State
        {
            get
            {
                if (_state == null)
                {
                    _state = this.GetState();

                    WriteVerbose("Fetching state from current session, new state: " + _state);
                }

                return _state;
            }
            set
            {
                _state = value;
            }
        }

        protected B2Client Client;
        private B2SaveState _state;

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