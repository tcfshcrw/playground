#include "isv57communication.h"
#include "Main.h"

Modbus modbus(Serial1);


// initialize the communication
isv57communication::isv57communication()
{
  Serial1.begin(38400, SERIAL_8N1, ISV57_RXPIN, ISV57_TXPIN, true); // Modbus serial
  modbus.init(MODE);
}




// send tuned servo parameters
void isv57communication::setupServoStateReading() {

  
  // The iSV57 has four registers (0x0191, 0x0192, 0x0193, 0x0194) in which we can write, which values we want to obtain cyclicly
  // These registers can be obtained by sending e.g. the command: 0x63, 0x03, 0x0191, target_sate, CRC
  // tell the modbus slave, which registers will be read cyclicly
  modbus.holdingRegisterWrite(slaveId, 0x0191, reg_add_position_given_p);
  delay(50);
  modbus.holdingRegisterWrite(slaveId, 0x0192, reg_add_velocity_current_feedback_percent);
  delay(50);
  modbus.holdingRegisterWrite(slaveId, 0x0193, reg_add_position_error_p);
  delay(50);
  modbus.holdingRegisterWrite(slaveId, 0x0194, reg_add_voltage_0p1V);
  delay(50);


}


void isv57communication::readAllServoParameters() {
  for (uint16_t reg_sub_add_u16 = 0;  reg_sub_add_u16 < pr_6_00; reg_sub_add_u16++)
  {
    modbus.readParameter(slaveId, pr_0_00 + reg_sub_add_u16);
  }
}

// send tuned servo parameters
void isv57communication::sendTunedServoParameters() {
  
  bool retValue_b = false;


  // Pr0 register
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+1, 0); // control mode 
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+2, 0); // deactivate auto gain
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+3, 10); // machine stiffness
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+4, 80); // ratio of inertia
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+8, 1600); // microsteps
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+9, 1); // 1st numerator 
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+10, 1); // & denominator
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+13, 500); // 1st torque limit
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+14, 500); // position deviation setup
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+16, 50); // regenerative braking resitor
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+17, 50);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+18, 0); // vibration suppression
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_0_00+19, 0);

  // Pr1 register
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+0, 600); // 1st position gain
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+1, 300); // 1st velocity loop gain
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+2, 300); // 1st time constant of velocity loop
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+3, 15); // 1st filter of velocity detection
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+4, 150); // 1st torque filter
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+10, 200); // velocity feed forward gain
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+11, 6000); // velocity feed forward filter
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+12, 0); // torque feed forward gain
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+13, 0); // torque feed forward filter
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+15, 0); // control switching mode
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+33, 0); // speed given filter
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+35, 0); // position command filter
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+36, 0); // encoder feedback
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_1_00+37, 1052); // special function register
  // see https://www.oyostepper.com/images/upload/File/ISV57T-180.pdf
  // 0x01 = 1: velocity feedforward disabled
  // 0x02 = 2: torque feedforward disabled
  // 0x04 = 4: motor overspeed alarm disabled
  // 0x08 = 8: position following alarm disabled
  // 0x10 = 16: overload alarm disabled
  // 0x400 = 1024: undervoltage disabled

  // Pr2 register
  // vibration suppression 
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+1, 50);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+2, 20);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+3, 99);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+4, 90);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+5, 20);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+6, 99);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+22, 0);
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_2_00+23, 0);

  // Pr3 register
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_3_00+24, 5000); // maximum rpm
  
  // Pr5 register
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_5_00+13, 5000); // overspeed level
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_5_00+20, 1); // encoder output resolution

  // Enable & tune reactive pumping. This will act like a braking resistor and reduce EMF voltage.
  // See https://en.wikipedia.org/wiki/Bleeder_resistor
  // Info from iSV2 manual: The external resistance is activated when the actual bus voltage is higher than Pr7.32 plus Pr7.33 and is deactivated when the actual bus voltage is lower than Pr7.32 minus Pr7.33
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_7_00+31, 0); // bleeder control mode; 0: is default and seems to enable braking mode, contrary to manual
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_7_00+32, 40); // bleeder braking voltage. Voltage when braking is activated
  retValue_b |= modbus.checkAndReplaceParameter(slaveId, pr_7_00+33, 1); // bleeder hysteresis voltage; Contrary to the manual this seems to be an offset voltage, thus Braking disabling voltage = Pr7.32 + Pr.33
  

  // store the settings to servos NVM if necesssary
  if (retValue_b)
  {
    Serial.println("Servo registered in NVM have been updated! Please power cycle the servo and the ESP!");
    modbus.holdingRegisterWrite(slaveId, 0x019A, 0x5555); // store the settings to servos NVM
    delay(2000);
  }

}

bool isv57communication::findServosSlaveId()
{
  bool slaveIdFound = false;
  for (int slaveIdTest = 0; slaveIdTest<256; slaveIdTest++)
  {
      if(modbus.requestFrom(slaveIdTest, 0x03, 0x0000, 2) > 0)
      {
        slaveId = slaveIdTest;
        slaveIdFound = true;
        Serial.print("Found servo slave ID:");
        Serial.print(slaveId);
        Serial.print("\r\n");
        break;
      }
  }
  return slaveIdFound;
}




bool isv57communication::checkCommunication()
{
  if(modbus.requestFrom(slaveId, 0x03, 0x0000, 2) > 0)
  {
    //Serial.println("Lifeline check: true");
    return true;
  }
  else
  {
    //Serial.println("Lifeline check: false");
    return false;
  }
}



void isv57communication::setZeroPos()
{
  zeroPos = servo_pos_given_p;
}

void isv57communication::applyOfsetToZeroPos(int16_t givenPosOffset_i16)
{
  zeroPos += givenPosOffset_i16;
}

int16_t isv57communication::getZeroPos()
{
  return zeroPos;
}


// read servo states
void isv57communication::readServoStates() {

  // read the four registers simultaneously
  int8_t numberOfRegistersToRead_u8 = 4;
  int bytesReceived_i = modbus.requestFrom(slaveId, 0x03, ref_cyclic_read_0, numberOfRegistersToRead_u8);
  if(bytesReceived_i == (numberOfRegistersToRead_u8*2))
  {
    modbus.RxRaw(raw,  len);
    for (uint8_t regIdx = 0; regIdx < numberOfRegistersToRead_u8; regIdx++)
    { 
      regArray[regIdx] = modbus.uint16(regIdx);
    }

    // write to public variables
    servo_pos_given_p = regArray[0];
    servo_current_percent = regArray[1];
    servo_pos_error_p = regArray[2];
    servo_voltage_0p1V = regArray[3];
  }
  //Serial.print("Bytes :");
  //Serial.println(bytesReceived_i);
  
  
  
  // print registers
  if (0)
  {
    Serial.print("Pos_given:");
    Serial.print(servo_pos_given_p);

    Serial.print(",Pos_error:");
    Serial.print(servo_pos_error_p);

    Serial.print(",Cur_given:");
    Serial.print(servo_current_percent);

    Serial.print(",Voltage:");
    Serial.print(servo_voltage_0p1V);

    Serial.println(" "); 
  }
  
}



bool isv57communication::clearServoAlarms() {

  // read the alarm list
  int8_t numberOfRegistersToRead_u8 = 0;
  // Alarm register address: 0x02
  //int bytesReceived_i = modbus.requestFrom(slaveId, 0x03, 0x02, numberOfRegistersToRead_u8);

  // clear alarm list
  modbus.holdingRegisterWrite(slaveId, 0x019a, 0x7777); 
    
  return 1;
}
