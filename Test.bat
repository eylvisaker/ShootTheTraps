@ECHO OFF

packages\xunit.runner.console.2.4.0\tools\net452\xunit.console.exe UnitTests\bin\Debug\UnitTests.dll -noshadow

exit /b %ERRORLEVEL%
