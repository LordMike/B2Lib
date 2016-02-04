using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading;
using B2Lib.Objects;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet("Download", "B2File")]
    public class DownloadFileCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "multiple", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public B2FileBase[] Files { get; set; }

        [Parameter(ParameterSetName = "multiple")]
        public string TargetDirectory { get; set; }

        [Parameter(ParameterSetName = "single", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public B2FileBase File { get; set; }

        [Parameter(ParameterSetName = "single")]
        public string TargetFile { get; set; }

        protected override void ProcessRecordInternal()
        {
            List<DownloadSpec> todo = new List<DownloadSpec>();
            if (ParameterSetName == "single")
            {
                DownloadSpec tmp = new DownloadSpec();

                tmp.File = File;
                tmp.LocalFile = TargetFile;

                todo.Add(tmp);
            }
            else if (ParameterSetName == "multiple")
            {
                string targetDir = Path.GetFullPath(TargetDirectory ?? ".");

                foreach (B2FileBase file in Files)
                {
                    DownloadSpec tmp = new DownloadSpec();

                    tmp.File = file;
                    tmp.LocalFile = Path.Combine(targetDir, file.FileName);

                    todo.Add(tmp);
                }
            }

            WriteVerbose($"Downloading {todo.Count:N0} files");

            ProgressRecord progressRecord = new ProgressRecord(1, "placeholder", "placeholder");

            for (int i = 0; i < todo.Count; i++)
            {
                DownloadSpec spec = todo[i];

                WriteVerbose($"Downloading {spec.File.FileName} to {spec.LocalFile}");

                progressRecord.Activity = $"Downloading {todo.Count - i} files";
                progressRecord.StatusDescription = spec.File.FileName;

                progressRecord.PercentComplete = 0;
                WriteProgress(progressRecord);

                // Download file
                string dirPath = Path.GetDirectoryName(spec.LocalFile);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                using (FileStream fs = System.IO.File.OpenWrite(spec.LocalFile))
                using (B2DownloadResult res = Client.DownloadFileContent(spec.File))
                {
                    int read;
                    byte[] buffer = new byte[4096];
                    while ((read = res.Stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, read);

                        progressRecord.PercentComplete = (int)(fs.Position * 100f / res.Info.ContentLength);
                        WriteProgress(progressRecord);
                    }
                }
            }
        }

        class DownloadSpec
        {
            public B2FileBase File { get; set; }

            public string LocalFile { get; set; }
        }
    }
}