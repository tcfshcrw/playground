#pragma once

#include <Kalman.h>

static const int Nobs_0624 = 1;      // 1 filter input:   observed value
static const int Nstate_0624 = 2;    // 2 filter outputs: change & velocity
static const int Ncom_0624 = 1; // Number of commands, u vector


class KalmanFilter_0624 {
private:
  KALMAN<Nstate_0624, Nobs_0624, Ncom_0624> _K;
  unsigned long _timeLastObservation;

public:
  KalmanFilter_0624(float varianceEstimate);

  float filteredValue(float observation, float command, uint8_t modelNoiseScaling_u8);
  float changeVelocity();
};