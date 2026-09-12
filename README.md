# Script Utilities

[![NuGet](https://img.shields.io/nuget/v/Markwardt.ScriptUtilities.svg?label=NuGet)](https://www.nuget.org/packages/Markwardt.ScriptUtilities)
[![License: MIT](https://img.shields.io/github/license/Markwardt-Labs/Script-Utilities.svg)](LICENSE)
[![Build](https://github.com/Markwardt-Labs/Script-Utilities/actions/workflows/build.yml/badge.svg)](https://github.com/Markwardt-Labs/Script-Utilities/actions/workflows/build.yml)
[![Coverage](https://raw.githubusercontent.com/Markwardt-Labs/Script-Utilities/main/.github/badges/badge_linecoverage.svg)](https://github.com/Markwardt-Labs/Script-Utilities/actions/workflows/build.yml)

A single static `Script` class of console-based file, process, and console helpers for single-file C#
script programs (`dotnet run some-script.cs`) - the kind of small ad hoc automation that used to be
written in a purpose-built scripting language, now available directly as plain C# method calls.

## Installing

```sh
dotnet add package Markwardt.ScriptUtilities
```

## Getting started

```csharp
using Markwardt.ScriptUtilities;

Script.Folder("build");
(await Script.Run("dotnet", "publish", "-c", "Release", "-o", "build")).Verify();
Script.Copy("README.md", "build/README.md");
Script.Log("Build complete.");
```
