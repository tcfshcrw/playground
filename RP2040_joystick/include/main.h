#pragma once
#include<Arduino.h>
#include <stdint.h>
const uint8_t DAP_PAYLOAD_TYPE_JOYSTICKUART=230;
const uint16_t JOYSTICK_VALUE_MAX=1023;
struct payloadjoystick
{
  uint8_t payloadtype;
  int32_t controllerValue_i32[3];
  int8_t pedal_status;
  uint16_t checkSum;
} ;
struct payloadFooter 
{
  // To check if structure is valid
  uint16_t checkSum;
};

struct DAP_JoystickUART_State 
{
  payloadjoystick _payloadjoystick;
  payloadFooter _payloadfooter;
};
