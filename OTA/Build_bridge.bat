@echo off
REM Save the current directory
set CUR_DIR=%cd%

REM Change to the directory where the PlatformIO project is located (relative to this batch file)
cd /d "%~dp0\..\ESP32_master\"

REM Run the PlatformIO build command
platformio run

REM Change back to the original directory
cd /d %CUR_DIR%

REM Pause to keep the command prompt open after the build
pause