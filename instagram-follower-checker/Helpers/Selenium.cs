using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace instagram_follower_checker.helpers;

public static class Selenium
{
    /// <summary>
    /// scroll in an element down
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <param name="followerList">the reference of followers</param>
    /// <returns>String "ok" or error message if error ocourred</returns>
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
                return $"modal not found (any more)";

            
            var getTempFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));

            if (Equals(getTempFollowerList!.LastOrDefault(), lastElementInLastRound))
            {
                try
                {
                    //read link buttons from followers
                    Console.WriteLine("read follower list...");
                    var readFollowerslist = getTempFollowerList.Where(x => x.Text.Contains("Entfernen")).ToList();
                    Console.WriteLine("follower list reading finished");

                    count = 0;
                    foreach (var rfl in readFollowerslist)
                    {
                        count++;
                        
                        Console.Write($"read name of follower {count}: ");
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
            
            Console.WriteLine($"load more follower (pass {count})");
            
            //copy last entry
            lastElementInLastRound = getTempFollowerList.LastOrDefault();
            
            if (count % 100 == 0)
            {
                Console.WriteLine(getTempFollowerList.Where(x => x.Text.Contains("Entfernen")).Count() + " followers loaded so far");
            }
            
            //scoll to last element
            Actions actions = new Actions(driver);
            actions.MoveToElement(lastElementInLastRound);
            actions.Perform();
            
            //wait because html animations
            Thread.Sleep(500);
            //read last element again and compare it
            var checkWaitFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));
            if (Equals(checkWaitFollowerList.LastOrDefault(), lastElementInLastRound))
            {
                //wait because maybe html animations
                Thread.Sleep(1000);
                //for safety read last element again and compare it
                checkWaitFollowerList = getModal.FindElements(By.CssSelector("div[role='button']"));
                if (Equals(checkWaitFollowerList.LastOrDefault(), lastElementInLastRound))
                {
                    //wait because maybe html animations. sometimes animations are slow...
                    Thread.Sleep(1000);
                }
            }
            
            //after this wating, loop again
        } while (true);
        
        return "ok";
    }

    
    /// <summary>
    /// Send text to an input
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <param name="element">the <see cref="IWebElement"/> of the input</param>
    /// <param name="text">the text to write in an input</param>
    /// <returns>Returns whether the action was successful</returns>
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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <param name="element">the HTML element to click at (a, button, ...)</param>
    /// <param name="value">optional: the value of the element</param>
    /// <param name="valueEndsWith">optional: the value of the elements endswith</param>
    /// <param name="waitSeconds">optional: how long wait for the element to appear</param>
    /// <returns>Returns whether the action was successful</returns>
    public static string ClickElement(this ChromeDriver driver, string element, string value = "",string valueEndsWith = "", int waitSeconds = 0)
    {
        //if you want to click an element with a specific value
        if (!value.IsEmpty())
        {
            var getElement = driver.FindElement(element, value: value, waitSeconds: waitSeconds);

            if (getElement == null) 
                return $"there was no element '{element}' with the value '{value}' found.";

            try
            {
                getElement.Click();
            }
            catch (Exception e)
            {
                return $"there was an error at clicking the element '{element}' with the value '{value}': " + e.Message;
            }
        }
        
        //if you want to click an element with a specific value ends with
        if (!valueEndsWith.IsEmpty())
        {
            var getElement = driver.FindElement(element, valueEndsWith: valueEndsWith, waitSeconds: waitSeconds);

            if (getElement == null)
                return $"there was no element '{element}' ends with the value '{valueEndsWith}' found.";

            try
            {
                getElement.Click();
            }
            catch (Exception e)
            {
                return $"there was an error at clicking the element '{element}' with ends with the value '{value}': " + e.Message;
            }
        }

        return "ok";
    }


    /// <summary>
    /// Find an element
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <param name="element">the HTML element to click at (a, button, ...)</param>
    /// <param name="value">optional: the value of the element</param>
    /// <param name="valueEndsWith">optional: the value of the elements endswith</param>
    /// <param name="waitSeconds">optional: how long wait for the element to appear</param>
    /// <returns>returns a <see cref="IWebElement"/> otherwise null</returns>
    private static IWebElement? FindElement(this ChromeDriver driver, string element, string value = "",string valueEndsWith = "",
        int waitSeconds = 0)
    {
        var result = driver.FindElements(element, value:value, valueEndsWith: valueEndsWith, waitSeconds: waitSeconds);
        if (result != null)
            return result.First();
        else
            return null;
    }


    /// <summary>
    /// Find an elements
    /// </summary>
    /// <param name="driver">the selenium driver</param>
    /// <param name="element">the HTML element to click at (a, button, ...)</param>
    /// <param name="value">optional: the value of the element</param>
    /// <param name="valueEndsWith">optional: the value of the elements endswith</param>
    /// <param name="indexOfElement">optional: when you only want a specific index from found elements</param>
    /// <param name="waitSeconds">optional: how long wait for the element to appear</param>
    /// <returns>returns a <see cref="IWebElement"/> otherwise null</returns>
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

            //wait because animations
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