using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace instagram_checker.Helpers;

public static class Selenium
{
    

    public static string ScrollDown(this ChromeDriver driver, ref List<string> followerList)
    {
        IWebElement lastElementInLastRound = null;
        var count = 0;
        
        Thread.Sleep(1000);
        do
        {
            count++;
            
            var getModal = driver.FindElements(By.CssSelector("div[role='dialog']")).LastOrDefault();
            
            if (getModal == null)
                return $"Modal mit Followern konnte nicht (mehr) gefunden werden";

            
            var getTempFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));

            if (Equals(getTempFollowerList!.LastOrDefault(), lastElementInLastRound))
            {
                try
                {
                    //Links der follower zur Liste hinzufügen
                    Console.WriteLine("Follower Liste auslesen...");
                    var readFolowerlist = getTempFollowerList.Where(x => x.Text.Contains("Entfernen")).ToList();
                    Console.WriteLine("Follower Liste ausgelesen");

                    count = 0;
                    foreach (var rfl in readFolowerlist)
                    {
                        count++;
                        
                        Console.Write($"lese Follower {count} aus: ");
                        var instaName =rfl 
                            .FindElement(By.XPath("parent::*"))
                            .FindElement(By.XPath("parent::*"))
                            .FindElement(By.XPath("parent::*"))
                            .FindElements(By.CssSelector("a[role='link']"))
                            .Last()
                            .Text;
                        Console.WriteLine(instaName);
                        
                        followerList.Add(instaName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
                break;
            }
            
            Console.WriteLine($"mehr follower laden ({count})");
            
            //letzten Eintrag für nächste Runde merken
            lastElementInLastRound = getTempFollowerList.LastOrDefault();
            
            if (count % 100 == 0)
            {
                Console.WriteLine("Follower geladen: " + getTempFollowerList.Where(x => x.Text.Contains("Entfernen")).Count());
            }
            
            //zum letzten scrollen
            Actions actions = new Actions(driver);
            actions.MoveToElement(lastElementInLastRound);
            actions.Perform();
            
            //warten wegen Animation
            Thread.Sleep(500);
            //letztes ELement neu auslesen und prüfen ob es sich geändert hat
            var checkWaitFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));
            if (Equals(checkWaitFollowerList.LastOrDefault(), lastElementInLastRound))
            {
                Thread.Sleep(500);
                //zur Sicherheit nochmal letztes ELement neu auslesen und prüfen ob es sich geändert hat
                checkWaitFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));
                if (Equals(checkWaitFollowerList.LastOrDefault(), lastElementInLastRound))
                {
                    Thread.Sleep(500);
                }
            }
        } while (true);
        
        return "ok";
    }

    public static bool SendKeys(this ChromeDriver driver, IWebElement element, string text)
    {
        try
        {
            element.SendKeys(text);
        }
        catch (Exception e)
        {
            return false;
        }
        
        return true;
    }


    public static string ClickElement(this ChromeDriver driver, string element, string value = "",string valueEndsWith = "", int waitSeconds = 0)
    {
        if (!value.IsEmpty())
        {
            var getElement = driver.FindElement(element, value: value, waitSeconds: waitSeconds);

            if (getElement == null)
                return $"Es wurde kein Elemente ({element}) mit dem Wert '{value}' gefunden.";

            try
            {
                getElement.Click();
            }
            catch (Exception e)
            {
                return $"Es konnte nicht auf das Element '{element}' mit dem value '{value}' geklickt werden: " + e.Message;
            }
        }
        
        if (!valueEndsWith.IsEmpty())
        {
            var getElement = driver.FindElement(element, valueEndsWith: valueEndsWith, waitSeconds: waitSeconds);

            if (getElement == null)
                return $"Es wurde kein Elemente ({element}) mit dem Wert '{valueEndsWith}' gefunden.";

            try
            {
                getElement.Click();
            }
            catch (Exception e)
            {
                return $"Es konnte nicht auf das Element '{element}' mit dem value '{valueEndsWith}' geklickt werden: " + e.Message;
            }
        }

        return "ok";
    }


    private static IWebElement? FindElement(this ChromeDriver driver, string element, string value = "",string valueEndsWith = "",
        int waitSeconds = 0)
    {
        var result = driver.FindElements(element, value:value, valueEndsWith: valueEndsWith, waitSeconds: waitSeconds);
        if (result != null)
            return result.First();
        else
            return null;
    }


    private static List<IWebElement>? FindElements(this ChromeDriver driver, string element, string value = "", string valueEndsWith = "",
        int indexOfElement = -1, int waitSeconds = 0)
    {
        var getElements = new List<IWebElement>();
        var timer = new Stopwatch();
        timer.Start();

        do
        {
            if (value != "")
            {
                getElements = driver.FindElements(By.CssSelector($"{element}"))
                    .Where(x => x.Text == value)
                    .ToList();
            }
            else if(valueEndsWith != "")
            {
                var test = getElements = driver.FindElements(By.CssSelector($"{element}")).ToList();
                
                getElements = driver.FindElements(By.CssSelector($"{element}"))
                    .Where(x => x.Text.EndsWith(valueEndsWith))
                    .ToList();
            }

            if (getElements.Count() != 0)
            {
                break;
            }

            //warte kurz
            Thread.Sleep(500);
        } while (timer.Elapsed.Seconds < waitSeconds);

        timer.Stop();

        if (getElements.Count != 0)
        {
            if (indexOfElement < 0)
                return getElements;
            else
                return new List<IWebElement>
                {
                    getElements[indexOfElement]
                };
        }
        else
        {
            return null;
        } 
    }
}