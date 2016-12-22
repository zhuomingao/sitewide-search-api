@echo off & setlocal
rem Development (integration) build.

set SCRIPT_PATH=%~dp0
for %%i in ("%~dp0..\..") do set PROJECT_HOME=%%~fi
set TEST_ROOT=%PROJECT_HOME%\test
set CURDIR=%CD%


echo Running Integration Build.

rem Go to the project home foldder and restore packages
cd %PROJECT_HOME%
echo Restoring packages
dotnet restore

rem Build and run unit tests.
echo Executing unit tests
set ERRORS=
for /d %%i in (%TEST_ROOT%\*) do (
    dotnet test %%i
    if errorlevel 1 set ERRORS=true
)

if '%ERRORS%' EQU '' (
    echo All tests passed.
) ELSE (
    echo Errors have occured.
)

rem Put things back the way we found them.
cd %CURDIR%