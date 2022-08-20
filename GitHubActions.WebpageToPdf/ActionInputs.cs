using CommandLine;

namespace GitHubActions.WebpageToPdf;

public class ActionInputs
{
    [Option('a', "address",
        Required = true,
        HelpText = "Webpage address to generate PDF from.")]
    public string WebpageAddress { get; init; } = null!;

    [Option('o', "output",
        Required = true,
        HelpText = "Output directory. Right now it has to exist.")]
    public string OutputDirectory { get; init; } = null!;

    [Option('f', "file-name",
        Required = true,
        HelpText = "Output file name.")]
    public string OutputFileName { get; init; } = null!;

    [Option('m', "append-metadata",
        Required = false,
        HelpText = "Append timestamp to file name.")]
    public bool? AppendMetadata { get; init; } = false;

    [Option("media-type-screen",
        Required = false,
        HelpText = "Emulate screen media type.",
        SetName = "MediaTypeScreen")]
    public bool EmulateScreenMediaType { get; init; }

    [Option("media-type-print",
        Required = false,
        HelpText = "Emulate print media type.",
        SetName = "MediaTypePrint")]
    public bool EmulatePrintMediaType { get; init; }
}
