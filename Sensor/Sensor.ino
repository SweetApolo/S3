//CODIGO CHECKSUM
unsigned int uiCrc16Cal(unsigned char const *pucY, unsigned char ucX)
{
  const uint16_t PRESET_VALUE = 0xFFFF;
  const uint16_t POLYNOMIAL = 0x8408;


  unsigned char ucI, ucJ;
  unsigned short int uiCrcValue = PRESET_VALUE;

  for (ucI = 0; ucI < ucX; ucI++)
  {
    uiCrcValue = uiCrcValue ^ *(pucY + ucI);
    for (ucJ = 0; ucJ < 8; ucJ++)
    {
      if (uiCrcValue & 0x0001)
      {
        uiCrcValue = (uiCrcValue >> 1) ^ POLYNOMIAL;
      }
      else
      {
        uiCrcValue = (uiCrcValue >> 1);
      }
    }
  }
  return uiCrcValue;
}
/////

//Bytes --> Float
float getFloat(byte packet[], byte i){
  union u_tag{

    byte bin[4];
    float num;
    
  } 

  N;
  N.bin[0] = packet[i];
  N.bin[1] = packet[i+1];
  N.bin[2] = packet[i+2];
  N.bin[3] = packet[i+3];

  return N.num;
  
}


void taskCom(){
  static uint8_t state = 1;
  static float N1 = 0.0;
  static float N2 = 0.0;
  static float R = 0.0;
  static uint8_t arrR[6] = {0};
  static uint8_t bufferRx[11] = {0};
  static uint8_t dataCounter = 0;
  static uint8_t arrC[2] = {0};
  static uint8_t arrCs[2] = {0};

  switch (state){
    case 1:
    
      while(Serial.available()){
//Datos lectura
        uint8_t dataRx = Serial.read();

        if(dataCounter >= 10){

          bufferRx[dataCounter] = dataRx;
          dataCounter = 0;

          uint16_t Cp = uiCrc16Cal(bufferRx,9);
          memcpy(arrC,(uint8_t *)&Cp, 2);
//Comparacion Checksum Arduino y Checksum C#
          if(arrC[0] == bufferRx[9] and arrC[1] == bufferRx[10]){

            state = 2;
          }
        }
        else{

          bufferRx[dataCounter] = dataRx;
          dataCounter++;
        }
      }
      break;
      
    case 2:
      N1 = getFloat(bufferRx, 0);
      N2 = getFloat(bufferRx, 4);

      if(((char)bufferRx[8])== '+'){
      R = N1 + N2;
  } else if (((char)bufferRx[8])== '-'){
    R = N1 - N2;
  } else if (((char)bufferRx[8])== '*'){
    R = N1 * N2;
  } else if (((char)bufferRx[8])== '/'){
    R = N1 / N2;
  }

  state = 3;
  break;

//Checksum y enviar datos
  case 3:
    memcpy(arrR, (uint8_t *)&R, 4);

    uint16_t Cs = uiCrc16Cal(arrR,4);
    memcpy(arrCs, (uint8_t *)&Cs, 2);
    arrR[4] = arrCs[0];
    arrR[5] = arrCs[1];

    Serial.write(arrR, 6);
    state = 1;
    break;
    
   default:

    break;
  }
}

void setup() {

  Serial.begin(115200);
}

void loop() {
  taskCom();
}
