using System.Diagnostics;
using System.Text;
using Xunit.Abstractions;

namespace MyTemplates.Tests;

public sealed class TemplatePackageFixture : IDisposable
{
    private readonly string _fixtureDirectory;

    public TemplatePackageFixture()
    {
        _fixtureDirectory = Path.Combine(
            Path.GetTempPath(), "MyTemplatesFixture", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_fixtureDirectory);

        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        var srcProject = Path.Combine(rootDir, "src", "templates.csproj");
        var nupkgOutput = Path.Combine(_fixtureDirectory, "nupkg");

        var packResult = RunDotNet($"pack \"{srcProject}\" -c Release -o \"{nupkgOutput}\"");
        if (packResult.ExitCode != 0)
            throw new InvalidOperationException($"Template pack failed: {packResult.Output}");

        NupkgFile = Directory.GetFiles(nupkgOutput, "*.nupkg").FirstOrDefault()
            ?? throw new InvalidOperationException("No .nupkg file produced by pack.");

        // Ensure a clean install regardless of prior state.
        RunDotNet("new uninstall my-templates");
        var installResult = RunDotNet($"new install \"{NupkgFile}\" --force");
        if (installResult.ExitCode != 0)
            throw new InvalidOperationException($"Template install failed: {installResult.Output}");
    }


    public string NupkgFile { get; }

    public void Dispose()
    {
        RunDotNet("new uninstall my-templates");
        try
        {
            if (Directory.Exists(_fixtureDirectory))
                Directory.Delete(_fixtureDirectory, recursive: true);
        }
        catch {}
    }

    internal static (int ExitCode, string Output) RunDotNet(
        string arguments,
        string? workingDirectory = null,
        int timeoutMs = 120_000)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
            }
        };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        // Wire up async handlers BEFORE Start() to avoid missing early output.
        process.OutputDataReceived += (_, e) => { if (e.Data is not null) stdoutBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) stderrBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (!process.WaitForExit(timeoutMs))
        {
            process.Kill(entireProcessTree: true);
            return (-1, $"Timed out after {timeoutMs}ms running: dotnet {arguments}");
        }

        // Ensure async event handlers have fully flushed before reading builders.
        process.WaitForExit();

        return (process.ExitCode, stdoutBuilder.ToString() + stderrBuilder.ToString());
    }
}

[Collection("TemplateIntegration")]
public class TemplateIntegrationTests : IClassFixture<TemplatePackageFixture>
{
    private readonly ITestOutputHelper _output;
    private readonly string _testDirectory;

    public TemplateIntegrationTests(TemplatePackageFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        _testDirectory = Path.Combine(
            Path.GetTempPath(), "MyTemplatesTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void HybridApp_Default_CreatesSlnxWithoutScriptsOrEditorconfig()
    {
        var outputDir = NewDir("DefaultApp");

        var result = RunDotNet("new hybrid-app -n TestApp", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.True(File.Exists(Path.Combine(outputDir, "TestApp.slnx")),
            "Expected TestApp.slnx to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "TestApp.Core")),
            "Expected the Core project to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "TestApp.Application")),
            "Expected the Application project to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "TestApp.Infrastructure")),
            "Expected the Infrastructure project to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "TestApp.Api")),
            "Expected the Api project to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "TestApp.ArchitectureTests")),
            "Expected the ArchitectureTests project to exist.");
        Assert.False(Directory.Exists(Path.Combine(outputDir, "TestApp.Api", "Modules", "Orders", "Features")),
            "Did not expect the old API-owned Features folder.");
        Assert.False(Directory.Exists(Path.Combine(outputDir, "scripts")),
            "Did not expect a scripts/ directory.");
        Assert.False(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Did not expect an .editorconfig file.");
    }

    [Fact]
    public void HybridApp_WithScriptsAndConfigs_CreatesAllExpectedFiles()
    {
        var outputDir = NewDir("FullApp");

        var result = RunDotNet("new hybrid-app -n FullApp --scripts --configs", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.True(File.Exists(Path.Combine(outputDir, "FullApp.slnx")),
            "Expected FullApp.slnx to exist.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "scripts")),
            "Expected a scripts/ directory.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Expected an .editorconfig file.");
    }

    [Fact]
    public void HybridApp_NoApp_OnlyCreatesEditorconfig()
    {
        var outputDir = NewDir("ConfigOnly");

        var result = RunDotNet("new hybrid-app -n ConfigOnly --app false --configs", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.False(File.Exists(Path.Combine(outputDir, "ConfigOnly.slnx")),
            "Did not expect a .slnx file when --app is false.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Expected an .editorconfig file.");
    }

    [Fact]
    public void Scripts_Template_CreatesScriptsFolderOnly()
    {
        var outputDir = NewDir("ScriptsOnly");

        var result = RunDotNet("new scripts -n ScriptsOnly", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.False(File.Exists(Path.Combine(outputDir, "ScriptsOnly.slnx")),
            "Did not expect a .slnx file.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "scripts")),
            "Expected a scripts/ directory.");
        Assert.True(File.Exists(Path.Combine(outputDir, "scripts", "code-cleanup.ps1")),
            "Expected code-cleanup.ps1 inside scripts/.");
        Assert.False(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Did not expect an .editorconfig file.");
    }

    [Fact]
    public void Configs_Template_CreatesEditorconfigAndGitignore()
    {
        var outputDir = NewDir("ConfigsOnly");

        var result = RunDotNet("new configs -n ConfigsOnly", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.False(File.Exists(Path.Combine(outputDir, "ConfigsOnly.slnx")),
            "Did not expect a .slnx file.");
        Assert.False(Directory.Exists(Path.Combine(outputDir, "scripts")),
            "Did not expect a scripts/ directory.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Expected an .editorconfig file.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".gitignore")),
            "Expected a .gitignore file.");
    }

    [Fact]
    public void RepoFiles_Template_CreatesScriptsAndConfigs()
    {
        var outputDir = NewDir("RepoFiles");

        var result = RunDotNet("new repo-files -n RepoFiles", outputDir);
        Assert.True(result.ExitCode == 0, $"Template creation failed:\n{result.Output}");

        Assert.False(File.Exists(Path.Combine(outputDir, "RepoFiles.slnx")),
            "Did not expect a .slnx file.");
        Assert.True(Directory.Exists(Path.Combine(outputDir, "scripts")),
            "Expected a scripts/ directory.");
        Assert.True(File.Exists(Path.Combine(outputDir, "scripts", "code-cleanup.ps1")),
            "Expected code-cleanup.ps1 inside scripts/.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".editorconfig")),
            "Expected an .editorconfig file.");
        Assert.True(File.Exists(Path.Combine(outputDir, ".gitignore")),
            "Expected a .gitignore file.");
    }

    private string NewDir(string name)
    {
        var dir = Path.Combine(_testDirectory, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    private (int ExitCode, string Output) RunDotNet(string arguments, string? workingDirectory = null)
    {
        _output.WriteLine($"dotnet {arguments}");
        var result = TemplatePackageFixture.RunDotNet(arguments, workingDirectory);
        _output.WriteLine(result.Output);
        return result;
    }
}
