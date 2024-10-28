@echo off
set source=..\ESP32\.pio\build\esp32\firmware.bin
set destination=..\OTA\ControlBoard\esp32\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32\.pio\build\esp32s3usbotg\firmware.bin
set destination=..\OTA\ControlBoard\esp32S3\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32\.pio\build\esp32s3usbotg-gilphilbert\firmware.bin
set destination=..\OTA\ControlBoard\Gilphilbert_1_2\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32\.pio\build\esp32s3usbotg-gilphilbert_2_0\firmware.bin
set destination=..\OTA\ControlBoard\Gilphilbert_2_0\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y

set source=..\ESP32\.pio\build\esp32-speedcrafter\firmware.bin
set destination=..\OTA\ControlBoard\Speedcrafter\

echo Copying %source% to %destination%...
xcopy "%source%" "%destination%" /y


echo File copied successfully.
pause