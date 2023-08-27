// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using instagram_follower_checker.helpers;

//global variables for application
var startArgs = Environment.GetCommandLineArgs().ToList();
Environment.SetEnvironmentVariable("instagramFollowerCheckerPath",Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\instagram-follower-checker");
Environment.SetEnvironmentVariable("instagramFollowerCheckerUnfollowerFilename","unfollowers.txt");


//check arguments at start
if (
    startArgs.Any(x => x.StartsWith("-u")) &&
    startArgs.Any(x => x.StartsWith("-p"))
)
{
    try
    {
        //get index of login data
        var u = startArgs.IndexOf("-u");
        var p = startArgs.IndexOf("-p");
    
        Environment.SetEnvironmentVariable("instagramUsername",startArgs[u+1]);
        Environment.SetEnvironmentVariable("password",startArgs[p+1]);
        
        Console.WriteLine($"login for {Environment.GetEnvironmentVariable("instagramUsername")} was set");
        Thread.Sleep(3000);
    }
    catch (Exception e)
    {
        Console.WriteLine("there was an error at checking the login credentials from the arguments: " + e.Message);
        return 1;
    }
}
else
{
    //username and password not set. ask for username later
}

//gloabl variables for menu
var lastResult = "";
var MenuLoopEnd = false;

int Menu()
{
    var inputWasGood = false;
    var input = -1;
    var error = "";
    
    do
    {
        Console.Clear();
        Console.WriteLine("instagram-follower-checker");
        Console.WriteLine("version: " + Informations.Version);
        if(!Environment.GetEnvironmentVariable("instagramUsername").IsEmpty())
            Console.WriteLine("hi " + Environment.GetEnvironmentVariable("instagramUsername") + " :)");
        else
            Console.WriteLine("hi :)");
    
        //Ausgaben
        if (error != "")
        {
            Console.WriteLine();
            Console.WriteLine(error);
            Console.WriteLine();
            
            error = "";
        }
        
        
        if (lastResult != "")
        {
            Console.WriteLine();
            Console.WriteLine(lastResult);
            Console.WriteLine();
            
            lastResult = "";
        }
        
        Console.WriteLine(Environment.NewLine + "" +
                          "1 = search your instagram followers" + Environment.NewLine +
                          "2 = show all unfollowers" + Environment.NewLine +
                          "3 = open directory with logs" + Environment.NewLine +
                          "8 = update notes" + Environment.NewLine +
                          "9 = exit" + Environment.NewLine);
    
        //Eingabe
        Console.WriteLine();
        var read = Console.ReadLine();
        if (!int.TryParse(read, out var entry))
        {
            error = $"'{read}' war keine Zahl";
        }

        if (
            entry == 1 ||
            entry == 2 ||
            entry == 3 ||
            entry == 8 ||
            entry == 9
            )
        {
            inputWasGood = true;
            input = entry;
        }
        else
        {
            error = $"'{read}' was not a valid entry";
        }
    } while(inputWasGood == false);

    return input;
}


do
{
    switch (Menu())
    {
        case 1:
        {
            if (Environment.GetEnvironmentVariable("instagramUsername").IsEmpty())
            {
                Console.WriteLine("Gib deinen Benutzernamen für Instagram ein:");
                var username = Console.ReadLine();
                Environment.SetEnvironmentVariable("instagramUsername",username);
            }
            
            var result = Functions.GetFollowersFromInstagram();
            if (result.StartsWith("ok;"))
            {
                result = "new unfollowers:" + Environment.NewLine + result.Substring(3);
                result = result.Replace(";", Environment.NewLine);
                lastResult = result;
            }
            else
            {
                Console.WriteLine(result);
                MenuLoopEnd = true;
            }
            break;
        }
        case 2:
        {
            var result = Functions.ShowUnfollowersList();
            if (result.Count == 0)
                lastResult = "no unfollowers :D";
            else
            {
                lastResult = "all unfollowers";
                foreach (var item in result)
                {
                    lastResult += Environment.NewLine + item;
                }
            }
            break;
        }
        case 3:
        {
            var psi = new ProcessStartInfo
            {
                FileName = @"c:\windows\explorer.exe",
                Arguments = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\instagram-follower-checker"
            };
            Process.Start(psi);
            
            break;
        }
        case 8:
        {
            var result =  Informations.GetLastUpdateNotes;
            lastResult = "update notes:" + Environment.NewLine + "- " + String.Join(Environment.NewLine + "- ",result);
            
            break;
        }
        case 9:
        {
            Console.WriteLine("bye :)");
            Thread.Sleep(2000);
            return 0;
        }
    }
} while (!MenuLoopEnd);



//end of program, when exception is thrown
Console.WriteLine("press Enter to exit");
Console.ReadLine();
return 0;


/// <summary>
/// internal class with all release notes
/// </summary>
internal static class Informations
{
    public static readonly string Version = "0.0.2";
    public static List<string> GetLastUpdateNotes => UpdateNotes.FirstOrDefault().Notes;

    private static readonly List<UpdateNotes> UpdateNotes = new()
    {
        new UpdateNotes(
            version: "0.0.2", 
            notes: new List<string>()
            {
                "translation to english",
                "added UpdateNotes to Menu",
                "added summaries to functions",
                "move String.IsEmpty to Helper.cs",
                "ask for username if not in arguments at application start"
            }
        ),
        new UpdateNotes(
            version: "0.0.1", 
            notes: new List<string>()
            {
                "first release"
            }
        )
    };
}

/// <summary>
/// internal class for release notes entry
/// </summary>
internal class UpdateNotes
{
    public UpdateNotes(string version, List<string> notes)
    {
        Version = version;
        Notes = notes;
    }

    /// <summary>
    /// version of release
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// release notes
    /// </summary>
    public List<string> Notes { get;  }
}