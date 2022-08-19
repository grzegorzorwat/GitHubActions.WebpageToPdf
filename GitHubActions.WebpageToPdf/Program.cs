using CommandLine;
using GitHubActions.WebpageToPdf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using static CommandLine.Parser;

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

    Directory.CreateDirectory(inputs.OutputDirectory);

    var fileName = $"{inputs.OutputFileName}{(inputs.AppendMetadata ? $"_{DateTime.Now:yyyyMMddHHmmss}" : "")}.pdf";
    var fullPath = Path.Combine(inputs.OutputDirectory, fileName);

    await page.PdfAsync(fullPath);
    string title = $"Created pdf for webpage {inputs.WebpageAddress}.";
    Console.WriteLine($"::set-output name=title::{title}");

    Environment.Exit(0);
}

static MediaType GetMediaType(ActionInputs inputs) =>
    inputs.EmulateScreenMediaType
        ? MediaType.Screen
        : inputs.EmulatePrintMediaType
            ? MediaType.Print
            : MediaType.None;