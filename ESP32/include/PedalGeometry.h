#pragma once

#include "DiyActivePedal_types.h"
#include <Kalman.h>

static const float TRAVEL_PER_ROTATION_IN_MM = 5.0;   // determined by lead screw pitch & starts


class StepperWithLimits;

float sledPositionInMM(StepperWithLimits* stepper, DAP_config_st& config_st);
float pedalInclineAngleDeg(float sledPositionMM, DAP_config_st& config_st);
float pedalInclineAngleAccel(float pedalInclineAngleDeg_global);
float convertToPedalForce(float F_l, float sledPositionMM, DAP_config_st& config_st);
float convertToPedalForceGain(float sledPositionMM, DAP_config_st& config_st);


