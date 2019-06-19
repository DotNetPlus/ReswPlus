Imports System
Imports System.Reflection
Imports System.Resources
Imports ReswPlusLib

Module Program
    Sub Main(args As String())
        Console.WriteLine("Hello World!")
        Dim d = Assembly.GetExecutingAssembly()
        Dim resourceManager = My.Resources.Resources.ResourceManager

        Console.WriteLine("---------------------------------")
        Console.WriteLine(String.Format(resourceManager.GetPlural("RunDistance", 0), 0))
        Console.WriteLine(String.Format(resourceManager.GetPlural("RunDistance", 1), 1))
        Console.WriteLine(String.Format(resourceManager.GetPlural("RunDistance", 8), 8))
        Console.WriteLine("---------------------------------")
        Console.WriteLine(String.Format(resourceManager.GetPlural("YouGotMail", 0), 0))
        Console.WriteLine(String.Format(resourceManager.GetPlural("YouGotMail", 1), 1))
        Console.WriteLine(String.Format(resourceManager.GetPlural("YouGotMail", 8.3), 8.3))
        Console.WriteLine("---------------------------------")

        Console.WriteLine("Press key to quit...")
        Console.ReadKey()
    End Sub
End Module
