#pragma once
#include <Arduino.h>
#include <stdint.h>

#define baud 921600
#define handshakeGPIO 7
#define LED_PIN 25
bool RESET_BOOTLOADER_b = false;
bool DEBUG_OUTPUT_b= false;

void restart_UART()
{
    Serial2.end();
    digitalWrite(handshakeGPIO, LOW);
    delay(3000);
    Serial2.begin(baud);
    digitalWrite(handshakeGPIO, HIGH);
}