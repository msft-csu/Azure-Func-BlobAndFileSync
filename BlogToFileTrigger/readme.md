# BlobTrigger - C<span>#</span>

The `BlobTrigger` makes it incredibly easy to react to new Blobs inside of Azure Blob Storage. This sample demonstrates a use case of processing data from a given Blob and moving those files to a File Share in the same Azure Storage Account using C#.

## How it works

For a `BlobTrigger` to work, you provide a path which dictates where the blobs are located inside your container, and can also help restrict the types of blobs you wish to return. For instance, you can set the path to `samples/{name}.png` to restrict the trigger to only the samples path and only blobs with ".png" at the end of their name.

## Learn more

Set STORAGE_CONN_STRING and DESTINATION_SHARE in your appsettings and in function.json of this directory you will want to set the path variable to what location in your blob storage you are wanting to copy to the fileshare.

This function will trigger on Add and Updates of blobs.  Deletes are NOT propogated to the file share.