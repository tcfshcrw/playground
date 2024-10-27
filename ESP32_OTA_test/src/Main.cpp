
#include <WiFi.h>
#include <SPIFFS.h>
#include "Update.h"
#include <WiFiClientSecure.h>
#include "OTA_Pull.h"

void setup() {
  #if PCB_VERSION==5 || PCB_VERSION==8
    Serial.setTxTimeoutMs(0);
  #endif

  
  Serial.begin(921600);
  delay(10000);
  /*
  if (!SPIFFS.begin(true)) {
    Serial.println("SPIFFS Mount Failed");
    return;
  }
  */

  connectToWiFi();
  //DisplayInfo();


	// First example: update should NOT occur, because Version string in JSON matches local VERSION value.
	ESP32OTAPull ota;

	ota.SetCallback(callback);
	//Serial.printf("We are running version %s of the sketch, Board='%s', Device='%s'.\n", VERSION, ARDUINO_BOARD, WiFi.macAddress().c_str());
	Serial.printf("Checking %s to see if an update is available...\n", JSON_URL);
	int ret = ota.CheckForOTAUpdate(JSON_URL, VERSION);
	Serial.printf("CheckForOTAUpdate returned %d (%s)\n\n", ret, errtext(ret));

	delay(3000);

	// Second example: update *will* happen because we are pretending we have an earlier version
  /*
	Serial.printf("But if we pretend like we're running version 0.0.0, we SHOULD see an update happen.\n");
	ret = ota.CheckForOTAUpdate(JSON_URL, "0.0.0");
	Serial.printf("(If the update succeeds, the reboot should prevent us ever getting here.)\n");
	Serial.printf("CheckOTAForUpdate returned %d (%s)\n\n", ret, errtext(ret));
  */

}

void loop() {
  // Nothing to do here
}














