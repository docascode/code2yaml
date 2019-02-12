@ECHO OFF
PUSHD %~dp0

SETLOCAL
SETLOCAL ENABLEDELAYEDEXPANSION

ECHO Checking Dotnet CLI version...
dotnet --version
IF NOT '%ErrorLevel%' == '0' (
    ECHO Error: build.cmd requires Dotnet CLI. Please follow https://www.microsoft.com/net/core to install .NET Core."
    SET ERRORLEVEL=1
    GOTO :Exit
)

:EnvSet
SET BuildProj=%~dp0Code2Yaml.sln
SET Configuration=%1
IF '%Configuration%'=='' (
    SET Configuration=Release
)
SET CachedNuget=%LocalAppData%\NuGet\NuGet.exe

:: nuget wrapper requires nuget.exe path in %PATH%
SET PATH=%PATH%;%LocalAppData%\NuGet

:: Download Doxygen
CALL :DownloadDoxygen

:: Build
dotnet build %BuildProj%
SET BuildErrorLevel=%ERRORLEVEL%
GOTO :Exit

:DownloadDoxygen
SET DoxygenLocation=%~dp0src\Microsoft.Content.Build.Code2Yaml.Steps\tools
IF NOT EXIST "%DoxygenLocation%\doxygen.exe" (
IF NOT EXIST "%DoxygenLocation%\temp" MD "%DoxygenLocation%\temp"
powershell -NoProfile -ExecutionPolicy UnRestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://cfhcable.dl.sourceforge.net/project/doxygen/rel-1.8.12/doxygen-1.8.12.windows.x64.bin.zip' -OutFile '%DoxygenLocation%\temp\doxygen.zip'"
powershell -NoProfile -Command "Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('%DoxygenLocation%\temp\doxygen.zip', '%DoxygenLocation%\temp'); Move-Item '%DoxygenLocation%\temp\doxygen.exe' '%DoxygenLocation%' -force; Move-Item '%DoxygenLocation%\temp\*.dll' '%DoxygenLocation%' -force; Remove-Item '%DoxygenLocation%\temp' -Recurse -force;"
)

:Exit
POPD
ECHO.
EXIT /B %ERRORLEVEL%