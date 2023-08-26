# instagram-follower-checker
This console application is used to check who unfollowed you on instagram. It uses a chromedriver with selenium to login to your instagram account and scan your followers. 
It saves the list of your followers in a file. When you run the application again it compares the new list with the old one and saves the difference in a new file. You can see who unfollowed you by opening the file or use the application.
- --
###### License [MIT](https://choosealicense.com/licenses/mit/)

## Requirements
- [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)
- OS x68 or x64
- Nuget: Selenium.WebDriver 4.11.0

## Language
- c# 11.0

## How to use
1. Download the latest release 
2. Open the application in the terminal
   1. Start without parameters
   2. Start with parameters: "-u instagramUsername -p instagramPassword"
3. Choose an option in the menu
   1. option 1 (scan followers) 
      1. Wait for the application to finish and do nothing in the browser from selenium
      2. you see a list of all your scanned followers. when you do this twice you can see who unfollowed you 
   2. option 2 (see list of unfollowers)
      1. when you did option 1 twice you can see who unfollowed you
   3. option 3 (open directory)
      1. opens the directory where the application saves all lists

## How to build
1. Download the source code
2. Open the solution in Rider or Visual Studio
   3. or something else for C# projects
3. Build the solution
   1. publish the project
   2. go to the publish directory
   3. copy the `instagram-follower-checker.exe` file to the directory where you want
   4. copy the `selenium-manager` directory to the same destination
      1. includes chrome driver for windows, linux and mac
   5. use the application

## How to contribute
1. Fork the repository
   1. Make your changes
   2. Create a pull request
2. create an issue 

## Credits
Muutzel @ github

## Updates
### 0.0.1 initial upload in german
