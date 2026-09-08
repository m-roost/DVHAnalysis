# Third-party components and licenses

DVH Analysis is licensed under the GNU General Public License v3.0 (see [LICENSE.txt](./LICENSE.txt)).
This repository distributes **source code only**. The third-party libraries below are *referenced* by the
projects and are downloaded from public package registries (nuget.org, PyPI) when you build or run the code.
They are **not** bundled in this repository. Each remains under its own license, listed here for transparency.

Last reviewed: 2026-09-08.

## .NET / NuGet packages

| Package | Version | License | Used by |
|---|---|---|---|
| [NLog](https://nlog-project.org/) (NLog, NLog.Config, NLog.Schema) | 4.5.9 | BSD-3-Clause | UMRO.DvhAnalysis.Logging.NLog |
| [OxyPlot](https://oxyplot.org/) (OxyPlot.Core, OxyPlot.Wpf) | 1.0.0 | MIT | UMRO.DvhAnalysis.Script |
| [PDFsharp / MigraDoc](http://www.pdfsharp.net/) (PDFsharp-MigraDoc-WPF) | 1.32.2608.0 | MIT | UMRO.DvhAnalysis.Script |
| [Entity Framework 6](https://github.com/dotnet/ef6) | 6.1.3 | Apache-2.0 | listed in UMRO.DvhAnalysis.Script packages.config (not referenced by code) |
| [Microsoft.Xaml.Behaviors.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf) | 1.1.142 | MIT | UMRO.DvhAnalysis.Runner (development helper only) |
| [MvvmLight](https://github.com/lbugnion/mvvmlight) (MvvmLightLibs) | 5.3.0.0 | MIT | UMRO.DvhAnalysis.Runner (development helper only) |
| [EclipsePlugInRunner](https://github.com/redcurry/EclipsePlugInRunner) | 2.1.0 | MIT | UMRO.DvhAnalysis.Runner (development helper only) |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) | 3.9.0 | MIT | UMRO.DvhAnalysis.Runner (development helper only) |
| [CommonServiceLocator](https://github.com/unitycontainer/commonservicelocator) | 1.3 | Ms-PL (Microsoft Public License) | UMRO.DvhAnalysis.Runner, pulled in by MvvmLight and EclipsePlugInRunner (development helper only) |
| [NUnit](https://nunit.org/) | 3.0.1 | MIT | DVHAnalysis.UnitTests (tests only) |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | 13.0.3 | MIT | UMRO.Aria.Access.Rest |

Note on CommonServiceLocator: the Free Software Foundation considers the Ms-PL license incompatible with the
GPL. It is only used by the Runner development helper, which is not part of the plugin deployed to Eclipse and
is not distributed in binary form from this repository.

## Python packages (verification scripts only)

Used by the scripts in [verification/](./verification/). See [verification/requirements.txt](./verification/requirements.txt).

| Package | License |
|---|---|
| [pydicom](https://github.com/pydicom/pydicom) | MIT |
| [NumPy](https://numpy.org/) | BSD-3-Clause (with some 0BSD / MIT / Zlib / CC0 components) |
| [SciPy](https://scipy.org/) | BSD-3-Clause |
| [Matplotlib](https://matplotlib.org/) | Matplotlib license (PSF-based, GPL-compatible) |
| [pandas](https://pandas.pydata.org/) | BSD-3-Clause |

## Varian Eclipse Scripting API (ESAPI) - not included

The projects compile against `VMS.TPS.Common.Model.API.dll` and `VMS.TPS.Common.Model.Types.dll`, which are
proprietary software of Varian Medical Systems and subject to Varian's license terms. **They are not
included in this repository and must not be committed to it.** Users must supply the assemblies from their own
licensed Eclipse / ESAPI installation. See the [Developer Guide](./Doc/Developer_Guide.md#build-the-code).

## University of Michigan helper libraries (source recovered from binaries)

The projects `UMRO.Aria.Access.Rest`, `UMRO.Aria.Documents`, `UMRO.Utils.DVHViewer` and
`UMRO.Utils.FlexibleTitleBar` under [dvhanalysisgui/src/](./dvhanalysisgui/src/) were developed by the
Department of Radiation Oncology, University of Michigan, and are copyright The Regents of the University of
Michigan. Their original source was lost; the source in this repository was recovered by decompiling the
compiled assemblies (see the note at the top of each file). They are licensed under the same GPL-3.0 terms as
the rest of this repository and are built from source; no pre-compiled binaries are distributed.

## .NET Framework

The projects target the Microsoft .NET Framework 4.8 (4.7.2 for EncryptConnectionString), which is part of the
Windows operating system and is treated as a "System Library" under GPL-3.0 section 1.
