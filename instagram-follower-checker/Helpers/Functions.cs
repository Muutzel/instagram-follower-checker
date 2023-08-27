using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace instagram_follower_checker.helpers;

/// <summary>
/// Methods for the application
/// </summary>
public static class Functions
{
    /// <summary>
    /// Main method to search followers
    /// </summary>
    /// <returns>A string with "ok" or a string with an error if occoured</returns>
    /// <exception cref="Exception">When there is a exception/error with selenium</exception>
    public static string GetFollowersFromInstagram()
    {
        try
        {
            //create chromedriver
            using var driver = new ChromeDriver();
            
            //go to instagram
            driver.Navigate().GoToUrl("https://www.instagram.com/");


            //accept cookies
            var result = driver.AcceptCookies();
            if (result == "ok")
                Console.WriteLine("cookies accepted");
            else
            {
                return result;
            }
    
    
            //login
            Thread.Sleep(500);
            result = driver.LogIn();
            if (result != "ok")
                throw new Exception(result);


            //press enter when on startpage
            Console.Write("press Enter to continue when you are on the startpage");
            Console.ReadKey();
            
            
            //go to profile
            driver.Navigate().GoToUrl("https://www.instagram.com/" + Environment.GetEnvironmentVariable("instagramUsername"));
    
    
            //open follower modal
            Thread.Sleep(1000);
            result = driver.ClickElement("a", valueEndsWith:"Follower",waitSeconds:5);
            if (result != "ok")
                throw new Exception(result);
            Console.WriteLine("follower modal opened");
    
    
            //scroll down to load all followers
            var followerList = new List<string>();
            Thread.Sleep(1000);
            result = driver.ScrollDown(ref followerList);
            if (result != "ok")
                throw new Exception(result);
    
    
            //chromedriver will no longer be needed
            driver.Quit();
    
    
            //save follower list as txt file
            result = Functions.SaveFollowerNames(followerList);
            if (result != "ok")
                throw new Exception(result);
    
            
            //compare two saved lists and save unfollowers in a txt file
            result = Functions.CompareTwoLogs();
            if (!result.StartsWith("ok"))
                throw new Exception(result);
            
            return result;
        }
        catch (Exception e)
        {
            return "there was an unhandled error :/ " + Environment.NewLine + e.Message;
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
                line != "" &&
                !line.StartsWith("-----") &&
                !line.StartsWith("list created on")
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
    /// <param name="driver">the selenium driver</param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string AcceptCookies(this ChromeDriver driver)
    {
        //click on button to accept cookies
        var result = driver.ClickElement("button", value:"Alle Cookies erlauben",waitSeconds:5);

        return result;
    }


    /// <summary>
    /// Go to login page and log in
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string LogIn(this ChromeDriver driver)
    {
        var usernameInput = driver.FindElement(By.CssSelector("input[name='username']"));
        var passwordInput = driver.FindElement(By.CssSelector("input[name='password']"));
        
        #region checks

        if (usernameInput == null && passwordInput == null)
        {
            return "Username- und Password input field was not found";
        }

        if (usernameInput == null)
        {
            return "Username input field was not found";
        }

        if (passwordInput == null)
        {
            return "Password input field was not found";
        }

        #endregion
        
        
        if (Environment.GetEnvironmentVariable("password").IsEmpty())
        {
            var resultUsername = driver.SendKeys(usernameInput, Environment.GetEnvironmentVariable("instagramUsername"));
            
            //username not check because user has to login self
            
            Console.WriteLine("enter your password");
        }
        else
        {
            var resultUsername = driver.SendKeys(usernameInput, Environment.GetEnvironmentVariable("instagramUsername"));
            var resultPassword = driver.SendKeys(passwordInput, Environment.GetEnvironmentVariable("password"));
            
            var result = driver.ClickElement("button", value: "Anmelden");
            
            if (!resultUsername && !resultPassword)
            {
                return "Username and password input field could not be entered";
            }

            if (!resultUsername)
            {
                return "Username input field could not be entered";
            }

            if (!resultPassword)
            {
                return "Password input field could not be entered";
            }
            
            if (result != "ok")
                return result;
        }
        
        
        return "ok";
    }


    /// <summary>
    /// compare two logs and save the unfollowers in a txt file
    /// </summary>
    /// <returns></returns>
    private static string CompareTwoLogs()
    {
        var path = Environment.GetEnvironmentVariable("instagramFollowerCheckerPath");
        var filename = Environment.GetEnvironmentVariable("instagramFollowerCheckerUnfollowerFilename");
        
        if (!Directory.Exists(path))
        {
            return $"There are no saved follower lists to compare in '{path}'";
        }

        var files = Directory.GetFiles(path).Where(x => x.StartsWith("followers from ")).ToList();
        
        
        //read files
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
            
            //read first line. It contains information how many followers are in the list. skip this line
            var line = sr.ReadLine();
            
            var counterLine = 0;
            while (line != null)
            {
                counterLine++;
                //read next line
                line = sr.ReadLine();
                if (line != null)
                {
                    followerList[counterList].Add(line);
                    Console.WriteLine($"read follower ({counterLine}): '{line}'");
                        
                }
            }
            
            counterList++;
        }
        
        //compare lists
        Console.WriteLine(Environment.NewLine + "new unfollowers:");
        var jumpedOffFollowers = new List<string>();
        if (followerList.First().Count > 0 && followerList.Last().Count > 0)
        {
            foreach (var oldListFollower in followerList.First())
            {
                if (followerList.Last().All(x => x != oldListFollower))
                {
                    jumpedOffFollowers.Add(oldListFollower);
                    Console.WriteLine(oldListFollower);
                }
            }
        }
        
        //update txt with unfollow
        var lastResult = "";
        if (jumpedOffFollowers.Count > 0)
        {
            //read lines from unfollwers list
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
            
            
            //add new unfollowers to list in txt file
            using var sw = new StreamWriter(path + "\\" + filename);
            sw.WriteLine($"list created on {DateTime.Now.ToString("G")} - {jumpedOffFollowers.Count} unfollowers");
            foreach (var jumpedOffFollower in jumpedOffFollowers)
            {
                sw.WriteLine(jumpedOffFollower);
                lastResult += ";" + jumpedOffFollower;
            }
            
            //end of new added list
            sw.WriteLine("-------------------------------");
            
            //adding old list after new list
            foreach (var oldLine in oldLines)
            {
                sw.WriteLine(oldLine);
            }
        }
        else
        {
            Console.WriteLine("no new unfollows :D");
            lastResult = "no new unfollows :D";
        }

        return "ok" + ";" + lastResult;
    }

    
    /// <summary>
    /// save the follower names in a txt file
    /// </summary>
    /// <param name="followerList">List with unfollowers. give ordered list back</param>
    /// <returns>String "ok" or an error message if errors ocourred</returns>
    private static string SaveFollowerNames(List<string> followerList)
    {
        //sort list
        followerList = followerList.OrderBy(x => x).ToList();
        
        //save
        try
        {
            var path = Environment.GetEnvironmentVariable("instagramFollowerCheckerPath");
            var name = DateTime.Now.ToLocalTime().ToString("yy-MM-dd_hh-mm-ss");
            
            //check directory
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            //safe a little bit of space
            var files = Directory.GetFiles(path);
            var maxFiles = 25;
            if (files.Count() == maxFiles)
            {
                //delete first x files
                for (int i = 0; i < maxFiles - 5; i++)
                {
                    var firstFile = Directory.GetFiles(path).First();
                    File.Delete(firstFile);

                }
                
                //update files list
                files = Directory.GetFiles(path);
            }

            using var sw = new StreamWriter($"{path}\\{name}.txt");
            sw.WriteLine($"followers: " + followerList.Count);
            foreach (var username in followerList)
            {
                sw.WriteLine(username);
            }
            sw.Close();
            
            Console.WriteLine($"Follower was saved at '{path}\\{name}'");
        }
        catch (Exception e)
        {
            return "There was an error at saving the follower list: " + e.Message;
        }
        
        return "ok";
    }
}