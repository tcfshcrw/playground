// FanatecInterface.h

#ifndef FANATEC_INTERFACE_H
#define FANATEC_INTERFACE_H

#include <Arduino.h>
#include <HardwareSerial.h>

class FanatecInterface {
public:
    // Constructor
    FanatecInterface(int rxPin, int txPin);

    // Initialization function
    void begin();

    // Communication update function (to be called periodically in the loop)
    void update();
    void communicationUpdate();

    // Functions to set pedal values
    void setThrottle(uint16_t value);
    void setBrake(uint16_t value);
    void setClutch(uint16_t value);
    void setHandbrake(uint16_t value);

    // Function to set the connection callback
    void onConnected(void (*callback)());

    // Check if connected to the Fanatec device
    bool isConnected();

private:
    // Internal helper functions
    void performCommunicationSteps();
    void changeBaudRate(unsigned long baudrate);
    void makeCRCTable(uint8_t poly);
    uint8_t generateCRC(uint8_t* input, size_t length);
    void createPacket(uint8_t* packet);

    // UART pin settings
    int _rxPin;
    int _txPin;

    // HardwareSerial object
    HardwareSerial* _serial;

    // CRC table
    uint8_t _crcTable[256];

    // Pedal values
    uint16_t _throttle;
    uint16_t _brake;
    uint16_t _clutch;
    uint16_t _handbrake;

    // Connection status
    bool _connected;

    // Connection callback function pointer
    void (*_connectedCallback)();

    // Initialization flag
    bool _initialized;
};

#endif // FANATEC_INTERFACE_H