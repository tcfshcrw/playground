#include <Arduino.h>

HardwareSerial MySerial(1);  // UART1

typedef struct {
  uint8_t  payloadType;
  uint8_t    sensorValue;
} MyPacket;

void setup() {
  Serial.begin(115200); // USB log
  MySerial.begin(115200, SERIAL_8N1, 16, 15); 
  Serial.println("ESP32-S3 ready to send");
}
uint8_t sensor=0;
void loop() {
  MyPacket packet;
  packet.payloadType = 0x01;
  packet.sensorValue = sensor;

  MySerial.write((uint8_t*)&packet, sizeof(packet));

  Serial.printf("Sent: type=%d,  val=%d\n",
                packet.payloadType,  packet.sensorValue);
  sensor++;
  if(sensor>255)
  {
    sensor=0;
  }
  delay(1000);
}
















