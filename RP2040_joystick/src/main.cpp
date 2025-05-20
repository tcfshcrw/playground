#include <Joystick.h>
#include "main.h"
#define baud 921600
#define handshakeGPIO 7
DAP_JoystickUART_State dap_joystickUART_st_local;
unsigned long runtime_last=0;
unsigned long debug_message_last=0;
uint16_t checksumCalculator(uint8_t * data, uint16_t length)
{
   uint16_t curr_crc = 0x0000;
   uint8_t sum1 = (uint8_t) curr_crc;
   uint8_t sum2 = (uint8_t) (curr_crc >> 8);
   int index;
   for(index = 0; index < length; index = index+1)
   {
      sum1 = (sum1 + data[index]) % 255;
      sum2 = (sum2 + sum1) % 255;
   }
   return (sum2 << 8) | sum1;
}
void restart_UART()
{
  Serial2.end();
  delay(3000);
  Serial2.begin(baud);
}

typedef struct {
  uint8_t  payloadType;
  uint8_t    sensorValue;
} MyPacket;

void setup() 
{
  delay(10000);
  Serial.begin(115200);
  //Serial1.begin(115200);
  Serial2.begin(baud);
  //Serial.println("Use BOOTSEL to start the Joystick demo.");
  Joystick.begin();
  Joystick.useManualSend(true);
  Joystick.X(0);
  Joystick.Y(0);
  Joystick.Z(0);
  Joystick.sliderLeft(0);
  Joystick.sliderRight(0);
  Joystick.Zrotate(0);
  Serial.println("starting GPIO");
  pinMode(handshakeGPIO,OUTPUT);
  delay(10);
  digitalWrite(handshakeGPIO, HIGH);
  Serial.println("GPIO setting finished");
  
}

unsigned long temp=0;
unsigned long UART_last=0;
int uart_index=0;
uint8_t uart_buffer[sizeof(DAP_JoystickUART_State)];

void loop() 
{
  /*
  temp= millis()-runtime_last;
  runtime_last=millis();
  if(millis()-debug_message_last>500)
  {
    Serial.print("runtine interval:");
    Serial.println(temp);
    debug_message_last=millis();
  }
  */
  while (Serial2.available()) 
  {
    uart_buffer[uart_index++] = Serial2.read();
    if (uart_index >= sizeof(DAP_JoystickUART_State)) 
    {
      memcpy(&dap_joystickUART_st_local, uart_buffer, sizeof(DAP_JoystickUART_State));
      uart_index = 0;

      // 檢查封包有效性
      if (dap_joystickUART_st_local._payloadjoystick.payloadtype == DAP_PAYLOAD_TYPE_JOYSTICKUART) 
      {
        UART_last = millis();
        uint16_t crc = checksumCalculator((uint8_t*)&(dap_joystickUART_st_local._payloadjoystick),
                                          sizeof(dap_joystickUART_st_local._payloadjoystick));
        if (crc == dap_joystickUART_st_local._payloadfooter.checkSum) {
          int valX = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[0], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
          int valY = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[1], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
          int valZ = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[2], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);

          Joystick.X(valX);
          Joystick.Y(valY);
          Joystick.Z(valZ);
          Joystick.send_now(); // 只在有效封包後送出
        } else {
          Serial.println("CRC Error");
        }
      } else {
        Serial.println("Type Error");
      }
    }
  }
      if (millis() - UART_last > 10000) 
      {
        UART_last = millis();
        Serial.println("UART timeout");
        restart_UART();
      }
  // Timeout 處理

}

void main_code() 
{
  /*
  temp= millis()-runtime_last;
  runtime_last=millis();
  if(millis()-debug_message_last>500)
  {
    Serial.print("runtine interval:");
    Serial.println(temp);
    debug_message_last=millis();
  }
  */
  


  Joystick.send_now();
  uint16_t crc;
  bool structChecker = true;
  uint16_t n = Serial2.available();
  if (n >= sizeof(DAP_JoystickUART_State))
  {
    UART_last=millis();
    //Serial.println(Serial2.read());
    switch (n) 
    {
      /*
      case sizeof(MyPacket):
      {
        MyPacket packet;
        Serial2.readBytes((uint8_t*)&packet, sizeof(packet));
        Serial.printf("Received: type=%d,  val=%d\n",
        packet.payloadType,  packet.sensorValue);
        break;
      }
      */
      case sizeof(DAP_JoystickUART_State) :
      {
        //Serial.println("get package");
        //Serial.println(Serial2.read());
        Serial2.readBytes((uint8_t*)&dap_joystickUART_st_local, sizeof(DAP_JoystickUART_State));
        if ( dap_joystickUART_st_local._payloadjoystick.payloadtype != DAP_PAYLOAD_TYPE_JOYSTICKUART )
        { 
          structChecker = false;
          Serial.println("Type check error");
        }
          
        crc = checksumCalculator((uint8_t*)(&(dap_joystickUART_st_local._payloadjoystick)), sizeof(dap_joystickUART_st_local._payloadjoystick));
        if (crc != dap_joystickUART_st_local._payloadfooter.checkSum)
        { 
          structChecker = false;
          Serial.println("crc check error");
        }
        if (structChecker == true)
        { 
            int val0 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[0], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
            int val1 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[1], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
            int val2 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[2], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
            for(int i=0; i<3; i++)
            {
              /*
              Serial.print("controller value[");
              Serial.print(i);
              Serial.print("]:");
              Serial.println(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[i]);
              */
            }
            Joystick.X(val0);
            Joystick.Y(val1);
            Joystick.Z(val2);
            Joystick.send_now();

        }
        
        break;
        //Serial2.read(); 
        
      }            
      default:  
        Serial2.read();
      break;
      
      
    }
      
  }
  else
  {
    if(millis()-UART_last>10000)
    {
      Serial.println("UART timeout, Restart UART");
      UART_last=millis();
      restart_UART();
    }
  }

}


