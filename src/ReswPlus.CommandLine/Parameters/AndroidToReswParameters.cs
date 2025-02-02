using CommandLine;

namespace ReswPlusCmd.Parameters;

[Verb("xml-to-resw", HelpText = "Convert a android localization XML files to resw")]
public class AndroidToReswParameters
{
    [Value(0, HelpText = "Output Directory Path", MetaName = "output")]
    public string OutputPath { get; set; }

    [Option('i', "input", HelpText = "A single XML File or a path of a directory containing xml files", Required = true)]
    public string Input { get; set; }
}
