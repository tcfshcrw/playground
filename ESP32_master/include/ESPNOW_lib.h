#include <WiFi.h>
#include <esp_wifi.h>
#include <Arduino.h>
#include "esp_now.h"
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
int rssi_display;
//bool MAC_get=false;
bool ESPNOW_status =false;
bool ESPNow_initial_status=false;
bool ESPNow_update= false;
bool ESPNow_no_device=false;
bool update_basic_state=false;
bool update_extend_state=false;
uint16_t Joystick_value[]={0,0,0};
bool ESPNow_request_config_b[3]={false,false,false};
bool ESPNow_error_b=false;
uint16_t pedal_throttle_value=0;
uint16_t pedal_brake_value=0;
uint16_t pedal_cluth_value=0;
uint16_t pedal_brake_rudder_value=0;
uint16_t pedal_throttle_rudder_value=0;
uint8_t pedal_status=0;


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
//void onRecv(const esp_now_recv_info_t *esp_now_info, const uint8_t *data, int data_len)
{
  
  if(data_len==sizeof(myData))
	{
		memcpy(&myData, data, sizeof(myData));
		pedal_status=myData.pedal_status;
		//#ifdef ACTIVATE_JOYSTICK_OUTPUT
		// normalize controller output
		int32_t joystickNormalizedToInt32 = NormalizeControllerOutputValue(myData.controllerValue_i32, 0, 10000, 100); 
		//if(esp_now_info->src_addr[5]==Clu_mac[5])
    if(mac_addr[5]==Clu_mac[5])
		{
			pedal_cluth_value=joystickNormalizedToInt32;
			Joystick_value[0]=myData.controllerValue_i32;
			//joystick_update=true;
		}
		if(mac_addr[5]==Brk_mac[5])
		{
			pedal_brake_value=joystickNormalizedToInt32;
			Joystick_value[1]=myData.controllerValue_i32;
			//joystick_update=true;
		}
		if(mac_addr[5]==Gas_mac[5])
		{
			pedal_throttle_value=joystickNormalizedToInt32;
			Joystick_value[2]=myData.controllerValue_i32;
			//joystick_update=true;
		}
		#ifdef ESPNow_debug
		Serial.print("Bytes received: ");
		Serial.println(len);
		Serial.print("CycleCnt: ");
		Serial.println(myData.cycleCnt_u64);
		Serial.print("TimeSinceBoot in ms (shared): ");
		Serial.println(myData.timeSinceBoot_i64);
		Serial.print("controllerValue_i32: ");
		Serial.println(myData.controllerValue_i32);	
		Serial.println();
		#endif
	}
  
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

  if(data_len==sizeof(DAP_config_st))
  {
    memcpy(&dap_config_st_Temp, data, sizeof(DAP_config_st));
    ESPNow_request_config_b[dap_config_st_Temp.payLoadPedalConfig_.pedal_type]=true;
    if(dap_config_st_Temp.payLoadPedalConfig_.pedal_type==0)
    {
      memcpy(&dap_config_st_Clu, &dap_config_st_Temp, sizeof(DAP_config_st));
    }
    if(dap_config_st_Temp.payLoadPedalConfig_.pedal_type==1)
    {
      memcpy(&dap_config_st_Brk, &dap_config_st_Temp, sizeof(DAP_config_st));
    }
    if(dap_config_st_Temp.payLoadPedalConfig_.pedal_type==2)
    {
      memcpy(&dap_config_st_Gas, &dap_config_st_Temp, sizeof(DAP_config_st));
    }
    
  }

}
void OnSent(const uint8_t *mac_addr, esp_now_send_status_t status)
{

}

// The callback that does the magic
void promiscuous_rx_cb(void *buf, wifi_promiscuous_pkt_type_t type) {
  // All espnow traffic uses action frames which are a subtype of the mgmnt frames so filter out everything else.
  if (type != WIFI_PKT_MGMT)
    return;

  const wifi_promiscuous_pkt_t *ppkt = (wifi_promiscuous_pkt_t *)buf;
  //const wifi_ieee80211_packet_t *ipkt = (wifi_ieee80211_packet_t *)ppkt->payload;
  //const wifi_ieee80211_mac_hdr_t *hdr = &ipkt->hdr;

  int rssi = ppkt->rx_ctrl.rssi;
  rssi_display = rssi;
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
    //rssi calculate
    esp_wifi_set_promiscuous(true);
    esp_wifi_set_promiscuous_rx_cb(&promiscuous_rx_cb);
    ESPNow_initial_status=true;
    Serial.println("ESPNow Initialized");
  
}
