using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet("Upload", "B2File")]
    public class UploadFileCommand : B2CommandWithSaveState
    {
        [Parameter(Mandatory = true, Position = 0)]
        public B2Bucket Bucket { get; set; }

        [Parameter(ParameterSetName = "by_string", Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public string[] Files { get; set; }

        [Parameter(ParameterSetName = "by_fileinfo", Mandatory = true, ValueFromPipeline = true, Position = 1)]
        public FileInfo[] FileInfo { get; set; }

        protected override void ProcessRecordInternal()
        {
            List<UploadSpec> todo = new List<UploadSpec>();
            if (ParameterSetName == "by_string")
            {
                foreach (string file in Files)
                {
                    UploadSpec tmp = new UploadSpec();
                    tmp.LocalFile = new FileInfo(file);

                    todo.Add(tmp);
                }
            }
            else if (ParameterSetName == "by_fileinfo")
            {
                foreach (FileInfo file in FileInfo)
                {
                    UploadSpec tmp = new UploadSpec();
                    tmp.LocalFile = file;

                    todo.Add(tmp);
                }
            }
            else
            {
                throw new PSArgumentException("Must define a file to upload");
            }

            WriteVerbose($"Uploading {todo.Count:N0} files");

            ProgressRecord progressRecord = new ProgressRecord(1, "placeholder", "placeholder");

            for (int i = 0; i < todo.Count; i++)
            {
                UploadSpec spec = todo[i];

                WriteVerbose($"Uploading {spec.LocalFile.Name} to {""}");

                progressRecord.Activity = $"Uploading {todo.Count - i} files";
                progressRecord.StatusDescription = spec.LocalFile.Name;

                progressRecord.PercentComplete = 0;
                WriteProgress(progressRecord);

                // Upload file
                using (FileStream fs = spec.LocalFile.OpenRead())
                {
                    B2Uploader uploader = Client.GetUploader(Bucket);

                    uploader.SetInput(fs);
                    uploader.CalculateSha1FromInput();
                    uploader.SetFileName(spec.LocalFile.Name);

                    uploader.SetNotificationAction((position, length) =>
                    {
                        if (length == 0)
                            // Hack to avoid DivideByZero
                            length = 1;

                        progressRecord.PercentComplete = (int)(position * 100f / length);
                    });

                    Task<B2FileInfo> res = Client.UploadFileAsync(uploader);
                    Task[] tasks = { res };

                    while (!Task.WaitAll(tasks, 100))
                    {
                        WriteProgress(progressRecord);
                    }

                    WriteObject(res.Result);
                }
            }
        }

        class UploadSpec
        {
            public FileInfo LocalFile { get; set; }
        }
    }
}