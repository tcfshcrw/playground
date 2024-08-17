
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


#define SerialOutput
/**********************************************************************************************/
/*                                                                                            */
/*                         controller  definitions                                            */
/*                                                                                            */
/**********************************************************************************************/
#define ACTIVATE_JOYSTICK_OUTPUT
#ifdef ACTIVATE_JOYSTICK_OUTPUT
#include "Controller.h"
#endif

/**********************************************************************************************/
/*                                                                                            */
/*                         ESPNOW definitions                                                 */
/*                                                                                            */
/**********************************************************************************************/
//#include <esp_now.h>
#define Using_MCP4728
#include <WiFi.h>
#include <esp_wifi.h>
#include <ESPNowW.h>
#include "Wire.h"
#include "SPI.h"
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
void OnDataRecv(const uint8_t *mac_addr, const uint8_t *incomingData, int len) {

//void OnDataRecv(const esp_now_recv_info *info, const uint8_t *incomingData, int len) {
	if(len==sizeof(myData))
	{
		memcpy(&myData, incomingData, sizeof(myData));
		pedal_status=myData.pedal_status;
		#ifdef ACTIVATE_JOYSTICK_OUTPUT
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
		#else
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
	
	// send controller output
	
	//if (IsControllerReady()) 
	//{	
		// check whether sender was clutch, brake or throttle
		//check last macadress to identufy pedal
		//if(mac_addr[5]==0x32)
		//{
		//	SetControllerOutputValueAccelerator(joystickNormalizedToInt32);
		//}
		//if(mac_addr[5]==0x33)
		//{
		//	SetControllerOutputValueBrake(joystickNormalizedToInt32);
		//}
		//if(mac_addr[5]==0x34)
		//{
			//SetControllerOutputValueThrottle(joystickNormalizedToInt32);
		//}
		//boolean clutchCheck_b = true;
		//boolean brakeCheck_b = true;
		//boolean throttleCheck_b = true;

		// Check if sender was brake, thottle or cluth
		//for (uint8_t byteIdx_u8 = 0; byteIdx_u8 < 6; byteIdx_u8++ )
		//{
		//	clutchCheck_b &= info->src_addr[byteIdx_u8] == Clu_mac[byteIdx_u8];
		//	brakeCheck_b &= info->src_addr[byteIdx_u8] == Brk_mac[byteIdx_u8];
		//	throttleCheck_b &= info->src_addr[byteIdx_u8] == Gas_mac[byteIdx_u8];
		//}

		/*
		if (clutchCheck_b)
		{
			SetControllerOutputValueAccelerator(joystickNormalizedToInt32);
		}

		if (brakeCheck_b)
		{
			SetControllerOutputValueBrake(joystickNormalizedToInt32);
		}

		if (throttleCheck_b)
		{
			SetControllerOutputValueThrottle(joystickNormalizedToInt32);
		}
		*/

		//joystickSendState();
		
	//}
	//Serial.print("controllerValue_i32: ");
	//Serial.println(myData.controllerValue_i32);	





	//Serial.print("Bytes received: ");
	//Serial.println(len);
	//Serial.print("CycleCnt: ");
	//Serial.println(myData.cycleCnt_u64);
	//Serial.print("TimeSinceBoot in ms (shared): ");
	//Serial.println(myData.timeSinceBoot_i64);
	//Serial.print("controllerValue_i32: ");
	//Serial.println(myData.controllerValue_i32);	
	//Serial.println();



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
	esp_wifi_config_espnow_rate(WIFI_IF_STA, WIFI_PHY_RATE_MCS0_LGI);
	delay(3000);
	ESPNow.add_peer(Clu_mac);
	ESPNow.add_peer(Brk_mac);
	ESPNow.add_peer(Gas_mac);
	

	
	// Register for a callback function that will be called when data is received
	//esp_now_register_recv_cb(esp_now_recv_cb_t(OnDataRecv));
	ESPNow.reg_recv_cb(OnDataRecv);
	Serial.println("ESPNow Comunication Starting");
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

}




int32_t joystickNormalizedToInt32_local = 0;






/**********************************************************************************************/
/*                                                                                            */
/*                         Main function                                                      */
/*                                                                                            */
/**********************************************************************************************/
uint64_t cycleCntr_u64 = 0;
void loop() {
	delay(2);
	cycleCntr_u64++;
	//Serial.println(cycleCntr_u64);
	
	
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
#ifdef Using_MCP4728
if(MCP_status)
	{
		mcp.setChannelValue(MCP4728_CHANNEL_A, (uint16_t)((float)Joystick_value[0]/(float)JOYSTICK_RANGE*4096));
		mcp.setChannelValue(MCP4728_CHANNEL_B, (uint16_t)((float)Joystick_value[1]/(float)JOYSTICK_RANGE*4096));
		mcp.setChannelValue(MCP4728_CHANNEL_C, (uint16_t)((float)Joystick_value[2]/(float)JOYSTICK_RANGE*4096));
	}

#endif
	
	
}