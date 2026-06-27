# my-templates
[![Nuget version](https://img.shields.io/nuget/v/my-templates?color=ff4081&label=nuget%20version&logo=nuget&style=flat-square)](https://www.nuget.org/packages/my-templates/)
[![Nuget downloads](https://img.shields.io/nuget/dt/my-templates?color=ff4081&label=nuget%20downloads&logo=nuget&style=flat-square)](https://www.nuget.org/packages/my-templates/)

## Getting Started

### Installation
```
dotnet new install my-templates
```

### Usage

To generate a new project from this template:

```bash
dotnet new hybrid-app -n YourProjectName
```

### Template Options

You can mix and match which parts of the template you want to scaffold using the following flags:

* `--app`: Includes the Hybrid Modular Monolith template structure (Api, AppHost, etc.). Default is `true`.
* `--scripts`: Adds a `scripts/` folder with utilities (e.g. code cleanup, user secrets) to the root of your new project. Default is `false`.
* `--configs`: Adds standard repository config files (e.g. `.editorconfig`, `.gitignore`) directly to the root of your new project. Default is `false`.

**Examples:**

Generate the full app with scripts and configs:
```bash
dotnet new hybrid-app -n MyApp --scripts --configs
```

Generate *only* the scripts and configs (without the C# application code):
```bash
dotnet new hybrid-app -n MyScriptsRepo --app false --scripts --configs
```
