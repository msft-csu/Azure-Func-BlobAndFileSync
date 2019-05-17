#load "../Shared/Helper.csx"
#r "Microsoft.WindowsAzure.Storage"
#r "System.Collections"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections;


public static void Run(CloudBlockBlob blobin, string name, ILogger log)
{
    log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {blobin.Properties.Length} Bytes");
    log.LogInformation($"Blob URI:{blobin.StorageUri.ToString()}");
    var connStr = ConnectionHelper.GetConnectionStr();

    // Get Client for Cloud
    var fileClient = ConnectionHelper.GetConnection(connStr);

    // Grab the share we want to replicate to from the app settings
    var shareName = AppSettingsHelper.GetAppSetting("DESTINATION_SHARE");
    log.LogInformation("DESTINATION_SHARE: " + shareName );
    
    // Create a CloudFileShare object that references the share we are replicating to
    var share = fileClient.GetShareReference(shareName);
    log.LogInformation("ShareName: " + share.Name );

    // Get Filename only
    var fileName = Path.GetFileName(name);
    log.LogInformation("GetFileName: " + name);

    // The CloudDirectory method CreateIfNotExists does NOT create subdirectories
    // This requires us to build paths from the root up.  GetStackofNames
    // Provides a LIFO Stack with directory names.
    var fileStack = FileNameHelper.GetStackofNames(log, name);
    log.LogInformation("Stack length: " + fileStack.Count);

    // CreateFullPathIfNotExists uses the LIFO Stack to iterate over the directory
    // structure and create all parent/child directories and returns the last directory
    // path created as a string.
    var path = FileNameHelper.CreateFullPathIfNotExists(fileClient, share, fileStack).Replace(@"\","/");
    log.LogInformation("FullPath: " + path);

    // Create a reference to the destination CloudFile that will be created in File Storage
    var cloudFile = GetTargetCloudFile(fileClient, share, path, fileName);
    log.LogInformation("DestFile: " + cloudFile.StorageUri.PrimaryUri.ToString() );

    // Using Uri's to copy files in Azure Storage requires SAS Token and URI
    var blobSas = blobin.GetSharedAccessSignature(new SharedAccessBlobPolicy()
    {
        // Only read permissions are required for the source file.
        Permissions = SharedAccessBlobPermissions.Read,
        SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
    });

    // Construct the URI to the source file, including the SAS token.
    var blobSasUri = new Uri(blobin.StorageUri.PrimaryUri.ToString() + blobSas);
    log.LogInformation("sourceUri: " + blobSasUri.AbsoluteUri );


    // Copy the file to the blob.
    cloudFile.StartCopyAsync(blobSasUri).Wait();
    log.LogInformation("File Created: " + cloudFile.StorageUri.PrimaryUri.ToString());
}

public static void PrintValues( ILogger log, IEnumerable myCollection )  {
    foreach ( Object obj in myCollection ) {
        log.LogInformation( "    {0}", obj );
    }
}

public static CloudFile GetTargetCloudFile(CloudFileClient client, CloudFileShare fileShare, string targetFilePath, string targetDirectory = "") {
    var root = fileShare.GetRootDirectoryReference();
    CloudFileDirectory directory = null;
    CloudFile newFile = null;
    directory = root.GetDirectoryReference(targetDirectory);
    newFile = directory.GetFileReference(targetFilePath);
    return newFile;
}