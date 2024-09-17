#include "StepperWithLimits.h"
#include "RTDebugOutput.h"
#include "Main.h"
#include "Math.h"


#define STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT 20
#define MIN_POS_MAX_ENDSTOP STEPS_PER_MOTOR_REVOLUTION * 3 // servo has to drive minimum N steps before it allows the detection of the max endstop


//uint32_t speed_in_hz = TICKS_PER_S / ticks;
// TICKS_PER_S = 16000000L
// ticks = TICKS_PER_S / speed_in_hz
#define maxSpeedInTicks  (TICKS_PER_S / MAXIMUM_STEPPER_SPEED)

static const uint8_t LIMIT_TRIGGER_VALUE = LOW;                                   // does endstop trigger high or low
static const int32_t ENDSTOP_MOVEMENT = (float)STEPS_PER_MOTOR_REVOLUTION / 100.0f;         // how much to move between trigger checks
static const int32_t ENDSTOP_MOVEMENT_SENSORLESS = ENDSTOP_MOVEMENT * 5;


FastAccelStepperEngine& stepperEngine() {
  static FastAccelStepperEngine myEngine = FastAccelStepperEngine();   // this is a factory and manager for all stepper instances

  static bool firstTime = true;
  if (firstTime) {
     myEngine.init();
     firstTime = false;
  }

  return myEngine;
}



StepperWithLimits::StepperWithLimits(uint8_t pinStep, uint8_t pinDirection, uint8_t pinMin, uint8_t pinMax, bool invertMotorDir_b)
  : _pinMin(pinMin), _pinMax(pinMax)
  , _limitMin(0),    _limitMax(0)
  , _posMin(0),      _posMax(0)
{

  
  pinMode(pinMin, INPUT);
  pinMode(pinMax, INPUT);
  
  
  _stepper = stepperEngine().stepperConnectToPin(pinStep);

  

  // Stepper Parameters
  if (_stepper) {
    _stepper->setDirectionPin(pinDirection, invertMotorDir_b);
    _stepper->setAutoEnable(true);
    _stepper->setAbsoluteSpeedLimit( maxSpeedInTicks ); // ticks
    _stepper->setSpeedInTicks( maxSpeedInTicks ); // ticks
    _stepper->setAcceleration(MAXIMUM_STEPPER_ACCELERATION);  // steps/s²
    _stepper->setForwardPlanningTimeInMs(8);

    //uint16_t tmp = _stepper->getMaxSpeedInTicks();
    //Serial.print("Max speed in Hz: ");
    //Serial.println(tmp);

//#if defined(SUPPORT_ESP32_PULSE_COUNTER)
//    _stepper->attachToPulseCounter(1, 0, 0);
//#endif
  }
}


void StepperWithLimits::findMinMaxSensorless(isv57communication * isv57, DAP_config_st dap_config_st)
{

  if (! hasValidStepper()) return;

  // obtain servo states
  isv57->readServoStates();
  bool endPosDetected = abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;


  int32_t setPosition = _stepper->getCurrentPosition();
  while(!endPosDetected){
    delay(10);
    isv57->readServoStates();
    endPosDetected = abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;

    setPosition = setPosition - ENDSTOP_MOVEMENT_SENSORLESS;
    _stepper->moveTo(setPosition, true);

    //Serial.print("Min_DetValue: ");
    //Serial.println(isv57->servo_current_percent);
  }

  // move away from min position to reduce servos current reading
  _stepper->forceStop();
  setPosition = setPosition + 5 * ENDSTOP_MOVEMENT_SENSORLESS;
  _stepper->moveTo(setPosition, true);
  _stepper->forceStopAndNewPosition(0);
  _stepper->moveTo(0);
  _limitMin = 0;

  // wait N ms to let the endPosDetected become 0 again
  //delay(300);

  // read servo states again
  //isv57->readServoStates();
  endPosDetected = 0;//abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;

  setPosition = _stepper->getCurrentPosition();

  // calculate max steps for endstop limit
  float spindlePitch = max( dap_config_st.payLoadPedalConfig_.spindlePitch_mmPerRev_u8, (uint8_t)1 );
  float maxRevToReachEndPos = (float)dap_config_st.payLoadPedalConfig_.lengthPedal_travel / spindlePitch;
  float maxStepsToReachEndPos = maxRevToReachEndPos * (float)STEPS_PER_MOTOR_REVOLUTION;

  Serial.print("Max travel steps: ");
  Serial.println(maxStepsToReachEndPos);

  while (!endPosDetected) {
    delay(10);
    isv57->readServoStates();

    // only trigger when difference is significant
    if (setPosition > MIN_POS_MAX_ENDSTOP)
    {
      endPosDetected = abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;  
    }

    // trigger endstop if configured max travel is reached 
    if (setPosition > maxStepsToReachEndPos)
    {
      endPosDetected = true;
    }
    

    setPosition = setPosition + ENDSTOP_MOVEMENT_SENSORLESS;
    _stepper->moveTo(setPosition, true);

    //Serial.print("Max_DetValue: ");
    //Serial.println(isv57->servo_current_percent);
  }

  //_stepper->forceStop();
  //setPosition = setPosition - 5 * ENDSTOP_MOVEMENT;

  _limitMax = _stepper->getCurrentPosition();

  
  // move slowly to min position
  moveSlowlyToPos(_posMin);


#if defined(SUPPORT_ESP32_PULSE_COUNTER)
  _stepper->clearPulseCounter();
#endif


}


void StepperWithLimits::moveSlowlyToPos(int32_t targetPos_ui32) {
  // reduce speed and accelerartion
  _stepper->setSpeedInHz(MAXIMUM_STEPPER_SPEED / 4);
  _stepper->setAcceleration(MAXIMUM_STEPPER_ACCELERATION / 4);

  // move to min
  _stepper->moveTo(targetPos_ui32, true);

  // increase speed and accelerartion
  _stepper->setAcceleration(MAXIMUM_STEPPER_ACCELERATION);
  _stepper->setSpeedInHz(MAXIMUM_STEPPER_SPEED);
}


void StepperWithLimits::findMinMaxEndstops() {
  if (! hasValidStepper()) return;

  int32_t setPosition = _stepper->getCurrentPosition();
  while(! (LIMIT_TRIGGER_VALUE == digitalRead(_pinMin))){
    setPosition = setPosition - ENDSTOP_MOVEMENT;
    _stepper->moveTo(setPosition, true);
  }

  
  _stepper->forceStopAndNewPosition(0);
  _stepper->moveTo(0);
  _limitMin = 0;

  setPosition = _stepper->getCurrentPosition();
  while(! (LIMIT_TRIGGER_VALUE == digitalRead(_pinMax))){
    setPosition = setPosition + ENDSTOP_MOVEMENT;
    _stepper->moveTo(setPosition, true);
  }

  _limitMax = _stepper->getCurrentPosition();

  _stepper->moveTo(_posMin, true);
#if defined(SUPPORT_ESP32_PULSE_COUNTER)
  _stepper->clearPulseCounter();
#endif
}

void StepperWithLimits::updatePedalMinMaxPos(uint8_t pedalStartPosPct, uint8_t pedalEndPosPct) {
  int32_t limitRange = _limitMax - _limitMin;
  _posMin = _limitMin + ((limitRange * pedalStartPosPct) / 100);
  _posMax = _limitMin + ((limitRange * pedalEndPosPct) / 100);
}

void StepperWithLimits::refindMinLimit() {
  int32_t setPosition = _stepper->getCurrentPosition();
  while(! (LIMIT_TRIGGER_VALUE == digitalRead(_pinMin))){
    setPosition = setPosition - ENDSTOP_MOVEMENT;
    _stepper->moveTo(setPosition, true);
  }
  _stepper->forceStopAndNewPosition(_limitMin);
}

void StepperWithLimits::refindMinLimitSensorless(isv57communication * isv57) {

  // obtain servo states
  isv57->readServoStates();
  bool endPosDetected = abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;


  int32_t setPosition = _stepper->getCurrentPosition();
  while(!endPosDetected){
    delay(10);
    isv57->readServoStates();
    endPosDetected = abs( isv57->servo_current_percent) > STEPPER_WITH_LIMITS_SENSORLESS_CURRENT_THRESHOLD_IN_PERCENT;

    setPosition = setPosition - ENDSTOP_MOVEMENT_SENSORLESS;
    _stepper->moveTo(setPosition, true);

    //Serial.print("Min_DetValue: ");
    //Serial.println(isv57->servo_current_percent);
  }

  // move away from min position to reduce servos current reading
  _stepper->forceStop();
  setPosition = setPosition + 5 * ENDSTOP_MOVEMENT_SENSORLESS;
  _stepper->moveTo(setPosition, true);
  _stepper->forceStopAndNewPosition(_limitMin);
}

void StepperWithLimits::checkLimitsAndResetIfNecessary() {
  // in case the stepper loses its position and therefore an endstop is triggered reset position
  if (LIMIT_TRIGGER_VALUE == digitalRead(_pinMin)) {
    _stepper->forceStopAndNewPosition(_limitMin);
    _stepper->moveTo(_posMin, true);
  }
  if (LIMIT_TRIGGER_VALUE == digitalRead(_pinMax)) {
    _stepper->forceStopAndNewPosition(_limitMin);
    _stepper->moveTo(_posMax, true);
  }
}

int8_t StepperWithLimits::moveTo(int32_t position, bool blocking) {
  return _stepper->moveTo(position, blocking);
}

int32_t StepperWithLimits::getCurrentPositionFromMin() const {
  return _stepper->getCurrentPosition() - _posMin;
}

int32_t StepperWithLimits::getCurrentPosition() const {
  return _stepper->getCurrentPosition();
}


double StepperWithLimits::getCurrentPositionFraction() const {
  return double(getCurrentPositionFromMin()) / getTravelSteps();
}

double StepperWithLimits::getCurrentPositionFractionFromExternalPos(int32_t extPos_i32) const {
  return (double(extPos_i32) - _posMin)/ getTravelSteps();
}

int32_t StepperWithLimits::getTargetPositionSteps() const {
  return _stepper->getPositionAfterCommandsCompleted();
}


void StepperWithLimits::printStates()
{
  int32_t currentStepperPos = _stepper->getCurrentPosition();
  int32_t currentStepperVel = _stepper->getCurrentSpeedInUs();
  int32_t currentStepperVel2 = _stepper->getCurrentSpeedInMilliHz();


  //Serial.println(currentStepperVel);
  
  int32_t currentStepperAccel = _stepper->getCurrentAcceleration();

  static RTDebugOutput<int32_t, 4> rtDebugFilter({ "Pos", "Vel", "Vel2", "Accel"});
  rtDebugFilter.offerData({ currentStepperPos, currentStepperVel, currentStepperVel2, currentStepperAccel});
}


void StepperWithLimits::setSpeed(uint32_t speedInStepsPerSecond) 
{
  _stepper->setSpeedInHz(speedInStepsPerSecond);            // steps/s 
}

bool StepperWithLimits::isAtMinPos()
{

  bool isNotRunning = !_stepper->isRunning();
  bool isAtMinPos = getCurrentPositionFromMin() == 0;

  return isAtMinPos && isNotRunning;
}
bool StepperWithLimits::correctPos(int32_t posOffset)
{
  // 
  int32_t stepOffset =(int32_t)constrain(posOffset, -10, 10);

  // correct pos
  _stepper->setCurrentPosition(_stepper->getCurrentPosition() + stepOffset);
  return 1;
}

