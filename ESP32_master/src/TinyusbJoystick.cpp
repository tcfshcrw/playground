#include "TinyusbJoystick.h"
TinyusbJoystick* TinyusbJoystick::instance = nullptr;
TinyusbJoystick::TinyusbJoystick() 
{    
    isBridgeActionGet=false;
}

bool TinyusbJoystick::IsReady()
{
    bool returnValue_b = true;
    if (!TinyUSBDevice.mounted())
    {
        returnValue_b = false;
    }
    if (!usb_hid.ready())
    {
        returnValue_b = false;
    }

    return returnValue_b;
}

void TinyusbJoystick::begin(int VID, int PID)
{
    instance=this;
    //Serial.println("[L]starting USB joystick");
    TinyUSBDevice.setID(VID, PID);
    TinyUSBDevice.setProductDescriptor("DIY_FFB_PEDAL_JOYSTICK");
    TinyUSBDevice.setManufacturerDescriptor("OPENSOURCE");
    
    //ActiveSerial->
    // Manual begin() is required on core without built-in support e.g. mbed rp2040
    if (!TinyUSBDevice.isInitialized())
    {
        TinyUSBDevice.begin(0);
    }

    // Setup HID
    usb_hid.enableOutEndpoint(true); 
    usb_hid.setPollInterval(2); // time in ms
    usb_hid.setReportDescriptor(desc_hid_report, sizeof(desc_hid_report));
    usb_hid.begin();
    usb_hid.setReportCallback(NULL, TinyusbJoystick::context_callback);

    // If already enumerated, additional class driverr begin() e.g msc, hid, midi won't take effect until re-enumeration
    if (TinyUSBDevice.mounted())
    {
        TinyUSBDevice.detach();
        delay(10);
        TinyUSBDevice.attach();
        
    }
}

void TinyusbJoystick::setRxAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.rx = tmp;
}

void TinyusbJoystick::setRyAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.ry = tmp;
}
void TinyusbJoystick::setRzAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.rz = tmp;
}

void TinyusbJoystick::setXAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.x = tmp;
}
void TinyusbJoystick::setYAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.y = tmp;
}
void TinyusbJoystick::setZAxis(int32_t value)
{
    int32_t tmp = value;
    hid_report.z = tmp;
}

void TinyusbJoystick::sendState()
{
    usb_hid.sendReport(JOYSTICK_STRUCT, &hid_report, sizeof(hid_report));
}

void TinyusbJoystick::sendLargeData(uint8_t* data, size_t totalLen) 
{
    uint8_t packet[PACKET_SIZE];
    size_t offset = 0;
    
    while (offset < totalLen) {
        memset(packet, 0, PACKET_SIZE);
        size_t chunkLen = totalLen - offset;
        if (chunkLen > PAYLOAD_SIZE) {
            chunkLen = PAYLOAD_SIZE;
        }

        uint8_t type;
        if (offset == 0) {
            type = PKT_TYPE_START; 
        } else if (offset + chunkLen >= totalLen) {
            type = PKT_TYPE_END;   
        } else {
            type = PKT_TYPE_CONT;  
        }
        packet[0] = type;
        packet[1] = (uint8_t)totalLen;
        packet[2] = (uint8_t)chunkLen;
        memcpy(&packet[HEADER_SIZE], &data[offset], chunkLen);

        usb_hid.sendReport(HID_PAYLOAD_INPUT, &packet[0], PACKET_SIZE); // 注意: TinyUSB 參數微調
        
        offset += chunkLen;
        delay(2); 
    }
    if (totalLen <= PAYLOAD_SIZE) {
        memset(packet, 0, PACKET_SIZE);
        //packet[0] = HID_PAYLOAD;
        packet[0] = PKT_TYPE_END;
        packet[1] = totalLen; 
        packet[2] = 0; 
        usb_hid.sendReport(HID_PAYLOAD_INPUT, &packet[0], PACKET_SIZE);
    }
}

void TinyusbJoystick::onHIDReceived(uint8_t report_id, hid_report_type_t report_type, uint8_t const* buffer, uint16_t bufsize) 
{
    //isGetData=true;
    buffSizeDis=bufsize;
    reportID=report_id;
    reportType=report_type;
    memcpy(&buffDis[0], &buffer[0], bufsize);
    if (report_type != HID_REPORT_TYPE_OUTPUT || buffer[0] != HID_PAYLOAD_OUTPUT) return;
    if (bufsize < HEADER_SIZE) return;

    uint8_t type = buffer[1];
    uint8_t totallen = buffer[2];
    uint8_t len = buffer[3];
    rawLength= totallen;
    if (len > PAYLOAD_SIZE) len = PAYLOAD_SIZE;
    if (type == PKT_TYPE_START) {
        rxIndex = 0;
        isReceiving = true;
    }

    if (isReceiving) {
        if (rxIndex + len > sizeof(rxBuffer)) {
            isReceiving = false; 
            return;
        }
        if (len > 0) {
            memcpy(&rxBuffer[rxIndex], &buffer[HEADER_SIZE_GET], len);
            rxIndex += len;
        }
        if (type == PKT_TYPE_END) 
        {
            isReceiving = false;
            
            ProcessFullData(rxBuffer, totallen);
        }
    }
}
void TinyusbJoystick::context_callback(uint8_t report_id, hid_report_type_t report_type, uint8_t const* buffer, uint16_t bufsize) 
{
    if (instance != nullptr) {
        instance->onHIDReceived(report_id, report_type, buffer, bufsize);
    }
}

void TinyusbJoystick::ProcessFullData(uint8_t *rxBuffer, uint8_t totalLen)
{
    if(totalLen== sizeof(DAP_actions_st))
    {
        DAP_actions_st tmp;
        memcpy(&tmp, rxBuffer, totalLen);
        bool structChecker= true;
        if(tmp.payLoadHeader_.payloadType !=DAP_PAYLOAD_TYPE_ACTION ) structChecker = false;
        if(tmp.payLoadHeader_.version !=DAP_VERSION_CONFIG ) structChecker = false;
        uint16_t crc = checksumCal((uint8_t*)(&(tmp.payLoadHeader_)), sizeof(tmp.payLoadHeader_) + sizeof(tmp.payloadPedalAction_));
        if(crc != tmp.payloadFooter_.checkSum) structChecker = false;
        if(structChecker)
        {
            uint8_t pedalTag= tmp.payLoadHeader_.PedalTag;
            memcpy( &tmpAction[pedalTag], &tmp,totalLen);
            isActionGet[pedalTag]= true;
        }

    }
    if(totalLen== sizeof(DAP_config_st))
    {
        DAP_config_st tmp;
        memcpy(&tmp, rxBuffer, totalLen);
        bool structChecker= true;
        if(tmp.payLoadHeader_.payloadType !=DAP_PAYLOAD_TYPE_CONFIG ) structChecker = false;
        if(tmp.payLoadHeader_.version !=DAP_VERSION_CONFIG ) structChecker = false;
        uint16_t crc = checksumCal((uint8_t*)(&(tmp.payLoadHeader_)), sizeof(tmp.payLoadHeader_) + sizeof(tmp.payLoadPedalConfig_));
        if(crc != tmp.payloadFooter_.checkSum) structChecker = false;
        if(structChecker)
        {
            uint8_t pedalTag= tmp.payLoadHeader_.PedalTag;
            memcpy(&tmpConfig[pedalTag], &tmp,  totalLen);
            isConfigGet[pedalTag]= true;
        }

    }
    if(totalLen== sizeof(DAP_bridge_state_st))
    {
        DAP_bridge_state_st tmp;
        memcpy(&tmp, rxBuffer, totalLen);
        bool structChecker= true;
        if(tmp.payLoadHeader_.payloadType !=DAP_PAYLOAD_TYPE_BRIDGE_STATE ) structChecker = false;
        if(tmp.payLoadHeader_.version !=DAP_VERSION_CONFIG ) structChecker = false;
        uint16_t crc = checksumCal((uint8_t*)(&(tmp.payLoadHeader_)), sizeof(tmp.payLoadHeader_) + sizeof(tmp.payloadBridgeState_));
        if(crc != tmp.payloadFooter_.checkSum) structChecker = false;
        if(structChecker)
        {
            memcpy(&tmpBridgeAction, &tmp, totalLen);
            isBridgeActionGet= true;
        }
        
    }
    if(totalLen == sizeof(DAP_action_ota_st))
    {
        DAP_action_ota_st tmp;
        memcpy(&tmp, rxBuffer, totalLen);
        bool structChecker= true;
        if(tmp.payLoadHeader_.payloadType !=DAP_PAYLOAD_TYPE_ACTION_OTA ) structChecker = false;
        if(structChecker)
        {
            memcpy(&tmpOtaAction, &tmp, totalLen);
            isOtaActionGet = true;
        }
    }
}

uint16_t TinyusbJoystick::checksumCal(uint8_t * data, uint16_t length)
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

