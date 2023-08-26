// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using instagram_checker.Helpers;

var startArgs = Environment.GetCommandLineArgs().ToList();
Environment.SetEnvironmentVariable("instagramCheckerPath",Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "\\instagram-follower-checker");
Environment.SetEnvironmentVariable("instagramCheckerEntfolgtFilename","entfolgt.txt");


//prüfen ob Logindaten übergeben wurden
if (
    startArgs.Any(x => x.StartsWith("-u")) &&
    startArgs.Any(x => x.StartsWith("-p"))
)
{
    try
    {
        //index der Logindaten holen
        var u = startArgs.IndexOf("-u");
        var p = startArgs.IndexOf("-p");
    
        Environment.SetEnvironmentVariable("instagramUsername",startArgs[u+1]);
        Environment.SetEnvironmentVariable("password",startArgs[p+1]);
        
        Console.WriteLine($"Logindaten wurden für {Environment.GetEnvironmentVariable("instagramUsername")} übergeben");
        Thread.Sleep(3000);
    }
    catch (Exception e)
    {
        Console.WriteLine("Es trat ein Fehler beim auslesen der Logindaten auf (wurde Username und Passwort angegeben?): " + e.Message);
        Console.WriteLine("Mit Parameter aufrufen: \"-u username -p password\"");
        return 1;
    }
}
else
{
    //username wird später abgefragt. Passwort wird selbst eingegeben
}

var lastResult = "";
var end = false;

int Menu()
{
    var inputWasGood = false;
    var input = -1;
    var error = "";
    
    do
    {
        var inputMin = 1;
        var inputMax = 3;
        Console.Clear();
        Console.WriteLine("instagram-follower-checker");
        Console.WriteLine("Version: " + Informations.Version);
        if(!Environment.GetEnvironmentVariable("instagramUsername").IsEmpty())
            Console.WriteLine("Hallo " + Environment.GetEnvironmentVariable("instagramUsername") + " :)");
        else
            Console.WriteLine("Hallo :)");
    
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
                          "1 = Alle Follower von instagram auslesen" + Environment.NewLine +
                          "2 = bisherige entfollower anzeigen" + Environment.NewLine +
                          "3 = Ordner mit gesammelten Daten öffnen" + Environment.NewLine +
                          "9 = beenden" + Environment.NewLine);
    
        //Eingabe
        Console.WriteLine();
        var read = Console.ReadLine();
        if (!int.TryParse(read, out var entry))
        {
            error = $"'{read}' war keine Zahl";
        }

        if (entry > inputMin-1 && entry < inputMax + 1)
        {
            inputWasGood = true;
            input = entry;
        }
        else if(entry == 9)
        {
            inputWasGood = true;
            input = entry;
        }
        else
        {
            error = $"'{read}' war keine Zahl zwischen {inputMin} und {inputMax}";
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
            var result = Functions.GetFollowersFromInstagram();
            if (result.StartsWith("ok;"))
            {
                result = "Folgende Follower sind entfolgt" + Environment.NewLine + result.Substring(3);
                result = result.Replace(";", Environment.NewLine);
                lastResult = result;
            }
            else
            {
                Console.WriteLine(result);
                end = true;
            }
            break;
        }
        case 2:
        {
            var result = Functions.ShowUnfollowersList();
            if (result.Count == 0)
                lastResult = "keine entfollower :D";
            else
            {
                lastResult = "Folgende Follower sind bisher entfolgt";
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
        case 9:
        {
            Console.WriteLine("Tschüüüüü");
            Thread.Sleep(3000);
            return 0;
        }
    }
} while (!end);



//Durch Exception kommt man hier hin
Console.WriteLine("Enter drücken zum beenden");
Console.ReadLine();
return 0;


internal static class Informations
{
    public static readonly string Version = "0.0.1";
}