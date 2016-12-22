@echo off
setlocal
set opencover=%localAppData%\Apps\OpenCover\OpenCover.Console.exe
set filters=+[NCI.OCPL.*]* -[NCI.OCPL.*.Tests]*
echo on
%opencover% -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"test ..\test\NCI.OCPL.Api.SiteWideSearch.Tests" -register:user -output:coverage.xml -oldstyle  -filter:"%filters%"

set reportgenerator=%localAppData%\Apps\ReportGenerator\ReportGenerator.exe
set report_out=reports
if exist %report_out% rd %report_out% /q/set
mkdir %report_out%
%reportgenerator% -reports:coverage.xml -targetdir:%report_out% 

