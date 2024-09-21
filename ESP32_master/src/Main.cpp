
/* Todo*/
// https://github.com/espressif/arduino-esp32/issues/7779

#define ESTIMATE_LOADCELL_VARIANCE
#define ISV_COMMUNICATION
//#define PRINT_SERVO_STATES

#define DEBUG_INFO_0_CYCLE_TIMER 1
#define DEBUG_INFO_0_STEPPER_POS 2
#define DEBUG_INFO_0_LOADCELL_READING 4
#define DEBUG_INFO_0_SERVO_READINGS 8
#define DEBUG_INFO_0_PRINT_ALL_SERVO_REGISTERS 16
#define DEBUG_INFO_0_STATE_BASIC_INFO_STRUCT 32
#define DEBUG_INFO_0_STATE_EXTENDED_INFO_STRUCT 64
#define DEBUG_INFO_0_CONTROL_LOOP_ALGO 128



#define PI 3.14159267
#define DEG_TO_RAD PI / 180

#include "Arduino.h"
#include "Main.h"




//#define ALLOW_SYSTEM_IDENTIFICATION

/**********************************************************************************************/
/*                                                                                            */
/*                         function declarations                                              */
/*                                                                                            */
/**********************************************************************************************/


//void serialCommunicationTask( void * pvParameters );

//void OTATask( void * pvParameters );

//void ESPNOW_SyncTask( void * pvParameters);
// https://www.tutorialspoint.com/cyclic-redundancy-check-crc-in-arduino
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








#include <EEPROM.h>







#include "DiyActivePedal_types.h"
DAP_config_st dap_config_st;
DAP_calculationVariables_st dap_calculationVariables_st;
DAP_state_basic_st dap_state_basic_st;
DAP_state_extended_st dap_state_extended_st;
DAP_actions_st dap_actions_st;
DAP_bridge_state_st dap_bridge_state_st;
DAP_config_st dap_config_st_Clu;
DAP_config_st dap_config_st_Brk;
DAP_config_st dap_config_st_Gas;
DAP_config_st dap_config_st_Temp;
DAP_ESPPairing_st dap_esppairing_st;//saving
DAP_ESPPairing_st dap_esppairing_lcl;//sending
//DAP_config_st dap_config_st_store[3];
#include "CycleTimer.h"


#include "RTDebugOutput.h"
#include "Wire.h"
#include "SPI.h"
#include <EEPROM.h>
#define EEPROM_offset 15
/**********************************************************************************************/
/*                                                                                            */
/*                         iterpolation  definitions                                          */
/*                                                                                            */
/**********************************************************************************************/





/**********************************************************************************************/
/*                                                                                            */
/*                         multitasking  definitions                                          */
/*                                                                                            */
/**********************************************************************************************/
#ifndef CONFIG_IDF_TARGET_ESP32S3
  #include "soc/rtc_wdt.h"
#endif
#ifdef LED_ENABLE_WAVESHARE
  #include "soc/soc_caps.h"
  #include <Adafruit_NeoPixel.h>
  #define LEDS_COUNT 1
  #define CHANNEL 0
  #define LED_BRIGHT 30
  /*
  static const crgb_t L_RED = 0xff0000;
  static const crgb_t L_GREEN = 0x00ff00;
  static const crgb_t L_BLUE = 0x0000ff;
  static const crgb_t L_WHITE = 0xe0e0e0;
  static const crgb_t L_YELLOW = 0xffde21;
  static const crgb_t L_ORANGE = 0xffa500;
  static const crgb_t L_CYAN = 0x00ffff;
  static const crgb_t L_PURPLE = 0x800080;
  */
  Adafruit_NeoPixel pixels(LEDS_COUNT, LED_GPIO, NEO_RGB + NEO_KHZ800);

#endif

//#define PRINT_USED_STACK_SIZE
// https://stackoverflow.com/questions/55998078/freertos-task-priority-and-stack-size
#define STACK_SIZE_FOR_TASK_1 0.2 * (configTOTAL_HEAP_SIZE / 4)
#define STACK_SIZE_FOR_TASK_2 0.2 * (configTOTAL_HEAP_SIZE / 4)


TaskHandle_t Task1;
TaskHandle_t Task2;
TaskHandle_t Task3;
TaskHandle_t Task4;
//static SemaphoreHandle_t semaphore_updateConfig=NULL;
  bool configUpdateAvailable = false;                              // semaphore protected data
  DAP_config_st dap_config_st_local;

//static SemaphoreHandle_t semaphore_updateJoystick=NULL;
  int32_t joystickNormalizedToInt32 = 0;                           // semaphore protected data

//static SemaphoreHandle_t semaphore_resetServoPos=NULL;
bool resetPedalPosition = false;

//static SemaphoreHandle_t semaphore_readServoValues=NULL;

//static SemaphoreHandle_t semaphore_updatePedalStates=NULL;

/**********************************************************************************************/
/*                                                                                            */
/*                         controller  definitions                                            */
/*                                                                                            */
/**********************************************************************************************/

#include "Controller.h"


/**********************************************************************************************/
/*                                                                                            */
/*                         pedal mechanics definitions                                        */
/*                                                                                            */
/**********************************************************************************************/

#include "PedalGeometry.h"

/**********************************************************************************************/
/*                                                                                            */
/*                         OTA                                                                */
/*                                                                                            */
/**********************************************************************************************/
//OTA update
#ifdef OTA_update
#include "ota.h"
TaskHandle_t Task4;
#endif

#ifdef Using_MCP4728
  #include <Adafruit_MCP4728.h>
  Adafruit_MCP4728 mcp;
  TwoWire MCP4728_I2C= TwoWire(1);
  bool MCP_status =false;
#endif


//ESPNOW
#ifdef ESPNOW_Enable
  #include "ESPNOW_lib.h"
  TaskHandle_t Task6;
#endif

bool dap_action_update= false;
#include "MovingAverageFilter.h"
MovingAverageFilter rssi_filter(30);
int32_t joystickNormalizedToInt32_local = 0;
unsigned long pedal_last_update[3]={1,1,1};
uint8_t pedal_avaliable[3]={0,0,0};
void ESPNOW_SyncTask( void * pvParameters);
void Joystick_Task( void * pvParameters);
void LED_Task( void * pvParameters);
void Serial_Task( void * pvParameters);
/**********************************************************************************************/
/*                                                                                            */
/*                         setup function                                                     */
/*                                                                                            */
/**********************************************************************************************/

void setup()
{
  //Serial.begin(115200);
  //Serial.begin(921600);
  //Serial.begin(512000);
  //
  

  #if PCB_VERSION == 6
    //Serial.setTxTimeoutMs(0);
    Serial.setRxBufferSize(1024);
    Serial.setTimeout(5);
    Serial.begin(3000000);
    
    //Serial0.begin(921600);
    //Serial0.setDebugOutput(false);
    //esp_log_level_set("*",ESP_LOG_INFO);
  #else
    Serial.setRxBufferSize(1024);
    Serial.begin(921600);
    Serial.setTimeout(5);
    
  #endif
  #ifdef USB_JOYSTICK
	SetupController();
  #endif
  Serial.println(" ");
  Serial.println(" ");
  Serial.println(" ");
  
  Serial.println("This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.");
  Serial.println("Please check github repo for more detail: https://github.com/ChrGri/DIY-Sim-Racing-FFB-Pedal");


  // setup multi tasking
  /*
  semaphore_updateJoystick = xSemaphoreCreateMutex();
  semaphore_updateConfig = xSemaphoreCreateMutex();
  semaphore_resetServoPos = xSemaphoreCreateMutex();
  semaphore_updatePedalStates = xSemaphoreCreateMutex();
  */
  delay(10);
  //button read setup
  pinMode(Pairing_GPIO, INPUT_PULLUP);
  EEPROM.begin(256);
/*
  if(semaphore_updateJoystick==NULL)
  {
    Serial.println("Could not create semaphore");
    ESP.restart();
  }
  if(semaphore_updateConfig==NULL)
  {
    Serial.println("Could not create semaphore");
    ESP.restart();
  }
  */

  disableCore0WDT();
  //enable ESP-NOW
  ESPNow_initialize();
  //ESPNow multi-tasking    
  xTaskCreatePinnedToCore(
                        ESPNOW_SyncTask,   
                        "ESPNOW_update_Task", 
                        10000,  
                        //STACK_SIZE_FOR_TASK_2,    
                        NULL,      
                        1,         
                        &Task1,    
                        0);     
  delay(500);
  //Serial multitasking
  xTaskCreatePinnedToCore(
                        Serial_Task,   
                        "Serial_update_Task", 
                        5000,  
                        //STACK_SIZE_FOR_TASK_2,    
                        NULL,      
                        1,         
                        &Task4,    
                        1);     
  delay(500);

  xTaskCreatePinnedToCore(
                        Joystick_Task,   
                        "Joystick_update_Task", 
                        10000,  
                        //STACK_SIZE_FOR_TASK_2,    
                        NULL,      
                        1,         
                        &Task2,    
                        1);     
  delay(500);


  #ifdef Using_MCP4728
    MCP4728_I2C.begin(MCP_SDA,MCP_SCL,400000);
    uint8_t i2c_address[8]={0x60,0x61,0x62,0x63,0x64,0x65,0x66,0x67};
    int index_address=0;
    int found_address=0;
    int error;
    for(index_address=0;index_address<8;index_address++)
    {
      MCP4728_I2C.beginTransmission(i2c_address[index_address]);
      error = MCP4728_I2C.endTransmission();
      if (error == 0)
      {
        Serial.print("I2C device found at address");
        Serial.print(i2c_address[index_address]);
        Serial.println("  !");
        found_address=index_address;
        break;
        
      }
      else
      {
        Serial.print("try address");
        Serial.println(i2c_address[index_address]);
      }
    }
    
    if(mcp.begin(i2c_address[found_address], &MCP4728_I2C)==false)
    {
      Serial.println("Couldn't find MCP4728, will not have analog output");
      MCP_status=false;
    }
    else
    {
      Serial.println("MCP4728 founded");
      MCP_status=true;
      //MCP.begin();
    }
    
  #endif
        
  //initialize time_record
  uint8_t pedalIDX;
  for(pedalIDX=0;pedalIDX<3;pedalIDX++)
  {
    pedal_last_update[pedalIDX]=millis();
  }
  #ifdef LED_ENABLE_WAVESHARE
    pixels.begin();
    pixels.setBrightness(20);
    pixels.setPixelColor(0,0xff,0xff,0xff);
    pixels.show();
    xTaskCreatePinnedToCore(
                          LED_Task,   
                          "LED_update_Task", 
                          3000,  
                          //STACK_SIZE_FOR_TASK_2,    
                          NULL,      
                          1,         
                          &Task3,    
                          0);     
    delay(500);
  #endif

  Serial.println("Setup end");
  
}








/**********************************************************************************************/
/*                                                                                            */
/*                         Main function                                                      */
/*                                                                                            */
/**********************************************************************************************/
uint32_t loop_count=0;
bool basic_rssi_update=false;
unsigned long bridge_state_last_update=millis();
unsigned long Pairing_state_start;
unsigned long Pairing_state_last_sending;
uint8_t press_count=0;
uint Pairing_timeout=20000;
bool Pairing_timeout_status=false;
bool building_dap_esppairing_lcl =false;
void loop() 
{
  taskYIELD();
  //delay(2); 
}

void ESPNOW_SyncTask( void * pvParameters )
{
  for(;;)
  {
    #ifdef ESPNow_Pairing_function
      if(digitalRead(Pairing_GPIO)==LOW)
      {
        Serial.println("Bridge Pairing.....");
        delay(1000);
        Pairing_state_start=millis();
        Pairing_state_last_sending=millis();
        ESPNow_pairing_action_b=true;
        building_dap_esppairing_lcl=true;
        
      }
      if(ESPNow_pairing_action_b)
      {
        unsigned long now=millis();
        //sending package
        if(building_dap_esppairing_lcl)
        {
          uint16_t crc=0;          
          building_dap_esppairing_lcl=false;
          dap_esppairing_lcl.payloadESPNowInfo_._deviceID=deviceID;
          dap_esppairing_lcl.payLoadHeader_.payloadType=DAP_PAYLOAD_TYPE_ESPNOW_PAIRING;
          dap_esppairing_lcl.payLoadHeader_.PedalTag=deviceID;
          dap_esppairing_lcl.payLoadHeader_.version=DAP_VERSION_CONFIG;
          crc = checksumCalculator((uint8_t*)(&(dap_esppairing_lcl.payLoadHeader_)), sizeof(dap_esppairing_lcl.payLoadHeader_) + sizeof(dap_esppairing_lcl.payloadESPNowInfo_));
          dap_esppairing_lcl.payloadFooter_.checkSum=crc;
        }
        if(now-Pairing_state_last_sending>400)
        {
          Pairing_state_last_sending=now;
          ESPNow.send_message(broadcast_mac,(uint8_t *) &dap_esppairing_lcl, sizeof(dap_esppairing_lcl));
        }
        

        //timeout check
        if(now-Pairing_state_start>Pairing_timeout)
        {
          Serial.println("Reach timeout");
          ESPNow_pairing_action_b=false;
          if(UpdatePairingToEeprom)
          {
            EEPROM.put(EEPROM_offset,_ESP_pairing_reg);
            EEPROM.commit();
            UpdatePairingToEeprom=false;
            
            //list eeprom
            ESP_pairing_reg ESP_pairing_reg_local;
            EEPROM.get(EEPROM_offset, ESP_pairing_reg_local);
            for(int i=0;i<4;i++)
            {
              if(ESP_pairing_reg_local.Pair_status[i]==1)
              {                
                Serial.print("#");
                Serial.print(i);
                Serial.print("Pair: ");
                Serial.print(ESP_pairing_reg_local.Pair_status[i]);
                Serial.printf(" Mac: %02X:%02X:%02X:%02X:%02X:%02X\n", ESP_pairing_reg_local.Pair_mac[i][0], ESP_pairing_reg_local.Pair_mac[i][1], ESP_pairing_reg_local.Pair_mac[i][2], ESP_pairing_reg_local.Pair_mac[i][3], ESP_pairing_reg_local.Pair_mac[i][4], ESP_pairing_reg_local.Pair_mac[i][5]);
              }
            }
            //adding peer
            
            for(int i=0; i<4;i++)
            {
              if(_ESP_pairing_reg.Pair_status[i]==1)
              {
                if(i==0)
                {
                  ESPNow.remove_peer(Clu_mac);
                  memcpy(&Clu_mac,&_ESP_pairing_reg.Pair_mac[i],6);
                  delay(300);
                  ESPNow.add_peer(Clu_mac);
                  
                }
                if(i==1)
                {
                  ESPNow.remove_peer(Brk_mac);
                  memcpy(&Brk_mac,&_ESP_pairing_reg.Pair_mac[i],6);
                  delay(300);
                  ESPNow.add_peer(Brk_mac);
                  Serial.printf(" Mac: %02X:%02X:%02X:%02X:%02X:%02X\n", Brk_mac[0], Brk_mac[1], Brk_mac[2], Brk_mac[3], Brk_mac[4], Brk_mac[5]);

                }
                if(i==2)
                {
                  ESPNow.remove_peer(Gas_mac);
                  memcpy(&Gas_mac,&_ESP_pairing_reg.Pair_mac[i],6);
                  delay(300);
                  ESPNow.add_peer(Gas_mac);
                }        
                if(i==3)
                {
                  ESPNow.remove_peer(esp_Host);
                  memcpy(&esp_Host,&_ESP_pairing_reg.Pair_mac[i],6);
                  delay(300);
                  ESPNow.add_peer(esp_Host);                
                }        
              }
            }
          }
        }
      }
    #endif
    if(configUpdateAvailable)
    {
      if(dap_config_st.payLoadHeader_.PedalTag==0)
      {
        if(dap_bridge_state_st.payloadBridgeState_.Pedal_availability[0]==1)
        {
          ESPNow.send_message(Clu_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
          Serial.println("Clutch config sent");
          configUpdateAvailable=false;
        }
      }
      if(dap_config_st.payLoadHeader_.PedalTag==1)
      {
        if(dap_bridge_state_st.payloadBridgeState_.Pedal_availability[1]==1)
        {
          ESPNow.send_message(Brk_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
          Serial.println("BRK config sent");
          configUpdateAvailable=false;
        }

      }
      if(dap_config_st.payLoadHeader_.PedalTag==2)
      {
        if(dap_bridge_state_st.payloadBridgeState_.Pedal_availability[2]==1)
        {
          ESPNow.send_message(Gas_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
          Serial.println("Throttle config sent");
          configUpdateAvailable=false;
        }

      }

    }


    if(dap_action_update)
    {
      
      if(dap_actions_st.payLoadHeader_.PedalTag==0 && dap_bridge_state_st.payloadBridgeState_.Pedal_availability[0]==1)
      {
        ESPNow.send_message(Clu_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
        //Serial.println("BRK sent");
      }
      if(dap_actions_st.payLoadHeader_.PedalTag==1 && dap_bridge_state_st.payloadBridgeState_.Pedal_availability[1]==1)
      {
        ESPNow.send_message(Brk_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
        //Serial.println("BRK sent");
      }
                  
      if(dap_actions_st.payLoadHeader_.PedalTag==2 && dap_bridge_state_st.payloadBridgeState_.Pedal_availability[2]==1)
      {
        ESPNow.send_message(Gas_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
        //Serial.println("GAS sent");
      }
      
      //ESPNow.send_message(broadcast_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
      //Serial.println("Broadcast sent");
      dap_action_update=false;
    }

    delay(2);
  }
}

void Serial_Task( void * pvParameters)
{
  for(;;)
  {  
    uint16_t crc;
    uint16_t n = Serial.available();
    unsigned long current_time=millis();
    if(current_time-bridge_state_last_update>200)
    {
      basic_rssi_update=true;
      bridge_state_last_update=millis();
    }

    bool structChecker = true;
    if (n > 0)
    {
      switch (n) 
      {
        case sizeof(DAP_actions_st) :            
          Serial.readBytes((char*)&dap_actions_st, sizeof(DAP_actions_st));
          

          if ( dap_actions_st.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_ACTION )
          { 
            structChecker = false;
            Serial.print("Payload type expected: ");
            Serial.print(DAP_PAYLOAD_TYPE_ACTION);
            Serial.print(",   Payload type received: ");
            Serial.println(dap_config_st_local.payLoadHeader_.payloadType);
          }
          if ( dap_actions_st.payLoadHeader_.version != DAP_VERSION_CONFIG )
          { 
            structChecker = false;
            Serial.print("Config version expected: ");
            Serial.print(DAP_VERSION_CONFIG);
            Serial.print(",   Config version received: ");
            Serial.println(dap_config_st_local.payLoadHeader_.version);
          }
          crc = checksumCalculator((uint8_t*)(&(dap_actions_st.payLoadHeader_)), sizeof(dap_actions_st.payLoadHeader_) + sizeof(dap_actions_st.payloadPedalAction_));
          if (crc != dap_actions_st.payloadFooter_.checkSum)
          { 
            structChecker = false;
            Serial.print("CRC expected: ");
            Serial.print(crc);
            Serial.print(",   CRC received: ");
            Serial.println(dap_actions_st.payloadFooter_.checkSum);
          }
          if (structChecker == true)
          {
            dap_action_update=true;                
          }
          break;

        case sizeof(DAP_config_st):
                
          DAP_config_st * dap_config_st_local_ptr;
          dap_config_st_local_ptr = &dap_config_st;
          Serial.readBytes((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));        
          // check if data is plausible          
          if ( dap_config_st.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_CONFIG )
          { 
            structChecker = false;
            Serial.print("Payload type expected: ");
            Serial.print(DAP_PAYLOAD_TYPE_CONFIG);
            Serial.print(",   Payload type received: ");
            Serial.println(dap_config_st_local.payLoadHeader_.payloadType);
          }
          if ( dap_config_st.payLoadHeader_.version != DAP_VERSION_CONFIG )
          { 
            structChecker = false;
            Serial.print("Config version expected: ");
            Serial.print(DAP_VERSION_CONFIG);
            Serial.print(",   Config version received: ");
            Serial.println(dap_config_st_local.payLoadHeader_.version);
          }
              // checksum validation
          crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
          if (crc != dap_config_st.payloadFooter_.checkSum)
          { 
            structChecker = false;
            Serial.print("CRC expected: ");
            Serial.print(crc);
            Serial.print(",   CRC received: ");
            Serial.println(dap_config_st.payloadFooter_.checkSum);
          }


              // if checks are successfull, overwrite global configuration struct
              if (structChecker == true)
              {
                //Serial.println("Updating pedal config");
                configUpdateAvailable = true;     
                //memcpy(&dap_config_st, &dap_config_st_local, sizeof(dap_config_st));     
              }
              

          break;

        default:
        // flush the input buffer
          while (Serial.available()) 
            Serial.read();
            //Serial.flush();
            Serial.println("\nIn byte size: ");
            Serial.println(n);
            Serial.println("    Exp config size: ");
            Serial.println(sizeof(DAP_config_st) );
            Serial.println("    Exp action size: ");
            Serial.println(sizeof(DAP_actions_st) );

          break;          
      }
    }



    if(update_basic_state)
    {
      update_basic_state=false;
      Serial.write((char*)&dap_state_basic_st, sizeof(DAP_state_basic_st));
      Serial.print("\r\n");
      if(dap_bridge_state_st.payloadBridgeState_.Pedal_availability[dap_state_basic_st.payLoadHeader_.PedalTag]==0)
      {
        Serial.print("Found Pedal:");
        Serial.println(dap_state_basic_st.payLoadHeader_.PedalTag);
      }
      dap_bridge_state_st.payloadBridgeState_.Pedal_availability[dap_state_basic_st.payLoadHeader_.PedalTag]=1;
      pedal_last_update[dap_state_basic_st.payLoadHeader_.PedalTag]=millis();

    }
    if(update_extend_state)
    {
      update_extend_state=false;
      Serial.write((char*)&dap_state_extended_st, sizeof(dap_state_extended_st));
      Serial.print("\r\n");

    }
    int pedal_config_IDX=0;
    for(pedal_config_IDX=0;pedal_config_IDX<3;pedal_config_IDX++)
    {
      if(ESPNow_request_config_b[pedal_config_IDX])
      {
        DAP_config_st * dap_config_st_local_ptr;
        DAP_config_st dap_config_st_local;
        if(pedal_config_IDX==0)
        {
          memcpy(&dap_config_st_local, &dap_config_st_Clu, sizeof(DAP_config_st));
          //dap_config_st_local_ptr = &dap_config_st_Clu;
        }
        if(pedal_config_IDX==1)
        {
          memcpy(&dap_config_st_local, &dap_config_st_Brk, sizeof(DAP_config_st));
          //dap_config_st_local_ptr = &dap_config_st_Brk;
        }
        if(pedal_config_IDX==2)
        {
          memcpy(&dap_config_st_local, &dap_config_st_Gas, sizeof(DAP_config_st));
          //dap_config_st_local_ptr = &dap_config_st_Gas;
        }
        dap_config_st_local_ptr= &dap_config_st_local;
        
        //uint16_t crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
        crc = checksumCalculator((uint8_t*)(&(dap_config_st_local.payLoadHeader_)), sizeof(dap_config_st_local.payLoadHeader_) + sizeof(dap_config_st_local.payLoadPedalConfig_));
        //dap_config_st_local_ptr->payloadFooter_.checkSum = crc;
        dap_config_st_local_ptr->payLoadHeader_.PedalTag=dap_config_st_local_ptr->payLoadPedalConfig_.pedal_type;
        Serial.write((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
        Serial.print("\r\n");
        ESPNow_request_config_b[pedal_config_IDX]=false;
        Serial.print("Pedal:");
        Serial.print(pedal_config_IDX);
        Serial.println("config returned");
        delay(3);
      }
    }

    if(ESPNow_error_b)
    {
      Serial.print("Pedal:");
      Serial.print(dap_state_basic_st.payLoadHeader_.PedalTag);
      Serial.print(" E:");
      Serial.println(dap_state_basic_st.payloadPedalState_Basic_.error_code_u8);
      ESPNow_error_b=false;    
    }
    if(basic_rssi_update)
    {
      int rssi_filter_value=constrain(rssi_filter.process(rssi_display),-100,0) ;
      dap_bridge_state_st.payloadBridgeState_.Pedal_RSSI=(uint8_t)(rssi_filter_value+101);
      dap_bridge_state_st.payLoadHeader_.PedalTag=5; //5 means bridge
      dap_bridge_state_st.payLoadHeader_.payloadType=DAP_PAYLOAD_TYPE_BRIDGE_STATE;
      dap_bridge_state_st.payLoadHeader_.version=DAP_VERSION_CONFIG;
      crc = checksumCalculator((uint8_t*)(&(dap_bridge_state_st.payLoadHeader_)), sizeof(dap_bridge_state_st.payLoadHeader_) + sizeof(dap_bridge_state_st.payloadBridgeState_));
      dap_bridge_state_st.payloadFooter_.checkSum=crc;
      DAP_bridge_state_st * dap_bridge_st_local_ptr;
      dap_bridge_st_local_ptr = &dap_bridge_state_st;
      Serial.write((char*)dap_bridge_st_local_ptr, sizeof(DAP_bridge_state_st));
      Serial.print("\r\n");
      basic_rssi_update=false;
      /*
      if(rssi_filter_value<-88)
      {
        Serial.println("Warning: BAD WIRELESS CONNECTION");
        //Serial.print("Pedal:");
        //Serial.print(dap_state_basic_st.payLoadHeader_.PedalTag);
        Serial.print(" RSSI:");
        Serial.println(rssi_filter_value);  
      }
      */
        #ifdef ESPNow_debug
          Serial.print("Pedal:");
          Serial.print(dap_state_basic_st.payLoadHeader_.PedalTag);
          Serial.print(" RSSI:");
          Serial.println(rssi_filter_value);        
        #endif
        
    }
    uint8_t pedalIDX;
    for(pedalIDX=0;pedalIDX<3;pedalIDX++)
    {
      unsigned long current_time=millis();
      if(dap_bridge_state_st.payloadBridgeState_.Pedal_availability[pedalIDX]==1)
      {
        if(current_time-pedal_last_update[pedalIDX]>3000)
        {
          Serial.print("Pedal:");
          Serial.print(pedalIDX);
          Serial.println(" Disconnected");
          dap_bridge_state_st.payloadBridgeState_.Pedal_availability[pedalIDX]=0;
        }
      }

    }
    
    delay(2);
  }
}
unsigned long last_serial_joy_out =millis();
unsigned long now;
void Joystick_Task( void * pvParameters )
{
  for(;;)
  {   
    #ifdef USB_JOYSTICK
    if(IsControllerReady())
    {
      if(pedal_status==0)
      {
        SetControllerOutputValueAccelerator(pedal_cluth_value);
        SetControllerOutputValueBrake(pedal_brake_value);
        SetControllerOutputValueThrottle(pedal_throttle_value);
        SetControllerOutputValueRudder(0);
        SetControllerOutputValueRudder_brake(0,0);
      }
      if (pedal_status==1)
      {
        SetControllerOutputValueAccelerator(0);
        SetControllerOutputValueBrake(0);
        SetControllerOutputValueThrottle(0);
        //3% deadzone
        if(pedal_brake_value<((int16_t)(0.47*JOYSTICK_RANGE))||pedal_brake_value>((int16_t)(0.53*JOYSTICK_RANGE)))
        {
          SetControllerOutputValueRudder(pedal_brake_value);
        }
        else
        {
          SetControllerOutputValueRudder((int16_t)(0.5*JOYSTICK_RANGE));
        }
        SetControllerOutputValueRudder_brake(0,0);
        
      }
      if (pedal_status==2)
      {
        SetControllerOutputValueAccelerator(0);
        SetControllerOutputValueBrake(0);
        SetControllerOutputValueThrottle(0);
        SetControllerOutputValueRudder((int16_t)(0.5*JOYSTICK_RANGE));
        //int16_t filter_brake=0;
        //int16_t filter_throttle=0;
        
        SetControllerOutputValueRudder_brake(pedal_brake_value,pedal_throttle_value);
        
      }
      

      joystickSendState();
    }
    #endif
    // set analog value
    #ifdef Using_analog_output

      dacWrite(Analog_brk,(uint16_t)((float)((Joystick_value[1])/(float)(JOYSTICK_RANGE))*255));
      dacWrite(Analog_gas,(uint16_t)((float)((Joystick_value[2])/(float)(JOYSTICK_RANGE))*255));
    #endif
    //set MCP4728 analog value
    #ifdef Using_MCP4728
      //Serial.print("MCP/");
      now=millis();
      if(MCP_status)
      {
        /*
        if(now-last_serial_joy_out>1000)
        {
          Serial.print("MCP/");
          Serial.print(Joystick_value[0]);
          Serial.print("/");
          Serial.print(Joystick_value[1]);
          Serial.print("/");
          Serial.print(Joystick_value[2]); 
        }
        */
      
        mcp.setChannelValue(MCP4728_CHANNEL_A, (uint16_t)((float)Joystick_value[0]/(float)JOYSTICK_RANGE*0.8f*4096));
        mcp.setChannelValue(MCP4728_CHANNEL_B, (uint16_t)((float)Joystick_value[1]/(float)JOYSTICK_RANGE*0.8f*4096));
        mcp.setChannelValue(MCP4728_CHANNEL_C, (uint16_t)((float)Joystick_value[2]/(float)JOYSTICK_RANGE*0.8f*4096));
      }

    #endif
      delay(2);
  }
}

//LED task
uint8_t LED_bright_index=0;
uint8_t LED_bright_direction=1;
void LED_Task( void * pvParameters)
{
  for(;;)
  {
    #ifdef LED_ENABLE_WAVESHARE
    //LED status update
      if(LED_bright_index>30)
      {
        LED_bright_direction=-1;
      }
      if(LED_bright_index<2)
      {
        LED_bright_direction=1;
      }
      LED_bright_index=LED_bright_index+LED_bright_direction;
      pixels.setBrightness(LED_bright_index);
      uint8_t led_status=dap_bridge_state_st.payloadBridgeState_.Pedal_availability[0]+dap_bridge_state_st.payloadBridgeState_.Pedal_availability[1]*2+dap_bridge_state_st.payloadBridgeState_.Pedal_availability[2]*4;
      switch (led_status)
      {
        case 0:
          pixels.setPixelColor(0,0xff,0xff,0xff);
          //pixels.setPixelColor(0,0x52,0x00,0xff);//Orange
          pixels.show();
          break;
        case 1:
          pixels.setPixelColor(0,0xff,0x00,0x00);//Red
          pixels.show();
          break;
        case 2:
          pixels.setPixelColor(0,0xff,0x0f,0x00);//Orange
          pixels.show();
          break;
        case 3:
          pixels.setPixelColor(0,0x52,0x00,0xff);//Cyan
          pixels.show();
          break; 
        case 4:
          pixels.setPixelColor(0,0x5f,0x5f,0x00);//Yellow
          pixels.show();
          break;
        case 5:
          pixels.setPixelColor(0,0x00,0x00,0xff);//Blue
          pixels.show();
          break;      
        case 6:
          pixels.setPixelColor(0,0x00,0xff,0x00);//Green
          pixels.show();
          break;  
        case 7:
          pixels.setPixelColor(0, 0x80, 0x00, 0x80);//Purple
          pixels.show();
          break;                                         
        default:
          break;
      }
      delay(150);   
    #endif  
  }
}


/**********************************************************************************************/
/*                                                                                            */
/*                         pedal update task                                                  */
/*                                                                                            */
/**********************************************************************************************/


//long lastCallTime = micros();
unsigned long cycleTimeLastCall = micros();
unsigned long minCyclesForFirToInit = 1000;
unsigned long firCycleIncrementer = 0;

float filteredReading_exp_filter = 0;
unsigned long printCycleCounter = 0;





  








/**********************************************************************************************/
/*                                                                                            */
/*                         communication task                                                 */
/*                                                                                            */
/**********************************************************************************************/












