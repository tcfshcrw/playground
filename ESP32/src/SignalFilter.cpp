#include "SignalFilter.h"





// v = s / t
// a = v/t
// a = s / t^2
// a = 300 / delta_t^2
// adjust model noise here s = 0.5 * a * delta_t^2 --> a = 2 * s / delta_t^2
//static const float KF_MODEL_NOISE_FORCE_ACCELERATION = ( 2.0f * 1000.0f / 0.05f/ 0.05f );
static const double KF_MODEL_NOISE_FORCE_ACCELERATION = ( 8.0f * 4.0f / 0.1f/ 0.1f );


float position = 0;        // Estimated position
float velocity = 0;        // Estimated velocity
float dt = 0.1;            // Time step (seconds)

float P[2][2] = {          // Initial error covariance
    {1, 0},
    {0, 1}
};
float F[2][2] = {          // State transition matrix
    {1, dt},
    {0, 1}
};
float Q[2][2] = {          // Process noise covariance
    {0.1, 0.0},
    {0.0, 0.1}
};
float H[1][2] = {          // Measurement matrix
    {1, 0}
};
float R = 1;               // Measurement noise covariance
float K[2];                // Kalman Gain
float z;                   // Measurement (position)
float y;                   // Measurement residual
float S;                   // Residual covariance





KalmanFilter::KalmanFilter(float varianceEstimate)
  : _timeLastObservation(micros())
{
  // // evolution matrix. Size is <Nstate,Nstate>
  // _K.F = {(double)1.0, 0.0,
  //         0.0, (double)1.0};

  // // command matrix.  Size is <Nstate,Ncom>
  // _K.B = {1.0, 
  //         0.0};
        
  // // measurement matrix. Size is <Nobs,Nstate>
  // _K.H = {1.0, 0.0};

  // // model covariance matrix. Size is <Nstate,Nstate>
  // _K.Q = {1.0, 0.0,
  //         0.0, 1.0};

  // // measurement covariance matrix. Size is <Nobs,Nobs>
  // _K.R = { varianceEstimate };
}

float KalmanFilter::filteredValue(float observation, float command, uint8_t modelNoiseScaling_u8) {


  // obtain time
  unsigned long currentTime = micros();
  unsigned long elapsedTime = currentTime - _timeLastObservation;
  float modelNoiseScaling_fl32 = modelNoiseScaling_u8;
  modelNoiseScaling_fl32 /= 255.0f;
  modelNoiseScaling_fl32 /= 1000.0f;
  modelNoiseScaling_fl32 /= 20.0f;

  

  if (modelNoiseScaling_fl32 < 0.000001)
  {
    modelNoiseScaling_fl32 = 0.000001;
  }
  if (elapsedTime < 1) { elapsedTime=1; }
  _timeLastObservation = currentTime;

  if (elapsedTime > 5000) { elapsedTime=5000; }
  _timeLastObservation = currentTime;


  // update state transition and system covariance matrices
  //float delta_t = ((float)elapsedTime)  / 1000000.0f;/// 1000000.0f; // convert to seconds
  float delta_t = ((float)elapsedTime) / 1000.0f;
  float delta_t_pow2 = delta_t * delta_t;
  float delta_t_pow3 = delta_t_pow2 * delta_t;
  float delta_t_pow4 = delta_t_pow2 * delta_t_pow2;

  // update transition matrix
  F[0][1] = delta_t;

  float K_Q_11 = modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (float)0.5f * delta_t_pow3;
  Q[0][0] = modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (float)0.25f * delta_t_pow4;
  Q[0][1] = K_Q_11;
  Q[1][0] = K_Q_11;
  Q[1][1] = modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * delta_t_pow2;

  // Predict Step
    float x_pred[2] = {
        F[0][0] * position + F[0][1] * velocity,
        F[1][0] * position + F[1][1] * velocity
    };
    float P_pred[2][2] = {
        {F[0][0] * P[0][0] + F[0][1] * P[1][0], F[0][0] * P[0][1] + F[0][1] * P[1][1]},
        {F[1][0] * P[0][0] + F[1][1] * P[1][0], F[1][0] * P[0][1] + F[1][1] * P[1][1]}
    };

    P_pred[0][0] += Q[0][0];
    P_pred[1][1] += Q[1][1];

    // Update Step
    z = observation;  // Measurement
    y = z - H[0][0] * x_pred[0]; // Residual
    S = H[0][0] * P_pred[0][0] * H[0][0] + R; // Residual covariance

    K[0] = P_pred[0][0] * H[0][0] / S;  // Kalman Gain
    K[1] = P_pred[1][0] * H[0][0] / S;

    // Update state estimate
    position = x_pred[0] + K[0] * y;
    velocity = x_pred[1] + K[1] * y;

    // Update error covariance
    P[0][0] = (1 - K[0] * H[0][0]) * P_pred[0][0];
    P[0][1] = (1 - K[0] * H[0][0]) * P_pred[0][1];
    P[1][0] = -K[1] * H[0][0] * P_pred[0][0] + P_pred[1][0];
    P[1][1] = -K[1] * H[0][0] * P_pred[0][1] + P_pred[1][1];


  return position;


  // // obtain time
  // unsigned long currentTime = micros();
  // unsigned long elapsedTime = currentTime - _timeLastObservation;
  // double modelNoiseScaling_fl32 = modelNoiseScaling_u8;
  // modelNoiseScaling_fl32 /= 255.0;

  // if (modelNoiseScaling_fl32 < 0.001)
  // {
  //   modelNoiseScaling_fl32 = 0.001;
  // }
  // if (elapsedTime < 1) { elapsedTime=1; }
  // _timeLastObservation = currentTime;

  // // update state transition and system covariance matrices
  // double delta_t = ((double)elapsedTime)  / 1000000.0f;/// 1000000.0f; // convert to seconds
  // double delta_t_pow2 = delta_t * delta_t;
  // double delta_t_pow3 = delta_t_pow2 * delta_t;
  // double delta_t_pow4 = delta_t_pow2 * delta_t_pow2;

  // _K.F = {(double)1.0,  delta_t, 
  //         0.0,  (double)1.0};

  // _K.B = {1.0, 
  //         0.0};

  // double K_Q_11 = modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (double)0.5f * delta_t_pow3;
  // _K.Q = {modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * (double)0.25f * delta_t_pow4,   K_Q_11,
  //       K_Q_11, modelNoiseScaling_fl32 * KF_MODEL_NOISE_FORCE_ACCELERATION * delta_t_pow2};
        

  // // APPLY KALMAN FILTER
  // _K.update({observation}, {command});
  // return _K.x(0,0);
}

float KalmanFilter::changeVelocity() {
  // return _K.x(0,1) / 1.0f;
  return velocity * 1000.0f;
}
