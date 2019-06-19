using System;
using System.Reflection;
using System.Resources;
using ReswPlusLib;

namespace TestCSharpCoreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var resourceManager = TestCSharpCoreConsole.Resources.Resources.ResourceManager;
            Console.WriteLine("---------------------------------");
            Console.WriteLine(string.Format(resourceManager.GetPlural("RunDistance", 0), 0));
            Console.WriteLine(string.Format(resourceManager.GetPlural("RunDistance", 1), 1));
            Console.WriteLine(string.Format(resourceManager.GetPlural("RunDistance", 8), 8));
            Console.WriteLine("---------------------------------");
            Console.WriteLine(string.Format(resourceManager.GetPlural("YouGotMail", 0), 0));
            Console.WriteLine(string.Format(resourceManager.GetPlural("YouGotMail", 1), 1));
            Console.WriteLine(string.Format(resourceManager.GetPlural("YouGotMail", 8.3), 8.3));
            Console.WriteLine("---------------------------------");

            Console.WriteLine("Press key to quit...");
            Console.ReadKey();
        }
    }
}
