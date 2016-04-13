using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using B2Lib.Client;
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
            
            for (int i = 0; i < todo.Count; i++)
            {
                UploadSpec spec = todo[i];

                WriteVerbose($"Uploading {spec.LocalFile.Name} to {Bucket.BucketName}");
                
                // Upload file
                B2File newFile = Bucket.CreateFile(spec.LocalFile.Name);
                newFile.UploadFileData(spec.LocalFile);

                WriteObject(newFile);
            }
        }

        class UploadSpec
        {
            public FileInfo LocalFile { get; set; }
        }
    }
}