#include <Arduino.h>
#include "Joystick_RP2040.h"

typedef struct __attribute__((packed)) payloadjoystick {
  uint8_t payloadtype;
  int32_t controllerValue_i32[3];
  int8_t pedal_status;
  uint16_t checkSum;
} ;

typedef struct __attribute__((packed)) payloadFooter {
  uint16_t checkSum;
};

typedef struct __attribute__((packed)) DAP_JoystickUART_State {
  payloadjoystick _payloadjoystick;
  payloadFooter _payloadfooter;
};

Joystick_RP2040 Joystick;
DAP_JoystickUART_State dap_joystickUART_st_local;
const uint8_t DAP_PAYLOAD_TYPE_JOYSTICKUART = 230;

uint16_t checksumCalculator(uint8_t *data, uint16_t length) {
  uint8_t sum1 = 0, sum2 = 0;
  for (int i = 0; i < length; i++) {
    sum1 = (sum1 + data[i]) % 255;
    sum2 = (sum2 + sum1) % 255;
  }
  return (sum2 << 8) | sum1;
}

void setup() 
{
  //Serial1.begin(115200, SERIAL_8N1, 5, 4);  // RX=5, TX=4
  Serial1.begin(115200);
  //TinyUSBDevice.setManufacturerDescriptor("YourCompany");
  //TinyUSBDevice.setProductDescriptor("Custom Joystick");
  //TinyUSBDevice.setSerialDescriptor("SN2025A01");

  Joystick.begin();
}

void loop() {
  if (Serial1.available() >= sizeof(DAP_JoystickUART_State)) {
    Serial1.readBytes((char*)&dap_joystickUART_st_local, sizeof(DAP_JoystickUART_State));
    bool structChecker = true;

    if (dap_joystickUART_st_local._payloadjoystick.payloadtype != DAP_PAYLOAD_TYPE_JOYSTICKUART)
      structChecker = false;

    uint16_t crc = checksumCalculator(
        (uint8_t*)&(dap_joystickUART_st_local._payloadjoystick),
        sizeof(dap_joystickUART_st_local._payloadjoystick));

    if (crc != dap_joystickUART_st_local._payloadfooter.checkSum)
      structChecker = false;

    if (structChecker) {
      int val0 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[0], 0, 10000, -127, 127), -127, 127);
      int val1 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[1], 0, 10000, -127, 127), -127, 127);
      int val2 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[2], 0, 10000, -127, 127), -127, 127);

      Joystick.setXAxis(val0);
      Joystick.setYAxis(val1);
      Joystick.setZAxis(val2);
      Joystick.setRxAxis(val0);
      Joystick.setRyAxis(val1);
      Joystick.setRzAxis(val2);
      Joystick.sendState();
    }
  }
}
