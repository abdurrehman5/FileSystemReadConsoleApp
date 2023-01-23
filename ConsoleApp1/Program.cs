// See https://aka.ms/new-console-template for more information
using ConsoleApp1;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    //Console.WriteLine("Hello, World!");

    public static int count = 0;

    // public static int sharedInt=0;
    public static Stopwatch stopWatch = new Stopwatch();
    static void Main()
    {
        //TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        //int arrCount = Convert.ToInt32(Console.ReadLine().Trim());

        //List<int> arr = Console.ReadLine().TrimEnd().Split(' ').ToList().Select(arrTemp => Convert.ToInt32(arrTemp)).ToList();
        MyObject myObject = new MyObject();
    Cabinet:
        // Type your username and press enter
        Console.WriteLine("Enter Cabinet Path:");
        //Console.ReadKey();
        // Create a string variable and get user input from the keyboard and store it in the variable
        string cabinetPath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(cabinetPath))
            cabinetPath= "D:\\NG-FHU61Q86";
        if (!Directory.Exists(cabinetPath))
        {
            Console.Error.WriteLine("Cabinet path is invalid. please give valid path");
            Console.Error.Close();
            goto Cabinet;
            // Close redirected error stream.
        }
    Workspace:
        Console.WriteLine("Enter Workspaces Name with Comma Seprated:");
        //Console.ReadKey();
        // Create a string variable and get user input from the keyboard and store it in the variable
        string workspaceName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(workspaceName))
            workspaceName= "8888888 8888888";
        var workpsacesList = workspaceName.Split(@",").ToList();
        foreach (string item in workpsacesList)
        {
            if (!Directory.Exists(cabinetPath+"/"+item))
            {
                Console.Error.WriteLine($"Workspace {item} not Found in this cabinet {cabinetPath}. please give valid workspace name");
                Console.Error.Close();
                goto Workspace;
                // Close redirected error stream.
            }
        }
        //myObject.ReadFolder("C:\\Users\\TOSHIBA\\Downloads\\MediaGet Downloads");
        myObject.ReadFolder(cabinetPath, workpsacesList);
        List<int> arr = new List<int>()
        {
        1,4,3,2
        };

        //List<int> res = MyObject.reverseArray(arr);

        //textWriter.WriteLine(String.Join(" ", res));

        //textWriter.Flush();
        //textWriter.Close();
        // int EnvironmentProcessorCount = 300;
        //int threadCount = EnvironmentProcessorCount;


        //stopWatch.Start();
        //List<Thread> threads = new List<Thread>();
        //for (int i = 0; i < 3; ++i)
        //{
        //    MyObject mob = new MyObject(i, threadCount);


        //    // mob.MyOwnMethod();



        //    Thread thr = new Thread(mob.MyOwnMethod);
        //    thr.Start();
        //    threads.Add(thr);
        //}
        //end of Main() function
    }

}
