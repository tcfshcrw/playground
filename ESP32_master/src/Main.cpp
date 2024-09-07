
///*
//  Rui Santos & Sara Santos - Random Nerd Tutorials
//  Complete project details at https://RandomNerdTutorials.com/get-change-esp32-esp8266-mac-address-arduino/
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files.  
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//*/
//#include <WiFi.h>
//#include <esp_wifi.h>
//
//void readMacAddress(){
//  uint8_t baseMac[6];
//  esp_err_t ret = esp_wifi_get_mac(WIFI_IF_STA, baseMac);
//  if (ret == ESP_OK) {
//    Serial.printf("%02x:%02x:%02x:%02x:%02x:%02x\n",
//                  baseMac[0], baseMac[1], baseMac[2],
//                  baseMac[3], baseMac[4], baseMac[5]);
//  } else {
//    Serial.println("Failed to read MAC address");
//  }
//}
//
//void setup(){
//  Serial.begin(115200);
//
//  WiFi.mode(WIFI_STA);
//  WiFi.begin();
//
//  
//}
// 
//void loop(){
//	delay(1000);
//	Serial.print("[DEFAULT] ESP32 Board MAC Address: ");
//  	readMacAddress();
//}

#include "Arduino.h"
#include "Main.h"
#define SerialOutput
/**********************************************************************************************/
/*                                                                                            */
/*                         controller  definitions                                            */
/*                                                                                            */
/**********************************************************************************************/


#include "Controller.h"


/**********************************************************************************************/
/*                                                                                            */
/*                         ESPNOW definitions                                                 */
/*                                                                                            */
/**********************************************************************************************/
//#include <esp_now.h>
#include <WiFi.h>
#include <esp_wifi.h>
#include <ESPNowW.h>
#include "Wire.h"
#include "SPI.h"
//#define ESPNow_debug
#ifdef Using_MCP4728
  #include <Adafruit_MCP4728.h>
  Adafruit_MCP4728 mcp;
  TwoWire MCP4728_I2C= TwoWire(1);
  bool MCP_status =false;
#endif


// Set your new MAC Address
//uint8_t newMACAddress[] = {0x32, 0xAE, 0xA4, 0x07, 0x0D, 0x66};
uint8_t esp_master[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x31};
uint8_t Clu_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x32};
uint8_t Gas_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x33};
uint8_t Brk_mac[] = {0x36, 0x33, 0x33, 0x33, 0x33, 0x34};
uint16_t pedal_throttle_value=0;
uint16_t pedal_brake_value=0;
uint16_t pedal_cluth_value=0;
uint16_t pedal_brake_rudder_value=0;
uint16_t pedal_throttle_rudder_value=0;
uint8_t pedal_status=0;
bool joystick_update=false;
uint16_t Joystick_value[]={0,0,0};

typedef struct struct_message {
    uint64_t cycleCnt_u64;
    int64_t timeSinceBoot_i64;
	int32_t controllerValue_i32;
	int8_t pedal_status; //0=default, 1=rudder, 2=rudder brake
} struct_message;

// Create a struct_message called myData
struct_message myData;

// Callback when data is received
void OnDataRecv(const uint8_t *mac_addr, const uint8_t *incomingData, int len) 
{

//void OnDataRecv(const esp_now_recv_info *info, const uint8_t *incomingData, int len) {
	if(len==sizeof(myData))
	{
		memcpy(&myData, incomingData, sizeof(myData));
		pedal_status=myData.pedal_status;
		//#ifdef ACTIVATE_JOYSTICK_OUTPUT
		// normalize controller output
		int32_t joystickNormalizedToInt32 = NormalizeControllerOutputValue(myData.controllerValue_i32, 0, 10000, 100); 
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
	
	



}
// The callback that does the magic
int rssi_display=0;
void promiscuous_rx_cb(void *buf, wifi_promiscuous_pkt_type_t type) 
{
  // All espnow traffic uses action frames which are a subtype of the mgmnt frames so filter out everything else.
  if (type != WIFI_PKT_MGMT)
    return;

  const wifi_promiscuous_pkt_t *ppkt = (wifi_promiscuous_pkt_t *)buf;
  //const wifi_ieee80211_packet_t *ipkt = (wifi_ieee80211_packet_t *)ppkt->payload;
  //const wifi_ieee80211_mac_hdr_t *hdr = &ipkt->hdr;

  int rssi = ppkt->rx_ctrl.rssi;
  rssi_display = rssi;
}

/**********************************************************************************************/
/*                                                                                            */
/*                         setup function                                                     */
/*                                                                                            */
/**********************************************************************************************/
void setup()
{

	// Initialize Serial Monitor
  	Serial.begin(921600);
	
#ifdef ACTIVATE_JOYSTICK_OUTPUT
	SetupController();
#endif
	delay(4000);
	// https://randomnerdtutorials.com/get-change-esp32-esp8266-mac-address-arduino/
	// https://randomnerdtutorials.com/esp-now-two-way-communication-esp32/

	//WiFi.mode(WIFI_STA);
	//Change ESP32 Mac Address
	WiFi.mode(WIFI_MODE_STA);
    Serial.println("Initializing Rudder, please wait"); 
    Serial.print("Current MAC Address:  ");  
    Serial.println(WiFi.macAddress());
	esp_err_t err = esp_wifi_set_mac(WIFI_IF_STA, &esp_master[0]);
	if (err == ESP_OK) 
	{
		Serial.println("Success changing Mac Address");
		
	}
    Serial.print("Modified MAC Address:  ");  
    Serial.println(WiFi.macAddress());
	delay(300);
	ESPNow.init();
    Serial.println("wait 10s for ESPNOW initialized");
#ifdef Using_ESP32S3
	esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_11M_L);
#endif

#ifdef Using_ESP32
	esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_MCS0_LGI);
#endif
	delay(3000);
	ESPNow.add_peer(Clu_mac);
	ESPNow.add_peer(Brk_mac);
	ESPNow.add_peer(Gas_mac);
	

	
	// Register for a callback function that will be called when data is received
	//esp_now_register_recv_cb(esp_now_recv_cb_t(OnDataRecv));
	ESPNow.reg_recv_cb(OnDataRecv);
	//rssi calculate
    esp_wifi_set_promiscuous(true);
    esp_wifi_set_promiscuous_rx_cb(&promiscuous_rx_cb);
	Serial.println("ESPNow Comunication Starting");
	#ifdef Using_MCP4728
    MCP4728_I2C.begin(MCP_SDA,MCP_SCL,100000);
    uint8_t i2c_address[8]={0x60,0x61,0x62,0x63,0x64,0x65,0x66,0x67};
    int index_address=0;
    int found_address=0;
    int error;
    for(index_address=0;index_address<8;index_address++)
    {
      MCP4728_I2C.beginTransmission(i2c_address[index_address]);
	  //MCP4728_I2C.beginTransmission(index_address);
      error = MCP4728_I2C.endTransmission();
      if (error == 0)
      {
        Serial.print("I2C device found at address");
        Serial.print(i2c_address[index_address]);
		//Serial.print(index_address);
        Serial.println("  !");
        found_address=index_address;
        break;
        
      }
      else
      {
        Serial.print("try address");
        Serial.println(i2c_address[index_address]);
		//Serial.println(index_address);
      }
    }
    
    if(mcp.begin(i2c_address[found_address], &MCP4728_I2C)==false)
    {
      Serial.println("Couldn't find MCP4728, will not have analog output");
      MCP_status=false;
	  delay(10000);
	  ESP.restart();
    }
    else
    {
      Serial.println("MCP4728 founded");
      MCP_status=true;
      //MCP.begin();
    }
    
  #endif

}




int32_t joystickNormalizedToInt32_local = 0;






/**********************************************************************************************/
/*                                                                                            */
/*                         Main function                                                      */
/*                                                                                            */
/**********************************************************************************************/
uint64_t cycleCntr_u64 = 0;
unsigned long joystick_last_update_time = millis();
bool Joystick_debug_report_b = false;
void loop() {
	delay(2);
	cycleCntr_u64++;
	//Serial.println(cycleCntr_u64);
	unsigned long joystick_now = millis();
	if(joystick_now-joystick_last_update_time>2000)
	{
		Joystick_debug_report_b= true;
	}
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
#ifdef Using_MCP4728
	if(MCP_status)
	{
		mcp.setChannelValue(MCP4728_CHANNEL_A, (uint16_t)((float)Joystick_value[0]/(float)JOYSTICK_RANGE*0.8f*4096));
		mcp.setChannelValue(MCP4728_CHANNEL_B, (uint16_t)((float)Joystick_value[1]/(float)JOYSTICK_RANGE*0.8f*4096));
		mcp.setChannelValue(MCP4728_CHANNEL_C, (uint16_t)((float)Joystick_value[2]/(float)JOYSTICK_RANGE*0.8f*4096));
	}

#endif

#ifdef Using_analog_output
	dacWrite(Analog_brk,(uint16_t)((float)((Joystick_value[1])/(float)(JOYSTICK_RANGE))*255));
	dacWrite(Analog_gas,(uint16_t)((float)((Joystick_value[2])/(float)(JOYSTICK_RANGE))*255));
#endif
#ifdef JOYSTICK_DEBUG_OUT
	if(Joystick_debug_report_b)
	{
		Joystick_debug_report_b=false;
		joystick_last_update_time=joystick_now;
		Serial.print("Boot in");
		Serial.print(joystick_last_update_time);
		Serial.print(" ms, ");
		Serial.print("RSSI:");
		Serial.print(rssi_display);
		Serial.println(" -dBm");
		int index=0;
		for(index=0;index<3;index++)
		{
			Serial.print("Pedal:");
			Serial.print(index);
			Serial.print(" value:");
			Serial.println(Joystick_value[index]);
		}
	}
#endif
	
	
}