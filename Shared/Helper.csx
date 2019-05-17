#r "Microsoft.WindowsAzure.Storage"
#r "System.Collections"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System.Collections;

public static class ConnectionHelper {
    public static String GetConnectionStr() {
        var storageConnStr =  System.Environment.GetEnvironmentVariable("azuredevices_STORAGE", EnvironmentVariableTarget.Process);
        return storageConnStr;
    }
    
    public static CloudFileClient GetConnection(String conn) {
        var storageAccount = CloudStorageAccount.Parse(conn);
        var fileClient = storageAccount.CreateCloudFileClient();
        return fileClient;        
	}
}

public static class AppSettingsHelper {
    public static String GetAppSetting(String name) {
        var returnValue =  System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        return returnValue;
    }
}

public static class FileNameHelper {
    public static Stack GetStackofNames(ILogger log, String name) {
        var i = 0;
        var fullName = name;
        var nameStack = new Stack();
        while ( fullName != null ) {
            var directoryName = Path.GetDirectoryName(fullName);
            log.LogInformation("directoryName: " + directoryName);
            if ( directoryName != null && directoryName != "" ) {
                nameStack.Push(directoryName);
            }
            fullName = directoryName;
            if (i == 1){ 
                fullName = directoryName + @"\";  // this will preserve the previous path
            }
            i++;

        }
        return nameStack;
    }

    public static String CreateFullPathIfNotExists(ILogger log, CloudFileClient client, CloudFileShare share, IEnumerable filePathStack) {
        var fullPath = "";
        foreach ( Object obj in filePathStack ) {
            var dir = share.GetRootDirectoryReference().GetDirectoryReference(obj.ToString());
            fullPath = obj.ToString();
            dir.CreateIfNotExistsAsync().Wait();
            log.LogInformation("Path created: " + fullPath);
        }
        return fullPath; 
    }
}

