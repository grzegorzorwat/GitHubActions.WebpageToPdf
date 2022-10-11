using CommandLine;
using GitHubActions.WebpageToPdf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using static CommandLine.Parser;

const string PdfExtension = ".pdf";
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => { })
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger(typeof(Program).FullName!)
            .LogError(
                string.Join(
                    Environment.NewLine, errors.Select(error => error.ToString())));

        Environment.Exit(2);
    });

await parser.WithParsedAsync(options => StartPdfGenerationAsync(options, host));
await host.RunAsync();

static async Task StartPdfGenerationAsync(ActionInputs inputs, IHost host)
{
    var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(StartPdfGenerationAsync));
    var options = new PdfOptions();

    if (!string.IsNullOrEmpty(inputs.PaperFormat))
    {
        options.Format = GetPaperFormat(inputs.PaperFormat);
    }

    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--single-process" },
        ExecutablePath = "/usr/bin/google-chrome"
    });
    await using var page = await browser.NewPageAsync();
    await page.GoToAsync(inputs.WebpageAddress, WaitUntilNavigation.Networkidle0);
    await page.EmulateMediaTypeAsync(inputs.EmulateMediaType);

    if (!Directory.Exists(inputs.OutputDirectory))
    {
        logger.LogError("Output directory must exist.");
        Environment.Exit(2);
    }

    string fileName = GetFileName(inputs);
    string fullPath = Path.Combine(inputs.OutputDirectory, fileName);

    if (!string.IsNullOrEmpty(inputs.ScriptPath))
    {
        if (!File.Exists(inputs.ScriptPath))
        {
            logger.LogError("Script file must exist.");
            Environment.Exit(2);
        }

        string script = File.ReadAllText(inputs.ScriptPath);
        await page.EvaluateExpressionAsync(script);
    }

    if (!string.IsNullOrEmpty(inputs.CssPath))
    {
        if (!File.Exists(inputs.CssPath))
        {
            logger.LogError("Css file must exist.");
            Environment.Exit(2);
        }

        string css = File.ReadAllText(inputs.CssPath);
        await page.AddStyleTagAsync(new AddTagOptions
        {
            Content = css
        });
        options.PreferCSSPageSize = true;
    }

    await page.PdfAsync(fullPath, options);
    string title = $"Created pdf for webpage {inputs.WebpageAddress}.";
    Console.WriteLine($"::set-output name=title::{title}");

    Environment.Exit(0);
}

static string GetFileName(ActionInputs inputs)
{
    string fileName = inputs.OutputFileName.EndsWith(PdfExtension)
        ? inputs.OutputFileName.Remove(inputs.OutputFileName.Length - PdfExtension.Length, PdfExtension.Length)
        : inputs.OutputFileName;

    if (inputs.AppendMetadata == true)
    {
        fileName += $"_{DateTime.Now:yyyyMMddHHmmss}";

        if (!string.IsNullOrEmpty(inputs.Commit))
        {
            fileName += $"_{inputs.Commit}";
        }
    }

    fileName += PdfExtension;
    return fileName;
}

static PaperFormat? GetPaperFormat(string paperFormat) => paperFormat switch
{
    "A0" => PaperFormat.A0,
    "A1" => PaperFormat.A1,
    "A2" => PaperFormat.A2,
    "A3" => PaperFormat.A3,
    "A4" => PaperFormat.A4,
    "A5" => PaperFormat.A5,
    "A6" => PaperFormat.A6,
    "Ledger" => PaperFormat.Ledger,
    "Legal" => PaperFormat.Legal,
    "Letter" => PaperFormat.Letter,
    "Tabloid" => PaperFormat.Tabloid,
    _ => null
};