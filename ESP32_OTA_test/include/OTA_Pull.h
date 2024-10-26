#include <Arduino.h>
#include <ESP32OTAPull.h>
#include <esp_wifi.h>

// First, edit these values appropriately
#if PCB_VERSION==5
	#define JSON_URL   "https://raw.githubusercontent.com/tcfshcrw/playground/main/OTA/Gilphilbert_control_board_1_2/Version.json"
#endif
#if PCB_VERSION==6
	#define JSON_URL   "https://raw.githubusercontent.com/tcfshcrw/playground/main/OTA/dev_kit/Version.json"
#endif
#if PCB_VERSION==7
	#define JSON_URL   "https://raw.githubusercontent.com/tcfshcrw/playground/main/OTA/Gilphilbert_dongle/Version.json"
#endif
#if PCB_VERSION==8
	#define JSON_URL   "https://raw.githubusercontent.com/tcfshcrw/playground/main/OTA/FH4R2/Version.json"
#endif
#define SSID 	   "ASUS-4F-2"
#define PASS       "26634489"
#define VERSION    "0.0.0" // The current version of this program



void callback(int offset, int totallength);

const char *errtext(int code)
{
	switch(code)
	{
		case ESP32OTAPull::UPDATE_AVAILABLE:
			return "An update is available but wasn't installed";
		case ESP32OTAPull::NO_UPDATE_PROFILE_FOUND:
			return "No profile matches";
		case ESP32OTAPull::NO_UPDATE_AVAILABLE:
			return "Profile matched, but update not applicable";
		case ESP32OTAPull::UPDATE_OK:
			return "An update was done, but no reboot";
		case ESP32OTAPull::HTTP_FAILED:
			return "HTTP GET failure";
		case ESP32OTAPull::WRITE_ERROR:
			return "Write error";
		case ESP32OTAPull::JSON_PROBLEM:
			return "Invalid JSON";
		case ESP32OTAPull::OTA_UPDATE_FAIL:
			return "Update fail (no OTA partition?)";
		default:
			if (code > 0)
				return "Unexpected HTTP response code";
			break;
	}
	return "Unknown error";
}

void connectToWiFi() {
  // Begin connecting to WiFi using the provided SSID and password
  //esp_wifi_set_max_tx_power(WIFI_POWER_8_5dBm);
  WiFi.begin(SSID, PASS);

  // Display connection progress
  Serial.print("Connecting to WiFi");
  
  // Wait until WiFi is connected
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  
  // Print confirmation message when WiFi is connected
  Serial.println("WiFi connected");
  Serial.print("WiFi RSSI: ");
  Serial.println(WiFi.RSSI());
  
}

void DisplayInfo()
{
	char exampleImageURL[256];
	snprintf(exampleImageURL, sizeof(exampleImageURL), "https://example.com/Basic-OTA-Example-%s-%s.bin", ARDUINO_BOARD, VERSION);

	Serial.printf("Basic-OTA-Example v%s\n", VERSION);
	Serial.printf("You need to post a JSON (text) file similar to this:\n");
	Serial.printf("{\n");
	Serial.printf("  \"Configurations\": [\n");
	Serial.printf("    {\n");
	Serial.printf("      \"Board\": \"%s\",\n", ARDUINO_BOARD);
	Serial.printf("      \"Device\": \"%s\",\n", WiFi.macAddress().c_str());
	Serial.printf("      \"Version\": \"%s\",\n", VERSION);
	Serial.printf("      \"URL\": \"%s\"\n", exampleImageURL);
	Serial.printf("    }\n");
	Serial.printf("  ]\n");
	Serial.printf("}\n");
	Serial.printf("\n");
	Serial.printf("(Board, Device, Config, and Version are all *optional*.)\n");
	Serial.printf("\n");
	Serial.printf("Post the JSON at, e.g., %s\n", JSON_URL);
	Serial.printf("Post the compiled bin at, e.g., %s\n\n", exampleImageURL);
}

void callback(int offset, int totallength)
{
	Serial.printf("Updating %d of %d (%02d%%)...\n", offset, totallength, 100 * offset / totallength);
}