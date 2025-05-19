#include <Joystick.h>

Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_GAMEPAD,
                   0, 0,  // buttons, hat switch
                   true, true, true,   // X, Y, Z
                   true, true, true,   // Rx, Ry, Rz
                   false, false,
                   false, false, false);



typedef struct __attribute__((packed)) payloadjoystick
{
  uint8_t payloadtype;
  int32_t controllerValue_i32[3];
  int8_t pedal_status;
  uint16_t checkSum;
} ;
typedef struct __attribute__((packed)) payloadFooter 
{
  // To check if structure is valid
  uint16_t checkSum;
};

typedef struct __attribute__((packed)) DAP_JoystickUART_State 
{
  payloadjoystick _payloadjoystick;
  payloadFooter _payloadfooter;
};


void setup() {
  Joystick.begin();
  Serial1.begin(115200);  
}
DAP_JoystickUART_State dap_joystickUART_st_local;
const uint8_t DAP_PAYLOAD_TYPE_JOYSTICKUART=230;

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

void loop() 
{
  //static uint8_t* raw = (uint8_t*)malloc(sizeof(DAP_JoystickUART_State));
  //static size_t index = 0;
  uint16_t crc;
  bool structChecker = true;
  uint16_t n = Serial.available();
  
  if (n > 0)
  {
      //Serial.print("[L]get size:");
      //Serial.println(n);
      switch (n) 
      {
        case sizeof(DAP_JoystickUART_State) :
        {
          Serial.readBytes((char*)&dap_joystickUART_st_local, sizeof(DAP_JoystickUART_State));
          if ( dap_joystickUART_st_local._payloadjoystick.payloadtype != DAP_PAYLOAD_TYPE_JOYSTICKUART)
          { 
            structChecker = false;
          }
          
          crc = checksumCalculator((uint8_t*)(&(dap_joystickUART_st_local._payloadjoystick)), sizeof(dap_joystickUART_st_local._payloadjoystick));
          if (crc != dap_joystickUART_st_local._payloadfooter.checkSum)
          { 
            structChecker = false;
          }
          if (structChecker == true)
          { 
            
            int val0 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[0], 0, 10000, 0, 1023), 0, 1023);
            int val1 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[1], 0, 10000, 0, 1023), 0, 1023);
            int val2 = constrain(map(dap_joystickUART_st_local._payloadjoystick.controllerValue_i32[2], 0, 10000, 0, 1023), 0, 1023);

            Joystick.setXAxis(val0);
            Joystick.setYAxis(val1);
            Joystick.setZAxis(val2);
            Joystick.setRxAxis(val0);
            Joystick.setRyAxis(val1);
            Joystick.setRzAxis(val2);               
          }
          break;
        }            

        
        default:
          
        break;
      }
  }
 
}
