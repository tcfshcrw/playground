#include "Modbus.h"
#include <Arduino.h>

Modbus::Modbus()
{
    this->s = NULL;
    this->mode_ = -1;
}
Modbus::Modbus(HardwareSerial &st)
{
    this->s = &st;
}


bool Modbus::init(int mode, bool en_log)
{
     this->mode_ =  mode;
     this->log   =  en_log;
     //pinMode(mode_,OUTPUT);
     //digitalWrite(mode_, 0);  
     
     return true;
}

void Modbus::setTimeout(uint16_t timeout)
{
  timeout_ = timeout;
}

uint8_t Modbus::byteRead(int index)
{
  return rawRx[index+3];
}

int Modbus::blockRead(int index)
{
   return  ((dataRx[index*2] << 8) | dataRx[index*2+1]);
}

int Modbus::coilRead(int address){
 
    return coilRead(SlaveID,address);
}

int Modbus::coilRead(int id, int address){
   if(requestFrom(id,Coil_Register,address,1))
   {
    uint8_t x = byteRead(0);
    return bitRead(x,0);
   }else
   {
    return -1;
   }
}

int Modbus::discreteInputRead(int address)
{
   return discreteInputRead(SlaveID,address);
}

int Modbus::discreteInputRead(int id, int address)
{
   if(requestFrom(id,Discret_Register,address,1))
   {
    uint8_t x = byteRead(0);
    return bitRead(x,0);
   }else
   {
    return -1;
   }
}



// check target values at register address. If target value was already present, return 0. If target value has to be set, return 1.
void Modbus::readParameter(uint16_t slaveId_local_u16, uint16_t parameterAdress) {

  bool retValue_b = false;

  // check if value at address is already target value
  uint8_t raw2[2];
  uint8_t len;
  int16_t regArray[4];

  // read the four registers simultaneously
  if(requestFrom(slaveId_local_u16, 0x03, parameterAdress,  2) > 0)
  {
    RxRaw(raw2,  len);
    regArray[0] = uint16(0);
  }
  
  // write to public variables
  int16_t returnValue = regArray[0];



  // if value is not target value --> overwrite value
    Serial.print("Parameter address: ");
    Serial.print(parameterAdress);
    Serial.print(",    actual:");
    Serial.println(returnValue);


    delay(50);
}


// check target values at register address. If target value was already present, return 0. If target value has to be set, return 1.
bool Modbus::checkAndReplaceParameter(uint16_t slaveId_local_u16, uint16_t parameterAdress, long value) {

  bool retValue_b = false;

  // check if value at address is already target value
  uint8_t raw2[2];
  uint8_t len;
  int16_t regArray[4];

  // read the four registers simultaneously
  if(requestFrom(slaveId_local_u16, 0x03, parameterAdress,  2) > 0)
  {
    RxRaw(raw2,  len);
    regArray[0] = uint16(0);
  }
  
  // write to public variables
  int16_t returnValue = regArray[0];



  // if value is not target value --> overwrite value
  if(returnValue!= value)
  {
    Serial.print("Parameter adresse: ");
    Serial.print(parameterAdress);
    Serial.print(",    actual:");
    Serial.print(returnValue);
    Serial.print(",    target:");
    Serial.println(value);



    holdingRegisterWrite(slaveId_local_u16, parameterAdress, value); // deactivate auto gain
    delay(50);
    retValue_b = true;
  }

  return retValue_b;
}


long Modbus::holdingRegisterRead(int address)
{
  return holdingRegisterRead(SlaveID, address, 1);
}

long Modbus::holdingRegisterRead(int id, int address, int block)
{
  if(block > 2){block = 2;}
  if(requestFrom(SlaveID, Holding_Register, address, block))
  {
    if(block == 2)
    {
      return (blockRead(0) << 16 | blockRead(1));
    }
    else{
      return blockRead(0);
    }
  }
  else{
    return -1;
  }

}

long Modbus::inputRegisterRead(int address)
{
   return inputRegisterRead(SlaveID , address, 1);
}

long Modbus::inputRegisterRead(int id, int address, int block)
{
  if(block > 2){block = 2;}
  if(requestFrom(id, Input_Register,address,block))
  {
    if(block == 2)
    {
      return (blockRead(0) << 16 | blockRead(1));
    }
    else{
      return blockRead(0);
    }
  }
  else
  {
    return -1;
  }
}






int Modbus::requestFrom(int slaveId, int type, int address, int nb)
{
    
    // address = address - 1;
    int crc ;
    txout[0] = slaveId;
    txout[1] = type;
    txout[2] = address >> 8;
    txout[3] = address;
    txout[4] = nb >> 8;
    txout[5] = nb;
    crc = this->CheckCRC(txout,6);
    txout[6] = crc ;
    txout[7] = crc >> 8;
 
     
    if(log){
      Serial.print("TX: ");
       for(int i =0; i < 8; i++)
            {
                Serial.printf("%02X ",txout[i] );
            }
            Serial.print("\t");
     }

    //digitalWrite(mode_,1);
    //delay(1);
    this->s->write(txout,8);
    this->s->flush();
    //digitalWrite(mode_,0);
    //delay(1);
    uint32_t t = millis();
    lenRx   = 0;
    datalen = 0;
    int ll = 0;
    int rx;
    uint8_t found = 0;
  
    while((millis() - t) < timeout_){
       if(this->s->available())
       {
        rx = this->s->read();
        t = millis();
        
        if(found == 0)
        {
          if(txout[ll] == rx){ll++;}else{ll = 0;}
          if(ll == 2)
          { 
            found = 1; 
          }
        }
        else if(found == 1){
          
         rawRx[0] = txout[0]; // Slave ID
         rawRx[1] = txout[1]; // Function code
         rawRx[2] = rx; // Bytes count to follow + 2 Byte CRC
         lenRx = 3;
         found = 2;
        } 
        else if(found == 2)
        {
         this->rawRx[lenRx++] =  rx;

         // the receive message looks like this
         // Byte 1: SalveId e.g. 0x3F
         // Byte 2: Function code e.g. 0x03
         // Byte 3: Bytes to read e.g. m=8 to read 4 consecutive registers
         // Byte 4-(N-2): Register values
         // Byte N-1: CRC MSB
         // Byte N: CRC LSB

         // The total message length is thus N = 5+m
         if(lenRx >= rawRx[2] + 5) { break; }
        }
        
       }
        

    }

    if(log){
        Serial.print("RX: ");
        for(int i =0; i < lenRx; i++)
            {
             Serial.printf("%02X ",rawRx[i] );
            }
            Serial.println();
     }

    /*Serial.print(lenRx);
    Serial.println();*/


    if(lenRx > 2){
        int crc1 = rawRx[lenRx - 1] <<8 | rawRx[lenRx - 2];
        int crc2 = CheckCRC(rawRx, lenRx - 2);
        //Serial.printf("CRC1: %04X CRC2: %04X\n",crc1, crc2);


        /*Serial.print("CRC1: ");
        Serial.print(crc1);
        Serial.print(",   CRC2: ");
        Serial.print(crc2);
        Serial.println();*/

         if(crc1 == crc2)
          {
            datalen = rawRx[2];
            /*Serial.print("Datalen: ");
            Serial.print(datalen);
            Serial.println();*/
            return datalen;
          }
          else
          { 
            return -1; 
          }
    }else{
        return -1;
    }
}







int Modbus::ReadCoilReg(int add)
{
    return ReadCoilReg(1,  add, 1);
}

int Modbus::ReadCoilReg(int slaveId, int add)
{
    return ReadCoilReg( slaveId, add, 1);
}

int Modbus::ReadCoilReg(int slaveId, int add, int nbit)
{
   if(requestFrom(slaveId,Coil_Register,add,nbit))
   {
    return byteRead(0);
   }else
   {
    return -1;
   }
 
}

int Modbus::ReadDiscretReg(int add)
{
    return ReadDiscretReg(1,add,1);
}

int Modbus::ReadDiscretReg(int slaveId, int add)
{
    return ReadDiscretReg(slaveId,add,1);
}

int Modbus::ReadDiscretReg(int slaveId, int add, int nbit)
{
    if(requestFrom(slaveId,Discret_Register,add,nbit)) {
    return byteRead(0);
   }
   else {
    return -1;
   }
}

int Modbus::ReadHoldingReg(int add)
{
    return 0;
}

int Modbus::ReadHoldingReg(int slaveId, int add)
{
    return 0;
}

int Modbus::ReadHoldingReg(int slaveId, int add, int nbyte)
{
    return 0;
}

int Modbus::ReadInputReg(int add)
{
    return 0;
}

int Modbus::ReadInputReg(int slaveId, int add)
{
    return 0;
}

int Modbus::ReadInputReg(int slaveId, int add, int nbyte)
{
    return 0;
}

int8_t Modbus::uint8(int add)
{
    return rawRx[add*2+3];
}




uint16_t Modbus::uint16(int add)
{
    int add_ = (add)*2 + 3;
 
    return (rawRx[add_] << 8 | rawRx[add_+1]);
}

uint32_t Modbus::uint32(int add, bool byteHL)
{
    uint32_t val ;
    if (byteHL)
    {
      val = uint16(add) << 16 | uint16(add+1);
    }
    else
    {
      val = uint16(add+1)<< 16 | uint16(add);
    }
    return val;
}

void Modbus::RxRaw(uint8_t*raw, uint8_t &rlen)
{
   
   
   for(int i =0; i < lenRx; i++)
    {
      raw[i] = rawRx[i];
      //  if(rawRx[i] < 16)
      //    {
      //      Serial.print("0");
      //     }
      //     Serial.print(rawRx[i],HEX);
    }
     rlen = this->lenRx;
    //  Serial.println(rlen);
}


void Modbus::TxRaw(uint8_t*raw, uint8_t &rlen)
{
   
   
   for(int i =0; i < 8; i++)
    {
      raw[i] = txout[i];
      //  if(rawRx[i] < 16)
      //    {
      //      Serial.print("0");
      //     }
      //     Serial.print(rawRx[i],HEX);
    }
     rlen = 8;
    //  Serial.println(rlen);
}


int Modbus::CheckCRC(uint8_t*buf, int len)
{
  int nominal = 0xA001;
  int crc = 0xFFFF;
  unsigned char pos,i;
 
  for ( pos = 0; pos < len; pos++) {
    crc ^= (unsigned int)buf[pos];          // XOR byte into least sig. byte of crc
 
    for (i = 8; i != 0; i--) {        // Loop over each bit
      if ((crc & 0x0001) != 0) {      // If the LSB is set
        crc >>= 1;                    // Shift right and XOR 0xA001
        crc ^= nominal;
      }
      else                            // Else LSB is not set
        crc >>= 1;                    // Just shift right
    }
  }
  // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
  return crc;  
}


    
int Modbus::holdingRegisterWrite(int id, int address, uint16_t value)
{
    int crc ;
	
	// form signal
    txout[0] = id;
    txout[1] = Write_Holding_Register;
    txout[2] = address >> 8;
    txout[3] = address;
    txout[4] = value >> 8;
    txout[5] = value;
    crc = this->CheckCRC(txout,6);
    txout[6] = crc ;
    txout[7] = crc >> 8;
	
	// send signal
	digitalWrite(mode_,1);
    delay(1);
    this->s->write(txout,8);
    this->s->flush();
    digitalWrite(mode_,0);
    delay(1);
	
	
	return 1;
	
	
	
	
}