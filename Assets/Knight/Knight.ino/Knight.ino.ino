/**************************************************************************************** 
 * Wakaka Studio 2017
 * Author: iLYuSha Dawa-mumu Wakaka Kocmocovich Kocmocki KocmocA
 * Project: Knight
 * Tools: Unity 5.6 + Arduino Mega2560
 * Last Updated: 2017/11/06
 ****************************************************************************************/

#include <SPI.h>//include the SPI bus library
#include <MFRC522.h>//include the RFID reader library

#define SS_PIN 10  //slave select pin
#define RST_PIN 9  //reset pin
MFRC522 mfrc522(SS_PIN, RST_PIN);        // instatiate a MFRC522 reader object.
MFRC522::MIFARE_Key key;//create a MIFARE_Key struct named 'key', which will hold the card information

#define SHOW_TAG_ID
/* RFID Variable */
int tag[4];
int escape = 0;
/* Constant for Tag ID */
int tagPlayer[12][4] = 
{
  {100,96,165,213},{22,144,168,88}, {213,77,113,99}, {249,118,197,13},
  {133,151,139,9}, {179,120,111,133},{218,40,170,213},{106,15,165,213},
  {6,111,177,2}, {249,97,115,13}, {133,170,40,9}, {9,127,102,14}
};
int tagPlayerA[4] = {100,96,165,213}; // 6460A5D5
int tagPlayerB[4] = {22,144,168,88}; // 1690A858
int tagPlayerC[4] = {213,77,113,99}; // D54D7163
int tagPlayerD[4] = {249,118,197,13}; // F976C5D
int tagPlayerE[4] = {133,151,139,9}; // 85978B9
int tagPlayerF[4] = {179,120,111,133}; // B3786F85
int tagPlayerG[4] = {218,40,170,213}; // DA28AAD5
int tagPlayerH[4] = {106,15,165,213}; // 6AFA5D5
int tagPlayerW[4] = {6,111,177,2}; // 66FB12
int tagPlayerX[4] = {249,97,115,13}; // F96173D
int tagPlayerY[4] = {133,170,40,9}; // 85AA289
int tagPlayerZ[4] = {9,127,102,14}; // 97F66E

void setup() 
{
  Serial.begin(9600);        // Initialize serial communications with the PC
  SPI.begin();               // Init SPI bus
  mfrc522.PCD_Init();        // Init MFRC522 card (in case you wonder what PCD means: proximity coupling device)
  
  for (byte i = 0; i < 6; i++) {
          key.keyByte[i] = 0xFF;//keyByte is defined in the "MIFARE_Key" 'struct' definition in the .h file of the library
  }

  Serial.println("Knight Game 2017.04.11 iLYuSha");
}

int block=2;//this is the block number we will write into and then read. Do not write into 'sector trailer' block, since this can make the block unusable.
byte blockcontent[16] = {"makecourse_____"};//an array with 16 bytes to be written into one of the 64 card blocks is defined
byte readbackblock[18];//This array is used for reading out a block. The MIFARE_Read method requires a buffer that is at least 18 bytes to hold the 16 bytes of a block.

void loop()
{
    if(escape == -1)
    {
        for (int i = 0; i < sizeof(tagPlayer)/sizeof(tagPlayer[0]); i++)
        {
            int tagPlayerDetect[4] = {tagPlayer[i][0],tagPlayer[i][1],tagPlayer[i][2],tagPlayer[i][3]};
            if(CheckTagID(tagPlayerDetect))
            {
                Serial.print("is Player ");
                Serial.println(i+1);
//                Serial.print(" ");
//                ShowTagID();
            }
        }

    }
  /* 重啟機制 */
  /********************************************* 
   *  避免RFID當掉的自動重啟方法
  **********************************************/
  else if(escape > 300)
  {
    escape = 0;
    mfrc522.PCD_Init();
  }
  /* 復位機制 */
  /********************************************* 
   *  Important!!! escape每次迴圈都會+1
   *  Arduino RFID 第一次進入迴圈會確認Tag並讀取(escape = -1)
   *  Arduino RFID 第二次進入迴圈【一定】會return跳出(escape = 0)
   *  故判斷Tag離開會在第三次進入迴圈
   *  此時如果Tag仍留著，escape = -1
   *  反之，escape = 1
   *  所以第四次迴圈判斷條件如下
  **********************************************/
  else if(escape > 0)
  {
  }
  /*****************************************establishing contact with a tag/card**********************************************************************/
  // Look for new cards (in case you wonder what PICC means: proximity integrated circuit card)
  if ( ! mfrc522.PICC_IsNewCardPresent()) {//if PICC_IsNewCardPresent returns 1, a new card has been found and we continue
    escape++;
    return;//if it did not find a new card is returns a '0' and we return to the start of the loop
  }
  escape = -1;
  
  // Select one of the cards
  if ( ! mfrc522.PICC_ReadCardSerial()) {//if PICC_ReadCardSerial returns 1, the "uid" struct (see MFRC522.h lines 238-45)) contains the ID of the read card.
    return;//if it returns a '0' something went wrong and we return to the start of the loop
  }
 
  Serial.print("uid:");
  for(int i=0;i<mfrc522.uid.size;i++)
  {
    tag[i] = mfrc522.uid.uidByte[i];
    Serial.print(mfrc522.uid.uidByte[i],HEX);
  }
  Serial.print(" ");
  ShowTagID();
}

void ShowTagID()
{
  #ifdef SHOW_TAG_ID
  Serial.print("Tag ID: ");
  Serial.print(mfrc522.uid.uidByte[0]);
  Serial.print(" , ");
  Serial.print(mfrc522.uid.uidByte[1]);
  Serial.print(" , ");
  Serial.print(mfrc522.uid.uidByte[2]);
  Serial.print(" , ");
  Serial.println(mfrc522.uid.uidByte[3]);
  #endif
}

boolean CheckTagID(int tagWakaka [])
{
  for(int i = 0; i < 4 ; i++ )
  {
    if(tag[i] != tagWakaka[i])
      return false;
  }
  return true;
}
