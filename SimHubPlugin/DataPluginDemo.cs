﻿using GameReaderCommon;
//using log4net.Plugin;
using SimHub.Plugins;
using SimHub.Plugins.DataPlugins.RGBMatrixDriver.Settings;
using SimHub.Plugins.DataPlugins.ShakeItV3.UI.Effects;
using SimHub.Plugins.OutputPlugins.Dash.GLCDTemplating;
using System;
using System.IO.Ports;
using System.Media;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.UI.Notifications;
using static System.Net.Mime.MediaTypeNames;




// https://stackoverflow.com/questions/14344305/best-way-to-structure-class-struct-in-c-sharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
[Serializable]

static class Constants
{
    // payload revisiom
    public const uint pedalConfigPayload_version = 139;


    // pyload types
    public const uint pedalConfigPayload_type = 100;
    public const uint pedalActionPayload_type = 110;
    public const uint pedalStateBasicPayload_type = 120;
    public const uint pedalStateExtendedPayload_type = 130;
}



public struct payloadHeader
{
    // structure identification via payload
    public byte payloadType;

    // variable to check if structure at receiver matched version from transmitter
    public byte version;

    public byte storeToEeprom;
    public byte PedalTag;
}


public struct payloadPedalAction
{
    public byte triggerAbs_u8;
    public byte system_action_u8; //1=reset position, 2=restart ESP
    public byte startSystemIdentification_u8;
    public byte returnPedalConfig_u8;
    public byte RPM_u8;
    public byte G_value;
    public byte WS_u8;
    public byte impact_value;
    public byte Trigger_CV_1;
    public byte Trigger_CV_2;
    public byte Rudder_action;
    public byte Rudder_brake_action;
};

public struct payloadPedalState_Basic
{
    public UInt16 pedalPosition_u16;
    public UInt16 pedalForce_u16;
    public UInt16 joystickOutput_u16;
    public byte error_code_u8;

};

public struct payloadPedalState_Extended
{
    public UInt32 timeInMs_u32;
    public float pedalForce_raw_fl32;
    public float pedalForce_filtered_fl32;
    public float forceVel_est_fl32;

    // register values from servo
    public Int16 servoPosition_i16;
    public Int16 servoPositionTarget_i16;
    public Int16 servo_voltage_0p1V_i16;
    public Int16 servo_current_percent_i16;
};

public struct payloadPedalConfig
{
    // configure pedal start and endpoint
    // In percent
    public byte pedalStartPosition;
    public byte pedalEndPosition;

    // configure pedal forces
    public byte maxForce;
    public byte preloadForce;

    // design force vs travel curve
    // In percent
    public byte relativeForce_p000;
    public byte relativeForce_p020;
    public byte relativeForce_p040;
    public byte relativeForce_p060;
    public byte relativeForce_p080;
    public byte relativeForce_p100;

    // parameter to configure damping
    public byte dampingPress;
    public byte dampingPull;

    // configure ABS effect 
    public byte absFrequency; // In Hz
    public byte absAmplitude; // In kg/20
    public byte absPattern; // 0: sinewave, 1: sawtooth
    public byte absForceOrTarvelBit;


    // geometric properties of the pedal
    // in mm
    public Int16 lengthPedal_a;
    public Int16 lengthPedal_b;
    public Int16 lengthPedal_d;
    public Int16 lengthPedal_c_horizontal;
    public Int16 lengthPedal_c_vertical;
    public Int16 lengthPedal_travel;


    public byte Simulate_ABS_trigger; //simulateABS
    public byte Simulate_ABS_value; //simulated ABS value
    public byte RPM_max_freq;
    public byte RPM_min_freq;
    public byte RPM_AMP;
    public byte BP_trigger_value;
    public byte BP_amp;
    public byte BP_freq;
    public byte BP_trigger;
    public byte G_multi;
    public byte G_window;
    public byte WS_amp;
    public byte WS_freq;
    public byte Impact_multi;
    public byte Impact_window;
    //Custom Vibration 1
    public byte CV_amp_1;
    public byte CV_freq_1;
    //Custom Vibration 2
    public byte CV_amp_2;
    public byte CV_freq_2;
    // cubic spline params
    public float cubic_spline_param_a_0;
    public float cubic_spline_param_a_1;
    public float cubic_spline_param_a_2;
    public float cubic_spline_param_a_3;
    public float cubic_spline_param_a_4;

    public float cubic_spline_param_b_0;
    public float cubic_spline_param_b_1;
    public float cubic_spline_param_b_2;
    public float cubic_spline_param_b_3;
    public float cubic_spline_param_b_4;

    // PID settings
    public float PID_p_gain;
    public float PID_i_gain;
    public float PID_d_gain;
    public float PID_velocity_feedforward_gain;

    // MPC settings
    public float MPC_0th_order_gain;
    public float MPC_1st_order_gain;
    public float MPC_2nd_order_gain;



    public byte control_strategy_b;

    public byte maxGameOutput;

    // Kalman filter model noise
    public byte kf_modelNoise;
    public byte kf_modelOrder;

    // debug flags, sued to enable debug output
    public byte debug_flags_0;

    // loadcell rating in kg / 2 --> to get value in kg, muiltiply by 2
    public byte loadcell_rating;

    // use loadcell or travel as joystick output
    public byte travelAsJoystickOutput_u8;

    // invert loadcell sign
    public byte invertLoadcellReading_u8;

    // invert motor direction
    public byte invertMotorDirection_u8;

    // spindle pitch in mm/rev
    public byte spindlePitch_mmPerRev_u8;

    // pedal type
    public byte pedal_type;

    // OTA update flag
    public byte OTA_flag;

    // OTA update flag
    public byte enableReboot_u8;

}

public struct payloadFooter
{
    // To check if structure is valid
    public UInt16 checkSum;
}



public struct DAP_action_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalAction payloadPedalAction_;
    public payloadFooter payloadFooter_;
}


public struct DAP_config_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalConfig payloadPedalConfig_;
    public payloadFooter payloadFooter_;
}

public struct DAP_state_basic_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalState_Basic payloadPedalBasicState_;
    public payloadFooter payloadFooter_;
}

public struct DAP_state_extended_st
{
    public payloadHeader payloadHeader_;
    public payloadPedalState_Extended payloadPedalExtendedState_;
    public payloadFooter payloadFooter_;
}


namespace User.PluginSdkDemo
{
    [PluginDescription("The Plugin was for FFB pedal, To tune the pedal parameters and communicates with the pedal over USB.")]
    [PluginAuthor("OpenSource")]
    [PluginName("DIY active pedal plugin")]
    public class DIY_FFB_Pedal : IPlugin, IDataPlugin, IWPFSettingsV2
    {

        public PluginManager pluginHandle;// = this;

        public bool sendAbsSignal = false;
		public DAP_config_st dap_config_initial_st;
        public byte rpm_last_value = 0 ;
        public double g_force_last_value = 128;
        public byte Road_impact_last = 0;
        public byte game_running_index = 0 ;
        public uint testValue = 0;
        public uint[] profile_flag = new uint[4] { 0,0,0,0};
        public uint[] select_button_flag = new uint[2] { 0, 0, };// define the up and down selection
        public uint slotA_flag = 0;
        public uint slotB_flag = 0;
        public uint slotC_flag = 0;
        public uint slotD_flag = 0;
        public uint sendconfig_flag = 0;
        public SettingsControlDemo wpfHandle;
        public uint in_game_flag = 0; // check current game is off or pause
        public string current_profile = "NA" ;
        public uint profile_index = 0;
        //public uint Page_update_flag = 0;
        public bool binding_check=false;
        public bool pedal_select_update_flag = false;
        public string current_pedal = "NA";
        public string current_action = "NA";
        public bool Page_update_flag =false;
        public uint overlay_display = 0;
        public string simhub_theme_color = "#7E87CEFA";
        public uint debug_value = 0;
        public bool Rudder_enable_flag=false;
        public bool clear_action = false;
        public bool Rudder_status = false;
        public bool Rudder_brake_enable_flag = false;
        public bool Rudder_brake_status = false;
        public byte pedal_state_in_ratio = 0;
        public bool Sync_esp_connection_flag=false;
        public byte PedalErrorCode = 0;
        public byte PedalErrorIndex = 0;
        public byte[] random_pedal_action_interval=new byte[3] { 50,51,53};



        //effect trigger timer
        DateTime[] Action_currentTime = new DateTime[3];
        DateTime[] Action_lastTime = new DateTime[3];
        

        // ABS trigger timer
        DateTime absTrigger_currentTime = DateTime.Now;
        DateTime absTrigger_lastTime = DateTime.Now;

        //G force timer
        DateTime GTrigger_currentTime = DateTime.Now;
        DateTime GTrigger_lastTime = DateTime.Now;

        //Road effect
        DateTime RoadTrigger_currentTime = DateTime.Now;
        DateTime RoadTrigger_lastTime = DateTime.Now;

        //// payload revisiom
        //public uint pedalConfigPayload_version = 110;

        //// pyload types
        //public uint pedalConfigPayload_type = 100;
        //public uint pedalActionPayload_type = 110;

        //public SettingsControlDemo settings { get; }

        //SettingsControlDemo wpfHandler;


        //https://www.c-sharpcorner.com/uploadfile/eclipsed4utoo/communicating-with-serial-port-in-C-Sharp/
        public SerialPort[] _serialPort = new SerialPort[4] {new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One),
            new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One),
            new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One),new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One)};

        public SerialPort ESPsync_serialPort = new SerialPort("COM7", 921600, Parity.None, 8, StopBits.One);

        //for (byte pedalIdx_lcl = 0; pedalIdx_lcl< 3; pedalIdx_lcl++)
        //{
        //    _serialPortt[pedalIdx_lcl].RtsEnable = false;
        //    _serialPort[pedalIdx_lcl].DtrEnable = true;
        //}   







        public bool[] connectSerialPort = { false, false, false };


        public DataPluginDemoSettings Settings;



        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "FFB Pedal Dashboard";
        //public string LeftMenuTitle => "DIY FFB Pedal";

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        /// 
        unsafe public UInt16 checksumCalc(byte* data, int length)
        {

            UInt16 curr_crc = 0x0000;
            byte sum1 = (byte)curr_crc;
            byte sum2 = (byte)(curr_crc >> 8);
            int index;
            for (index = 0; index < length; index = index + 1)
            {
                int v = (sum1 + (*data));
                sum1 = (byte)v;
                sum1 = (byte)(v % 255);

                int w = (sum1 + sum2) % 255;
                sum2 = (byte)w;

                data++;// = data++;
            }

            int x = (sum2 << 8) | sum1;
            return (UInt16)x;
        }

        public byte[] getBytes_Action(DAP_action_st aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }

        unsafe public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
			
			bool sendAbsSignal_local_b = false;
            bool sendTcSignal_local_b = false;
            double RPM_value =0;
            double RPM_MAX = 0;
            double _G_force = 128;
            byte WS_value = 0;
            byte Road_impact_value = 0;
            byte CV1_value = 0;
            byte CV2_value = 0;
            //bool WS_flag = false;

            if (data.GamePaused | (!data.GameRunning))
            {
                in_game_flag = 0;
            }
            else 
            {
                in_game_flag = 1;
            }
            //for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            //{
            //    if (_serialPort[pedalIdx].IsOpen)
            //    {
            //        int receivedLength = _serialPort[pedalIdx].BytesToRead;

            //        settings.TextBox_debugOutput.Text = "Test";
            //    }
            //}


            // Send ABS signal when triggered by the game
            if (data.GameRunning)
            {
                if (data.OldData != null && data.NewData != null)
                {
                    if (data.NewData.ABSActive > 0)
                    {
                        sendAbsSignal_local_b = true;
                    }

                    if (data.NewData.TCActive > 0)
                    {
                        sendTcSignal_local_b = true;
                    }


                    // when test signal is activated, overwrite trigger signal
                    if (sendAbsSignal)
                    {
                        sendAbsSignal_local_b = true;
                        sendTcSignal_local_b = true;
                    }



                    //fill the RPM value
                    if (Settings.RPM_effect_type == 0)
                    {
                        if (data.NewData.CarSettings_MaxRPM == 0)
                        {
                            RPM_MAX = 10000;
                        }
                        else
                        {
                            RPM_MAX = data.NewData.CarSettings_MaxRPM;
                        }

                        RPM_value = (data.NewData.Rpms / RPM_MAX * 100);
                    }
                    else
                    {
                        if (data.NewData.MaxSpeedKmh == 0)
                        {
                            RPM_MAX = 300;
                        }
                        else
                        { 
                            RPM_MAX= data.NewData.MaxSpeedKmh;
                        }
                        RPM_value = (data.NewData.SpeedKmh / RPM_MAX * 100);
                    }

                    
                    if (data.NewData.GlobalAccelerationG != 0)
                    {
                        _G_force = -1 * data.NewData.GlobalAccelerationG + 128;
                    }
                    else
                    {
                        _G_force = 128;
                    }

                    game_running_index = 1;
                    

                }
                else
                {
                    RPM_value = 0;
                    _G_force = 128;
                    


                }
            }
            else
            {
                RPM_value = 0;
                _G_force = 128;
                
            }
			




            absTrigger_currentTime = DateTime.Now;
            TimeSpan diff = absTrigger_currentTime - absTrigger_lastTime;
            int millisceonds = (int)diff.TotalMilliseconds;
            if (millisceonds <= 10)
            {
                sendAbsSignal_local_b = false;
                sendTcSignal_local_b = false;
                

            }
            else
            {
                absTrigger_lastTime = DateTime.Now;
            }




            bool update_flag = false;

            if (data.GameRunning)
            {
                // Send ABS trigger signal via serial
                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    
                    

                        DAP_action_st tmp;
                        tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                        tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                        tmp.payloadHeader_.PedalTag = (byte)pedalIdx;
                        tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                        tmp.payloadPedalAction_.RPM_u8 = (Byte)rpm_last_value;
                        
                        tmp.payloadPedalAction_.WS_u8 = 0;
                        tmp.payloadPedalAction_.impact_value = 0;
                        tmp.payloadPedalAction_.Trigger_CV_1 = 0;
                        tmp.payloadPedalAction_.Trigger_CV_2 = 0;
                        tmp.payloadPedalAction_.Rudder_action = 0;
                        tmp.payloadPedalAction_.Rudder_brake_action = 0;
                        if (Settings.G_force_enable_flag[pedalIdx] == 1)
                        {
                            tmp.payloadPedalAction_.G_value = (Byte)g_force_last_value;
                        }
                        else
                        {
                            tmp.payloadPedalAction_.G_value = 128;
                        }


                        if (Settings.RPM_enable_flag[pedalIdx] == 1)
                        {

                            if (Math.Abs(RPM_value - rpm_last_value) > 3)
                            {
                                tmp.payloadPedalAction_.RPM_u8 = (Byte)RPM_value;
                                update_flag = true;
                                rpm_last_value = (Byte)RPM_value;
                            }

                        }
                        else
                        {
                            tmp.payloadPedalAction_.RPM_u8 = 0;
                        }

                        //G force effect only effect on brake
                        if (pedalIdx == 1)
                        {

                            GTrigger_currentTime = DateTime.Now;
                            TimeSpan diff_G = GTrigger_currentTime - GTrigger_lastTime;
                            int millisceonds_G = (int)diff_G.TotalMilliseconds;
                            if (millisceonds <= 10)
                            {
                                _G_force = g_force_last_value;
                            }
                            else
                            {
                                GTrigger_lastTime = DateTime.Now;
                            }
                            if (Settings.G_force_enable_flag[pedalIdx] == 1)
                            {
                                //double value_check_g = 1 - _G_force / ((double)g_force_last_value);
                                double value_check_g = (_G_force - (double)g_force_last_value);
                                if (Math.Abs(value_check_g) > 2)
                                {
                                    tmp.payloadPedalAction_.G_value = (Byte)_G_force;
                                    update_flag = true;
                                    g_force_last_value = (Byte)_G_force;
                                }

                            }
                        }

                        //Wheel slip
                        
                        if (Settings.WS_enable_flag[pedalIdx] == 1)
                        {
                            if (pluginManager.GetPropertyValue(Settings.WSeffect_bind) != null)
                            {
                                /*object tmp_ws = (pluginManager.GetPropertyValue(Settings.WSeffect_bind));
                                int tmp_ws_number = Int32.Parse(tmp_ws.ToString());
                                WS_value = (byte)tmp_ws_number;
                                */
                                WS_value = Convert.ToByte(pluginManager.GetPropertyValue(Settings.WSeffect_bind));
                                //pluginManager.SetPropertyValue("Wheelslip-test", this.GetType(), WS_value);
                                if (WS_value >= (Settings.WS_trigger + 50))
                                {
                                    tmp.payloadPedalAction_.WS_u8 = 1;
                                    update_flag = true;
                                }
                            }
                        }
                        //Road impact
                        if (Settings.Road_impact_enable_flag[pedalIdx] == 1)
                        {
                            if (pluginManager.GetPropertyValue(Settings.Road_impact_bind) != null)
                            {
                                Road_impact_value = Convert.ToByte(pluginManager.GetPropertyValue(Settings.Road_impact_bind));

                                RoadTrigger_currentTime = DateTime.Now;
                                TimeSpan diff_Road = RoadTrigger_currentTime - RoadTrigger_lastTime;
                                int millisceonds_G = (int)diff_Road.TotalMilliseconds;
                                if (millisceonds <= 10)
                                {
                                    Road_impact_value = Road_impact_last;
                                }
                                else
                                {
                                    RoadTrigger_lastTime = DateTime.Now;
                                }
                                if (true)
                                {
                                    //double value_check_g = 1 - _G_force / ((double)g_force_last_value);
                                    double value_check_road = Road_impact_value - Road_impact_last;
                                    if (Math.Abs(value_check_road) > 2)
                                    {
                                        tmp.payloadPedalAction_.impact_value = Road_impact_value;
                                        update_flag = true;
                                        Road_impact_last = Road_impact_value;
                                        debug_value = Road_impact_value;
                                    }

                                }
                            }
                        }
                        if (Settings.CV1_enable_flag[pedalIdx] == true)
                        {
                            if (pluginManager.GetPropertyValue(Settings.CV1_bindings[pedalIdx]) != null)
                            {

                                CV1_value = Convert.ToByte(pluginManager.GetPropertyValue(Settings.CV1_bindings[pedalIdx]));
                                if (CV1_value > (Settings.CV1_trigger[pedalIdx]))
                                {
                                    tmp.payloadPedalAction_.Trigger_CV_1 = 1;
                                    update_flag = true;
                                }
                            }
                        }
                        if (Settings.CV2_enable_flag[pedalIdx] == true)
                        {
                            if (pluginManager.GetPropertyValue(Settings.CV2_bindings[pedalIdx]) != null)
                            {

                                CV2_value = Convert.ToByte(pluginManager.GetPropertyValue(Settings.CV2_bindings[pedalIdx]));
                                if (CV2_value > (Settings.CV2_trigger[pedalIdx]))
                                {
                                    tmp.payloadPedalAction_.Trigger_CV_2 = 1;
                                    update_flag = true;
                                }
                            }
                        }




                        if (pedalIdx == 1)
                        {
                            if (sendAbsSignal_local_b & Settings.ABS_enable_flag[pedalIdx] ==1)
                            {
                                //_serialPort[1].Write("2");

                                // compute checksum
                                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                                update_flag = true;

                            }
                        }
                        if (pedalIdx == 2)
                        {
                            if (sendTcSignal_local_b & Settings.ABS_enable_flag[pedalIdx] == 1)
                            {
                                // compute checksum

                                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                                update_flag = true;

                            }
                        }
                    // check the update interval
                    if (update_flag)
                    {
                        Action_currentTime[pedalIdx] = DateTime.Now;
                        TimeSpan diff_action = Action_currentTime[pedalIdx] - Action_lastTime[pedalIdx];
                        int millisceonds_action = (int)diff_action.TotalMilliseconds;
                        if (millisceonds_action <= random_pedal_action_interval[pedalIdx])
                        {
                            update_flag = false;
                        }
                        else
                        {
                            Action_lastTime[pedalIdx] = DateTime.Now;
                            
                        }

                        
                    }


                    if (update_flag)
                    {

                        DAP_action_st* v = &tmp;
                            byte* p = (byte*)v;
                            tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                            int length = sizeof(DAP_action_st);
                            byte[] newBuffer = new byte[length];
                            newBuffer = getBytes_Action(tmp);

                            if (Settings.Pedal_ESPNow_Sync_flag[pedalIdx])
                            {
                                if (ESPsync_serialPort.IsOpen)
                                {
                                    ESPsync_serialPort.DiscardInBuffer();
                                    ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                                    System.Threading.Thread.Sleep(10);
                                }

                            }
                            else
                            {
                                if (_serialPort[pedalIdx].IsOpen)
                                {
                                    // clear inbuffer 
                                    _serialPort[pedalIdx].DiscardInBuffer();

                                    // send query command
                                    _serialPort[pedalIdx].Write(newBuffer, 0, newBuffer.Length);
                                }

                            }



                    }
                    
                }
                
            }
            else
            {
                if (game_running_index == 1)
                {
                    game_running_index = 0;
                    clear_action = true;   
                }
            }



            // Send ABS test signal if requested
            if (sendAbsSignal)
            {
                sendAbsSignal_local_b = true;
                sendTcSignal_local_b = true;
                DAP_action_st tmp;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                tmp.payloadPedalAction_.triggerAbs_u8 = 1;
                tmp.payloadPedalAction_.RPM_u8 = 0;
                tmp.payloadPedalAction_.G_value = 128;
                tmp.payloadPedalAction_.WS_u8 = 0;
                tmp.payloadPedalAction_.impact_value = 0;
                tmp.payloadPedalAction_.Trigger_CV_1 = 0;
                tmp.payloadPedalAction_.Trigger_CV_2 = 0;
                tmp.payloadPedalAction_.Rudder_action = 0;
                tmp.payloadPedalAction_.Rudder_brake_action = 0;

                for (uint PIDX = 1; PIDX < 3; PIDX++)
                {
                    tmp.payloadHeader_.PedalTag = (byte)PIDX;
                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes_Action(tmp);
                    if (Settings.Pedal_ESPNow_Sync_flag[PIDX])
                    {
                        if (ESPsync_serialPort.IsOpen) 
                        {
                            ESPsync_serialPort.DiscardInBuffer();  
                            ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                            System.Threading.Thread.Sleep(30);
                        }
                    }
                    else
                    {
                        if (_serialPort[PIDX].IsOpen)
                        {
                            // clear inbuffer 
                            _serialPort[PIDX].DiscardInBuffer();

                            // send query command
                            _serialPort[PIDX].Write(newBuffer, 0, newBuffer.Length);
                        }
                    }
                }
                    

            }
            if (Rudder_enable_flag)
            {
                if (Rudder_status == false)
                {
                    Rudder_status = true;
                }
                else
                {
                    Rudder_status = false;
                }
                DAP_action_st tmp;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;                    
                tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                tmp.payloadPedalAction_.RPM_u8 = 0;
                tmp.payloadPedalAction_.G_value = 128;
                tmp.payloadPedalAction_.WS_u8 = 0;
                tmp.payloadPedalAction_.impact_value = 0;
                tmp.payloadPedalAction_.Trigger_CV_1 = 0;
                tmp.payloadPedalAction_.Trigger_CV_2 = 0;
                tmp.payloadPedalAction_.Rudder_action = 1;
                tmp.payloadPedalAction_.Rudder_brake_action = 0;

                for (uint PIDX = 1; PIDX < 3; PIDX++)
                {
                    tmp.payloadHeader_.PedalTag = (byte)PIDX;
                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes_Action(tmp);
                    
                    
                    if (Settings.Pedal_ESPNow_Sync_flag[PIDX])
                    {
                        if (ESPsync_serialPort.IsOpen)
                        {
                            ESPsync_serialPort.DiscardInBuffer();
                            ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                            System.Threading.Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        if (_serialPort[PIDX].IsOpen)
                        {
                            // clear inbuffer 
                            _serialPort[PIDX].DiscardInBuffer();

                            // send query command
                            _serialPort[PIDX].Write(newBuffer, 0, newBuffer.Length);
                        }

                    }

                    
                    Rudder_enable_flag = false;
                    System.Threading.Thread.Sleep(50);
                }
                SystemSounds.Beep.Play();

            }



            if (Rudder_brake_enable_flag)
            {
                if (Rudder_brake_status == false)
                {
                    Rudder_brake_status = true;
                    
                }
                else
                {
                    Rudder_brake_status = false;
                }
                SystemSounds.Beep.Play();
                DAP_action_st tmp;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                tmp.payloadPedalAction_.RPM_u8 = 0;
                tmp.payloadPedalAction_.G_value = 128;
                tmp.payloadPedalAction_.WS_u8 = 0;
                tmp.payloadPedalAction_.impact_value = 0;
                tmp.payloadPedalAction_.Trigger_CV_1 = 0;
                tmp.payloadPedalAction_.Trigger_CV_2 = 0;
                tmp.payloadPedalAction_.Rudder_action = 0;
                tmp.payloadPedalAction_.Rudder_brake_action = 1;

                for (uint PIDX = 1; PIDX < 3; PIDX++)
                {
                    tmp.payloadHeader_.PedalTag = (byte)PIDX;
                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes_Action(tmp);
                    if (Settings.Pedal_ESPNow_Sync_flag[PIDX])
                    {
                        if (ESPsync_serialPort.IsOpen)
                        {
                            ESPsync_serialPort.DiscardInBuffer();
                            ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                            System.Threading.Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        if (_serialPort[PIDX].IsOpen)
                        {
                            // clear inbuffer 
                            _serialPort[PIDX].DiscardInBuffer();

                            // send query command
                            _serialPort[PIDX].Write(newBuffer, 0, newBuffer.Length);
                        }

                    }
                    Rudder_brake_enable_flag = false;
                    System.Threading.Thread.Sleep(50);
                }

            }


            if (clear_action)
            {
                
                DAP_action_st tmp;
                tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                tmp.payloadPedalAction_.triggerAbs_u8 = 0;
                tmp.payloadPedalAction_.RPM_u8 = 0;
                tmp.payloadPedalAction_.G_value = 128;
                tmp.payloadPedalAction_.WS_u8 = 0;
                tmp.payloadPedalAction_.impact_value = 0;
                tmp.payloadPedalAction_.Trigger_CV_1 = 0;
                tmp.payloadPedalAction_.Trigger_CV_2 = 0;
                tmp.payloadPedalAction_.Rudder_action = 0;
                tmp.payloadPedalAction_.Rudder_brake_action = 0;
                rpm_last_value = 0;
                Road_impact_last = 0;
                debug_value = 0;

                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    tmp.payloadHeader_.PedalTag = (byte)pedalIdx;
                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes_Action(tmp);
                    if (Settings.Pedal_ESPNow_Sync_flag[pedalIdx])
                    {
                        if (ESPsync_serialPort.IsOpen)
                        {
                            ESPsync_serialPort.DiscardInBuffer();
                            ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                            System.Threading.Thread.Sleep(5);
                        }
                    }
                    else
                    {
                        if (_serialPort[pedalIdx].IsOpen)
                        {
                            // clear inbuffer 
                            _serialPort[pedalIdx].DiscardInBuffer();

                            // send query command
                            _serialPort[pedalIdx].Write(newBuffer, 0, newBuffer.Length);
                        }

                    }
                }
                clear_action = false;
            }


            this.AttachDelegate("CurrentProfile", () => current_profile);
            pluginManager.SetPropertyValue("SelectedPedal", this.GetType(), current_pedal);
            pluginManager.SetPropertyValue("Action", this.GetType(), current_action);
            pluginManager.SetPropertyValue("ABS_effect_status", this.GetType(), Settings.ABS_enable_flag[Settings.table_selected]);
            pluginManager.SetPropertyValue("RPM_effect_status", this.GetType(), Settings.RPM_enable_flag[Settings.table_selected]);
            pluginManager.SetPropertyValue("Gforce_effect_status", this.GetType(), Settings.G_force_enable_flag[Settings.table_selected]);
            pluginManager.SetPropertyValue("WheelSlip_effect_status", this.GetType(), Settings.WS_enable_flag[Settings.table_selected]);
            pluginManager.SetPropertyValue("RoadImpact_effect_status", this.GetType(), Settings.Road_impact_enable_flag[Settings.table_selected]);
            pluginManager.SetPropertyValue("Overlay_display", this.GetType(), overlay_display);
            pluginManager.SetPropertyValue("Theme_color", this.GetType(), simhub_theme_color);
            pluginManager.SetPropertyValue("ProfileIndex", this.GetType(), profile_index);
            pluginManager.SetPropertyValue("debugvalue", this.GetType(), debug_value);
            pluginManager.SetPropertyValue("rudder_status", this.GetType(), Rudder_status);
            pluginManager.SetPropertyValue("rudder_brake_status", this.GetType(), Rudder_brake_status);
            pluginManager.SetPropertyValue("pedal_position", this.GetType(), pedal_state_in_ratio);
            pluginManager.SetPropertyValue("PedalErrorIndex", this.GetType(), PedalErrorIndex);
            pluginManager.SetPropertyValue("PedalErrorCode", this.GetType(), PedalErrorCode);


        }




        /////////********************************************************************************************************************/
        /////////*							read serial stream																		*/
        /////////********************************************************************************************************************/
        ////////public System.Windows.Forms.Timer[] pedal_serial_read_timer = new System.Windows.Forms.Timer[3];
        ////////public void openSerialAndAddReadCallback(uint pedalIdx)
        ////////{

        ////////    // serial port settings
        ////////    _serialPort[pedalIdx].Handshake = Handshake.None;
        ////////    _serialPort[pedalIdx].Parity = Parity.None;
        ////////    //_serialPort[pedalIdx].StopBits = StopBits.None;


        ////////    _serialPort[pedalIdx].ReadTimeout = 2000;
        ////////    _serialPort[pedalIdx].WriteTimeout = 500;

        ////////    // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
        ////////    _serialPort[pedalIdx].Encoding = System.Text.Encoding.GetEncoding(28591);

        ////////    _serialPort[pedalIdx].DtrEnable = false;

        ////////    _serialPort[pedalIdx].NewLine = "\r\n";
        ////////    _serialPort[pedalIdx].ReadBufferSize = 10000;


        ////////    _serialPort[pedalIdx].Open();


        ////////    // read callback
        ////////    pedal_serial_read_timer[pedalIdx] = new System.Windows.Forms.Timer();
        ////////    pedal_serial_read_timer[pedalIdx].Tick += new EventHandler(timer1_Tick);
        ////////    pedal_serial_read_timer[pedalIdx].Tag = pedalIdx;
        ////////    pedal_serial_read_timer[pedalIdx].Interval = 100; // in miliseconds
        ////////    pedal_serial_read_timer[pedalIdx].Start();
        ////////    System.Threading.Thread.Sleep(100);
        ////////}

        ////////public void closeSerialAndStopReadCallback(uint pedalIdx)
        ////////{
        ////////    if (pedal_serial_read_timer[pedalIdx] != null)
        ////////    {
        ////////        pedal_serial_read_timer[pedalIdx].Stop();
        ////////        pedal_serial_read_timer[pedalIdx].Dispose();
        ////////    }
        ////////    System.Threading.Thread.Sleep(300);
        ////////    if (_serialPort[pedalIdx].IsOpen)
        ////////    {
        ////////        _serialPort[pedalIdx].DiscardInBuffer();
        ////////        _serialPort[pedalIdx].DiscardOutBuffer();
        ////////        _serialPort[pedalIdx].Close();

        ////////    }
        ////////}


        ////////int printCtr = 0;
        ////////unsafe public void timer1_Tick(object sender, EventArgs e)
        ////////{

        ////////    // if WPF isn't available, update the WPF handler and skip
        ////////    if (wpfHandler == null)
        ////////    {
        ////////        //SettingsControlDemo wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(this);

        ////////        wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(pluginHandle);
        ////////        return;
        ////////    }

        ////////    int pedalSelected = Int32.Parse((sender as System.Windows.Forms.Timer).Tag.ToString());
        ////////    //int pedalSelected = (int)(sender as System.Windows.Forms.Timer).Tag;

        ////////    bool pedalStateHasAlreadyBeenUpdated_b = false;

        ////////    // once the pedal has identified, go ahead
        ////////    if (pedalSelected < 3)
        ////////    //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
        ////////    {

        ////////        SerialPort sp = _serialPort[pedalSelected];



        ////////        // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


        ////////        //int length = sizeof(DAP_config_st);
        ////////        //byte[] newBuffer_config = new byte[length];

        ////////        if (sp.IsOpen)
        ////////        {
        ////////            int receivedLength = sp.BytesToRead;

        ////////            if (receivedLength > 0)
        ////////            {

        ////////                string incomingData = sp.ReadExisting();

        ////////                //if the data doesn't end with a stop char this will signal to keep it in _data 
        ////////                //for appending to the following read of data
        ////////                bool endsWithStop = wpfHandler.EndsWithStop(incomingData);

        ////////                //each array object will be sent separately to the callback
        ////////                string[] dataArray = incomingData.Split(wpfHandler.STOPCHAR, StringSplitOptions.None);

        ////////                for (int i = 0; i < dataArray.Length - 1; i++)
        ////////                {
        ////////                    string newData = dataArray[i];

        ////////                    //if you are at the last object in the array and this hasn't got a stopchar after
        ////////                    //it will be saved in _data
        ////////                    if (!endsWithStop && (i == dataArray.Length - 2))
        ////////                    {
        ////////                        wpfHandler._data[pedalSelected] += newData;
        ////////                    }
        ////////                    else
        ////////                    {
        ////////                        string dataToSend = wpfHandler._data[pedalSelected] + newData;
        ////////                        wpfHandler._data[pedalSelected] = "";



        ////////                        // check for pedal state struct
        ////////                        if ((dataToSend.Length == sizeof(DAP_state_st)))
        ////////                        {

        ////////                            // transform string into byte
        ////////                            fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
        ////////                            {
        ////////                                // create a fixed size buffer
        ////////                                int length = sizeof(DAP_state_st);
        ////////                                byte[] newBuffer_state_2 = new byte[length];

        ////////                                // copy the received bytes into byte array
        ////////                                for (int j = 0; j < length; j++)
        ////////                                {
        ////////                                    newBuffer_state_2[j] = p[j];
        ////////                                }

        ////////                                // parse byte array as config struct
        ////////                                DAP_state_st pedalState_read_st = wpfHandler.getStateFromBytes(newBuffer_state_2);

        ////////                                // check whether receive struct is plausible
        ////////                                DAP_state_st* v_state = &pedalState_read_st;
        ////////                                byte* p_state = (byte*)v_state;

        ////////                                // payload type check
        ////////                                bool check_payload_state_b = false;
        ////////                                if (pedalState_read_st.payloadHeader_.payloadType == Constants.pedalStatePayload_type)
        ////////                                {
        ////////                                    check_payload_state_b = true;
        ////////                                }

        ////////                                // CRC check
        ////////                                bool check_crc_state_b = false;
        ////////                                if (checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState)) == pedalState_read_st.payloadFooter_.checkSum)
        ////////                                {
        ////////                                    check_crc_state_b = true;
        ////////                                }

        ////////                                if ((check_payload_state_b) && check_crc_state_b)
        ////////                                {

        ////////                                    if (pedalStateHasAlreadyBeenUpdated_b == false)
        ////////                                    {
        ////////                                        wpfHandler.TextBox_debugOutput.Text = "Pedal pos: " + pedalState_read_st.payloadPedalState_.pedalPosition_u16;
        ////////                                        wpfHandler.TextBox_debugOutput.Text += "Pedal force: " + pedalState_read_st.payloadPedalState_.pedalForce_u16;
        ////////                                        pedalStateHasAlreadyBeenUpdated_b = true;

        ////////                                        wpfHandler.text_point_pos.Opacity = 0;
        ////////                                        double control_rect_value_max = 65535;
        ////////                                        double dyy = wpfHandler.canvas.Height / control_rect_value_max;
        ////////                                        double dxx = wpfHandler.canvas.Width / control_rect_value_max;


        ////////                                        Canvas.SetLeft(wpfHandler.rect_State, dxx * pedalState_read_st.payloadPedalState_.pedalPosition_u16 - wpfHandler.rect_State.Width / 2);
        ////////                                        Canvas.SetTop(wpfHandler.rect_State, wpfHandler.canvas.Height - dyy * pedalState_read_st.payloadPedalState_.pedalForce_u16 - wpfHandler.rect_State.Height / 2);
        ////////                                    }


        ////////                                    continue;
        ////////                                }
        ////////                            }
        ////////                        }


        ////////                        // decode into config struct
        ////////                        if ((wpfHandler.waiting_for_pedal_config[pedalSelected]) && (dataToSend.Length == sizeof(DAP_config_st)))
        ////////                        {
        ////////                            DAP_config_st tmp;


        ////////                            // transform string into byte
        ////////                            fixed (byte* p = System.Text.Encoding.GetEncoding(28591).GetBytes(dataToSend))
        ////////                            {
        ////////                                // create a fixed size buffer
        ////////                                int length = sizeof(DAP_config_st);
        ////////                                byte[] newBuffer_config_2 = new byte[length];

        ////////                                // copy the received bytes into byte array
        ////////                                for (int j = 0; j < length; j++)
        ////////                                {
        ////////                                    newBuffer_config_2[j] = p[j];
        ////////                                }

        ////////                                // parse byte array as config struct
        ////////                                DAP_config_st pedalConfig_read_st = wpfHandler.getConfigFromBytes(newBuffer_config_2);

        ////////                                // check whether receive struct is plausible
        ////////                                DAP_config_st* v_config = &pedalConfig_read_st;
        ////////                                byte* p_config = (byte*)v_config;

        ////////                                // payload type check
        ////////                                bool check_payload_config_b = false;
        ////////                                if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
        ////////                                {
        ////////                                    check_payload_config_b = true;
        ////////                                }

        ////////                                // CRC check
        ////////                                bool check_crc_config_b = false;
        ////////                                if (checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
        ////////                                {
        ////////                                    check_crc_config_b = true;
        ////////                                }

        ////////                                if ((check_payload_config_b) && check_crc_config_b)
        ////////                                {
        ////////                                    wpfHandler.waiting_for_pedal_config[pedalSelected] = false;
        ////////                                    wpfHandler.dap_config_st[pedalSelected] = pedalConfig_read_st;
        ////////                                    wpfHandler.updateTheGuiFromConfig();

        ////////                                    continue;
        ////////                                }
        ////////                                else
        ////////                                {
        ////////                                    wpfHandler.TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
        ////////                                    wpfHandler.TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
        ////////                                }
        ////////                            }

        ////////                        }
        ////////                        //else
        ////////                        //{


        ////////                        // When too many messages are received, only print every Nth message

        ////////                        // When only a few messages are received, make the counter greater than N thus every message is printed
        ////////                        if (dataArray.Length < 10)
        ////////                        {
        ////////                            printCtr = 600;
        ////////                        }

        ////////                        if (printCtr++ > 200)
        ////////                        {
        ////////                            printCtr = 0;
        ////////                            wpfHandler.TextBox_serialMonitor.Text += dataToSend + "\n";
        ////////                            wpfHandler.TextBox_serialMonitor.ScrollToEnd();
        ////////                        }

        ////////                        //}


        ////////                    }

        ////////                    try
        ////////                    {
        ////////                        while (wpfHandler.TextBox_serialMonitor.LineCount > 30)
        ////////                        {
        ////////                            wpfHandler.TextBox_serialMonitor.Text = wpfHandler.TextBox_serialMonitor.Text.Remove(0, wpfHandler.TextBox_serialMonitor.GetLineLength(0));
        ////////                        }
        ////////                    }
        ////////                    catch { }







        ////////                    //limits the data stored to 1000 to avoid using up all the memory in case of 
        ////////                    //failure to register callback or include stopchar

        ////////                    if (wpfHandler._data[pedalSelected].Length > 1000)
        ////////                    {
        ////////                        wpfHandler._data[pedalSelected] = "";
        ////////                    }


        ////////                }

        ////////                // obtain data and check whether it is from known payload type or just debug info

        ////////            }

        ////////        }
        ////////    }
        ////////}



        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {

            return new SettingsControlDemo(this);
        }


        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {           
            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);

            // close serial communication
            if (wpfHandle != null)
            {

                try
                {
                    //wpfHandle.joystick.Release();
                    //wpfHandle.joystick.Dispose();
                    if (wpfHandle.joystick != null)
                    {
                        wpfHandle.joystick.RelinquishVJD(Settings.vjoy_order);
                    }
                    
                    
                }
                catch (Exception caughtEx)
                { 
                }
                

                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    wpfHandle.closeSerialAndStopReadCallback(pedalIdx);
                }
            }
            
            if (ToastNotificationManager.History.GetHistory("Pedal_notification").Count != 0)
            {
                ToastNotificationManager.History.Remove("Pedal_notification");
            }
            

        }



        public bool PortExists(string portName)
        {
            string[] portNames = SerialPort.GetPortNames();
            return Array.Exists(portNames, name => name.Equals(portName, StringComparison.OrdinalIgnoreCase));
        }





        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {

            pluginHandle = pluginManager;
        
            SimHub.Logging.Current.Info("Starting DIY active pedal plugin");

            // Load settings
            Settings = this.ReadCommonSettings<DataPluginDemoSettings>("GeneralSettings", () => new DataPluginDemoSettings());

            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            //this.AttachDelegate("CurrentDateTime", () => DateTime.Now);
            pluginManager.AddProperty("ProfileIndex", this.GetType(), profile_index);
            pluginManager.AddProperty("SelectedPedal", this.GetType(), current_pedal);
            pluginManager.AddProperty("Action", this.GetType(), current_action);
            pluginManager.AddProperty("ABS_effect_status", this.GetType(), Settings.ABS_enable_flag[Settings.table_selected]);
            pluginManager.AddProperty("RPM_effect_status", this.GetType(), Settings.RPM_enable_flag[Settings.table_selected]);
            pluginManager.AddProperty("Gforce_effect_status", this.GetType(), Settings.G_force_enable_flag[Settings.table_selected]);
            pluginManager.AddProperty("WheelSlip_effect_status", this.GetType(), Settings.WS_enable_flag[Settings.table_selected]);
            pluginManager.AddProperty("RoadImpact_effect_status", this.GetType(), Settings.Road_impact_enable_flag[Settings.table_selected]);
            pluginManager.AddProperty("Overlay_display", this.GetType(), overlay_display);
            pluginManager.AddProperty("Theme_color", this.GetType(), simhub_theme_color);
            pluginManager.AddProperty("debugvalue", this.GetType(), debug_value);
            pluginManager.AddProperty("rudder_status", this.GetType(), Rudder_status);
            pluginManager.AddProperty("rudder_brake_status", this.GetType(), Rudder_brake_status);
            pluginManager.AddProperty("pedal_position", this.GetType(), pedal_state_in_ratio);
            pluginManager.AddProperty("PedalErrorIndex", this.GetType(), PedalErrorIndex);
            pluginManager.AddProperty("PedalErrorCode", this.GetType(), PedalErrorCode);

            for (uint pedali=0; pedali < 3; pedali++)
            {
                Action_currentTime[pedali] = new DateTime();
                Action_currentTime[pedali]=DateTime.Now;
                Action_lastTime[pedali] = new DateTime();
                Action_lastTime[pedali] = DateTime.Now;
            }
            

            // Declare an event
            //this.AddEvent("SpeedWarning");


            // Declare an action which can be called
            /*
            this.AddAction("IncrementSpeedWarning",(a, b) =>
            {
                Settings.SpeedWarningLevel++;
                SimHub.Logging.Current.Info("Speed warning changed");
            });

            // Declare an action which can be called
            this.AddAction("DecrementSpeedWarning", (a, b) =>
            {
                Settings.SpeedWarningLevel--;
            });

            */


            this.AddAction("ChangeSlotA", (a, b) =>
            {
                profile_index = 0;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotA");
                current_profile = "Slot A";
                current_action= "Slot A";
            });

            this.AddAction("ChangeSlotB", (a, b) =>
            {
                
                profile_index = 1;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotB");
                current_profile = "Slot B";
                current_action = "Slot B";
            });

            this.AddAction("ChangeSlotC", (a, b) =>
            {
                profile_index = 2;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotC");
                current_profile = "Slot C";
                current_action = "Slot C";
            });

            this.AddAction("ChangeSlotD", (a, b) =>
            {
                profile_index = 3;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotD");
                current_profile = "Slot D";
                current_action = "Slot D";
            });
            this.AddAction("ChangeSlotE", (a, b) =>
            {
                profile_index = 4;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotE");
                current_profile = "Slot E";
                current_action = "Slot E";
            });
            this.AddAction("ChangeSlotF", (a, b) =>
            {
                profile_index = 5;
                Page_update_flag = true;
                SimHub.Logging.Current.Info("SlotF");
                current_profile = "Slot F";
                current_action = "Slot F";
            });
            this.AddAction("SendConfigToPedal", (a, b) =>
            {
                sendconfig_flag =1;
                SimHub.Logging.Current.Info("SendConfig");
                current_action = "Send Config to Pedal";
            });

            this.AddAction("PreviousProfile", (a, b) =>
            {
                if (profile_index == 0)
                {
                    profile_index=5;
                }
                else
                {
                    profile_index--;
                }
                

                Page_update_flag = true;
                SimHub.Logging.Current.Info("PreviousProfile");
                current_action = "Previous Profile";
            });

            this.AddAction("NextProfile", (a, b) =>
            {
                profile_index++;
                if (profile_index > 5)
                {
                    profile_index = 0;
                }
                Page_update_flag = true;
                SimHub.Logging.Current.Info("NextProfile");
                current_action = "Next Profile";
            });
            this.AddAction("NextPedal", (a, b) =>
            {
                Settings.table_selected++;
                if (Settings.table_selected > 2)
                {
                    Settings.table_selected = 0;
                }
                Page_update_flag = true;
                SimHub.Logging.Current.Info("NextPedal");
                current_action = "Next Pedal";
            });
            this.AddAction("PreviousPedal", (a, b) =>
            {
                
                if (Settings.table_selected == 0)
                {
                    Settings.table_selected = 2;
                }
                else
                {
                    Settings.table_selected--;
                }
                Page_update_flag = true;
                SimHub.Logging.Current.Info("PreviousPedal");
                current_action = "Previous Pedal";
            });
            this.AddAction("ABStoggle", (a, b) =>
            {
                if (Settings.ABS_enable_flag[Settings.table_selected] == 0)
                {
                    Settings.ABS_enable_flag[Settings.table_selected] = 1;
                    SimHub.Logging.Current.Info("ABS on");
                    current_action = "ABS On";
                }
                else
                {
                    Settings.ABS_enable_flag[Settings.table_selected] = 0;
                    SimHub.Logging.Current.Info("ABS off");
                    current_action = "ABS Off";
                }
                Page_update_flag = true;
            });
            this.AddAction("RPMtoggle", (a, b) =>
            {
                if (Settings.RPM_enable_flag[Settings.table_selected] == 0)
                {
                    Settings.RPM_enable_flag[Settings.table_selected] = 1;
                    SimHub.Logging.Current.Info("RPM on");
                    current_action = "RPM On";
                }
                else
                {
                    Settings.RPM_enable_flag[Settings.table_selected] = 0;
                    SimHub.Logging.Current.Info("RPM off");
                    current_action = "RPM Off";
                }
                Page_update_flag = true;
            });
            this.AddAction("Gforce_toggle", (a, b) =>
            {
                if (Settings.table_selected == 1)
                {
                    if (Settings.G_force_enable_flag[Settings.table_selected] == 0)
                    {
                        Settings.G_force_enable_flag[Settings.table_selected] = 1;
                        SimHub.Logging.Current.Info("Gforce on");
                        current_action = "Gforce On";
                    }
                    else
                    {
                        Settings.G_force_enable_flag[Settings.table_selected] = 0;
                        SimHub.Logging.Current.Info("Gforce off");
                        current_action = "Gforce Off";
                    }
                    Page_update_flag = true;
                }

            });
            this.AddAction("WheelSliptoggle", (a, b) =>
            {
                if (Settings.WS_enable_flag[Settings.table_selected] == 0)
                {
                    Settings.WS_enable_flag[Settings.table_selected] = 1;
                    SimHub.Logging.Current.Info("WheelSlip on");
                    current_action = "Wheel Slip On";
                }
                else
                {
                    Settings.WS_enable_flag[Settings.table_selected] = 0;
                    SimHub.Logging.Current.Info("WheelSlip off");
                    current_action = "Wheel Slip Off";
                }
                Page_update_flag = true;
            });

            this.AddAction("RoadImpacttoggle", (a, b) =>
            {
                if (Settings.Road_impact_enable_flag[Settings.table_selected] == 0)
                {
                    Settings.Road_impact_enable_flag[Settings.table_selected] = 1;
                    SimHub.Logging.Current.Info("RoadImpact on");
                    current_action = "Wheel Slip On";
                }
                else
                {
                    Settings.Road_impact_enable_flag[Settings.table_selected] = 0;
                    SimHub.Logging.Current.Info("RoadImpact off");
                    current_action = "RoadImpact Off";
                }
                Page_update_flag = true;
            });

            this.AddAction("OverlayToggle", (a, b) =>
            {
                Page_update_flag = true;
                SimHub.Logging.Current.Info("OverlayToggle");               
                current_action = "OverlayToggle";
                if (overlay_display == 1)
                {
                    overlay_display = 0;
                }
                else
                {
                    overlay_display = 1;
                }
            });
            this.AddAction("Rudder Brake", (a, b) =>
            {
                Rudder_brake_enable_flag = true;
                SimHub.Logging.Current.Info("Rudder Brake");

            });
            this.AddAction("Rudder", (a, b) =>
            {

                Rudder_enable_flag=true;
                SimHub.Logging.Current.Info("Rudder action");

            });

            //Settings.selectedJsonIndexLast[0]
            //SimHub.Logging.Current.Info("Diy active pedas plugin - Test 1");
            //SimHub.Logging.Current.Info("Diy active pedas plugin - COM port: " + Settings.selectedComPortNames[0]);




            // get WPF handler
            //wpfHandler = (SettingsControlDemo)GetWPFSettingsControl(pluginManager);

            //if (wpfHandler.)
            {
                // prepare serial port interfaces
                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {

                    _serialPort[pedalIdx].Handshake = Handshake.None;
                    _serialPort[pedalIdx].RtsEnable = false;
                    _serialPort[pedalIdx].DtrEnable = false;


                    if (_serialPort[pedalIdx].IsOpen)
                    {
                        System.Threading.Thread.Sleep(300);
                    }


                    try
                    {
                        _serialPort[pedalIdx].PortName = Settings.selectedComPortNames[pedalIdx];
                    }
                    catch (Exception caughtEx)
                    {
                    }
                    
                    //try connect back to com port
                    if (Settings.auto_connect_flag[pedalIdx] == 1)
                    {

                        if (Settings.connect_status[pedalIdx] == 1)
                        {
                            //_serialPort[pedalIdx].PortName = Settings.selectedComPortNames[pedalIdx];
                            //SerialPort.GetPortNames
                            if (PortExists(_serialPort[pedalIdx].PortName))
                            {
                                if (_serialPort[pedalIdx].IsOpen == false)
                                {
                                    //if (wpfHandle != null)
                                    //{
                                    //    wpfHandle.openSerialAndAddReadCallback(pedalIdx);
                                    //}

                                    connectSerialPort[pedalIdx] = true;
                                }
                                else
                                {
                                    //if (wpfHandle != null)
                                    //{
                                    //    wpfHandle.closeSerialAndStopReadCallback(pedalIdx);
                                    //}
                                    //ConnectToPedal.IsChecked = false;
                                    //TextBox_debugOutput.Text = "Serialport already open, close it";
                                    //Settings.connect_status[pedalIdx] = 0;
                                    connectSerialPort[pedalIdx] = false;
                                }


                            }
                            else
                            {
                                //Settings.connect_status[pedalIdx] = 0;
                                connectSerialPort[pedalIdx] = false;
                            }
                        }
                        else
                        {
                            //Settings.connect_status[pedalIdx] = 0;
                            connectSerialPort[pedalIdx] = false;
                        }

                    }
                    

                }

            }

            
            
            

         


            //// check if Json config files are present, otherwise create new ones
            //for (uint jsonIndex = 0; jsonIndex < ComboBox_JsonFileSelected.Items.Count; jsonIndex++)
            //{
            //	// which config file is seleced
            //	string currentDirectory = Directory.GetCurrentDirectory();
            //	string dirName = currentDirectory + "\\PluginsData\\Common";
            //	string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            //	string fileName = dirName + "\\" + jsonFileName + ".json";


            //	// Check if file already exists, otherwise create    
            //	if (!File.Exists(fileName))
            //	{
            //		// create default config
            //		// https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
            //		// https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
            //		var stream1 = new MemoryStream();
            //		var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
            //		ser.WriteObject(stream1, dap_config_initial_st);

            //		stream1.Position = 0;
            //		StreamReader sr = new StreamReader(stream1);
            //		string jsonString = sr.ReadToEnd();

            //		System.IO.File.WriteAllText(fileName, jsonString);
            //	}
            //}



            dap_config_initial_st.payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
            dap_config_initial_st.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            dap_config_initial_st.payloadHeader_.storeToEeprom = 0;
            dap_config_initial_st.payloadPedalConfig_.pedalStartPosition = 35;
            dap_config_initial_st.payloadPedalConfig_.pedalEndPosition = 80;
            dap_config_initial_st.payloadPedalConfig_.maxForce = 50;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p020 = 20;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p040 = 40;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p060 = 60;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p080 = 80;
            dap_config_initial_st.payloadPedalConfig_.relativeForce_p100 = 100;
            dap_config_initial_st.payloadPedalConfig_.dampingPress = 0;
            dap_config_initial_st.payloadPedalConfig_.dampingPull = 0;
            dap_config_initial_st.payloadPedalConfig_.absFrequency = 5;
            dap_config_initial_st.payloadPedalConfig_.absAmplitude = 100;
            dap_config_initial_st.payloadPedalConfig_.absPattern = 0;
            dap_config_initial_st.payloadPedalConfig_.absForceOrTarvelBit = 0;

            dap_config_initial_st.payloadPedalConfig_.lengthPedal_a = 205;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_b = 220;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_d = 60;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_c_horizontal = 215;
            dap_config_initial_st.payloadPedalConfig_.lengthPedal_c_vertical = 60;

            dap_config_initial_st.payloadPedalConfig_.Simulate_ABS_trigger = 0;
            dap_config_initial_st.payloadPedalConfig_.Simulate_ABS_value = 50;
            dap_config_initial_st.payloadPedalConfig_.RPM_max_freq = 40;
            dap_config_initial_st.payloadPedalConfig_.RPM_min_freq = 10;
            dap_config_initial_st.payloadPedalConfig_.RPM_AMP = 5;
            dap_config_initial_st.payloadPedalConfig_.BP_trigger_value = 50;
            dap_config_initial_st.payloadPedalConfig_.BP_amp = 1;
            dap_config_initial_st.payloadPedalConfig_.BP_freq = 15;
            dap_config_initial_st.payloadPedalConfig_.BP_trigger = 0;
            dap_config_initial_st.payloadPedalConfig_.G_multi = 50;
            dap_config_initial_st.payloadPedalConfig_.G_window = 60;
            dap_config_initial_st.payloadPedalConfig_.WS_amp = 1;
            dap_config_initial_st.payloadPedalConfig_.WS_freq = 15;

            dap_config_initial_st.payloadPedalConfig_.maxGameOutput = 100;

            dap_config_initial_st.payloadPedalConfig_.kf_modelNoise = 128;
            dap_config_initial_st.payloadPedalConfig_.kf_modelOrder = 0;
            dap_config_initial_st.payloadPedalConfig_.debug_flags_0 = 0;

            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_0 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_1 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_2 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_3 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_a_4 = 0;

            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_0 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_1 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_2 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_3 = 0;
            dap_config_initial_st.payloadPedalConfig_.cubic_spline_param_b_4 = 0;

            dap_config_initial_st.payloadPedalConfig_.PID_p_gain = 0.3f;
            dap_config_initial_st.payloadPedalConfig_.PID_i_gain = 50.0f;
            dap_config_initial_st.payloadPedalConfig_.PID_d_gain = 0.0f;
            dap_config_initial_st.payloadPedalConfig_.PID_velocity_feedforward_gain = 0.0f;

            dap_config_initial_st.payloadPedalConfig_.MPC_0th_order_gain = 1.0f;

            dap_config_initial_st.payloadPedalConfig_.control_strategy_b = 2;

            dap_config_initial_st.payloadPedalConfig_.loadcell_rating = 150;

            dap_config_initial_st.payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;

            dap_config_initial_st.payloadPedalConfig_.invertLoadcellReading_u8 = 0;

            dap_config_initial_st.payloadPedalConfig_.invertMotorDirection_u8 = 0;

            dap_config_initial_st.payloadPedalConfig_.spindlePitch_mmPerRev_u8 = 5;
            dap_config_initial_st.payloadPedalConfig_.pedal_type = 0;
            dap_config_initial_st.payloadPedalConfig_.OTA_flag = 0;
            dap_config_initial_st.payloadPedalConfig_.enableReboot_u8 = 1;








        }
    }
}