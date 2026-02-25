# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [src\Obfuscator.csproj](#srcobfuscatorcsproj)
  - [tests\Obfuscator.tests\Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 2 | All require upgrade |
| Total NuGet Packages | 10 | 3 need upgrade |
| Total Code Files | 6 |  |
| Total Code Files with Incidents | 2 |  |
| Total Lines of Code | 502 |  |
| Total Number of Issues | 5 |  |
| Estimated LOC to modify | 0+ | at least 0,0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [src\Obfuscator.csproj](#srcobfuscatorcsproj) | net8.0 | 🟢 Low | 2 | 0 |  | ClassLibrary, Sdk Style = True |
| [tests\Obfuscator.tests\Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | net8.0 | 🟢 Low | 1 | 0 |  | DotNetCoreApp, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 7 | 70,0% |
| ⚠️ Incompatible | 1 | 10,0% |
| 🔄 Upgrade Recommended | 2 | 20,0% |
| ***Total NuGet Packages*** | ***10*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 489 |  |
| ***Total APIs Analyzed*** | ***489*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| coverlet.collector | 6.0.4 |  | [Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | ✅Compatible |
| Microsoft.Extensions.Compliance.Abstractions | 8.10.0 |  | [Obfuscator.csproj](#srcobfuscatorcsproj) | ✅Compatible |
| Microsoft.Extensions.Compliance.Redaction | 8.10.0 |  | [Obfuscator.csproj](#srcobfuscatorcsproj) | ✅Compatible |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.2 | 10.0.3 | [Obfuscator.csproj](#srcobfuscatorcsproj) | NuGet package upgrade is recommended |
| Microsoft.NET.Test.Sdk | 18.0.1 |  | [Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | ✅Compatible |
| Microsoft.SourceLink.GitHub | 8.0.0 |  | [Obfuscator.csproj](#srcobfuscatorcsproj) | ✅Compatible |
| Moq | 4.20.72 |  | [Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | ✅Compatible |
| System.Text.Json | 8.0.6 | 10.0.3 | [Obfuscator.csproj](#srcobfuscatorcsproj) | NuGet package upgrade is recommended |
| xunit | 2.9.3 |  | [Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | ⚠️NuGet package is deprecated |
| xunit.runner.visualstudio | 3.1.5 |  | [Obfuscator.tests.csproj](#testsobfuscatortestsobfuscatortestscsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Obfuscator.csproj</b><br/><small>net8.0</small>"]
    P2["<b>📦&nbsp;Obfuscator.tests.csproj</b><br/><small>net8.0</small>"]
    P2 --> P1
    click P1 "#srcobfuscatorcsproj"
    click P2 "#testsobfuscatortestsobfuscatortestscsproj"

```

## Project Details

<a id="srcobfuscatorcsproj"></a>
### src\Obfuscator.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 1
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 134
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P2["<b>📦&nbsp;Obfuscator.tests.csproj</b><br/><small>net8.0</small>"]
        click P2 "#testsobfuscatortestsobfuscatortestscsproj"
    end
    subgraph current["Obfuscator.csproj"]
        MAIN["<b>📦&nbsp;Obfuscator.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srcobfuscatorcsproj"
    end
    P2 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 116 |  |
| ***Total APIs Analyzed*** | ***116*** |  |

<a id="testsobfuscatortestsobfuscatortestscsproj"></a>
### tests\Obfuscator.tests\Obfuscator.tests.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 3
- **Number of Files with Incidents**: 1
- **Lines of Code**: 368
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Obfuscator.tests.csproj"]
        MAIN["<b>📦&nbsp;Obfuscator.tests.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#testsobfuscatortestsobfuscatortestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;Obfuscator.csproj</b><br/><small>net8.0</small>"]
        click P1 "#srcobfuscatorcsproj"
    end
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 373 |  |
| ***Total APIs Analyzed*** | ***373*** |  |

