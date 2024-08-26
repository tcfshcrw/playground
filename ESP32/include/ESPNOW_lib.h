#include <WiFi.h>
#include <esp_wifi.h>
#include <Arduino.h>
#include "ESPNowW.h"
//#define ESPNow_debug
uint8_t esp_master[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x31};
//uint8_t esp_master[] = {0xdc, 0xda, 0x0c, 0x22, 0x8f, 0xd8}; // S3
//uint8_t esp_master[] = {0x48, 0x27, 0xe2, 0x59, 0x48, 0xc0}; // S2 mini
uint8_t Clu_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x32};
uint8_t Gas_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x33};
uint8_t Brk_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x34};
uint8_t broadcast_mac[]={0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
uint8_t esp_Host[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x35};
uint8_t* Recv_mac;
uint16_t ESPNow_send=0;
uint16_t ESPNow_recieve=0;
//bool MAC_get=false;
bool ESPNOW_status =false;
bool ESPNow_initial_status=false;
bool ESPNow_update= false;
bool ESPNow_no_device=false;
bool ESPNow_config_request=false;
bool ESPNow_restart=false;
bool ESPNow_OTA_enable=false;
uint8_t ESPNow_error_code=0;



struct ESPNow_Send_Struct
{ 
  uint16_t pedal_position;
  float pedal_position_ratio;
};

typedef struct struct_message {
  uint64_t cycleCnt_u64;
  int64_t timeSinceBoot_i64;
	int32_t controllerValue_i32;
  int8_t pedal_status; //0=default, 1=rudder, 2=rudder brake
} struct_message;

// Create a struct_message called myData
struct_message myData;

ESPNow_Send_Struct _ESPNow_Recv;
ESPNow_Send_Struct _ESPNow_Send;
void sendMessageToMaster(int32_t controllerValue)
{

  myData.cycleCnt_u64++;
  myData.timeSinceBoot_i64 = esp_timer_get_time() / 1000;
  myData.controllerValue_i32 = controllerValue;
  if(dap_calculationVariables_st.Rudder_status)
  {
    if(dap_calculationVariables_st.rudder_brake_status)
    {
      myData.pedal_status=2;
    }
    else
    {
      myData.pedal_status=1;
    }
  }
  else
  {
    myData.pedal_status=0;
  }
  esp_now_send(broadcast_mac, (uint8_t *) &myData, sizeof(myData));

  
  
  //esp_now_send(esp_master, (uint8_t *) &myData, sizeof(myData));
  /*
  if (result != ESP_OK) 
  {
    ESPNow_no_device=true;
    //Serial.println("Failed send data to ESP_Master");
  }
  else
  {
    ESPNow_no_device=false;
  }
  */
  
  /*if (result == ESP_OK) {
    Serial.println("Sent with success");
  }
  else {
    Serial.println("Error sending the data");
  }*/
}
void onRecv(const uint8_t *mac_addr, const uint8_t *data, int data_len) 
{
  /*
  if(ESPNOW_status)
  {
    memcpy(&ESPNow_recieve, data, sizeof(ESPNow_recieve));
    ESPNow_update=true;
  }
  */
  if(ESPNOW_status)
  {
    if(data_len==sizeof(_ESPNow_Recv))
    {
      memcpy(&_ESPNow_Recv, data, sizeof(_ESPNow_Recv));
      ESPNow_update=true;
    }
    
    if(data_len==sizeof(dap_config_st))
    {

      if(mac_addr[5]==esp_Host[5])
      {
        //Serial.println("dap_config_st ESPNow recieved");
        if(semaphore_updateConfig!=NULL)
        {
          if(xSemaphoreTake(semaphore_updateConfig, (TickType_t)1)==pdTRUE)
          {
            bool structChecker = true;
            uint16_t crc;
            DAP_config_st * dap_config_st_local_ptr;
            dap_config_st_local_ptr = &dap_config_st_local;
            //Serial.readBytes((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
            memcpy(dap_config_st_local_ptr, data, sizeof(DAP_config_st));
       
  

            // check if data is plausible
            if ( dap_config_st_local.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_CONFIG )
            { 
              structChecker = false;
              ESPNow_error_code=101;

            }
            if ( dap_config_st_local.payLoadHeader_.version != DAP_VERSION_CONFIG )
            { 
              structChecker = false;
              ESPNow_error_code=102;

            }
                    // checksum validation
            crc = checksumCalculator((uint8_t*)(&(dap_config_st_local.payLoadHeader_)), sizeof(dap_config_st_local.payLoadHeader_) + sizeof(dap_config_st_local.payLoadPedalConfig_));
            if (crc != dap_config_st_local.payloadFooter_.checkSum)
            { 
              structChecker = false;
              ESPNow_error_code=103;

            }


                    // if checks are successfull, overwrite global configuration struct
            if (structChecker == true)
            {
              //Serial.println("Updating pedal config");
              configUpdateAvailable = true;          
            }
              xSemaphoreGive(semaphore_updateConfig);
          }
        }
      }  
      
    }

    DAP_actions_st dap_actions_st;
    if(data_len==sizeof(dap_actions_st))
    {
            
            memcpy(&dap_actions_st, data, sizeof(dap_actions_st));
            //Serial.readBytes((char*)&dap_actions_st, sizeof(DAP_actions_st));
            if(dap_actions_st.payLoadHeader_.PedalTag==dap_config_st.payLoadPedalConfig_.pedal_type)
            {
              bool structChecker = true;
              uint16_t crc;
              if ( dap_actions_st.payLoadHeader_.payloadType != DAP_PAYLOAD_TYPE_ACTION )
              { 
                structChecker = false;
                ESPNow_error_code=111;

              }
              if ( dap_actions_st.payLoadHeader_.version != DAP_VERSION_CONFIG ){ 
                structChecker = false;
                ESPNow_error_code=112;

              }
              crc = checksumCalculator((uint8_t*)(&(dap_actions_st.payLoadHeader_)), sizeof(dap_actions_st.payLoadHeader_) + sizeof(dap_actions_st.payloadPedalAction_));
              if (crc != dap_actions_st.payloadFooter_.checkSum){ 
                structChecker = false;
                ESPNow_error_code=113;

              }


              if (structChecker == true)
              {

                //1=trigger reset pedal position
                if (dap_actions_st.payloadPedalAction_.system_action_u8==1)
                {
                  resetPedalPosition = true;
                }
                //2= restart pedal
                if (dap_actions_st.payloadPedalAction_.system_action_u8==2)
                {
                  ESPNow_restart = true;
                }
                //3= Wifi OTA
                if (dap_actions_st.payloadPedalAction_.system_action_u8==3)
                {
                  ESPNow_OTA_enable = true;
                }
                // trigger ABS effect
                if (dap_actions_st.payloadPedalAction_.triggerAbs_u8)
                {
                  absOscillation.trigger();
                }
                //RPM effect
                _RPMOscillation.RPM_value=dap_actions_st.payloadPedalAction_.RPM_u8;
                //G force effect
                _G_force_effect.G_value=dap_actions_st.payloadPedalAction_.G_value-128;       
                //wheel slip
                if (dap_actions_st.payloadPedalAction_.WS_u8)
                {
                  _WSOscillation.trigger();
                }     
                //Road impact
                _Road_impact_effect.Road_Impact_value=dap_actions_st.payloadPedalAction_.impact_value_u8;
                // trigger system identification
                if (dap_actions_st.payloadPedalAction_.startSystemIdentification_u8)
                {
                  systemIdentificationMode_b = true;
                }
                // trigger Custom effect effect 1
                if (dap_actions_st.payloadPedalAction_.Trigger_CV_1)
                {
                  CV1.trigger();
                }
                // trigger Custom effect effect 2
                if (dap_actions_st.payloadPedalAction_.Trigger_CV_2)
                {
                  CV2.trigger();
                }
                // trigger return pedal position
                if (dap_actions_st.payloadPedalAction_.returnPedalConfig_u8)
                {
                  ESPNow_config_request=true;
                  /*
                  DAP_config_st * dap_config_st_local_ptr;
                  dap_config_st_local_ptr = &dap_config_st;
                  //uint16_t crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
                  crc = checksumCalculator((uint8_t*)(&(dap_config_st.payLoadHeader_)), sizeof(dap_config_st.payLoadHeader_) + sizeof(dap_config_st.payLoadPedalConfig_));
                  dap_config_st_local_ptr->payloadFooter_.checkSum = crc;
                  Serial.write((char*)dap_config_st_local_ptr, sizeof(DAP_config_st));
                  Serial.print("\r\n");
                  */
                }
                if(dap_actions_st.payloadPedalAction_.Rudder_action==1)
                {
                  if(dap_calculationVariables_st.Rudder_status==false)
                  {
                    dap_calculationVariables_st.Rudder_status=true;
                    //Serial.println("Rudder on");
                    moveSlowlyToPosition_b=true;
                    //Serial.print("status:");
                    //Serial.println(dap_calculationVariables_st.Rudder_status);
                  }
                  else
                  {
                    dap_calculationVariables_st.Rudder_status=false;
                    //Serial.println("Rudder off");
                    moveSlowlyToPosition_b=true;
                    //Serial.print("status:");
                    //Serial.println(dap_calculationVariables_st.Rudder_status);
                  }
                }
                if(dap_actions_st.payloadPedalAction_.Rudder_brake_action==1)
                {
                  if(dap_calculationVariables_st.rudder_brake_status==false&&dap_calculationVariables_st.Rudder_status==true)
                  {
                    dap_calculationVariables_st.rudder_brake_status=true;
                    //Serial.println("Rudder brake on");
                    //Serial.print("status:");
                    //Serial.println(dap_calculationVariables_st.Rudder_status);
                  }
                  else
                  {
                    dap_calculationVariables_st.rudder_brake_status=false;
                    //Serial.println("Rudder brake off");
                    //Serial.print("status:");
                    //Serial.println(dap_calculationVariables_st.Rudder_status);
                  }
                }
              }
            }

            

          
            
    }

  }

}
void OnSent(const uint8_t *mac_addr, esp_now_send_status_t status)
{

}
void ESPNow_initialize()
{

    WiFi.mode(WIFI_MODE_STA);
    Serial.println("Initializing ESPNow, please wait"); 
    Serial.print("Current MAC Address:  ");  
    Serial.println(WiFi.macAddress());
    if(dap_config_st.payLoadPedalConfig_.pedal_type==0)
    {
      esp_wifi_set_mac(WIFI_IF_STA, &Clu_mac[0]);
    }
    if(dap_config_st.payLoadPedalConfig_.pedal_type==1)
    {
      esp_wifi_set_mac(WIFI_IF_STA, &Brk_mac[0]);
    }
    if(dap_config_st.payLoadPedalConfig_.pedal_type==2)
    {
      esp_wifi_set_mac(WIFI_IF_STA, &Gas_mac[0]);
    }
    delay(300);
    Serial.print("Modified MAC Address:  ");  
    Serial.println(WiFi.macAddress());
    ESPNow.init();
    Serial.println("wait  for ESPNOW initialized");
    delay(3000);
    #ifdef ESPNow_S3
      //esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_11M_L);
      esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_11M_L);
    #endif
    #ifdef ESPNow_ESP32
      esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_MCS0_LGI);
      //esp_wifi_config_espnow_rate(WIFI_IF_STA, 	WIFI_PHY_RATE_54M);
    #endif
    
    if(dap_config_st.payLoadPedalConfig_.pedal_type==1)
    {
      Recv_mac=Gas_mac;      
    }

    if(dap_config_st.payLoadPedalConfig_.pedal_type==2)
    {
      Recv_mac=Brk_mac;
    }
    if(ESPNow.add_peer(Recv_mac)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add peer");
    }
    else
    {
      ESPNOW_status=false;
      Serial.println("Fail to add peer");
    }
    
    if(ESPNow.add_peer(esp_master)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add host peer");
    }
    if(ESPNow.add_peer(esp_Host)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add host peer");
    }
    if(ESPNow.add_peer(broadcast_mac)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add broadcast peer");
    }
    ESPNow.reg_recv_cb(onRecv);
    ESPNow.reg_send_cb(OnSent);
    ESPNow_initial_status=true;
    ESPNOW_status=true;
    Serial.println("ESPNow Initialized");
  
}
