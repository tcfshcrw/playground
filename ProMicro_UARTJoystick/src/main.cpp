#include <Joystick.h>

Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD,
                   0, 0,  // buttons, hat switch
                   true, true, true,   // X, Y, Z
                   true, true, true,   // Rx, Ry, Rz
                   false, false,
                   false, false, false);

typedef struct __attribute__((packed)) {
  uint8_t payloadtype;
  int32_t controllerValue_i32[3];
  int8_t pedal_status;
  uint16_t checkSum;
} DAP_JoystickUART_State;

void setup() {
  Joystick.begin();
  Serial1.begin(115200);  // Pro Micro 的 RX 是 pin 0
}

void loop() {
  static uint8_t* raw = (uint8_t*)malloc(sizeof(DAP_JoystickUART_State));
  static size_t index = 0;
  size_t total = sizeof(DAP_JoystickUART_State);

  while (Serial1.available()) {
    raw[index++] = Serial1.read();
    if (index >= total) {
      DAP_JoystickUART_State* msg = (DAP_JoystickUART_State*)raw;

      // 將 controllerValue_i32 映射到 0~1023（10-bit joystick 範圍）
      int val0 = constrain(map(msg->controllerValue_i32[0], -32768, 32767, 0, 1023), 0, 1023);
      int val1 = constrain(map(msg->controllerValue_i32[1], -32768, 32767, 0, 1023), 0, 1023);
      int val2 = constrain(map(msg->controllerValue_i32[2], -32768, 32767, 0, 1023), 0, 1023);

      // 映射到 X/Y/Z, Rx/Ry/Rz
      Joystick.setXAxis(val0);
      Joystick.setYAxis(val1);
      Joystick.setZAxis(val2);
      Joystick.setRxAxis(val0);
      Joystick.setRyAxis(val1);
      Joystick.setRzAxis(val2);

      index = 0;  // 重設 buffer
    }
  }
}
