using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using B2Lib.Client;
using B2Lib.SyncExtensions;

namespace B2Powershell
{
    [Cmdlet("Download", "B2File")]
    public class DownloadFileCommand : B2CommandWithSaveState
    {
        [Parameter(ParameterSetName = "multiple", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public B2File[] Files { get; set; }

        [Parameter(ParameterSetName = "multiple")]
        public string TargetDirectory { get; set; }

        [Parameter(ParameterSetName = "single", Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public B2File File { get; set; }

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

                foreach (B2File file in Files)
                {
                    DownloadSpec tmp = new DownloadSpec();

                    tmp.File = file;
                    tmp.LocalFile = Path.Combine(targetDir, file.FileName);

                    todo.Add(tmp);
                }
            }

            WriteVerbose($"Downloading {todo.Count:N0} files");

            for (int i = 0; i < todo.Count; i++)
            {
                DownloadSpec spec = todo[i];

                WriteVerbose($"Downloading {spec.File.FileName} to {spec.LocalFile}");

                // Download file
                string dirPath = Path.GetDirectoryName(spec.LocalFile);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                using (FileStream fs = System.IO.File.OpenWrite(spec.LocalFile))
                using (Stream res = spec.File.DownloadData())
                {
                    res.CopyTo(fs);
                }
            }
        }

        class DownloadSpec
        {
            public B2File File { get; set; }

            public string LocalFile { get; set; }
        }
    }
}