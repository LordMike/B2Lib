B2Lib
=======

A .NET library for backblaze.com's B2 service.

Nuget (Coming soon)
-----

Install from Nuget using the command: **Install-Package B2Lib**
View more about that here: http://nuget.org/packages/B2Lib/

Changelog
---------

**0.0.1.0**

 - Initial version
 - Support for all methods as of `2015-11-21`

Todo
-------

 - Handle changes to the API (B2 is still in Beta)
 - Extend the unit test coverage

In General
-------

- `B2Communicator` this is a class which provides low-level API call functions. You can use this in your own scenarios. It's the base of `B2Client`.
- `B2Client` this is your entry point to the B2 API. It will handle the meta-operations, such as hooking authentication up and allowing you to cache frequently accessed data. It also provides simpler interfaces in to listing, downloading and uploading files.


Examples
--------

Simple example, authenticating and listing buckets

    B2Client client = new B2Client();
    client.Login("account-id", "application-key").Wait();
    
    List<B2Bucket> buckets = client.ListBuckets().Result;

Listing files

    foreach (B2FileWithSize file in client.ListFiles(buckets.First()))
    {
        Console.WriteLine(file.FileName);
    }

Upload file

    client.UploadFile(buckets.First(), new FileInfo("my-file"), "my-file").Wait();

Download file

    B2FileWithSize file = client.ListFileVersions(buckets.First()).First();
    B2FileDownloadResult info;
    
    using (FileStream fs = File.OpenWrite("my-file"))
    using (Stream src = client.DownloadFileContent(file, out info))
    {
        Console.WriteLine("Downloading " + info.FileName + " with SHA1: " + info.ContentSha1);
        src.CopyTo(fs);
    }
