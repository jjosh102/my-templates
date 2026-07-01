using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MyTemplate.ArchitectureTests;

public sealed class ArchitectureRules
{
    [Fact]
    public void Projects_follow_the_allowed_dependency_direction()
    {
        Assert.Empty(ProjectReferences("MyTemplate.Core"));
        AssertProjectReferences("MyTemplate.Application", "MyTemplate.Core");
        AssertProjectReferences("MyTemplate.Infrastructure", "MyTemplate.Core");
        AssertProjectReferences(
            "MyTemplate.Api",
            "MyTemplate.Application",
            "MyTemplate.Infrastructure",
            "MyTemplate.ServiceDefaults");
    }

    [Fact]
    public void Core_stays_business_focused()
    {
        Assert.Empty(PackageReferences("MyTemplate.Core"));
        AssertNoDirectories("MyTemplate.Core", "Features", "Shared");
        AssertSourceDoesNotContain(
            "MyTemplate.Core",
            "MyTemplate.Application",
            "MyTemplate.Infrastructure",
            "MyTemplate.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Application_exposes_use_cases_without_infrastructure_leaks()
    {
        AssertProjectReferences("MyTemplate.Application", "MyTemplate.Core");
        AssertSourceDoesNotContain(
            "MyTemplate.Application",
            "MyTemplate.Infrastructure",
            "MyTemplate.Api",
            "Microsoft.EntityFrameworkCore",
            "DbContext");

        var useCaseSources = SourceFiles("MyTemplate.Application", "UseCases").ToArray();
        var suffixMatches = useCaseSources
            .SelectMany(file => Regex.Matches(File.ReadAllText(file), @"\b(class|record)\s+(?<name>\w+(UseCase|Workflow))\b")
                .Select(match => $"{RelativePath(file)}: {match.Groups["name"].Value}"))
            .ToArray();

        Assert.Empty(suffixMatches);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_application_or_entrypoints()
    {
        AssertProjectReferences("MyTemplate.Infrastructure", "MyTemplate.Core");
        AssertSourceDoesNotContain(
            "MyTemplate.Infrastructure",
            "MyTemplate.Application",
            "MyTemplate.Api");
    }

    [Fact]
    public void Api_is_a_thin_entrypoint_over_application()
    {
        AssertProjectReferences(
            "MyTemplate.Api",
            "MyTemplate.Application",
            "MyTemplate.Infrastructure",
            "MyTemplate.ServiceDefaults");

        AssertSourceDoesNotContain(
            "MyTemplate.Api",
            "MyTemplate.Core",
            "Microsoft.EntityFrameworkCore",
            "DbContext");
    }

    private static void AssertProjectReferences(string projectName, params string[] expectedProjects)
    {
        var actual = ProjectReferences(projectName)
            .Select(Path.GetFileNameWithoutExtension)
            .Order()
            .ToArray();
        var expected = expectedProjects.Order().ToArray();

        Assert.Equal(expected, actual);
    }

    private static void AssertNoDirectories(string projectName, params string[] forbiddenNames)
    {
        var projectDirectory = ProjectDirectory(projectName);
        var matches = forbiddenNames
            .SelectMany(name => Directory.EnumerateDirectories(projectDirectory, name, SearchOption.AllDirectories))
            .Select(RelativePath)
            .ToArray();

        Assert.Empty(matches);
    }

    private static void AssertSourceDoesNotContain(string projectName, params string[] forbiddenText)
    {
        var matches = SourceFiles(projectName)
            .SelectMany(file => forbiddenText
                .Where(text => File.ReadAllText(file).Contains(text, StringComparison.Ordinal))
                .Select(text => $"{RelativePath(file)} contains {text}"))
            .ToArray();

        Assert.Empty(matches);
    }

    private static IEnumerable<string> ProjectReferences(string projectName)
    {
        return ProjectDocument(projectName)
            .Descendants("ProjectReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => Path.GetFullPath(Path.Combine(ProjectDirectory(projectName), x!)));
    }

    private static IEnumerable<string> PackageReferences(string projectName)
    {
        return ProjectDocument(projectName)
            .Descendants("PackageReference")
            .Select(x => x.Attribute("Include")?.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))!;
    }

    private static XDocument ProjectDocument(string projectName)
    {
        return XDocument.Load(Path.Combine(ProjectDirectory(projectName), $"{projectName}.csproj"));
    }

    private static IEnumerable<string> SourceFiles(string projectName, string? childPath = null)
    {
        var directory = childPath is null
            ? ProjectDirectory(projectName)
            : Path.Combine(ProjectDirectory(projectName), childPath);

        if (!Directory.Exists(directory))
            return [];

        return Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static string ProjectDirectory(string projectName)
    {
        return Path.Combine(SolutionRoot, projectName);
    }

    private static string SolutionRoot { get; } = FindSolutionRoot();

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "MyTemplate.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate MyTemplate.slnx.");
    }

    private static string RelativePath(string path)
    {
        return Path.GetRelativePath(SolutionRoot, path);
    }
}
