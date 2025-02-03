using System;
using System.IO;
using CommandLine;
using ReswPlus.Core.ResourceParser;
using ReswPlusCmd.Converters;
using ReswPlusCmd.Parameters;

namespace ReswPlusCmd;

internal class Program
{
    private static int Main(string[] args)
    {
        var returnValue = 0;
        _ = Parser.Default.ParseArguments<ReswToAndroidParameters, AndroidToReswParameters>(args)
            .WithParsed((Action<ReswToAndroidParameters>)(parameters =>
            {
                returnValue = ReswToAndroidCommand(parameters);
            })).WithParsed((Action<AndroidToReswParameters>)(parameters =>
            {
                returnValue = AndroidToReswCommand(parameters);
            }));

        return returnValue;
    }

    #region Commands
    private static int AndroidToReswCommand(AndroidToReswParameters parameters)
    {
        if (Directory.Exists(parameters.Input))
        {
            var success = AndroidXMLConverter.AndroidXMLDirectoryToResw(parameters.Input, parameters.OutputPath!);
            if (success)
            {
                Console.WriteLine($"Directory created: {parameters.OutputPath}");
                return 0;
            }
            else
            {
                Console.WriteLine($"Error during the conversion");
                return -1;
            }
        }
        else if (File.Exists(parameters.Input))
        {
            var success = AndroidXMLConverter.AndroidXMLFileToResw(parameters.Input, parameters.OutputPath!);
            if (success)
            {
                Console.WriteLine($"File created: {parameters.OutputPath}");
                return 0;
            }
            else
            {
                Console.WriteLine($"Error during the conversion");
                return -1;
            }
        }
        else
        {
            Console.WriteLine($"The file {parameters.Input} doesn't exist");
            return -1;
        }
    }

    private static int ReswToAndroidCommand(ReswToAndroidParameters parameters)
    {
        if (!File.Exists(parameters.Input))
        {
            Console.WriteLine($"The file {parameters.Input} doesn't exist");
            return -1;
        }

        var reswContent = File.ReadAllText(parameters.Input);
        var resw = ReswParser.Parse(reswContent);
        if (resw == null)
        {
            Console.WriteLine($"Can't parse the resw file: {parameters.Input}");
            return -1;
        }
        var androidXML = AndroidXMLConverter.ReswToAndroidXML(resw, parameters.SupportPluralization);
        if (androidXML == null)
        {
            Console.WriteLine($"Error during the conversion of the file: {parameters.Input}");
            return -1;
        }
        androidXML.Save(parameters.OutputFilePath!);
        return 0;
    }
    #endregion
}
