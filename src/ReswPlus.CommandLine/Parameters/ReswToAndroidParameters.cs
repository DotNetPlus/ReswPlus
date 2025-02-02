using CommandLine;

namespace ReswPlusCmd.Parameters;

[Verb("resw-to-xml", HelpText = "Convert a single resw file to Android XML format")]
public class ReswToAndroidParameters
{
    [Value(0, HelpText = "Output File Path", MetaName = "output")]
    public string OutputFilePath { get; set; }

    [Option('i', "input", HelpText = "Resw input file", Required = true)]
    public string Input { get; set; }

    [Option('p', "pluralization", Default = true, HelpText = "boolean indicating if the resw file supports pluralization", Required = false)]
    public bool SupportPluralization { get; set; }
}
