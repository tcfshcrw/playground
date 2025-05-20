#include <Joystick.h>
#include "main.h"
#include "pico/bootrom.h"
#define baud 921600
#define handshakeGPIO 7
#define LED_PIN 25
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
  digitalWrite(handshakeGPIO,LOW);
  delay(3000);
  Serial2.begin(baud);
  digitalWrite(handshakeGPIO, HIGH);
}


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
  Serial.println("starting GPIO initializing");
  pinMode(handshakeGPIO,OUTPUT);
  delay(10);
  pinMode(LED_PIN, OUTPUT);
  delay(10);
  digitalWrite(handshakeGPIO, HIGH);
  delay(100);
  digitalWrite(LED_PIN, HIGH);
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
    uint8_t incoming = Serial2.read();

    
    if (uart_index == 0 && incoming != DAP_PAYLOAD_TYPE_JOYSTICKUART)
      continue;
    if (uart_index == 1 && incoming != 0x99)
    {
      uart_index = 0;
      continue;
    }
    uart_buffer[uart_index++] = incoming;

    
    if (uart_index >= sizeof(DAP_JoystickUART_State))
    {
      
      /*
      Serial.print("Raw: ");
      for (int i = 0; i < uart_index; i++)
      {
        Serial.print(uart_buffer[i],HEX);
        Serial.print(" ");
      }
      Serial.println();
      */
      
      memcpy(&dap_joystickUART_st_local, uart_buffer, sizeof(DAP_JoystickUART_State));
      uart_index = 0; 


      if (dap_joystickUART_st_local._payloadjoystick.payloadtype == DAP_PAYLOAD_TYPE_JOYSTICKUART)
      {
        // CRC 檢查
        uint16_t crc = checksumCalculator((uint8_t *)&(dap_joystickUART_st_local._payloadjoystick),sizeof(dap_joystickUART_st_local._payloadjoystick));

        if (crc == dap_joystickUART_st_local._payloadfooter.checkSum)
        {
          int valX = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[0], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
          int valY = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[1], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);
          int valZ = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[2], 0, JOYSTICK_VALUE_MAX, 0, 1023), 0, 1023);

          // 寫入 Joystick
          if (dap_joystickUART_st_local._payloadjoystick.pedal_status == Pedal_status_Pedal)
          {
            Joystick.Zrotate(valX);     // clutch
            Joystick.sliderLeft(valY);  // brake
            Joystick.sliderRight(valZ); // throttle
          }
          else if (dap_joystickUART_st_local._payloadjoystick.pedal_status == Pedal_status_Rudder)
          {
            // TODO: Rudder 模式處理
          }
          else if (dap_joystickUART_st_local._payloadjoystick.pedal_status == Pedal_status_RudderBrake)
          {
            // TODO: Rudder+Brake 處理
          }

          Joystick.send_now();
          UART_last = millis(); // 成功時更新 UART 時間
        }
        else
        {
          Serial.println("CRC Error");
          uart_index = 0; // 發生錯誤 → 清空 buffer 對齊
        }
      }
      else
      {
        Serial.print("Payload Type Error, Get:");
        Serial.print(dap_joystickUART_st_local._payloadjoystick.payloadtype);
        Serial.print(", Expect:");
        Serial.println(DAP_PAYLOAD_TYPE_JOYSTICKUART);
        uart_index = 0; // 發生錯誤 → 清空 buffer 對齊
      }
    }
  }

  // UART timeout 檢查
  if (millis() - UART_last > 5000)
  {
    UART_last = millis();
    Serial.println("UART timeout");
    restart_UART();
  }

  // bootloader mode
  if (BOOTSEL)
  {
    reset_usb_boot(0, 0);
  }
}
