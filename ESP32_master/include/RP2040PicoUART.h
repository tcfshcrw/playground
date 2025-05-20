#pragma once
#include <Arduino.h>
#include <HardwareSerial.h>
const int RP2040baudrate=921600;
const int handshakeGPIO=17;
const int RP2040txPin=15;
const int RP2040rxPin=16;

class RP2040PicoUART
{
    public:
        RP2040PicoUART(int rxPin, int txPin, int handshakePin, int baud);
        void UARTrestart();
        void UARTSendPacket(uint8_t* data, size_t len);
        HardwareSerial* SerialExt;
        int _txPin;
        int _rxPin;
        int _handshakePin;
        int _baud;


};