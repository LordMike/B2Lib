namespace B2Lib.Objects
{
    public class B2CanceledLargeFile
    {
        public string FileId { get; set; }

        public string FileName { get; set; }

        public string BucketId { get; set; }

        public string AccountId { get; set; }
        
        public override string ToString()
        {
            return FileName;
        }
    }
}