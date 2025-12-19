#pragma once
#include "Arduino.h"
#include <string>
#include <DiyActivePedal_types.h>
#include "Adafruit_TinyUSB.h"
//extern Adafruit_USBD_HID usb_hid;
// HID Report Descriptor for Bridge, 6 axis
uint8_t const desc_hid_report_old[] = {
    0x05, 0x01, // Usage Page (Generic Desktop Ctrls)
    0x09, 0x04, // Usage (Joystick)
    0xA1, 0x01, // Collection (Application)

    // Define six 16-bit axes (X, Y, Z, Rx, Ry, Rz)
    0x05, 0x01,       //   Usage Page (Generic Desktop Ctrls)
    0x09, 0x30,       //   Usage (X)
    0x09, 0x31,       //   Usage (Y)
    0x09, 0x32,       //   Usage (Z)
    0x09, 0x33,       //   Usage (Rx)
    0x09, 0x34,       //   Usage (Ry)
    0x09, 0x35,       //   Usage (Rz)
    0x16, 0x00, 0x80, //   Logical Minimum (-32768)
    0x26, 0xFF, 0x7F, //   Logical Maximum (32767)
    0x75, 0x10,       //   Report Size (16)
    0x95, 0x06,       //   Report Count (6) 
    0x81, 0x02,       //   Input (Data,Var,Abs)

    0xC0, // End Collection
};
#define HID_LOGICAL_MAX_32(n) 0x27, (uint8_t)((n) & 0xFF), (uint8_t)(((n) >> 8) & 0xFF), (uint8_t)(((n) >> 16) & 0xFF), (uint8_t)(((n) >> 24) & 0xFF)


uint8_t const desc_hid_report[] = {
    HID_USAGE_PAGE ( HID_USAGE_PAGE_DESKTOP     ),
    HID_USAGE      ( HID_USAGE_DESKTOP_JOYSTICK ),
    HID_COLLECTION ( HID_COLLECTION_APPLICATION ),
        HID_REPORT_ID( 3 ) // Report ID 3
        
        HID_USAGE_PAGE ( HID_USAGE_PAGE_DESKTOP ),
        HID_USAGE      ( HID_USAGE_DESKTOP_POINTER ),
        HID_LOGICAL_MIN( 0 ),
        HID_LOGICAL_MAX_32( 65535 ), 
        HID_REPORT_SIZE( 16 ),
        HID_REPORT_COUNT( 6 ),
        
        HID_COLLECTION ( HID_COLLECTION_PHYSICAL ),
            HID_USAGE ( HID_USAGE_DESKTOP_X  ),
            HID_USAGE ( HID_USAGE_DESKTOP_Y  ),
            HID_USAGE ( HID_USAGE_DESKTOP_Z  ),
            HID_USAGE ( HID_USAGE_DESKTOP_RX ),
            HID_USAGE ( HID_USAGE_DESKTOP_RY ),
            HID_USAGE ( HID_USAGE_DESKTOP_RZ ),
            HID_INPUT ( HID_DATA | HID_VARIABLE | HID_ABSOLUTE ),
        HID_COLLECTION_END,
        
    HID_COLLECTION_END,
    HID_USAGE_PAGE_N ( HID_USAGE_PAGE_VENDOR, 2 ),
    HID_USAGE        ( 0x01 ),
    HID_COLLECTION   ( HID_COLLECTION_APPLICATION ),
        HID_REPORT_ID(2)
        HID_USAGE(0x10),
        HID_REPORT_SIZE(8),
        HID_REPORT_COUNT(sizeof(DAP_actions_st)),
        HID_OUTPUT(HID_DATA | HID_VARIABLE | HID_ABSOLUTE),
        HID_REPORT_ID(4)
        HID_USAGE(0x30),
        HID_REPORT_SIZE(8),
        HID_REPORT_COUNT(sizeof(DAP_config_st)),
        HID_FEATURE(HID_DATA | HID_VARIABLE | HID_ABSOLUTE),
    HID_COLLECTION_END,
};

class TinyusbJoystick {
private:
    static const uint16_t JOYSTICK_MIN_VALUE = INT16_MIN;
    static const uint16_t JOYSTICK_MAX_VALUE = INT16_MAX;
    static const uint16_t JOYSTICK_RANGE = JOYSTICK_MAX_VALUE - JOYSTICK_MIN_VALUE;
    // USB HID object
    // Report payload for the two axes
    typedef struct
    {
        int16_t x;
        int16_t y;
        int16_t z;
        int16_t rx;
        int16_t ry;
        int16_t rz;
    } hid_report_t;
    Adafruit_USBD_HID usb_hid;
    hid_report_t hid_report = {0};

public:
  TinyusbJoystick();
  bool IsReady();
  void begin();
  void setXAxis(int16_t value);
  void setYAxis(int16_t value);
  void setZAxis(int16_t value);
  void setRxAxis(int16_t value);
  void setRyAxis(int16_t value);
  void setRzAxis(int16_t value);
  void sendState();
};