using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace ConsoleApp1
{

    public class MyObject
    {
        Dictionary<string, Object> jsonData = new Dictionary<string, Object>();
        //Dictionary<string, Object> workspaceDocumentsjsonData = new Dictionary<string, Object>();
        Dictionary<string, string> accessUrl = new Dictionary<string, string>();
        List<Attributes> attributeList = new List<Attributes>();
        Random rand = new Random();
        public void ReadFolder(string Location, List<string> workspacesName)
        {
            DirectoryInfo currentDirectorycabinetInfo = new DirectoryInfo(Location);
            string cabinetId = currentDirectorycabinetInfo.Name.Trim();
            for (int i = 3; i <= 32; i++)
            {
                attributeList.Add(new Attributes
                {
                    Id=i,
                    value="Alias"+i,
                    description="Descrtion"+i,

                });
            }
            var directories = Directory.GetDirectories(Location).ToList();
            List<FolderContent> cabinetFolderInfo = new List<FolderContent>();

            foreach (string item in directories)
            {
                bool isWorkspace = false;
                bool isCabinetFolder = false;
                DirectoryInfo currentDirectoryInfo = new DirectoryInfo(item);
                string cabinetType = currentDirectoryInfo.Name.Trim();
                string workspaceId = "";
                string key = RandomString();

                if (workspacesName.Any(o => o == currentDirectoryInfo.Name))
                {
                    string client = "";
                    string matter = "";
                    
                    var workspaceList = currentDirectoryInfo.Name.Split(" ");
                    if (workspaceList.Length > 0)
                    {
                        client = workspaceList[0].Trim();
                        matter = workspaceList.Length > 0 ? workspaceList[1].Trim() : "";
                        attributeList.Add(new Attributes { Id = 1, value = client, description = client });
                        attributeList.Add(new Attributes { Id = 2, value = matter, description = matter });
                    }
                    accessUrl.Add(ReplaceWhitespace($"/v1/Workspace/{cabinetId}/{client}/{matter}/info", ""), "/"+ key + "Info");
                    workspaceId =$"{cabinetId}/{client}/{matter}";
                    isWorkspace = true;
                    //cabinetType = currentDirectoryInfo.Name;

                    FolderContent standard = new FolderContent
                    {
                        id = key + "Info",
                        envId = key +"Info",
                        url = key + "Info",
                        name = Path.GetFileNameWithoutExtension(item),
                        extension = "ndws",
                        size = Convert.ToString(GetDirectorySize(item)),
                        created = DataTimeConvertintoEpochData(currentDirectoryInfo.CreationTimeUtc),
                        modified = DataTimeConvertintoEpochData(currentDirectoryInfo.LastWriteTimeUtc)
                    };
                    jsonData[key + "Info"] = new Info
                    {
                        customAttributes = attributeList,
                        standardAttributes = standard

                    };
                }
                else
                {
                    FolderContent standard = new FolderContent
                    {
                        id = key,
                        envId = key,
                        url = key,
                        name = Path.GetFileNameWithoutExtension(item),
                        extension = "ndfld",
                        size = Convert.ToString(GetDirectorySize(item)),
                        created = DataTimeConvertintoEpochData(currentDirectoryInfo.CreationTimeUtc),
                        modified = DataTimeConvertintoEpochData(currentDirectoryInfo.LastWriteTimeUtc)
                    };
                    cabinetFolderInfo.Add(standard);
                    jsonData[ReplaceSpecialCharacter(key + "Info")] = new Info
                    {
                        customAttributes = attributeList,
                        standardAttributes = standard
                    };
                    accessUrl.Add(ReplaceWhitespace("/v1/Folder/" + key + "/info", ""), @"/" + ReplaceSpecialCharacter(key + "Info"));
                    isCabinetFolder = true;
                }
                ReadFolderStructure(key,currentDirectoryInfo.FullName, cabinetId, workspacesName,workspaceId, isWorkspace, isCabinetFolder);
            }
            if (cabinetFolderInfo.Any())
            {
                string key = RandomString();

                string standardListkey = ReplaceWhitespace($"{(cabinetId)}", "");

                accessUrl.Add(ReplaceWhitespace("/v1/Cabinet/" + standardListkey+ "/folders?$select=standardAttributes&format=json", ""), @"/" + key);

                //accessUrl.Add(standardListkey+"/?standardList");
                Folders folders = new Folders();
                folders.standardList = cabinetFolderInfo;
                jsonData[key] = folders;
            }
            // Convert the dictionary to a JSON string
            string dataJson = JsonConvert.SerializeObject(jsonData);
            string urlJson = JsonConvert.SerializeObject(accessUrl);
            // Convert the dictionary to a JSON stringjsonData);
            //string json = JsonSerializer.Serialize(jsonData);
            System.IO.File.WriteAllText(@"D:\db.json", String.Empty);
            System.IO.File.WriteAllText(@"D:\routes.json", String.Empty);
            //File.WriteAllText(@"C:\src\url.json", String.Empty);

            System.IO.File.WriteAllText(@"D:\db.json", dataJson);
            System.IO.File.WriteAllText(@"D:\routes.json", urlJson);
            // Print the JSON string
            Console.WriteLine(dataJson);
            Console.WriteLine(urlJson);
            // Print the JSON string
            Console.WriteLine(string.Join(",", accessUrl));
        }
        //public  string RemoveSpecialCharacters(string str)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    foreach (char c in str)
        //    {
        //        if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
        //        {
        //            sb.Append(c);
        //        }
        //    }
        //    return sb.ToString();
        //}
        public void ReadFolderStructure(string ParentKey, string Location, string cabinetId, List<string> workspacesName, string workspaceId, bool isWorkspace = false,bool isCabinetFolder=false)
        {
            //string guid = GetIdentity(Location);
            //Guid guid = Guid.NewGuid();
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(Location);
            string currentDirectoryName = currentDirectoryInfo.Name+"_";
            string[] files = Directory.GetFiles(Location);
            List<FolderContent> folderContents = new List<FolderContent>();
            WorkspaceDocuments workspaceDocuments = new WorkspaceDocuments();
            workspaceDocuments.list = new List<WorkspaceDocumentList>();

            
            // Get a list of subdirectories in the directory
            string[] directories = Directory.GetDirectories(Location);

            // Create a dictionary to store the JSON representation of the files and folders
            //string standardListkey =ReplaceWhitespace($"{(currentDirectoryInfo.Name)}","");
            // Add the files to the dictionary
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                if (info!=null)
                {
                    // string key =ReplaceWhitespace( $"{(currentDirectoryName + Path.GetFileNameWithoutExtension(file) + Path.GetExtension(file))}","");
                    string key = RandomString();

                    //string  = (currentDirectoryName + Path.GetFileName(file));
                    FolderContent standard = new FolderContent
                    {
                        id =  key,
                        envId =  key,
                        url = key,
                        name =Path.GetFileNameWithoutExtension(file),
                        extension  =Path.GetExtension(file),
                        size =Convert.ToString((info?.Length??0)),
                        created = DataTimeConvertintoEpochData(info.CreationTimeUtc),
                        modified = DataTimeConvertintoEpochData(info.LastWriteTimeUtc)
                    };
                    if (isWorkspace)
                    {
                        WorkspaceDocumentList workspaceDocument = new WorkspaceDocumentList
                        {
                            envId =  key,
                            type  =Path.GetFileNameWithoutExtension(file),
                        };
                        workspaceDocuments.list.Add(workspaceDocument);
                    }
                    else
                    {

                        folderContents.Add(standard);
                    }
                    accessUrl.Add(ReplaceWhitespace("/v1/Document/" + key+"/info", ""), @"/"+ ReplaceSpecialCharacter(key + "Info"));

                    jsonData[ReplaceSpecialCharacter(key + "Info")] = new Info
                    {
                        customAttributes = attributeList,
                        standardAttributes = standard

                    };
                }
            }
            if (isWorkspace)
            {
                string key = RandomString();
                accessUrl.Add(ReplaceWhitespace($"/v1/Workspace/{workspaceId}/documents", ""), @"/"+ key);
                jsonData[key] =workspaceDocuments;

            }
            foreach (string directory in directories)
            {
                DirectoryInfo info = new DirectoryInfo(directory);
                if (info!=null)
                {

                    string key = RandomString();

                    FolderContent standard = new FolderContent
                    {
                        id = key,
                        envId = key,
                        url = key,
                        name = info.Name,
                        extension  ="ndfld",
                        size =Convert.ToString(GetDirectorySize(directory)),
                        created = DataTimeConvertintoEpochData(info.CreationTimeUtc),
                        modified = DataTimeConvertintoEpochData(info.LastWriteTimeUtc)
                    };
                    folderContents.Add(standard);

                   
                        accessUrl.Add(ReplaceWhitespace("/v1/Folder/" + key + "/info", ""), @"/" + key + "Info");
                      jsonData[ReplaceSpecialCharacter(key + "Info")] = new Info
                      {
                          customAttributes = attributeList,
                          standardAttributes = standard

                      };                  
                    ReadFolderStructure(key, directory, cabinetId, new List<string>(), workspaceId);

                    
                }

            }            
            Folders folders = new Folders();
            folders.standardList = folderContents;
            if (!isWorkspace)
            {
                accessUrl.Add(ReplaceWhitespace("/v1/Folder/" + ParentKey + "?$select=standardAttributes&format=json", ""), @"/" + ParentKey);
                jsonData[ParentKey] = folders;

            }
            else
            {
                accessUrl.Add(ReplaceWhitespace("/v1/Workspace/" + workspaceId + "?$select=standardAttributes", ""), @"/" + ParentKey);
                jsonData[ParentKey] = folders;
            }

        }
        //public string GetIdentity(string path)
        //{
        //    // remove any invalid character from the filename.  
        //    String ret = Regex.Replace(path.Trim(), "[^A-Za-z0-9_. ]+", "");
        //    return ret.Replace(" ", String.Empty);
        //    //return Convert.ToString(rand.Next(9, 99));
        //    // Return a unique identity for the file or folder
        //    // This could be a combination of the file/folder name and the creation date, for example
        //    //return path;
        //}
        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public string ReplaceWhitespace(string input, string replacement)
        {
             return sWhitespace.Replace(input, replacement);
            
        }
        private static Random random = new Random();

        public static string RandomString()
        {
            int length = 5;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string ReplaceSpecialCharacter(string input)
        {
            return input;
            //String ret = Regex.Replace(input.Trim(), "[^A-Za-z0-9_. ]+", "");
            //return ret.Replace(" ", String.Empty);
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
    //public class ContentResponseModelWrapper
    //{
    //    public List<FolderContent> standardList { get; set; }
    //}
    public class Folders
    {
        public List<FolderContent> standardList { get; set; }

    }
    public class FolderContent
    {
        public List<int> LatestVersionLabel { get; set; } = new List<int> { 1, 2 };
        public string VersionLabel { get; set; } = "1";
        public string aclStatus { get; set; } = "thawed";
        public string created { get; set; } = "Peter Möllmann";
        public string createdBy { get; set; }
        public string createdByGuid { get; set; } = "DE-QF6JR7OS";
        public string envId { get; set; }
        public string extension { get; set; }
        public string id { get; set; }
        public int latestVersionNumber { get; set; } = 1;
        public bool locked { get; set; } = false;
        public string modified { get; set; }
        public string modifiedBy { get; set; } = "Peter Möllmann";
        public string modifiedByGuid { get; set; } = "DE-QF6JR7OS";
        public string name { get; set; }
        public int officialVer { get; set; } = 1;
        public string size { get; set; }
        public long syncMod { get; set; } = 20210224172311841;
        public string url { get; set; }
        public int versions { get; set; } = 1;
    }

    public class Info
    {

        public List<Attributes> customAttributes { get; set; }
        public FolderContent standardAttributes { get; set; }

    }

    public class Attributes
    {
        public int Id { get; set; }
        public string value { get; set; }
        public string description { get; set; }
    }
    public class WorkspaceDocuments
    {
        public List<WorkspaceDocumentList> list { get; set; }
    }
    public class WorkspaceDocumentList
    {
        public string envId { get; set; }
        public string type { get; set; }
    }

}

