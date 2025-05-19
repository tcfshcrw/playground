#include "Joystick_RP2040.h"

static uint8_t hid_report_descriptor[] = {
  TUD_HID_REPORT_DESC_GAMEPAD(HID_REPORT_ID(1))
};

Joystick_RP2040::Joystick_RP2040(uint8_t report_id)
  : _report_id(report_id), _x(0), _y(0), _z(0), _rx(0), _ry(0), _rz(0) {
}

void Joystick_RP2040::begin() {
  _hid.setReportDescriptor(hid_report_descriptor, sizeof(hid_report_descriptor));
  _hid.begin();
  TinyUSBDevice.begin();
}

void Joystick_RP2040::setXAxis(int8_t value) { _x = value; }
void Joystick_RP2040::setYAxis(int8_t value) { _y = value; }
void Joystick_RP2040::setZAxis(int8_t value) { _z = value; }
void Joystick_RP2040::setRxAxis(int8_t value) { _rx = value; }
void Joystick_RP2040::setRyAxis(int8_t value) { _ry = value; }
void Joystick_RP2040::setRzAxis(int8_t value) { _rz = value; }

void Joystick_RP2040::sendState() {
  uint8_t report[] = {
    _report_id,
    (uint8_t)_x, (uint8_t)_y, (uint8_t)_z,
    (uint8_t)_rx, (uint8_t)_ry, (uint8_t)_rz
  };
  _hid.sendReport(_report_id, report, sizeof(report));
}
