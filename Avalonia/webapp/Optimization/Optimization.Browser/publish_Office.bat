@echo off
REM chang Path
setlocal
set PATH=C:\Users\coylee\scoop\apps\dotnet-sdk-lts\current;%PATH%
REM run publish
powershell -Command "dotnet publish -c Release -f net8.0-browser -o publish /p:AvaloniaUseWasm=true"
echo Done
pause