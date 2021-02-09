#include <LiquidCrystal.h>
#include <RTClib.h>
#include <Wire.h>

LiquidCrystal lcd(12, 11, 5, 4, 3, 6);
int lcdPreviousTime = 0;

int tempPin = A6;
int tempVal;
float tempVoltage;
int temperatureC;
int temperatureF;
int tempPreviousTime = 0;

int IR_Pin = 2;
int IR_Val;
int previousIRTime = 0;
volatile float rev = 0;
int oldtime = 0;
int time;
int rpm;
unsigned int mph;
float kmh;

int PreviousSerialTime = 0;
int PreviousClearTime = 0;

int SettingPin = 10;
int SettingSelector;
bool SystemConversion = false;
bool SystemFlag = false;
int PreviousSelectorTime = 0;
bool SelectorPause = false;
bool PreviousSelectorPauseTime = 0;

RTC_DS1307 rtc;
String returnTime() {
  DateTime now = rtc.now();
  int hrs = now.hour();
  String result;

  if (hrs == 0 && hrs != 12) {
    hrs = 12;
  } else if (hrs == 12 && hrs != 0) {
    hrs = 12;
  } else if (hrs < 12 && hrs != 0) {
    hrs = hrs;
  } else if (hrs > 12 && hrs != 0) {
    hrs = hrs - 12;
  }

  result += hrs;
  result += ':';
  result += now.minute(), DEC;
  result += ':';
  result += now.second(), DEC;
  result += " ";

  return result;
  result = "";
}

void InterruptServiceRoutine() { rev++; }

void setup() {
  Serial.begin(9600);
  Wire.begin();

  pinMode(IR_Pin, INPUT);
  pinMode(SettingPin, INPUT);
  attachInterrupt(digitalPinToInterrupt(IR_Pin), InterruptServiceRoutine, RISING);

  lcd.begin(16, 2);
  lcd.clear();

  bool RTC_Flag = false;
  if (!rtc.begin()) {
    Serial.println("Couldn't find RTC");
    RTC_Flag = true;
  }

  if (!rtc.isrunning() && RTC_Flag == false) {
    Serial.println("RTC is NOT running, Time manually set...");
    rtc.adjust(DateTime(F(__DATE__), F(__TIME__)));
  }

  if (rtc.isrunning() && rtc.begin()) {
    Serial.println("Welcome to Comyar's Bike Speedometer!! It is currently " + returnTime());
    Serial.println("----------------------------------");
    Serial.println();
  }

  lcd.print("Welcome To ");
  lcd.setCursor(0, 1);
  lcd.print("Comyar Bike Tool");
  delay(5000);
  lcd.clear();
}

void loop() {
    SettingSelector = digitalRead(SettingPin);
    int CurrentSelectorTime = millis();
    int CurrentSelectorPauseTime = millis();
    int SelectorSampleRate = 1000;
    int SelectorPauseSampleRate = 500;

    if(CurrentSelectorPauseTime - PreviousSelectorPauseTime >= SelectorPauseSampleRate){
      if(SelectorPause == true && SettingSelector == LOW){
        SelectorPause = false;
      }
        PreviousSelectorPauseTime = CurrentSelectorPauseTime;
    }

    if(CurrentSelectorTime - PreviousSelectorTime >= SelectorSampleRate && SelectorPause == false){
      if(SettingSelector == HIGH){
        if(SystemConversion == false){
          SystemConversion = true;
          SystemFlag = true;
          SelectorPause = true;
        }
        if(SystemConversion == true && SystemFlag == false){
          SystemConversion = false;
          SelectorPause = true;
        }
        SystemFlag = false;
      }
      PreviousSelectorTime = CurrentSelectorTime;
    }

    int IR_Rate = 1000;
    int CurrentIRTime = millis();

    if (CurrentIRTime - previousIRTime >= IR_Rate) {  
      detachInterrupt(digitalPinToInterrupt(IR_Pin));
      int IR_Val = digitalRead(IR_Pin);
      time = millis() - oldtime; 
      rpm = (rev/time) * 60000.0/3; 
      oldtime = millis(); 
      rev = 0;
      mph = (60 * (rpm * (20 * 3.14)))/63360.0;
      kmh = mph * 1.609;
      attachInterrupt(digitalPinToInterrupt(IR_Pin), InterruptServiceRoutine, RISING);

      previousIRTime = CurrentIRTime;
    }

    int tempSampleRate = 500;
    int tempCurrentTime = millis();

    if (tempCurrentTime - tempPreviousTime >= tempSampleRate) {
      tempVal = analogRead(A6);
      tempVoltage = (tempVal / 1024.0) * 5.0;
      temperatureC = (tempVoltage - .5) * 100;
      temperatureF = (temperatureC * 1.8) + 32;

      tempPreviousTime = tempCurrentTime;
    }

    int lcdSampleRate = 25;
    int lcdCurrentTime = millis();

    if (lcdCurrentTime - lcdPreviousTime >= lcdSampleRate) {
      lcd.setCursor(0, 1);
      lcd.print("S:");

      lcd.setCursor(6, 0);
      lcd.print("T:" + returnTime());

      if(SystemConversion == false){
        lcd.setCursor(0, 0);
        lcd.print("F:");
        lcd.setCursor(2, 0);
        lcd.print(String(temperatureF) + "*");
      }else{
        lcd.setCursor(0, 0);
        lcd.print("C:");
        lcd.setCursor(2, 0);
        lcd.print(String(temperatureC) + "*");
      }

      lcdPreviousTime = lcdCurrentTime;
    }

    int SerialSampleRate = 25;
    int SerialCurrentTime = millis();

    if(SerialCurrentTime - PreviousSerialTime >= SerialSampleRate){
      if(rpm > 0){
        Serial.println("rpm: " + String(rpm));
        Serial.println("mph: " + String(mph));
      
        lcd.setCursor(2, 1);
        if(SystemConversion == false){
          lcd.print(String(mph) + "mph");
        }else{
          lcd.print(String((int)(kmh)) + "kmh");
        }

        lcd.setCursor(8,1);
        lcd.print("R:" + String(rpm) + "rpm");
      }else{
        lcd.setCursor(2, 1);
        if(SystemConversion == false){
          lcd.print(String(mph) + "mph");
        }else{
          lcd.print(String((int)(kmh)) + "khm");
        }

        lcd.setCursor(8,1);
       lcd.print("R:" + String(rpm) + "rpm");

       PreviousSerialTime = SerialCurrentTime;
    }

    int ClearSampleRate = 250;
    int ClearCurrentTime = millis();

    if(ClearCurrentTime - PreviousClearTime >= ClearSampleRate){
      lcd.clear();
      PreviousClearTime = ClearCurrentTime;
    }
  }
}
