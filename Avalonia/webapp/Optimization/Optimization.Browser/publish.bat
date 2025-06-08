@echo off
REM run publish
powershell -Command "dotnet publish -c Release -f net8.0-browser -o publish /p:AvaloniaUseWasm=true"
echo Done
pause