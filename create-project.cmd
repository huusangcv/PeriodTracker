@echo off
setlocal

where node.exe >nul 2>&1
if %ERRORLEVEL%==0 (
  node.exe "%~dp0tools\new-project.js" %*
) else (
  echo Node.js was not found in PATH.
  echo Please install Node.js to use this command.
  exit /b 1
)

set EXIT_CODE=%ERRORLEVEL%

if NOT "%EXIT_CODE%"=="0" (
  echo.
  echo Project generation failed with exit code %EXIT_CODE%.
)

exit /b %EXIT_CODE%
