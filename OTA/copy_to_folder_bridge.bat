@echo off
set source=..\ESP32_master\.pio\build\esp32s3-Fanatec\firmware.bin
set destination=..\OTA\Bridge\Fanatec_Bridge\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32_master\.pio\build\esp32s3-gilphilbert\firmware.bin
set destination=..\OTA\Bridge\Gilphilbert_dongle\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32_master\.pio\build\esp32s3usbotg\firmware.bin
set destination=..\OTA\Bridge\dev_kit\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

echo File copied successfully.
pause