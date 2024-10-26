#include <Arduino.h>

struct Basic_WIfi_info
{ 
    uint8_t wifi_action;
    uint8_t mode_select;
    uint8_t SSID_Length;
    uint8_t PASS_Length;
    uint8_t WIFI_SSID[30];
    uint8_t WIFI_PASS[30];
};

Basic_WIfi_info _basic_wifi_info;
char* SSID;
char* PASS;

void wifi_initialized(char* Wifi_SSID, char* Wifi_PASS)
{
    Serial.print("[L]SSID: ");
    Serial.print(Wifi_SSID);
    Serial.print(" PASS: ");
    Serial.println(Wifi_PASS);

}
