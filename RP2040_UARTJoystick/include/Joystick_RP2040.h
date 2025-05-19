#pragma once
#include <Adafruit_TinyUSB.h>

class Joystick_RP2040 {
public:
  Joystick_RP2040(uint8_t report_id = 1);
  void begin();
  void setXAxis(int8_t value);
  void setYAxis(int8_t value);
  void setZAxis(int8_t value);
  void setRxAxis(int8_t value);
  void setRyAxis(int8_t value);
  void setRzAxis(int8_t value);
  void sendState();

private:
  uint8_t _report_id;
  int8_t _x, _y, _z, _rx, _ry, _rz;
  Adafruit_USBD_HID _hid;
};
