using CommandLine;
using PuppeteerSharp.Media;

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

    [Option('c', "commit",
        Required = false,
        HelpText = "Commit hash.")]
    public string? Commit { get; init; } = null;

    [Option('s', "script",
        Required = false,
        HelpText = "Path to javascript script to be run on the web page before the pdf is created.")]
    public string? ScriptPath { get; init; } = null;

    [Option( "css",
        Required = false,
        HelpText = "Path to css styles file to be run on the web page before the pdf is created.")]
    public string? CssPath { get; init; } = null;

    [Option('t', "media-type",
        Required = false,
        HelpText = "Emulate media type. Possible types: None/Screen/Print.",
        Default = MediaType.None)]
    public MediaType EmulateMediaType { get; init; }

    [Option("format",
        Required = false,
        HelpText = "Paper format.")]
    public string? PaperFormat { get; init; }
}
