#include <WiFi.h>
#include <esp_wifi.h>
#include <Arduino.h>
#include "ESPNowW.h"
#include "Main.h"
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
bool update_basic_state=false;
bool update_extend_state=false;
uint16_t Joystick_value[]={0,0,0};
bool ESPNow_request_config_b=false;
bool ESPNow_error_b=false;

//https://github.com/nickgammon/I2C_Anything/tree/master
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
struct_message broadcast_incoming;
ESPNow_Send_Struct _ESPNow_Recv;
ESPNow_Send_Struct _ESPNow_Send;
bool sendMessageToMaster(int32_t controllerValue)
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
  return true;
  
}
void onRecv(const uint8_t *mac_addr, const uint8_t *data, int data_len) 
{
  
  if(data_len==sizeof(dap_state_basic_st))
  {
    memcpy(&dap_state_basic_st, data, sizeof(dap_state_basic_st));
    //Joystick_value[dap_state_basic_st.payLoadHeader_.PedalTag]=dap_state_basic_st.payloadPedalState_Basic_.joystickOutput_u16;
    update_basic_state=true;
    if(dap_state_basic_st.payloadPedalState_Basic_.error_code_u8!=0)
    {
      ESPNow_error_b=true;
    }
  }

  if(data_len==sizeof(dap_state_extended_st))
  {
    memcpy(&dap_state_extended_st, data, sizeof(dap_state_extended_st));
    update_extend_state=true;
  }

  if(data_len==sizeof(dap_config_st))
  {
    memcpy(&dap_config_st, data, sizeof(dap_config_st));
    ESPNow_request_config_b=true;
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
    esp_wifi_set_mac(WIFI_IF_STA, &esp_Host[0]);
    delay(300);
    Serial.print("Modified MAC Address:  ");  
    Serial.println(WiFi.macAddress());
    ESPNow.init();
    Serial.println("wait  for ESPNOW initialized");
    delay(3000);
    #ifdef Using_Board_ESP32
    esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_MCS0_LGI);
    #endif
    
    #ifdef Using_Board_ESP32S3
    //esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_54M);
    esp_wifi_config_espnow_rate(WIFI_IF_STA, 	WIFI_PHY_RATE_11M_L);
    	
    #endif


    if(ESPNow.add_peer(Brk_mac)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add peer");
    }
    else
    {
      ESPNOW_status=false;
      Serial.println("Fail to add peer");
    }

    if(ESPNow.add_peer(Gas_mac)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add peer");
    }
    else
    {
      ESPNOW_status=false;
      Serial.println("Fail to add peer");
    }

    if(ESPNow.add_peer(Clu_mac)== ESP_OK)
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
    if(ESPNow.add_peer(broadcast_mac)== ESP_OK)
    {
      ESPNOW_status=true;
      Serial.println("Sucess to add broadcast peer");
    }
    ESPNow.reg_recv_cb(onRecv);
    ESPNow.reg_send_cb(OnSent);
    ESPNow_initial_status=true;
    Serial.println("ESPNow Initialized");
  
}
