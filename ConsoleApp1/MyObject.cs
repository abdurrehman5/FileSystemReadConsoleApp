using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1
{

    public class MyObject
    {
        Dictionary<string, Object> jsonData = new Dictionary<string, Object>();
        //Dictionary<string, Object> workspaceDocumentsjsonData = new Dictionary<string, Object>();
        Dictionary<string, string> accessUrl = new Dictionary<string, string>();
        List<Attributes> attributeList = new List<Attributes>();

        public void ReadFolder(string Location, List<string> workspacesName)
        {
            for (int i = 1; i <= 32; i++)
            {
                attributeList.Add(new Attributes
                {
                    Id=i,
                    Value="Alias"+i,
                    Description="Descrtion"+i,

                });
            }
            var directories = Directory.GetDirectories(Location).ToList();

            foreach (string item in directories)
            {
                bool isWorkspace = false;
                DirectoryInfo currentDirectoryInfo = new DirectoryInfo(item);
                string cabinetType = "CabinetRootFolder_";
                if (workspacesName.Any(o => o==currentDirectoryInfo.Name))
                {
                    isWorkspace=true;
                     cabinetType = "CabinetWorkspace_";

                }
                ReadFolderStructure(currentDirectoryInfo.FullName, cabinetType, workspacesName, true, isWorkspace);
            }
            // Convert the dictionary to a JSON string
            string dataJson = JsonConvert.SerializeObject(jsonData);
            string urlJson = JsonConvert.SerializeObject(accessUrl);
            // Convert the dictionary to a JSON stringjsonData);
            //string json = JsonSerializer.Serialize(jsonData);
            File.WriteAllText(@"C:\Users\TOSHIBA\db.json", String.Empty);
            File.WriteAllText(@"C:\Users\TOSHIBA\routes.json", String.Empty);
            //File.WriteAllText(@"C:\src\url.json", String.Empty);

            File.WriteAllText(@"C:\Users\TOSHIBA\db.json", dataJson);
            File.WriteAllText(@"C:\Users\TOSHIBA\routes.json", urlJson);
            // Print the JSON string
            Console.WriteLine(dataJson);
            Console.WriteLine(urlJson);
            // Print the JSON string
            Console.WriteLine(string.Join(",", accessUrl));

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = @"C:\Users\TOSHIBA",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                FileName = "json-server db.json --routes routes.json\r\n",
                RedirectStandardInput = true,
                UseShellExecute = false
            };
        }
        public void ReadFolderStructure(string Location, string parentFolderName, List<string> workspacesName, bool isRoot = false, bool isWorkspace = false)
        {
            Guid guid = Guid.NewGuid();
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(Location);
            string currentDirectoryName = currentDirectoryInfo.Name+"_";
            string[] files = Directory.GetFiles(Location);
            List<FolderContent> folderContents = new List<FolderContent>();
            List<WorkspaceDocument> workspaceDocuments = new List<WorkspaceDocument>();

            // Get a list of subdirectories in the directory
            string[] directories = Directory.GetDirectories(Location);

            // Create a dictionary to store the JSON representation of the files and folders
            string standardListkey = ReplaceWhitespace($"{(parentFolderName+ currentDirectoryInfo.Name)} ({guid})","");
            // Add the files to the dictionary
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (info!=null)
                {
                    string key =ReplaceWhitespace( $"{(currentDirectoryName + Path.GetFileName(file))} ({guid})","");

                    //string  = (currentDirectoryName + Path.GetFileName(file));
                    FolderContent standard = new FolderContent
                    {
                        envId =  key,
                        location = GetIdentity(file),
                        Name =Path.GetFileName(file),
                        extension  =Path.GetFileNameWithoutExtension(file),
                        size = (info?.Length??0),
                        fileCreatedDate = DataTimeConvertintoEpochData(info.CreationTimeUtc),
                        fileModifiedDate = DataTimeConvertintoEpochData(info.LastWriteTimeUtc)
                    };
                    if (isWorkspace)
                    {
                        WorkspaceDocument workspaceDocument = new WorkspaceDocument
                        {
                            envId =  key,
                            Name =Path.GetFileName(file),
                            extension  =Path.GetFileNameWithoutExtension(file),
                        };
                        workspaceDocuments.Add(workspaceDocument);
                    }
                    else
                    {

                        folderContents.Add(standard);
                    }
                    accessUrl.Add(ReplaceWhitespace("Document/"+key+"/info", ""), @"/"+key);

                    jsonData[key] = new Info
                    {
                        CustomAttributes = attributeList,
                        StandardList = standard

                    };
                }
            }
            if (isWorkspace)
            {
                accessUrl.Add(ReplaceWhitespace("Workspace/Document/"+standardListkey, ""),ReplaceWhitespace(@"/"+"Workspace_"+currentDirectoryInfo.Name+"Documents",""));

                jsonData["Workspace_"+currentDirectoryInfo.Name+"Documents"] =workspaceDocuments;
            }

            // Add the folders to the dictionary
            //string standardListkey = parentFolderName+ currentDirectoryInfo.Name;


            foreach (string directory in directories)
            {
                DirectoryInfo info = new DirectoryInfo(directory);
                if (info!=null)
                {

                    //folderContents.Add(new FolderContent
                    //{
                    //    envId = standardListkey,
                    //    location = GetIdentity(directory),
                    //    Name =Path.GetFileName(directory),
                    //    extension  ="ndfld"
                    //    //size = (info?.Length??0)

                    //});
                    //string key = currentDirectoryName+ Path.GetFileName(directory);
                    string key = ReplaceWhitespace($"{(currentDirectoryName+ Path.GetFileName(directory))} ({guid})","");

                    FolderContent standard = new FolderContent
                    {
                        envId = key,
                        location = GetIdentity(directory),
                        Name =Path.GetFileName(directory),
                        extension  =Path.GetFileNameWithoutExtension(directory),
                        size = GetDirectorySize(directory),
                        fileCreatedDate = DataTimeConvertintoEpochData(info.CreationTimeUtc),
                        fileModifiedDate = DataTimeConvertintoEpochData(info.LastWriteTimeUtc)
                    };
                    folderContents.Add(standard);

                    //accessUrl.Add("Folder/"+key+"/info");
                    //accessUrl[key] = new
                    //{
                    //    "Folder/"+key+"/info"

                    //};
                    accessUrl.Add(ReplaceWhitespace("Folder/"+key+"/info", ""), @"/"+key);
                    jsonData[key] = new Info
                    {
                        CustomAttributes = attributeList,
                        StandardList = standard

                    };
                    ReadFolderStructure(directory, currentDirectoryName, new List<string>());

                }

            }
            //accessUrl[standardListkey+"/?standardList"] = new
            //{
            //     standardListkey =standardListkey

            //};
            accessUrl.Add(ReplaceWhitespace(standardListkey+"/?standardList", ""), @"/"+standardListkey);

            //accessUrl.Add(standardListkey+"/?standardList");

            jsonData[standardListkey] = folderContents;
        }
        public string GetIdentity(string path)
        {
            // Return a unique identity for the file or folder
            // This could be a combination of the file/folder name and the creation date, for example
            return path;
        }
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public string ReplaceWhitespace(string input, string replacement)
        {
            return sWhitespace.Replace(input, replacement);
        }
        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private string DataTimeConvertintoEpochData(DateTimeOffset dataTime)
        {
            string dateConverted = null;
            long unixTimeMilliseconds = dataTime.ToUnixTimeMilliseconds();
            if (unixTimeMilliseconds>0)
            {
                dateConverted=$"Date({unixTimeMilliseconds})";
            }
            return dateConverted;
            //return dataTime.ToUniversalTime().Subtract(
            //    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            //    ).TotalMilliseconds;
            //return (Int64)t.TotalMilliseconds;

        }

    }
    public class Folders
    {
        public List<FolderContent> FolderContent { get; set; }

    }
    public class FolderContent
    {
        public string Name { get; set; }
        public string extension { get; set; }
        public long size { get; set; }
        public string envId { get; set; }
        public string location { get; set; }
        public string fileCreatedDate { get; set; }
        public string fileModifiedDate { get; set; }
    }

    public class Info
    {

        public List<Attributes> CustomAttributes { get; set; }
        public FolderContent StandardList { get; set; }

    }

    public class Attributes
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
    public class WorkspaceDocument
    {
        public string envId { get; set; }
        public string Name { get; set; }
        public string extension { get; set; }
    }

}

