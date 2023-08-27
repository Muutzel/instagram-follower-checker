using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace instagram_follower_checker;

/// <summary>
/// Methods for the application
/// </summary>
public static class Functions
{
    /// <summary>
    /// Main method to search follers
    /// </summary>
    /// <returns>A string with "ok" or a string with an error if occours</returns>
    /// <exception cref="Exception">When there is a exception/error with selenium</exception>
    public static string GetFollowersFromInstagram()
    {
        try
        {
            //ChromeDriver erstellen
            using var driver = new ChromeDriver();
            
            //driver erstellen und auf Profil gehen
            driver.Navigate().GoToUrl("https://www.instagram.com/");


            //Cookies akzeptieren, wenn sie auftauchen
            var result = driver.AcceptCookies();
            if (result == "ok")
                Console.WriteLine("Cookies wurden akzeptiert");
            else
            {
                return result;
            }
    
    
            //einloggen
            Thread.Sleep(500);
            result = driver.LogIn();
            if (result != "ok")
                throw new Exception(result);
            Console.WriteLine("Erledige 2FA");


            //Enter drücken, wenn man fertig ist
            Console.Write("Drücke Enter, wenn du auf der Startseite bist");
            Console.ReadKey();
            
            
            //gehe auf Profil
            driver.Navigate().GoToUrl("https://www.instagram.com/" + Environment.GetEnvironmentVariable("instagramUsername"));
    
    
            //gehe auf Follower Seite
            Thread.Sleep(1000);
            result = driver.ClickElement("a", valueEndsWith:"Follower",waitSeconds:5);
            if (result != "ok")
                throw new Exception(result);
            Console.WriteLine("Follower als Modal erschienen");
    
    
            //scrolle runter bis nichts mehr geladen wird
            var followerList = new List<string>();
            Thread.Sleep(1000);
            result = driver.ScrollDown(ref followerList);
            if (result != "ok")
                throw new Exception(result);
    
    
            //Browser wird nicht mehr gebraucht
            driver.Quit();
    
    
            //Liste speichern
            result = Functions.SaveUnfollowerNames(followerList);
            if (result != "ok")
                throw new Exception(result);
    
            
            //gespeicherte Listen vergleichen
            result = Functions.CompareTwoLogs();
            if (!result.StartsWith("ok"))
                throw new Exception(result);
            
            return result;
        }
        catch (Exception e)
        {
            return "Es trat ein Fehler auf: " + e.Message;
        }
    }
    
    
    /// <summary>
    /// Method to show the unfollowers 
    /// </summary>
    /// <returns>List of strings that contains username of unfollowers</returns>
    public static List<string> ShowUnfollowersList()
    {
        var path = Environment.GetEnvironmentVariable("instagramFollowerCheckerPath");
        var filename = Environment.GetEnvironmentVariable("instagramFollowerCheckerUnfollowerFilename");
        var unfollowers = new List<string>();

        if(File.Exists(path + "\\" + filename) == false)
            return unfollowers;
        
        using var sr = new StreamReader(path + "\\" + filename);
        var line = sr.ReadLine();
        while (line != null)
        {
            if (
                line != null &&
                line != "" &&
                !line.StartsWith("-----") &&
                !line.StartsWith("Liste erstellt am")
            )
            {
                unfollowers.Add(line);
            }
            line = sr.ReadLine();
        }
        
        return unfollowers;
    }


    /// <summary>
    /// Accept Cookies on the website
    /// </summary>
    /// <param name="driver"></param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string AcceptCookies(this ChromeDriver driver)
    {
        //Seite fertig laden lassen
        
        var result = driver.ClickElement("button", value:"Alle Cookies erlauben",waitSeconds:5);

        return result;
    }


    /// <summary>
    /// Go to login page and log in
    /// </summary>
    /// <param name="driver"></param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string LogIn(this ChromeDriver driver)
    {
        var usernameInput = driver.FindElement(By.CssSelector("input[name='username']"));
        var passwordInput = driver.FindElement(By.CssSelector("input[name='password']"));
        
        #region checks

        if (usernameInput == null && passwordInput == null)
        {
            return "Username- und Passworteingabefeld wurden nicht gefunden";
        }

        if (usernameInput == null)
        {
            return "Usernameeingabefeld wurde nicht gefunden";
        }

        if (passwordInput == null)
        {
            return "Passworteingabefeld wurde nicht gefunden";
        }

        #endregion
        
        
        if (Environment.GetEnvironmentVariable("password").IsEmpty())
        {
            var resultUsername = driver.SendKeys(usernameInput, Environment.GetEnvironmentVariable("instagramUsername"));
            
            //username not check because user has to login self
            
            Console.WriteLine("Gib deine Logindaten ein und bestätige");
        }
        else
        {
            var resultUsername = driver.SendKeys(usernameInput, Environment.GetEnvironmentVariable("instagramUsername"));
            var resultPassword = driver.SendKeys(passwordInput, Environment.GetEnvironmentVariable("password"));
            
            var result = driver.ClickElement("button", value: "Anmelden");
            
            if (!resultUsername && !resultPassword)
            {
                return "Username- und Passworteingabefeld konnten nicht eingetragen werden";
            }

            if (!resultUsername)
            {
                return "Usernameeingabefeld konnte nicht eingetragen werden";
            }

            if (!resultPassword)
            {
                return "Passworteingabefeld konnte nicht eingetragen werden";
            }
            
            if (result != "ok")
                return result;
        }
        
        
        return "ok";
    }


    /// <summary>
    /// compare two logs and save the unfollowers
    /// </summary>
    /// <returns></returns>
    private static string CompareTwoLogs()
    {
        var path = Environment.GetEnvironmentVariable("instagramFollowerCheckerPath");
        var filename = Environment.GetEnvironmentVariable("instagramFollowerCheckerUnfollowerFilename");
        
        if (!Directory.Exists(path))
        {
            return $"Der Pfad '{path}' zu den Follower-Listen existiert nicht";
        }

        var files = Directory.GetFiles(path).Where(x => !x.EndsWith(filename)).ToList();

        if (files.Count > 2)
            return $"Es wurden {files.Count} Dateien im Ordner '{path}' gefunden";
        
        
        //Daten auslesen
        var followerList = new List<List<string>>()
        {
            new List<string>(),
            new List<string>()
        };
        var counterList = 0;
        foreach (var file in files)
        {
            Console.WriteLine(Environment.NewLine + $"Liste {counterList+1} auslesen...");
            
            using var sr = new StreamReader(file);
            
            //erste Zeile einlesen. Die enthält Informationen wie viele Follower in der Liste sind
            var line = sr.ReadLine();
            
            var counterLine = 0;
            while (line != null)
            {
                counterLine++;
                //Zeile auslesen
                line = sr.ReadLine();
                if (line != null)
                {
                    followerList[counterList].Add(line);
                    Console.WriteLine($"Follower {counterLine} auslesen: '{line}'");
                        
                }
            }
            
            counterList++;
        }
        
        //vergleichen
        Console.WriteLine(Environment.NewLine + "new unfollower");
        var jumpedOffFollowers = new List<string>();
        if (followerList[0].Count > 0 && followerList[1].Count > 0)
        {
            foreach (var oldListFollower in followerList[0])
            {
                if (followerList[1].All(x => x != oldListFollower))
                {
                    jumpedOffFollowers.Add(oldListFollower);
                    Console.WriteLine(oldListFollower);
                }
            }
        }
        
        //aktualisiere txt mit entfollowern
        var lastResult = "";
        if (jumpedOffFollowers.Count > 0)
        {
            //alte Zeilen einlesen
            var oldLines = new List<string>();
        
            if(File.Exists(path + "\\" + filename))
            {
                using var sr = new StreamReader(path + "\\" + filename);
                var oldLine = sr.ReadLine();
                while (oldLine != null)
                {
                    oldLines.Add(oldLine);
                    oldLine = sr.ReadLine();
                }
            }
            
            
            //speichern
            using var sw = new StreamWriter(path + "\\" + filename);
            sw.WriteLine($"Liste erstellt am {DateTime.Now.ToString("G")} ({jumpedOffFollowers.Count} unfollowers)");
            foreach (var jumpedOffFollower in jumpedOffFollowers)
            {
                sw.WriteLine(jumpedOffFollower);
                lastResult += ";" + jumpedOffFollower;
            }
            
            sw.WriteLine("-------------------------------");
            sw.WriteLine();
            
            //alte Zeilen hinten anfügen
            foreach (var oldLine in oldLines)
            {
                sw.WriteLine(oldLine);
            }
        }
        else
        {
            Console.WriteLine("keine entfollower :D");
            lastResult = "keine entfollower :D";
        }

        return "ok" + ";" + lastResult;
    }

    
    /// <summary>
    /// save the unfollower names in a txt file
    /// </summary>
    /// <param name="followerList">List with unfollowers. give ordered list back</param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string SaveUnfollowerNames(List<string> followerList)
    {
        //liste sortieren
        followerList = followerList.OrderBy(x => x).ToList();
        
        //speichern
        try
        {
            var path = Environment.GetEnvironmentVariable("instagramFollowerCheckerPath");
            var name = DateTime.Now.ToLocalTime().ToString("yy-MM-dd_hh-mm-ss");
            
            //prüfen ob Ordner existiert
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            //wenn schon 3 Dateien vorhanden sind
            var files = Directory.GetFiles(path);
            if (files.Count() == 3)
                File.Delete(files.First());

            using var sw = new StreamWriter($"{path}\\{name}.txt");
            sw.WriteLine($"Anzahl: " + followerList.Count);
            foreach (var username in followerList)
            {
                sw.WriteLine(username);
            }
            sw.Close();
            
            Console.WriteLine($"Die Follower wurde unter '{path}\\{name}' gespeichert");
        }
        catch (Exception e)
        {
            return "Es gab einen Fehler beim speichern der Usernamen: " + e.Message;
        }
        
        return "ok";
    }
}