#include "SignalFilter.h"





// v = s / t
// a = v/t
// a = s / t^2
// a = 300 / delta_t^2
// adjust model noise here s = 0.5 * a * delta_t^2 --> a = 2 * s / delta_t^2
//static const float KF_MODEL_NOISE_FORCE_ACCELERATION = ( 2.0f * 1000.0f / 0.05f/ 0.05f );
static const double KF_MODEL_NOISE_FORCE_ACCELERATION = ( 8.0f * 4.0f / 0.1f/ 0.1f );


KalmanFilter::KalmanFilter(float varianceEstimate)
  : _timeLastObservation(micros())
{
  // evolution matrix. Size is <Nstate,Nstate>
  _K.F = {(double)1.0, 0.0,
          0.0, (double)1.0};

  // command matrix.  Size is <Nstate,Ncom>
  _K.B = {1.0, 
          0.0};
        
  // measurement matrix. Size is <Nobs,Nstate>
  _K.H = {1.0, 0.0};

  // model covariance matrix. Size is <Nstate,Nstate>
  _K.Q = {1.0, 0.0,
          0.0, 1.0};

  // measurement covariance matrix. Size is <Nobs,Nobs>
  _K.R = { varianceEstimate };
}

float KalmanFilter::filteredValue(float observation, float command, uint8_t modelNoiseScaling_u8) {
  // obtain time
  unsigned long currentTime = micros();
  unsigned long elapsedTime = currentTime - _timeLastObservation;
  double modelNoiseScaling_fl32 = modelNoiseScaling_u8;
  modelNoiseScaling_fl32 /= 255.0;

  if (modelNoiseScaling_fl32 < 0.001)
  {
    modelNoiseScaling_fl32 = 0.001;
  }
  if (elapsedTime < 1) { elapsedTime=1; }
  _timeLastObservation = currentTime;

  // update state transition and system covariance matrices
  double delta_t = ((double)elapsedTime)  / 1000000.0f;/// 1000000.0f; // convert to seconds
  double delta_t_pow2 = delta_t * delta_t;
  double delta_t_pow3 = delta_t_pow2 * delta_t;
  double delta_t_pow4 = delta_t_pow2 * delta_t_pow2;

  _K.F = {(double)1.0,  delta_t, 
          0.0,  (double)1.0};

  _K.B = {1.0, 
          0.0};

  double K_Q_11 = modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (double)0.5f * delta_t_pow3;
  _K.Q = {modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (double)0.25f * delta_t_pow4,   K_Q_11,
        K_Q_11, modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * delta_t_pow2};
        

  // APPLY KALMAN FILTER
  _K.update({observation}, {command});
  return _K.x(0,0);
}

float KalmanFilter::changeVelocity() {
  return _K.x(0,1) / 1.0f;
}
