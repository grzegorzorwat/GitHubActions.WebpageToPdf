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
    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        Args = new[] { "--no-sandbox", "--disable-gpu", "--disable-setuid-sandbox", "--disable-dev-shm-usage", "--single-process" },
        ExecutablePath = "/usr/bin/google-chrome"
    });
    await using var page = await browser.NewPageAsync();
    await page.GoToAsync(inputs.WebpageAddress, WaitUntilNavigation.Networkidle0);
    await page.EmulateMediaTypeAsync(GetMediaType(inputs));

    if (!Directory.Exists(inputs.OutputDirectory))
    {
        logger.LogError("Output directory must exist.");
        Environment.Exit(2);
    }

    string fileName = GetFileName(inputs);
    string fullPath = Path.Combine(inputs.OutputDirectory, fileName);

    await page.PdfAsync(fullPath);
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
    }

    fileName += PdfExtension;
    return fileName;
}

static MediaType GetMediaType(ActionInputs inputs) =>
    inputs.EmulateScreenMediaType
        ? MediaType.Screen
        : inputs.EmulatePrintMediaType
            ? MediaType.Print
            : MediaType.None;