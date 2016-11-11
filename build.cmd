@ECHO OFF
PUSHD %~dp0

SETLOCAL
SETLOCAL ENABLEDELAYEDEXPANSION

IF NOT DEFINED VisualStudioVersion (
    IF DEFINED VS140COMNTOOLS (
        CALL "%VS140COMNTOOLS%\VsDevCmd.bat"
        GOTO :EnvSet
    )

    ECHO Error: build.cmd requires Visual Studio 2015.
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


:: Restore packages for .csproj projects

CALL :RestorePackage

:: Download Doxygen
CALL :DownloadDoxygen

:: Log build command line
SET BuildLog=%~dp0msbuild.log
SET BuildPrefix=echo
SET BuildPostfix=^> "%BuildLog%"

CALL :Build %*

:: Build
SET BuildPrefix=
SET BuildPostfix=
CALL :Build %*
IF NOT '%ErrorLevel%'=='0' (
    GOTO :AfterBuild
)


:AfterBuild

:: Pull the build summary from the log file
ECHO.
ECHO === BUILD RESULT ===
findstr /ir /c:".*Warning(s)" /c:".*Error(s)" /c:"Time Elapsed.*" "%BuildLog%" & cd >nul
ECHO Exit Code: %BuildErrorLevel%
SET ERRORLEVEL=%BuildErrorLevel%
GOTO :Exit

:Build
%BuildPrefix% msbuild "%BuildProj%" /p:Configuration=%Configuration% /nologo /maxcpucount:1 /verbosity:minimal /nodeReuse:false /fileloggerparameters:Verbosity=diag;LogFile="%BuildLog%"; %BuildPostfix%
SET BuildErrorLevel=%ERRORLEVEL%
GOTO :Exit

:RestorePackage

SET CachedNuget=%LocalAppData%\NuGet\NuGet.exe
IF EXIST "%CachedNuget%" GOTO :Restore
ECHO Downloading latest version of NuGet.exe...
IF NOT EXIST "%LocalAppData%\NuGet" MD "%LocalAppData%\NuGet"
powershell -NoProfile -ExecutionPolicy UnRestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CachedNuget%'"

:Restore
:: Currently has corpnet dependency
nuget restore "%BuildProj%"

:DownloadDoxygen
SET DoxygenLocation=%~dp0src\Microsoft.Content.Build.Code2Yaml.Steps\tools
IF NOT EXIST "%DoxygenLocation%\doxygen.exe" (
IF NOT EXIST "%DoxygenLocation%\temp" MD "%DoxygenLocation%\temp"
powershell -NoProfile -ExecutionPolicy UnRestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'http://ftp.stack.nl/pub/users/dimitri/doxygen-1.8.12.windows.x64.bin.zip' -OutFile '%DoxygenLocation%\temp\doxygen.zip'"
powershell -NoProfile -Command "Add-Type -A 'System.IO.Compression.FileSystem'; [IO.Compression.ZipFile]::ExtractToDirectory('%DoxygenLocation%\temp\doxygen.zip', '%DoxygenLocation%\temp'); Move-Item '%DoxygenLocation%\temp\doxygen.exe' '%DoxygenLocation%' -force; Move-Item '%DoxygenLocation%\temp\*.dll' '%DoxygenLocation%' -force; Remove-Item '%DoxygenLocation%\temp' -Recurse -force;"
)

:Exit
POPD
ECHO.
EXIT /B %ERRORLEVEL%