# Script Utilities

[![NuGet](https://img.shields.io/nuget/v/Markwardt.ScriptUtilities.svg?label=NuGet)](https://www.nuget.org/packages/Markwardt.ScriptUtilities)
[![License: MIT](https://img.shields.io/github/license/cjmarkwardt/Script-Utilities.svg)](LICENSE)
[![C#](https://github.com/cjmarkwardt/Script-Utilities/actions/workflows/csharp.yml/badge.svg)](https://github.com/cjmarkwardt/Script-Utilities/actions/workflows/csharp.yml)
[![Coverage](https://raw.githubusercontent.com/cjmarkwardt/Script-Utilities/main/.github/badges/badge_linecoverage.svg)](https://github.com/cjmarkwardt/Script-Utilities/actions/workflows/csharp.yml)

A single static `Script` class of console-based file, process, and console helpers for single-file C#
script programs (`dotnet run some-script.cs`) - the kind of small ad hoc automation that used to be
written in a purpose-built scripting language, now available directly as plain C# method calls.

## Projects

| Project | Description |
|---------|-------------|
| **Core** | The `Script` static utility class. |
| **Tests** | xUnit tests for Core. |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Installing

```sh
dotnet add package Markwardt.ScriptUtilities
```

## Building

```sh
dotnet build
```

## Testing

```sh
dotnet test
```

## Getting started

```csharp
using Markwardt.ScriptUtilities;

Script.Folder("build");
Script.Run("dotnet publish -c Release -o build");
Script.Copy("README.md", "build/README.md");
Script.Log("Build complete.");
```

## License

[MIT](LICENSE)
