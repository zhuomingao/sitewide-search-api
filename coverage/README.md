# Code Coverage Reports
## Prerequisites
* Install the latest release from the [OpenCover project](https://github.com/OpenCover/opencover)
* Download the latest realease from the [ReportGenerator project](https://github.com/danielpalme/ReportGenerator)
There's no installer, so:
  1. Unzip the ReportGenerator_x.x.x.x.zip file
  1. Copy the files to %AppData%\Local\Apps\ReportGenerator (This is where coverage.bat assumes they'll be.)  

## Creating Code Coverage Reports
1. All of the projects in the solution must be set to generate full (i.e. "Windows only") debug information in the PDB files.
  * In project.json, find (or create) the "buildOptions" structure.
  * Change the value of the "debug" property from "portable" to "full"
1. In the root directory of the solution tree, run the command `dotnet restore`
1. In the coverage directory, run coverage.bat

The code coverage report is generated into the reports subdirectory. The main page is index.htm