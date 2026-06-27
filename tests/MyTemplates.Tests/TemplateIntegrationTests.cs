using System.Diagnostics;
using Xunit.Abstractions;

namespace MyTemplates.Tests;

public class TemplateIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _testDirectory;

    public TemplateIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _testDirectory = Path.Combine(Path.GetTempPath(), "MyTemplatesTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void Can_Create_Templates_With_Options()
    {
        // Arrange
        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        var srcProject = Path.Combine(rootDir, "src", "templates.csproj");
        var nupkgOutput = Path.Combine(_testDirectory, "nupkg");

        _output.WriteLine($"Packing template from: {srcProject}");
        var packResult = RunDotNet($"pack \"{srcProject}\" -c Release -o \"{nupkgOutput}\"");
        Assert.True(packResult.ExitCode == 0, $"Pack failed: {packResult.Output}");

        var nupkgFile = Directory.GetFiles(nupkgOutput, "*.nupkg").FirstOrDefault();
        Assert.NotNull(nupkgFile);

        _output.WriteLine($"Installing template from nupkg: {nupkgFile}");
        RunDotNet("new uninstall my-templates"); // Clean up existing to prevent duplicate sequence matches
        var installResult = RunDotNet($"new install \"{nupkgFile}\" --force");
        Assert.True(installResult.ExitCode == 0, $"Install failed: {installResult.Output}");

        // Test 1: Default
        _output.WriteLine("Testing Default options...");
        var defaultDir = Path.Combine(_testDirectory, "DefaultApp");
        Directory.CreateDirectory(defaultDir);
        var createDefault = RunDotNet("new hybrid-app -n TestApp", defaultDir);
        Assert.True(createDefault.ExitCode == 0, $"Default creation failed: {createDefault.Output}");
        Assert.True(File.Exists(Path.Combine(defaultDir, "TestApp.slnx")));
        Assert.False(Directory.Exists(Path.Combine(defaultDir, "scripts")));
        Assert.False(File.Exists(Path.Combine(defaultDir, ".editorconfig")));
        var buildDefault = RunDotNet("build TestApp.slnx", defaultDir);
        Assert.True(buildDefault.ExitCode == 0, "Build failed");

        // Test 2: With Scripts and Configs
        _output.WriteLine("Testing Full options...");
        var fullDir = Path.Combine(_testDirectory, "FullApp");
        Directory.CreateDirectory(fullDir);
        var createFull = RunDotNet("new hybrid-app -n FullApp --scripts --configs", fullDir);
        Assert.True(createFull.ExitCode == 0, $"Full creation failed: {createFull.Output}");
        Assert.True(File.Exists(Path.Combine(fullDir, "FullApp.slnx")));
        Assert.True(Directory.Exists(Path.Combine(fullDir, "scripts")));
        Assert.True(File.Exists(Path.Combine(fullDir, ".editorconfig")));
        var buildFull = RunDotNet("build FullApp.slnx", fullDir);
        Assert.True(buildFull.ExitCode == 0, "Build failed");

        // Test 3: No App, Only Configs
        _output.WriteLine("Testing No App options...");
        var configDir = Path.Combine(_testDirectory, "ConfigOnly");
        Directory.CreateDirectory(configDir);
        var createConfig = RunDotNet("new hybrid-app -n ConfigOnly --app false --configs", configDir);
        Assert.True(createConfig.ExitCode == 0, $"Config creation failed: {createConfig.Output}");
        Assert.False(File.Exists(Path.Combine(configDir, "ConfigOnly.slnx")));
        Assert.True(File.Exists(Path.Combine(configDir, ".editorconfig")));
    }

    private (int ExitCode, string Output) RunDotNet(string arguments, string? workingDirectory = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
            }
        };

        process.Start();
        process.WaitForExit();

        return (process.ExitCode, "");
    }

    public void Dispose()
    {
        RunDotNet("new uninstall my-templates");
        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Failed to clean up test directory: {ex.Message}");
        }
    }
}
