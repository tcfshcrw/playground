
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


void serialCommunicationTask( void * pvParameters );

void OTATask( void * pvParameters );

void ESPNOW_SyncTask( void * pvParameters);
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

#include "CycleTimer.h"


#include "RTDebugOutput.h"
#include "Wire.h"
#include "SPI.h"
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

//#define PRINT_USED_STACK_SIZE
// https://stackoverflow.com/questions/55998078/freertos-task-priority-and-stack-size
#define STACK_SIZE_FOR_TASK_1 0.2 * (configTOTAL_HEAP_SIZE / 4)
#define STACK_SIZE_FOR_TASK_2 0.2 * (configTOTAL_HEAP_SIZE / 4)


TaskHandle_t Task1;
TaskHandle_t Task2;


static SemaphoreHandle_t semaphore_updateConfig=NULL;
  bool configUpdateAvailable = false;                              // semaphore protected data
  DAP_config_st dap_config_st_local;

static SemaphoreHandle_t semaphore_updateJoystick=NULL;
  int32_t joystickNormalizedToInt32 = 0;                           // semaphore protected data

static SemaphoreHandle_t semaphore_resetServoPos=NULL;
bool resetPedalPosition = false;

static SemaphoreHandle_t semaphore_readServoValues=NULL;

static SemaphoreHandle_t semaphore_updatePedalStates=NULL;

/**********************************************************************************************/
/*                                                                                            */
/*                         target-specific  definitions                                       */
/*                                                                                            */
/**********************************************************************************************/




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
/*                         Kalman filter definitions                                          */
/*                                                                                            */
/**********************************************************************************************/










/**********************************************************************************************/
/*                                                                                            */
/*                         loadcell definitions                                               */
/*                                                                                            */
/**********************************************************************************************/





/**********************************************************************************************/
/*                                                                                            */
/*                         stepper motor definitions                                          */
/*                                                                                            */
/**********************************************************************************************/


StepperWithLimits* stepper = NULL;
//static const int32_t MIN_STEPS = 5;



bool moveSlowlyToPosition_b = false;
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


//I2C sync
#ifdef Using_I2C_Sync
  
  #include "I2CSync.h"
  TaskHandle_t Task5;
#endif

//ESPNOW
#ifdef ESPNOW_Enable
  #include "ESPNOW_lib.h"
  TaskHandle_t Task6;
#endif

bool dap_action_update= false;

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
    Serial.setTxTimeoutMs(0);
  #else
    Serial.begin(921600);
    Serial.setTimeout(5);
  #endif
  Serial.println(" ");
  Serial.println(" ");
  Serial.println(" ");
  
  Serial.println("This work is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.");
  Serial.println("Please check github repo for more detail: https://github.com/ChrGri/DIY-Sim-Racing-FFB-Pedal");
  //printout the github releasing version






// initialize configuration and update local variables
  dap_config_st.initialiseDefaults();

  // Load config from EEPROM, if valid, overwrite initial config
  EEPROM.begin(2048);
  //dap_config_st.loadConfigFromEprom(dap_config_st_local);


  // check validity of data from EEPROM  
  






  


  // interprete config values
  



  
  








  

  

  




  // setup multi tasking
  semaphore_updateJoystick = xSemaphoreCreateMutex();
  semaphore_updateConfig = xSemaphoreCreateMutex();
  semaphore_resetServoPos = xSemaphoreCreateMutex();
  semaphore_updatePedalStates = xSemaphoreCreateMutex();
  delay(10);


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



 
  


  disableCore0WDT();

  //create a task that will be executed in the Task2code() function, with priority 1 and executed on core 1

  /*
  xTaskCreatePinnedToCore(
                    serialCommunicationTask,   
                    "serialCommunicationTask", 
                    10000,  
                    //STACK_SIZE_FOR_TASK_2,    
                    NULL,      
                    1,         
                    &Task2,    
                    0);     
  delay(500);
  */





  

  //enable ESP-NOW
  
  
  

      ESPNow_initialize();
      /*
      xTaskCreatePinnedToCore(
                        ESPNOW_SyncTask,   
                        "ESPNOW_update_Task", 
                        10000,  
                        //STACK_SIZE_FOR_TASK_2,    
                        NULL,      
                        1,         
                        &Task6,    
                        0);     
      delay(500);
      */
    
    
      
    

    

 

  Serial.println("Setup end");
  
}








/**********************************************************************************************/
/*                                                                                            */
/*                         Main function                                                      */
/*                                                                                            */
/**********************************************************************************************/
uint32_t loop_count=0;
void loop() {
  taskYIELD();
  /*
  #ifdef OTA_update
  server.handleClient();
  //delay(1);
  #endif
  */
 /*
  DAP_actions_st _dap_actions;
  _dap_actions.payLoadHeader_.payloadType=DAP_PAYLOAD_TYPE_ACTION;
  _dap_actions.payLoadHeader_.version=DAP_VERSION_CONFIG;
  _dap_actions.payloadPedalAction_.G_value=128;
  _dap_actions.payloadPedalAction_.triggerAbs_u8=1;
  _dap_actions.payloadFooter_.checkSum=checksumCalculator((uint8_t*)(&(_dap_actions.payLoadHeader_)), sizeof(_dap_actions.payLoadHeader_) + sizeof(_dap_actions.payloadPedalAction_));
  ESPNow.send_message(Brk_mac,(uint8_t *) &_dap_actions,sizeof(_dap_actions));
  ESPNow.send_message(Gas_mac,(uint8_t *) &_dap_actions,sizeof(_dap_actions));
delay(5);
*/
  uint16_t crc;
  uint8_t n = Serial.available();
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
          // trigger return pedal position
          /*
          if (dap_actions_st.payloadPedalAction_.returnPedalConfig_u8)
          {
            DAP_config_st * dap_config_st_local_ptr;
            dap_config_st_local_ptr = &dap_config_st;
            //uint16_t crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
            crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
            dap_config_st_local_ptr->payloadFooter_.checkSum = crc;
            Serial.write((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
            Serial.print("\r\n");
          }
          */
          
                
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
        while (Serial.available()) Serial.read();
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


  if(configUpdateAvailable)
  {
    if(dap_config_st.payLoadHeader_.PedalTag==0)
    {
      ESPNow.send_message(Clu_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
      Serial.println("Clutch config sent");
      configUpdateAvailable=false;
    }
    if(dap_config_st.payLoadHeader_.PedalTag==1)
    {
      ESPNow.send_message(Brk_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
      Serial.println("BRK config sent");
      configUpdateAvailable=false;
    }
    if(dap_config_st.payLoadHeader_.PedalTag==2)
    {
      ESPNow.send_message(Gas_mac,(uint8_t *) &dap_config_st,sizeof(dap_config_st));
      Serial.println("Throttle config sent");
      configUpdateAvailable=false;
    }

  }


  if(dap_action_update)
  {
    
    if(dap_actions_st.payLoadHeader_.PedalTag==0)
    {
      ESPNow.send_message(Clu_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
      //Serial.println("BRK sent");
    }
    if(dap_actions_st.payLoadHeader_.PedalTag==1)
    {
      ESPNow.send_message(Brk_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
      //Serial.println("BRK sent");
    }
                
    if(dap_actions_st.payLoadHeader_.PedalTag==2)
    {
      ESPNow.send_message(Gas_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
      //Serial.println("GAS sent");
    }
    
    //ESPNow.send_message(broadcast_mac,(uint8_t *) &dap_actions_st,sizeof(dap_actions_st));
    //Serial.println("Broadcast sent");
    dap_action_update=false;
  }
  if(update_basic_state)
  {
    update_basic_state=false;
    Serial.write((char*)&dap_state_basic_st, sizeof(DAP_state_basic_st));
    Serial.print("\r\n");
  }
  if(ESPNow_request_config_b)
  {
    DAP_config_st * dap_config_st_local_ptr;
    dap_config_st_local_ptr = &dap_config_st;
    //uint16_t crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
    crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
    dap_config_st_local_ptr->payloadFooter_.checkSum = crc;
    dap_config_st_local_ptr->payLoadHeader_.PedalTag=dap_config_st_local_ptr->payLoadPedalConfig_.pedal_type;
    Serial.write((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
    Serial.print("\r\n");
    ESPNow_request_config_b=false;
    Serial.print("config returned");
  }
  if(ESPNow_error_b)
  {
    Serial.print("Pedal:");
    Serial.print(dap_state_basic_st.payLoadHeader_.PedalTag);
    Serial.print(" E:");
    Serial.println(dap_state_basic_st.payloadPedalState_Basic_.error_code_u8);
    ESPNow_error_b=false;
    
  }
  // set joysitck value
  #ifdef Using_analog_output
    dacWrite(Analog_brk,(uint16_t)(Joystick_value[1]/JOYSTICK_RANGE*255));
    dacWrite(Analog_gas,(uint16_t)(Joystick_value[2]/JOYSTICK_RANGE*255));
  #endif

  /*
  if(loop_count>3000)
  {
    Serial.println("Serial is still alive");
    loop_count=0;
  }
  else
  {
    loop_count++;
  }
  */
  delay(2);
  
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





int32_t joystickNormalizedToInt32_local = 0;
void serialCommunicationTask( void * pvParameters )
{

  for(;;){


    // average cycle time averaged over multiple cycles 
    if (dap_config_st.payLoadPedalConfig_.debug_flags_0 & DEBUG_INFO_0_CYCLE_TIMER) 
    {
      static CycleTimer timerSC("SC cycle time");
      timerSC.Bump();
    }

    uint16_t crc;




    delay( SERIAL_COOMUNICATION_TASK_DELAY_IN_MS );

   
    { 
      // read serial input 
      uint8_t n = Serial.available();

      bool structChecker = true;
      
      if (n > 0)
      {
        switch (n) {

          // likely config structure 
          case sizeof(DAP_config_st):
              
              if(semaphore_updateConfig!=NULL)
              {
                if(xSemaphoreTake(semaphore_updateConfig, (TickType_t)1)==pdTRUE)
                {
                  DAP_config_st * dap_config_st_local_ptr;
                  dap_config_st_local_ptr = &dap_config_st_local;
                  Serial.readBytes((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));

                  

                  // check if data is plausible
                  
                  if ( dap_config_st_local.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_CONFIG ){ 
                    structChecker = false;
                    Serial.print("Payload type expected: ");
                    Serial.print(DAP_PAYLOAD_TYPE_CONFIG);
                    Serial.print(",   Payload type received: ");
                    Serial.println(dap_config_st_local.payLoadHeader_.payloadType);
                  }
                  if ( dap_config_st_local.payLoadHeader_.version != DAP_VERSION_CONFIG ){ 
                    structChecker = false;
                    Serial.print("Config version expected: ");
                    Serial.print(DAP_VERSION_CONFIG);
                    Serial.print(",   Config version received: ");
                    Serial.println(dap_config_st_local.payLoadHeader_.version);
                  }
                  // checksum validation
                  crc = checksumCalculator((uint8_t*)(&(dap_config_st_local.payLoadHeader_)), sizeof(dap_config_st_local.payLoadHeader_) + sizeof(dap_config_st_local.payLoadPedalConfig_));
                  if (crc != dap_config_st_local.payloadFooter_.checkSum){ 
                    structChecker = false;
                    Serial.print("CRC expected: ");
                    Serial.print(crc);
                    Serial.print(",   CRC received: ");
                    Serial.println(dap_config_st_local.payloadFooter_.checkSum);
                  }


                  // if checks are successfull, overwrite global configuration struct
                  if (structChecker == true)
                  {
                    Serial.println("Updating pedal config");
                    configUpdateAvailable = true;          
                  }
                  xSemaphoreGive(semaphore_updateConfig);
                }
              }
            break;

          // likely action structure 
          case sizeof(DAP_actions_st) :

            
            Serial.readBytes((char*)&dap_actions_st, sizeof(DAP_actions_st));

            if ( dap_actions_st.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_ACTION ){ 
              structChecker = false;
              Serial.print("Payload type expected: ");
              Serial.print(DAP_PAYLOAD_TYPE_ACTION);
              Serial.print(",   Payload type received: ");
              Serial.println(dap_config_st_local.payLoadHeader_.payloadType);
            }
            if ( dap_actions_st.payLoadHeader_.version != DAP_VERSION_CONFIG ){ 
              structChecker = false;
              Serial.print("Config version expected: ");
              Serial.print(DAP_VERSION_CONFIG);
              Serial.print(",   Config version received: ");
              Serial.println(dap_config_st_local.payLoadHeader_.version);
            }
            crc = checksumCalculator((uint8_t*)(&(dap_actions_st.payLoadHeader_)), sizeof(dap_actions_st.payLoadHeader_) + sizeof(dap_actions_st.payloadPedalAction_));
            if (crc != dap_actions_st.payloadFooter_.checkSum){ 
              structChecker = false;
              Serial.print("CRC expected: ");
              Serial.print(crc);
              Serial.print(",   CRC received: ");
              Serial.println(dap_actions_st.payloadFooter_.checkSum);
            }



            if (structChecker == true)
            {


              dap_action_update=true;
              
              // trigger return pedal position
              if (dap_actions_st.payloadPedalAction_.returnPedalConfig_u8)
              {
                DAP_config_st * dap_config_st_local_ptr;
                dap_config_st_local_ptr = &dap_config_st;
                //uint16_t crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
                crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
                dap_config_st_local_ptr->payloadFooter_.checkSum = crc;
                Serial.write((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
                Serial.print("\r\n");
              }
        
              
             


            }

            break;

          default:

            // flush the input buffer
            while (Serial.available()) Serial.read();
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


      // send pedal state structs
      // update pedal states
      printCycleCounter++;
      DAP_state_basic_st dap_state_basic_st_lcl;
      DAP_state_extended_st dap_state_extended_st_lcl;

      if(semaphore_updatePedalStates!=NULL)
      {
        
        if(xSemaphoreTake(semaphore_updatePedalStates, (TickType_t)1)==pdTRUE) 
        {
        
          // UPDATE basic pedal state struct
          dap_state_basic_st_lcl = dap_state_basic_st;

          // UPDATE extended pedal state struct
          dap_state_extended_st_lcl = dap_state_extended_st;
            
          // release semaphore
          xSemaphoreGive(semaphore_updatePedalStates);

        }
      }
      else
      {
        semaphore_updatePedalStates = xSemaphoreCreateMutex();
      }



      // send the pedal state structs
      // send basic pedal state struct
      /*
      if ( !(dap_config_st.payLoadPedalConfig_.debug_flags_0 & DEBUG_INFO_0_STATE_BASIC_INFO_STRUCT) )
      {
        if (printCycleCounter >= 2)
        {
          printCycleCounter = 0;
          Serial.write((char*)&dap_state_basic_st_lcl, sizeof(DAP_state_basic_st));
          Serial.print("\r\n");
        }
      }

      if ( (dap_config_st.payLoadPedalConfig_.debug_flags_0 & DEBUG_INFO_0_STATE_EXTENDED_INFO_STRUCT) )
      {
        Serial.write((char*)&dap_state_extended_st_lcl, sizeof(DAP_state_extended_st));
        Serial.print("\r\n");
      }
      */


      // wait until transmission is finished
      // flush argument = true, do not clear Rx buffer
      //Serial.flush();
      //Serial.flush(true);

    }

    // transmit controller output
    //Serial.print("Joy 1");
    delay( SERIAL_COOMUNICATION_TASK_DELAY_IN_MS );
          //Serial.print(" 2");
    if(semaphore_updateJoystick!=NULL)
    {
      if(xSemaphoreTake(semaphore_updateJoystick, (TickType_t)1)==pdTRUE)
      {
         //Serial.print(" 3");
        joystickNormalizedToInt32_local = joystickNormalizedToInt32;
        xSemaphoreGive(semaphore_updateJoystick);
      }
    }
    if (IsControllerReady()) 
    {

      //Serial.print(" 4");
      //Serial.print("\r\n");
      if(dap_calculationVariables_st.Rudder_status==false)
      {
        //general output
        SetControllerOutputValue(joystickNormalizedToInt32_local);
      }
    
      
    }

  /*#ifdef SERIAL_TIMEOUT
    delay(10);
  #endif
*/

  }
}


#ifdef ESPNOW_Enable
int ESPNOW_count=0;
int error_count=0;
int print_count=0;
int ESPNow_no_device_count=0;
uint8_t error_out;
void ESPNOW_SyncTask( void * pvParameters )
{
  for(;;)
  {    
      delay(1);
  }
}
#endif



