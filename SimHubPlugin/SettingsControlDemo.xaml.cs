﻿//using SimHub.Plugins.OutputPlugins.Dash.GLCDTemplating;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media.TextFormatting;
using System.Text.Json;
using FMOD;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Web;
using MahApps.Metro.Controls;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Input;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using SimHub.Plugins.OutputPlugins.GraphicalDash.PSE;
using SimHub.Plugins.Styles;
using System.Windows.Media;
using System.Runtime.Remoting.Messaging;
using SimHub.Plugins.OutputPlugins.GraphicalDash.Behaviors.DoubleText.Imp;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
using SimHub.Plugins;
using log4net.Plugin;
//using System.Drawing;

using vJoyInterfaceWrap;
//using vJoy.Wrapper;
using System.Runtime;
using SimHub.Plugins.DataPlugins.ShakeItV3.Settings;
using System.Windows.Media.Effects;
using System.Diagnostics;
using System.Collections;
using System.Linq;
using Windows.UI.Notifications;
//using System.Diagnostics;
using System.Windows.Navigation;
using System.CodeDom;
using System.Media;

// Win 11 install, see https://github.com/jshafer817/vJoy/releases
//using vJoy.Wrapper;



namespace User.PluginSdkDemo
{


    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class SettingsControlDemo : System.Windows.Controls.UserControl
    {


        // payload revisiom
        //public uint pedalConfigPayload_version = 110;
        //public uint pedalConfigPayload_type = 100;
        //public uint pedalActionPayload_type = 110;

        public uint indexOfSelectedPedal_u = 1;
        public uint profile_select = 0;
        public DIY_FFB_Pedal Plugin { get; }


        public DAP_config_st[] dap_config_st = new DAP_config_st[3];
        private string stringValue;


        public bool[] waiting_for_pedal_config = new bool[3];
        public System.Windows.Forms.Timer[] pedal_serial_read_timer = new System.Windows.Forms.Timer[3];
        public System.Windows.Forms.Timer connect_timer;
        public System.Windows.Forms.Timer ESP_host_serial_timer;
        //public System.Timers.Timer[] pedal_serial_read_timer = new System.Timers.Timer[3];
        int printCtr = 0;

        public double[] Force_curve_Y = new double[100];
        public bool debug_flag = false;

        //public VirtualJoystick joystick;
        internal vJoyInterfaceWrap.vJoy joystick;


        public bool[] dumpPedalToResponseFile = new bool[3];

        private SolidColorBrush defaultcolor;
        private SolidColorBrush lightcolor;
        private string info_text_connection;
        private int current_pedal_travel_state= 0;
        private int gridline_kinematic_count_original = 0;
        private double[] Pedal_position_reading=new double[3];
        private bool[] Serial_connect_status = new bool[3] { false,false,false};

        /*
        private double kinematicDiagram_zeroPos_OX = 100;
        private double kinematicDiagram_zeroPos_OY = 20;
        private double kinematicDiagram_zeroPos_scale = 1;
        */



        // read config from JSON on startup
        //ReadStructFromJson();


        // read JSON config from JSON file
        //private void ReadStructFromJson()
        //{



        //    try
        //    {
        //        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0
        //        // https://www.educative.io/answers/how-to-read-a-json-file-in-c-sharp

        //        string currentDirectory = Directory.GetCurrentDirectory();
        //        string dirName = currentDirectory + "\\PluginsData\\Common";
        //        //string jsonFileName = ComboBox_JsonFileSelected.Text;
        //        string jsonFileName = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();
        //        string fileName = dirName + "\\" + jsonFileName + ".json";

        //        string text = System.IO.File.ReadAllText(fileName);

        //        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
        //        var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
        //        dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
        //        //TextBox_debugOutput.Text = "Config loaded!";
        //        //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.Text;
        //        //TextBox_debugOutput.Text += "    ";
        //        //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedIndex;

        //        updateTheGuiFromConfig();

        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }


        //}
        private void ToastNotification(string message1, string message2)
        {
            
            var xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var text = xml.GetElementsByTagName("text");
            text[0].AppendChild(xml.CreateTextNode(message1));
            text[1].AppendChild(xml.CreateTextNode(message2));
            var toast = new ToastNotification(xml);
            toast.ExpirationTime = DateTime.Now.AddSeconds(1);
            toast.Tag = "Pedal_notification";
            ToastNotificationManager.CreateToastNotifier("FFB Pedal Dashboard").Show(toast);



        }

        private void vjoy_axis_initialize()
        {
            //center all axis/hats reader
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_Y);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_Z);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RX);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RY);
            joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);
            //joystick.SetJoystickHat(0, Hats.Hat);
            //joystick.SetJoystickHat(0, Hats.HatExt1);
            //joystick.SetJoystickHat(0, Hats.HatExt2);
            //joystick.SetJoystickHat(0, Hats.HatExt3);

        }


        private void DrawGridLines()
        {
            // Specify the number of rows and columns for the grid
            int rowCount = 5;
            int columnCount = 5;

            // Calculate the width and height of each cell
            double cellWidth = canvas.Width / columnCount;
            double cellHeight = canvas.Height / rowCount;


            
            // Draw horizontal gridlines
            for (int i = 1; i < rowCount; i++)
            {
                Line line = new Line
                {
                    X1 = 0,
                    Y1 = i * cellHeight,
                    X2 = canvas.Width,
                    Y2 = i * cellHeight,
                    //Stroke = Brush.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1

                };
                canvas.Children.Add(line);
          
            }

            // Draw vertical gridlines
            for (int i = 1; i < columnCount; i++)
            {
                Line line = new Line
                {
                    X1 = i * cellWidth,
                    Y1 = 0,
                    X2 = i * cellWidth,
                    Y2 = canvas.Height,
                    //Stroke = Brushes.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1
                };
                canvas.Children.Add(line);
                
            }
        }



        private void DrawGridLines_kinematicCanvas(double OX, double OY, double scale_i)
        {

            if (gridline_kinematic_count_original > 0)
            {
                for (int i = 0; i < gridline_kinematic_count_original; i++)
                {
                    if (canvas_kinematic.Children.Count != 0)
                    {
                        canvas_kinematic.Children.RemoveAt(canvas_kinematic.Children.Count - 1);
                    }
                }                
            }
            double scale = scale_i;
            double gridlineSpacing = 50 / scale;

            double cellWidth = gridlineSpacing ;
            double cellHeight = gridlineSpacing ;

            // we want the gridlines to be centered at pedal position O
            // --> calculate an offset
            double xOffset = OX % gridlineSpacing;
            double yOffset = OY % gridlineSpacing;


            int rowCount = (int)Math.Floor((canvas.Height - 0 * yOffset) / gridlineSpacing);
            int columnCount = (int)Math.Floor((canvas.Width - 0 * xOffset) / gridlineSpacing);


            // Draw horizontal gridlines
            for (int i = 0; i < rowCount; i++)
            {

                Line line2 = new Line
                {
                    X1 = 0,
                    Y1 = canvas_kinematic.Height - (yOffset + i * cellHeight),
                    X2 = 400,
                    Y2 = canvas_kinematic.Height - (yOffset + i * cellHeight),
                    //Stroke = Brush.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1

                };
                canvas_kinematic.Children.Add(line2);
            }

            // Draw vertical gridlines
            for (int i = 0; i < columnCount; i++)
            {

                Line line2 = new Line
                {
                    X1 = xOffset + i * cellWidth,
                    Y1 = 0,
                    X2 = xOffset + i * cellWidth,
                    Y2 = canvas_kinematic.Height,
                    //Stroke = Brushes.Black,
                    Stroke = System.Windows.Media.Brushes.LightSteelBlue,
                    StrokeThickness = 1,
                    Opacity = 0.1
                };
                canvas_kinematic.Children.Add(line2);

            }
            gridline_kinematic_count_original = columnCount + rowCount;
        }

        private void InitReadStructFromJson()
        {



            try
            {
                // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0
                // https://www.educative.io/answers/how-to-read-a-json-file-in-c-sharp
                string jsonFileName = "NA";

                string currentDirectory = Directory.GetCurrentDirectory();
                string dirName = currentDirectory + "\\PluginsData\\Common";
                //string jsonFileName = ComboBox_JsonFileSelected.Text;
                if (indexOfSelectedPedal_u == 0)
                {
                    jsonFileName = ("DiyPedalConfig_Clutch_Default");
                }
                else if (indexOfSelectedPedal_u == 1)
                {
                    jsonFileName = ("DiyPedalConfig_Brake_Default");
                }
                else if (indexOfSelectedPedal_u == 2)
                {
                    jsonFileName = ("DiyPedalConfig_Accelerator_Default");
                }

                string fileName = dirName + "\\" + jsonFileName + ".json";
                string text = System.IO.File.ReadAllText(fileName);



                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
                dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
                TextBox_debugOutput.Text = "Config loaded!" + jsonFileName;
                //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.Text;
                //TextBox_debugOutput.Text += "    ";
                //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedIndex;

                updateTheGuiFromConfig();

            }
            catch (Exception caughtEx)
            {

                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }


        }

        private void UpdateSerialPortList_click()
        {

            var SerialPortSelectionArray = new List<SerialPortChoice>();
            string[] comPorts = SerialPort.GetPortNames();

            comPorts = comPorts.Distinct().ToArray(); // unique

            if (comPorts.Length > 0)
            {

                foreach (string portName in comPorts)
                {
                    SerialPortSelectionArray.Add(new SerialPortChoice(portName, portName));
                }
            }
            else
            {
                SerialPortSelectionArray.Add(new SerialPortChoice("NA", "NA"));
            }

            SerialPortSelection.DataContext = SerialPortSelectionArray;
            SerialPortSelection_ESPNow.DataContext = SerialPortSelectionArray;
        }

        private bool isDragging = false;
        private Point offset;

        public void DAP_config_set_default(uint pedalIdx)
        {
            dumpPedalToResponseFile[pedalIdx] = false;
            dap_config_st[pedalIdx].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
            dap_config_st[pedalIdx].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            dap_config_st[pedalIdx].payloadPedalConfig_.pedalStartPosition = 35;
            dap_config_st[pedalIdx].payloadPedalConfig_.pedalEndPosition = 80;
            dap_config_st[pedalIdx].payloadPedalConfig_.maxForce = 50;
            dap_config_st[pedalIdx].payloadPedalConfig_.preloadForce = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p020 = 20;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p040 = 40;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p060 = 60;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p080 = 80;
            dap_config_st[pedalIdx].payloadPedalConfig_.relativeForce_p100 = 100;
            dap_config_st[pedalIdx].payloadPedalConfig_.dampingPress = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.dampingPull = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.absFrequency = 5;
            dap_config_st[pedalIdx].payloadPedalConfig_.absAmplitude = 20;
            dap_config_st[pedalIdx].payloadPedalConfig_.absPattern = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.absForceOrTarvelBit = 0;

            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_a = 205;
            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_b = 220;
            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_d = 60;
            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_c_horizontal = 215;
            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_c_vertical = 60;
            dap_config_st[pedalIdx].payloadPedalConfig_.lengthPedal_travel = 100;

            dap_config_st[pedalIdx].payloadPedalConfig_.Simulate_ABS_trigger = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.Simulate_ABS_value = 80;
            dap_config_st[pedalIdx].payloadPedalConfig_.RPM_max_freq = 40;
            dap_config_st[pedalIdx].payloadPedalConfig_.RPM_min_freq = 10;
            dap_config_st[pedalIdx].payloadPedalConfig_.RPM_AMP = 30;
            dap_config_st[pedalIdx].payloadPedalConfig_.BP_trigger_value = 50;
            dap_config_st[pedalIdx].payloadPedalConfig_.BP_amp = 1;
            dap_config_st[pedalIdx].payloadPedalConfig_.BP_freq = 15;
            dap_config_st[pedalIdx].payloadPedalConfig_.BP_trigger = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.G_multi = 50;
            dap_config_st[pedalIdx].payloadPedalConfig_.G_window = 10;
            dap_config_st[pedalIdx].payloadPedalConfig_.WS_amp = 1;
            dap_config_st[pedalIdx].payloadPedalConfig_.WS_freq = 15;
            dap_config_st[pedalIdx].payloadPedalConfig_.Impact_multi = 50;
            dap_config_st[pedalIdx].payloadPedalConfig_.Impact_window = 60;
            dap_config_st[pedalIdx].payloadPedalConfig_.CV_amp_1 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.CV_freq_1 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.CV_amp_2 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.CV_freq_2 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.maxGameOutput = 100;
            dap_config_st[pedalIdx].payloadPedalConfig_.kf_modelNoise = 128;
            dap_config_st[pedalIdx].payloadPedalConfig_.kf_modelOrder = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_a_0 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_a_1 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_a_2 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_a_3 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_a_4 = 0;

            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_b_0 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_b_1 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_b_2 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_b_3 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.cubic_spline_param_b_4 = 0;

            dap_config_st[pedalIdx].payloadPedalConfig_.PID_p_gain = 0.3f;
            dap_config_st[pedalIdx].payloadPedalConfig_.PID_i_gain = 50.0f;
            dap_config_st[pedalIdx].payloadPedalConfig_.PID_d_gain = 0.0f;
            dap_config_st[pedalIdx].payloadPedalConfig_.PID_velocity_feedforward_gain = 0.0f;

            dap_config_st[pedalIdx].payloadPedalConfig_.MPC_0th_order_gain = 1.0f;

            dap_config_st[pedalIdx].payloadPedalConfig_.control_strategy_b = 2;

            dap_config_st[pedalIdx].payloadPedalConfig_.loadcell_rating = 150;

            dap_config_st[pedalIdx].payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;

            dap_config_st[pedalIdx].payloadPedalConfig_.invertLoadcellReading_u8 = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.invertMotorDirection_u8 = 0;

            dap_config_st[pedalIdx].payloadPedalConfig_.spindlePitch_mmPerRev_u8 = 5;
            dap_config_st[pedalIdx].payloadPedalConfig_.pedal_type = (byte)pedalIdx;
            dap_config_st[pedalIdx].payloadPedalConfig_.OTA_flag = 0;
            dap_config_st[pedalIdx].payloadPedalConfig_.enableReboot_u8 = 0;
        }


        public SettingsControlDemo()
        {


            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {


                DAP_config_set_default(pedalIdx);
                InitializeComponent();


            }
            // debug mode invisiable
            text_debug_flag.Visibility = Visibility.Hidden;
            text_serial.Visibility = Visibility.Hidden;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Hidden;
            //InvertLoadcellReading_check.Visibility = Visibility.Hidden;
            //InvertMotorDir_check.Visibility = Visibility.Hidden;
            textBox_debug_Flag_0.Visibility = Visibility.Hidden;
            Border_serial_monitor.Visibility=Visibility.Hidden;
            Advanced_Tab.Visibility=Visibility.Hidden;
           
            //btn_serial.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_restart.Visibility = System.Windows.Visibility.Hidden;
            btn_system_id.Visibility = System.Windows.Visibility.Hidden;
            btn_reset_default.Visibility = System.Windows.Visibility.Hidden;
            dump_pedal_response_to_file.Visibility = System.Windows.Visibility.Hidden;
            //Label_reverse_LC.Visibility=Visibility.Hidden;
            //Label_reverse_servo.Visibility=Visibility.Hidden;
            btn_test.Visibility=Visibility.Hidden;
            //setting drawing color with Simhub theme workaround
            SolidColorBrush buttonBackground_ = btn_update.Background as SolidColorBrush;

            Color color = Color.FromArgb(150, buttonBackground_.Color.R, buttonBackground_.Color.G, buttonBackground_.Color.B);
            Color color_2 = Color.FromArgb(200, buttonBackground_.Color.R, buttonBackground_.Color.G, buttonBackground_.Color.B);
            Color color_3 = Color.FromArgb(255, buttonBackground_.Color.R, buttonBackground_.Color.G, buttonBackground_.Color.B);
            SolidColorBrush Line_fill = new SolidColorBrush(color_2);
            //SolidColorBrush rect_fill = new SolidColorBrush(color);
            defaultcolor = new SolidColorBrush(color);
            lightcolor = new SolidColorBrush(color_3);
            //Plugin.simhub_theme_color=defaultcolor.ToString();            
            // Call this method to generate gridlines on the Canvas
            DrawGridLines();
            DrawGridLines_kinematicCanvas(100,20,1.5);
            CustomEffectTab.Visibility = Visibility.Hidden;









        }



        public byte[] getBytesPayload(payloadPedalConfig aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }


        public byte[] getBytes(DAP_config_st aux)
        {
            int length = Marshal.SizeOf(aux);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            byte[] myBuffer = new byte[length];

            Marshal.StructureToPtr(aux, ptr, true);
            Marshal.Copy(ptr, myBuffer, 0, length);
            Marshal.FreeHGlobal(ptr);

            return myBuffer;
        }


        //public byte[] getBytes_Action(DAP_action_st aux)
        //{
        //    int length = Marshal.SizeOf(aux);
        //    IntPtr ptr = Marshal.AllocHGlobal(length);
        //    byte[] myBuffer = new byte[length];

        //    Marshal.StructureToPtr(aux, ptr, true);
        //    Marshal.Copy(ptr, myBuffer, 0, length);
        //    Marshal.FreeHGlobal(ptr);

        //    return myBuffer;
        //}


        public DAP_config_st getConfigFromBytes(byte[] myBuffer)
        {
            DAP_config_st aux;

            // see https://stackoverflow.com/questions/31045358/how-do-i-copy-bytes-into-a-struct-variable-in-c
            int size = Marshal.SizeOf(typeof(DAP_config_st));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(myBuffer, 0, ptr, size);

            aux = (DAP_config_st)Marshal.PtrToStructure(ptr, typeof(DAP_config_st));
            Marshal.FreeHGlobal(ptr);

            return aux;
        }


        public DAP_state_basic_st getStateFromBytes(byte[] myBuffer)
        {
            DAP_state_basic_st aux;

            // see https://stackoverflow.com/questions/31045358/how-do-i-copy-bytes-into-a-struct-variable-in-c
            int size = Marshal.SizeOf(typeof(DAP_state_basic_st));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(myBuffer, 0, ptr, size);

            aux = (DAP_state_basic_st)Marshal.PtrToStructure(ptr, typeof(DAP_state_basic_st));
            Marshal.FreeHGlobal(ptr);

            return aux;
        }

        public DAP_state_extended_st getStateExtFromBytes(byte[] myBuffer)
        {
            DAP_state_extended_st aux;

            // see https://stackoverflow.com/questions/31045358/how-do-i-copy-bytes-into-a-struct-variable-in-c
            int size = Marshal.SizeOf(typeof(DAP_state_extended_st));
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(myBuffer, 0, ptr, size);

            aux = (DAP_state_extended_st)Marshal.PtrToStructure(ptr, typeof(DAP_state_extended_st));
            Marshal.FreeHGlobal(ptr);

            return aux;
        }


        //unsafe private UInt16 checksumCalc(byte* data, int length)
        //{

        //    UInt16 curr_crc = 0x0000;
        //    byte sum1 = (byte)curr_crc;
        //    byte sum2 = (byte)(curr_crc >> 8);
        //    int index;
        //    for (index = 0; index < length; index = index + 1)
        //    {
        //        int v = (sum1 + (*data));
        //        sum1 = (byte)v;
        //        sum1 = (byte)(v % 255);

        //        int w = (sum1 + sum2) % 255;
        //        sum2 = (byte)w;

        //        data++;// = data++;
        //    }

        //    int x = (sum2 << 8) | sum1;
        //    return (UInt16)x;
        //}


        public SettingsControlDemo(DIY_FFB_Pedal plugin) : this()
        {
            this.Plugin = plugin;
            plugin.testValue = 1;
            plugin.wpfHandle = this;


            UpdateSerialPortList_click();
            //closeSerialAndStopReadCallback(1);

             

            // check if Json config files are present, otherwise create new ones
            //for (int jsonIndex = 0; jsonIndex < ComboBox_JsonFileSelected.Items.Count; jsonIndex++)
            //{

            //    ComboBox_JsonFileSelected.SelectedIndex = jsonIndex;

            //    // which config file is seleced
            //    string currentDirectory = Directory.GetCurrentDirectory();
            //    string dirName = currentDirectory + "\\PluginsData\\Common";
            //    //string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            //    string jsonFileName = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();
            //    string fileName = dirName + "\\" + jsonFileName + ".json";


            //    // Check if file already exists, otherwise create    
            //    if (!File.Exists(fileName))
            //    {
            //        // create default config
            //        // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
            //        // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
            //        var stream1 = new MemoryStream();
            //        var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
            //        ser.WriteObject(stream1, Plugin.dap_config_initial_st);

            //        stream1.Position = 0;
            //        StreamReader sr = new StreamReader(stream1);
            //        string jsonString = sr.ReadToEnd();

            //        System.IO.File.WriteAllText(fileName, jsonString);
            //    }
            //}

            string currentDirectory = Directory.GetCurrentDirectory();
            string dirName = currentDirectory + "\\PluginsData\\Common";
            //string jsonFileName = ComboBox_JsonFileSelected(ComboBox_JsonFileSelected.Items[jsonIndex]).Text;
            string jsonFileNameA = "DiyPedalConfig_Accelerator_Default";
            string jsonFileNameB = "DiyPedalConfig_Brake_Default";
            string jsonFileNameC = "DiyPedalConfig_Clutch_Default";
            string fileNameA = dirName + "\\" + jsonFileNameA + ".json";
            string fileNameB = dirName + "\\" + jsonFileNameB + ".json";
            string fileNameC = dirName + "\\" + jsonFileNameC + ".json";
            /*
            if (!File.Exists(fileNameA))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameA, jsonString);
            }

            if (!File.Exists(fileNameB))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameB, jsonString);
            }
            if (!File.Exists(fileNameC))
            {
                // create default config
                // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                var stream1 = new MemoryStream();
                var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                ser.WriteObject(stream1, Plugin.dap_config_initial_st);

                stream1.Position = 0;
                StreamReader sr = new StreamReader(stream1);
                string jsonString = sr.ReadToEnd();

                System.IO.File.WriteAllText(fileNameC, jsonString);
            }
            */
            //InitReadStructFromJson();
            
            //for (uint pedalIndex = 0; pedalIndex < 3; pedalIndex++)
            //{
                //indexOfSelectedPedal_u = pedalIndex;
                //ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
                //ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u];
                //InitReadStructFromJson();
                /*
                if (plugin.Settings.connect_status[pedalIndex] == 1)
                {
                    if (plugin.Settings.reading_config == 1)
                    {
                        if (plugin._serialPort[pedalIndex].IsOpen)
                        {
                            Reading_config_auto(pedalIndex);
                        }
                        else
                        {
                            plugin.Settings.connect_status[pedalIndex] = 0;
                        }
                        
                    }

                }
                */


                /*
                if (plugin.PortExists(plugin._serialPort[pedalIndex].PortName))
                {
                    if (plugin.Settings.connect_status[pedalIndex] == 1)
                    {
                        if (plugin.Settings.reading_config == 1)
                        {
                            Reading_config_auto(pedalIndex);
                        }

                    }
                    
                }
                else
                {
                    plugin.Settings.connect_status[pedalIndex] = 0;
                }
                */


                //updateTheGuiFromConfig();
            //}
        

            if (plugin.Settings.reading_config == 1)
            {
                checkbox_pedal_read.IsChecked = true;

            }
            else
            {
                checkbox_pedal_read.IsChecked = false;
            }
            indexOfSelectedPedal_u = plugin.Settings.table_selected;
            MyTab.SelectedIndex = (int)indexOfSelectedPedal_u;

            //reconnect to com port
            if (plugin.Settings.auto_connect_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_auto_connect.IsChecked = true;
            }
            else
            {
                checkbox_auto_connect.IsChecked = false;
            }

            //auto connection with timmer
            if (connect_timer != null)
            {
                connect_timer.Dispose();
                connect_timer.Stop();
            }

            connect_timer = new System.Windows.Forms.Timer();
            connect_timer.Tick += new EventHandler(connection_timmer_tick);
            connect_timer.Interval = 5000; // in miliseconds try connect every 5s
            connect_timer.Start();
            System.Threading.Thread.Sleep(50);

            /*
            // autoconnect serial
            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {
                if (Plugin.connectSerialPort[pedalIdx] == true)
                {
                    if (Plugin.PortExists(Plugin._serialPort[pedalIdx].PortName))
                    {
                        if (Plugin._serialPort[pedalIdx].IsOpen == false)
                        {
                            if (Plugin.Settings.connect_status[pedalIdx] == 1)
                            {
                                openSerialAndAddReadCallback(pedalIdx);
                                Reading_config_auto(pedalIdx);
                            }

                        }
                    }
                    else
                    {
                        Plugin.connectSerialPort[pedalIdx] = false;
                        Plugin.Settings.connect_status[pedalIdx] = 0;
                    }

                }
            }
            */


            //vjoy initialized
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                Vjoy_out_check.IsChecked = true;
                uint vJoystickId = Plugin.Settings.vjoy_order;
                //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                joystick = new vJoyInterfaceWrap.vJoy();

                joystick.AcquireVJD(vJoystickId);
                //joystick.Aquire();
                vjoy_axis_initialize();
            }
            else
            {
                Vjoy_out_check.IsChecked = false;
            }


        }




        public void updateTheGuiFromConfig()
        {
            // update the sliders

            update_plot_ABS();
            update_plot_BP();
            update_plot_WS();
            update_plot_RPM();
            info_label.Content = "State:\nDAP Version:\nPlugin Version:";

            string plugin_version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (plugin_version == "1.0.0.0")
            {
                plugin_version = "Dev.";
            }
            string info_text;
            if (Plugin != null)
            {
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
                {
                    info_text = "Connected";
                }
                else
                {
                    if (Plugin.Settings.auto_connect_flag[indexOfSelectedPedal_u] == 1)
                    {
                        info_text = info_text_connection;
                    }
                    else
                    {
                        info_text = "Waiting...";
                    }
                }
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    if (Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u])
                    {
                        info_text = "Wireless Sync";
                    }
                }
                else
                {
                    if (Plugin.Settings.Pedal_ESPNow_auto_connect_flag)
                    {
                        info_text = info_text_connection;
                    }
                    else
                    {
                        info_text = "Waiting...";
                    }

                }
                info_text += "\n" + Constants.pedalConfigPayload_version + "\n" + plugin_version;
                if (Plugin.Rudder_status)
                {
                    info_text += "\nIn Action";
                    info_label.Content += "\nRudder:";
                }
                info_label_2.Content = info_text;
            }





            int debugFlagValue_0 = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0;
            textBox_debug_Flag_0.Text = debugFlagValue_0.ToString();

            //slider setting

            Slider_impact_smoothness.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_window;
            label_impact_window.Content = "Impact Smoothness: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_window;
            Slider_impact_multi.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_multi;
            label_impact_multi.Content = "Impact Multiplier: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_multi + "%";

            Slider_WS_freq.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_freq;
            label_WS_freq.Content = "Notification Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_freq + "Hz";
            Slider_WS_AMP.Value = (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_amp) / 20.0f;
            label_WS_AMP.Content = "Notification Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_amp / 20.0f + "kg";
            Slider_WS_trigger.Value = Plugin.Settings.WS_trigger;
            label_WS_trigger.Content = "Notification Trigger: " + (Plugin.Settings.WS_trigger + 50) + "%";

            Slider_G_force_smoothness.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_window;
            label_G_force_window.Content = "G Force Smoothness: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_window;
            Slider_G_force_multi.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_multi;
            label_G_force_multi.Content = "G Force Multiplier: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_multi + "%";

            Slider_BP_freq.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq;
            label_BP_freq.Content = "Bite Point Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq + "Hz";
            Slider_BP_AMP.Value = (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp) / 100.0f;
            label_BP_AMP.Content = "Bite Point Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp / 100.0f + "kg";

            Rangeslider_RPM_freq.LowerValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq;
            Rangeslider_RPM_freq.UpperValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq;
            label_RPM_freq_max.Content = "MAX:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq + "Hz";
            label_RPM_freq_min.Content = "MIN:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq + "Hz";


            Slider_RPM_AMP.Value = (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) / 100.0f;
            label_RPM_AMP.Content = "Effect Amplitude: " + (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) / 100.0f + "kg";

            Slider_ABS_freq.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency;
            label_ABS_freq.Content = "ABS/TC Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency + "Hz";
            Slider_ABS_AMP.Value = ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude) / 20.0f;
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition < 5)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition = 5;
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition > 95)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition=95;
            }
            Rangeslider_travel_range.LowerValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
            Rangeslider_travel_range.UpperValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition;
            
            if (Plugin != null)
            {
                Label_min_pos.Content = "MIN\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition + "%\n" + Math.Round((float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition) / 100) + "mm";
                Label_max_pos.Content = "MAX\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition + "%\n" + Math.Round((float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition) / 100) + "mm";
            }





            Rangeslider_force_range.UpperValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce;
            Rangeslider_force_range.LowerValue = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce;
            if (indexOfSelectedPedal_u != 1)
            {
                Rangeslider_force_range.Maximum = 50;
            }
            else
            {
                Rangeslider_force_range.Maximum = 200;
            }
            if (Plugin != null)
            {
                Label_max_force.Content = "Max force:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce + "kg";
                Label_min_force.Content = "Preload:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce + "kg";
            }

            label_damping.Content = "Damping factor: " + (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress * 0.00015f)+"s";
            Slider_damping.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress;
            Slider_LC_rate.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating * 2;
            label_LC_rate.Content = "Loadcell rate: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating*2+"kg";

            Slider_maxgame_output.Value= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput;
            label_maxgame_output.Content = "Max Game Output: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput + "%";

            Slider_KF.Value= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise;
            label_KF.Content = "KF: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise;

            Slider_MPC_0th_gain.Value= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.MPC_0th_order_gain;
            label_MPC_0th_gain.Content = "Foot spring stiffness: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.MPC_0th_order_gain, 2) + "kg/mm";

            Slider_Pgain.Value= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain;
            label_Pgain.Content = "P-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain, 2);

            Slider_Igain.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain;
            label_Igain.Content = "I-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain, 2);

            Slider_Dgain.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain;
            label_Dgain.Content = "D-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain, 4);

            Slider_VFgain.Value= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_velocity_feedforward_gain;
            label_VFgain.Content = "Feed Forward Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_velocity_feedforward_gain, 1);

            
            if (Plugin != null)
            {
                if (Plugin.Settings.CV1_enable_flag[indexOfSelectedPedal_u] == true)
                {
                    checkbox_enable_CV1.IsChecked = true;
                }
                else
                {
                    checkbox_enable_CV1.IsChecked = false;
                }

                if (Plugin.Settings.CV2_enable_flag[indexOfSelectedPedal_u] == true)
                {
                    checkbox_enable_CV2.IsChecked = true;
                }
                else
                {
                    checkbox_enable_CV2.IsChecked = false;
                }
                Slider_CV1_trigger.Value = Plugin.Settings.CV1_trigger[indexOfSelectedPedal_u];
                Slider_CV1_AMP.Value = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_1/20.0f;
                Slider_CV1_freq.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_1;
                Slider_CV2_trigger.Value = Plugin.Settings.CV2_trigger[indexOfSelectedPedal_u];
                Slider_CV2_AMP.Value = (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_2/20.0f;
                Slider_CV2_freq.Value = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_2;
                label_CV1_trigger.Content = "Effect Trigger:" + Plugin.Settings.CV1_trigger[indexOfSelectedPedal_u];
                label_CV1_AMP.Content = "Effect Amplitude:" + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_1/20.0f + "kg";
                label_CV1_freq.Content = "Effect Frequency:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_1 + "Hz";
                label_CV2_trigger.Content = "Effect Trigger:" + Plugin.Settings.CV2_trigger[indexOfSelectedPedal_u];
                label_CV2_AMP.Content = "Effect Amplitude:" + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_2 / 20.0f + "kg";
                label_CV2_freq.Content = "Effect Frequency:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_2 + "Hz";
                textBox_CV1_string.Text = Plugin.Settings.CV1_bindings[indexOfSelectedPedal_u];
                textBox_CV2_string.Text = Plugin.Settings.CV2_bindings[indexOfSelectedPedal_u];


                //rudder text
                if (Plugin.Rudder_status)
                {
                    btn_rudder.Content = "Rudder Off";
                }
                else
                {
                    btn_rudder.Content = "Rudder On";
                }
                if (Plugin.Sync_esp_connection_flag)
                {
                    btn_connect_espnow_port.Content = "Disconnect";
                }
                else
                {
                    btn_connect_espnow_port.Content = "Connect";
                }

                if (Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u])
                {
                    CheckBox_Pedal_ESPNow_SyncFlag.IsChecked = true;
                }
                else
                {
                    CheckBox_Pedal_ESPNow_SyncFlag.IsChecked = false;
                }

                if (Plugin.Settings.Pedal_ESPNow_auto_connect_flag)
                {
                    CheckBox_Pedal_ESPNow_autoconnect.IsChecked = true;
                }
                else
                { 
                    CheckBox_Pedal_ESPNow_autoconnect.IsChecked= false;
                }

                if (Plugin.Settings.Serial_auto_clean)
                {
                    Checkbox_auto_remove_serial_line.IsChecked = true;
                }
                else
                { 
                    Checkbox_auto_remove_serial_line.IsChecked= false;
                }
                
            }

            
           



            switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
            {
                case 0:
                    label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "kg";
                    break;
                case 1:
                    label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "%";
                    break;
                default:
                    break;
            }


            
            Update_BrakeForceCurve();
            //Simulated ABS trigger
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger == 1)
            {
                Simulate_ABS_check.IsChecked = true;
            }
            else
            {
                Simulate_ABS_check.IsChecked = false;
            }


            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b == 0)
            {
                ControlStrategy_Sel_1.IsChecked = true;
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b == 1)
            {
                ControlStrategy_Sel_2.IsChecked = true;
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b == 2)
            {
                ControlStrategy_Sel_3.IsChecked = true;
            }



            //set control point position
            text_point_pos.Visibility = Visibility.Hidden;
            double control_rect_value_max = 100;
            double dyy = canvas.Height / control_rect_value_max;
            Canvas.SetTop(rect0, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 - rect0.Height / 2);
            Canvas.SetLeft(rect0, 0 * canvas.Width / 5 - rect0.Width / 2);
            Canvas.SetTop(rect1, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 - rect0.Height / 2);
            Canvas.SetLeft(rect1, 1 * canvas.Width / 5 - rect1.Width / 2);
            Canvas.SetTop(rect2, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 - rect0.Height / 2);
            Canvas.SetLeft(rect2, 2 * canvas.Width / 5 - rect2.Width / 2);
            Canvas.SetTop(rect3, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 - rect0.Height / 2);
            Canvas.SetLeft(rect3, 3 * canvas.Width / 5 - rect3.Width / 2);
            Canvas.SetTop(rect4, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 - rect0.Height / 2);
            Canvas.SetLeft(rect4, 4 * canvas.Width / 5 - rect4.Width / 2);
            Canvas.SetTop(rect5, canvas.Height - dyy * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 - rect0.Height / 2);
            Canvas.SetLeft(rect5, 5 * canvas.Width / 5 - rect5.Width / 2);
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 != 32)
            {
                rect_State.Visibility = Visibility.Visible;
                text_state.Visibility = Visibility.Visible;
            }
            else
            {
                rect_State.Visibility = Visibility.Hidden;
                text_state.Visibility = Visibility.Hidden;
            }
            Canvas.SetTop(rect_State, canvas.Height - rect_State.Height / 2);
            Canvas.SetLeft(rect_State, -rect_State.Width / 2);
            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) /*+ rect_State.Width*/);
            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
            text_state.Text = "0%";
            //set for ABS slider
            Canvas.SetTop(rect_SABS_Control, (control_rect_value_max - dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value) * dyy - rect_SABS_Control.Height / 2);
            Canvas.SetLeft(rect_SABS_Control, 0);
            Canvas.SetTop(rect_SABS, 0);
            Canvas.SetLeft(rect_SABS, 0);
            rect_SABS.Height = canvas.Height - dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value * dyy;
            Canvas.SetTop(text_SABS, Canvas.GetTop(rect_SABS_Control) - text_SABS.Height - rect_SABS_Control.Height);
            Canvas.SetLeft(text_SABS, canvas.Width - text_SABS.Width);
            text_SABS.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value + "%";
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger == 1)
            {
                rect_SABS.Visibility = Visibility.Visible;
                rect_SABS_Control.Visibility = Visibility.Visible;
                text_SABS.Visibility = Visibility.Visible;
            }
            else
            {
                rect_SABS.Visibility = Visibility.Hidden;
                rect_SABS_Control.Visibility = Visibility.Hidden;
                text_SABS.Visibility = Visibility.Hidden;
            }
            //set for travel slider;
            double dx = 0;
           




            

            // ABS pattern
            try
            {
                AbsPattern.SelectedIndex = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern;
            }
            catch (Exception caughtEx)
            {
            }

            // ABS force or travel dependent
            try
            {
                EffectAppliedOnForceOrTravel_combobox.SelectedIndex = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit;
            }
            catch (Exception caughtEx)
            {
            }

            // spindle pitch
            try
            {
                SpindlePitch.SelectedIndex = (byte)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.spindlePitch_mmPerRev_u8;
            }
            catch (Exception caughtEx)
            {
            }



            // Filter type
            try
            {
                KF_filter_order.SelectedIndex = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelOrder;
            }
            catch (Exception caughtEx)
            {
            }



            //RPM AMP slider



            //Bite point control
            double BP_max = 100;
            dx = (double)canvas.Width / BP_max;
            text_BP.Text = "Bite Point:\n" + ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value) + "%";
            Canvas.SetLeft(rect_BP_Control, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value * dx - rect_BP_Control.Width / 2);
            Canvas.SetLeft(text_BP, Canvas.GetLeft(rect_BP_Control) + rect_BP_Control.Width + 3);
            Canvas.SetTop(text_BP, canvas.Height - text_BP.Height-15);



















            //// Select serial port accordingly
            string tmp = (string)Plugin._serialPort[indexOfSelectedPedal_u].PortName;
            try
            {
                SerialPortSelection.SelectedValue = tmp;
                TextBox_debugOutput.Text = "Serial port selected: " + SerialPortSelection.SelectedValue;

            }
            catch (Exception caughtEx)
            {
            }


            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == true)
            {
                ConnectToPedal.IsChecked = true;
                btn_pedal_connect.Content = "Disconnect From Pedal";
            }
            else
            {
                ConnectToPedal.IsChecked = false;
                btn_pedal_connect.Content = "Connect To Pedal";
            }

            if (Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_RPM.IsChecked = true;
                checkbox_enable_RPM.Content = "Effect Enabled";
            }
            else
            {
                checkbox_enable_RPM.IsChecked = false;
                checkbox_enable_RPM.Content = "Effect Disabled";
            }

            if (Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_ABS.IsChecked = true;
                checkbox_enable_ABS.Content = "ABS/TC Effect Enabled";
            }
            else
            {
                checkbox_enable_ABS.IsChecked = false;
                checkbox_enable_ABS.Content = "ABS/TC Effect Disabled";
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger == 1)
            {
                checkbox_enable_bite_point.IsChecked = true;
                text_BP.Visibility = Visibility.Visible;
                rect_BP_Control.Visibility = Visibility.Visible;
                checkbox_enable_bite_point.Content = "Bite Point Vibration Enabled";

            }
            else
            {
                checkbox_enable_bite_point.IsChecked = false;
                text_BP.Visibility = Visibility.Hidden;
                rect_BP_Control.Visibility = Visibility.Hidden;
                checkbox_enable_bite_point.Content = "Bite Point Vibration Disabled";
            }

            if (indexOfSelectedPedal_u == 1)
            {
                checkbox_enable_G_force.IsEnabled = true;
                if (Plugin.Settings.G_force_enable_flag[indexOfSelectedPedal_u] == 1)
                {
                    checkbox_enable_G_force.IsChecked = true;
                    checkbox_enable_G_force.Content = "G Force Effect Enabled";
                }
                else
                {
                    checkbox_enable_G_force.IsChecked = false;
                    checkbox_enable_G_force.Content = "G Force Effect Disabled";
                }
            }
            else
            {
                checkbox_enable_G_force.IsEnabled = false;
                checkbox_enable_G_force.IsChecked = false;
                checkbox_enable_G_force.Content = "G Force Effect Disabled";
            }

            if (Plugin.Settings.connect_status[indexOfSelectedPedal_u] == 1)
            {
                Serial_port_text.Text = "Serial Port connected";
                Serial_port_text.Visibility = Visibility.Visible;

            }
            else
            {
                Serial_port_text.Visibility = Visibility.Hidden;
            }

            if (Plugin.Settings.RPM_effect_type == 0)
            {
                RPMeffecttype_Sel_1.IsChecked = true;
            }
            else
            {
                RPMeffecttype_Sel_2.IsChecked = true;
            }

            if (Plugin.Settings.file_enable_check[profile_select, 0] == 1)
            {
                Label_clutch_file.Content = Plugin.Settings.Pedal_file_string[profile_select,0];
                Clutch_file_check.IsChecked = true;
            }
            else 
            {
                Label_clutch_file.Content = "";
                Clutch_file_check.IsChecked = false;
            }
            if (Plugin.Settings.file_enable_check[profile_select, 1] == 1)
            {
                Label_brake_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 1];
                Brake_file_check.IsChecked = true;
            }
            else
            {
                Label_brake_file.Content = "";
                Brake_file_check.IsChecked = false;
            }

            if (Plugin.Settings.file_enable_check[profile_select,2] == 1)
            {
                Label_gas_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 2];
                Gas_file_check.IsChecked = true;
            }
            else
            {
                Label_gas_file.Content = "";
                Gas_file_check.IsChecked = false;
            }
            
            if (Plugin.Settings.WS_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_wheelslip.IsChecked = true;
            }
            else
            {
                checkbox_enable_wheelslip.IsChecked = false;
            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.enableReboot_u8 == 1)
            {
                EnableReboot_check.IsChecked = true;
            }
            else
            {
                EnableReboot_check.IsChecked = false;
            }



            if (Plugin.Settings.Road_impact_enable_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_enable_impact.IsChecked = true;
            }
            else
            {
                checkbox_enable_impact.IsChecked = false;
            }

            if (Plugin.Settings.RTSDTR_False[indexOfSelectedPedal_u] == true)
            {
                CheckBox_RTSDTR.IsChecked = true;
            }
            else
            { 
                CheckBox_RTSDTR.IsChecked = false;
            }

            if (Plugin.Settings.auto_connect_flag[indexOfSelectedPedal_u] == 1)
            {
                checkbox_auto_connect.IsChecked = true;
            }
            else
            {
                checkbox_auto_connect.IsChecked= false;
            }

            textBox_wheelslip_effect_string.Text = Plugin.Settings.WSeffect_bind;
            textBox_impact_effect_string.Text = Plugin.Settings.Road_impact_bind;

            //TextBox2.Text = "" + Plugin.Settings.selectedComPortNames[0] + Plugin.Settings.selectedComPortNames[1] + Plugin.Settings.selectedComPortNames[2];
            JoystickOutput_check.IsChecked = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 == 1;
            InvertLoadcellReading_check.IsChecked = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 == 1;
            InvertMotorDir_check.IsChecked = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertMotorDirection_u8 == 1;

            
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            textbox_profile_name.Text = Plugin.Settings.Profile_name[profile_select];


            //pedal joint draw
            Pedal_joint_draw();
            /*
            Label_OA.Content= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            Label_OB.Content= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            Label_BC.Content = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            Label_CA.Content = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            Label_AD.Content= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d;
            Label_travel.Content = Plugin.Settings.Pedal_travel[indexOfSelectedPedal_u];
            */


            //try
            //{
            //    //ComboBox_JsonFileSelected.SelectedItem = Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
            //    //ComboBox_JsonFileSelected.SelectedValue = (string)Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];

            //    ComboBox_JsonFileSelected.SelectedIndex = Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u];

            //    //ReadStructFromJson();


            //    //SerialPortSelection.SelectedValue
            //    //TextBox_debugOutput.Text = "Error 2: ";
            //    //TextBox_debugOutput.Text += Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u];
            //    //TextBox_debugOutput.Text += "     ";
            //    //TextBox_debugOutput.Text += ComboBox_JsonFileSelected.SelectedValue;
            //}
            //catch (Exception caughtEx)
            //{
            //    string errorMessage = caughtEx.Message;
            //    TextBox_debugOutput.Text = "Error 1: ";
            //    TextBox_debugOutput.Text += errorMessage;
            //}

            //= ComboBox_JsonFileSelected.SelectedItem.ToString();

            //ConnectToPedal.IsChecked = true;

            //TextBox_debugOutput.Text = "Pedal selected: " + indexOfSelectedPedal_u;
            //TextBox_debugOutput.Text += ",    connected: " + ConnectToPedal.IsChecked;
            //TextBox_debugOutput.Text += ",    serial port name: " + tmp;

        }




        private void Update_BrakeForceCurve()
        {

            double[] x = new double[6];
            double[] y = new double[6];
            double x_quantity = 100;
            double y_max = 100;
            double dx = canvas.Width / x_quantity;
            double dy = canvas.Height / y_max;

            x[0] = 0;
            x[1] = 20;
            x[2] = 40;
            x[3] = 60;
            x[4] = 80;
            x[5] = 100;

            y[0] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000;
            y[1] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020;
            y[2] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040;
            y[3] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060;
            y[4] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080;
            y[5] = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100;

            // Use cubic interpolation to smooth the original data
            (double[] xs2, double[] ys2, double[] a, double[] b) = Cubic.Interpolate1D(x, y, 100);


            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_0 = (float)a[0];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_1 = (float)a[1];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_2 = (float)a[2];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_3 = (float)a[3];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_a_4 = (float)a[4];

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_0 = (float)b[0];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_1 = (float)b[1];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_2 = (float)b[2];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_3 = (float)b[3];
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.cubic_spline_param_b_4 = (float)b[4];


            //TextBox_debugOutput.Text = "";
            //for (uint i = 0; i < a.Length; i++)
            //{
            //    TextBox_debugOutput.Text += "\na[" + i + "]: " + a[i] + "      b[" + i + "]: " + b[i];
            //}


            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();


            for (int pointIdx = 0; pointIdx < 100; pointIdx++)
            {
                System.Windows.Point Pointlcl = new System.Windows.Point(dx * xs2[pointIdx], dy * ys2[pointIdx]);
                myPointCollection2.Add(Pointlcl);
                Force_curve_Y[pointIdx] = dy * ys2[pointIdx];
            }

            this.Polyline_BrakeForceCurve.Points = myPointCollection2;


        }

        private void update_plot_ABS()
        {
            int x_quantity = 200;
            double[] x = new double[x_quantity];
            double[] y = new double[x_quantity];
            
            double y_max =50;
            double dx = canvas_plot_ABS.Width / x_quantity;
            double dy = canvas_plot_ABS.Height / y_max;
            double freq = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency;
            double max_force = 255 / 20;
            double amp = ((double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude) /20;
            double peroid = x_quantity / freq;
            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern == 0)
            {
                for (int idx = 0; idx < x_quantity; idx++)
                {
                    x[idx] = idx;
                    y[idx] = -1 * amp / max_force * Math.Sin(2 * x[idx] / peroid * Math.PI) * y_max / 2;
                    System.Windows.Point Pointlcl = new System.Windows.Point(dx * x[idx], dy * y[idx] + 25);
                    myPointCollection2.Add(Pointlcl);
                }

            }
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern == 1)
            {
                for (int idx = 0; idx < x_quantity; idx++)
                {
                    x[idx] = idx;     
                    y[idx] = -1 * amp / max_force  * y_max * (x[idx]%peroid)/peroid;
                    System.Windows.Point Pointlcl = new System.Windows.Point(dx * x[idx], dy * y[idx] + 50);
                    myPointCollection2.Add(Pointlcl);
                }
            }

            this.Polyline_plot_ABS.Points = myPointCollection2;
        }
        private void update_plot_BP()
        {
            int x_quantity = 200;
            double[] x = new double[x_quantity];
            double[] y = new double[x_quantity];

            double y_max = 50;
            double dx = canvas_plot_BP.Width / x_quantity;
            double dy = canvas_plot_BP.Height / y_max;
            double freq = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq;
            double max_force = 200 / 20;
            double amp = ((double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp) / 20;
            double peroid = x_quantity / freq;
            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();
                for (int idx = 0; idx < x_quantity; idx++)
                {
                    x[idx] = idx;
                    y[idx] = -1 * amp / max_force * Math.Sin(2 * x[idx] / peroid * Math.PI) * y_max / 2;
                    System.Windows.Point Pointlcl = new System.Windows.Point(dx * x[idx], dy * y[idx] + 25);
                    myPointCollection2.Add(Pointlcl);
                }
            this.Polyline_plot_BP.Points = myPointCollection2;
        }
        private void update_plot_WS()
        {
            int x_quantity = 200;
            double[] x = new double[x_quantity];
            double[] y = new double[x_quantity];

            double y_max = 50;
            double dx = canvas_plot_WS.Width / x_quantity;
            double dy = canvas_plot_WS.Height / y_max;
            double freq = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_freq;
            double max_force = 250 / 20;
            double amp = ((double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_amp) / 20;
            double peroid = x_quantity / freq;
            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();
            for (int idx = 0; idx < x_quantity; idx++)
            {
                x[idx] = idx;
                y[idx] = -1 * amp / max_force * Math.Sin(2 * x[idx] / peroid * Math.PI) * y_max / 2;
                System.Windows.Point Pointlcl = new System.Windows.Point(dx * x[idx], dy * y[idx] + 25);
                myPointCollection2.Add(Pointlcl);
            }
            this.Polyline_plot_WS.Points = myPointCollection2;
        }
        private void update_plot_RPM()
        {
            int x_quantity = 1601;
            double[] x = new double[x_quantity];
            double[] y = new double[x_quantity];
            double[] peroid_x = new double[x_quantity];
            double[] freq= new double[x_quantity];
            double[] amp=new double[x_quantity];
            double y_max = 50;
            double dx = canvas_plot_RPM.Width / (x_quantity-1);
            double dy = canvas_plot_RPM.Height / y_max;
            double freq_max = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq;
            double freq_min= dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq;
            double max_force = 200 / 20*1.3;
            double amp_base = ((double)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) / 20;
            //double peroid = x_quantity / freq;
            System.Windows.Media.PointCollection myPointCollection2 = new System.Windows.Media.PointCollection();
            for (int idx = 0; idx < x_quantity; idx++)
            {
                x[idx] = idx;
                freq[idx] = freq_min+(((double)idx)/(double)x_quantity)*(freq_max-freq_min);
                peroid_x[idx] = x_quantity / freq[idx];
                amp[idx] = amp_base + amp_base * idx / x_quantity * 0.3;
                y[idx] = -1 * amp[idx] / max_force * Math.Sin(2* x[idx] / peroid_x[idx] * Math.PI) * y_max / 2;
                System.Windows.Point Pointlcl = new System.Windows.Point(dx * x[idx], dy * y[idx] + 25);
                myPointCollection2.Add(Pointlcl);
            }
            this.Polyline_plot_RPM.Points = myPointCollection2;
        }


        public class SerialPortChoice
        {
            public SerialPortChoice(string display, string value)
            {
                Display = display;
                Value = value;
            }

            public string Value { get; set; }
            public string Display { get; set; }
        }



        // Select which pedal to config
        // see https://stackoverflow.com/questions/772841/is-there-selected-tab-changed-event-in-the-standard-wpf-tab-control
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            indexOfSelectedPedal_u = (uint)MyTab.SelectedIndex;
            Plugin.Settings.table_selected = (uint)MyTab.SelectedIndex;
            // update the sliders & serial port selection accordingly
            updateTheGuiFromConfig();
        }





        /********************************************************************************************************************/
        /*							Slider callbacks																		*/
        /********************************************************************************************************************/

        public void TestAbs_click(object sender, RoutedEventArgs e)
        {
            //if (indexOfSelectedPedal_u == 1)
            if (TestAbs_check.IsChecked == false)
            {
                TestAbs_check.IsChecked = true;
                Plugin.sendAbsSignal = (bool)TestAbs_check.IsChecked;
                TextBox_debugOutput.Text = "ABS-Test begin";
                updateTheGuiFromConfig();
            }
            else
            {
                TestAbs_check.IsChecked = false;
                //Plugin.sendAbsSignal = !Plugin.sendAbsSignal;
                Plugin.sendAbsSignal = (bool)TestAbs_check.IsChecked;
                TextBox_debugOutput.Text = "ABS-Test stopped";
                updateTheGuiFromConfig();
            }

        }









        /********************************************************************************************************************/
        /*							PID tuning                      														*/
        /********************************************************************************************************************/
        public void PID_tuning_P_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain = (float)e.NewValue;
        }

        public void PID_tuning_I_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain = (float)e.NewValue;
        }

        public void PID_tuning_D_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain = (float)e.NewValue;
        }

        public void PID_tuning_Feedforward_gain_changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_velocity_feedforward_gain = (float)e.NewValue;
        }







        private void NumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //labelEingabe.Content = "Sie haben '" + textBox_debug_Flag_0.Text + "' eingegeben!";
            //TextBox_debugOutput.Text = textBox_debug_Flag_0.Text;

            if (int.TryParse(textBox_debug_Flag_0.Text, out int result))
            {
                if ((result >= 0) && (result <= 255))
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)result;
                }
            }
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            //if ((e.NewValue >= 0) && (e.NewValue <= 255))
            //{
            //    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)e.NewValue;
            //}

            // Use a regular expression to allow only numeric input
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,4}[0-9]*$");

            System.Windows.Controls.TextBox textBox = (System.Windows.Controls.TextBox)sender;

            e.Handled = !regex.IsMatch(textBox.Text + e.Text);

            ////if (!e.Handled)
            ////{
            ////    if (int.TryParse(textBox.Text + e.Text, out int result))
            ////    {
            ////        if ((result >= 0) && (result <= 255))
            ////        {
            ////            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.debug_flags_0 = (byte)result;
            ////        }
            ////    }
            ////}
        }


        /********************************************************************************************************************/
        /*							Write/read config to/from Json file														*/
        /********************************************************************************************************************/

        //private void ComboBox_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        // https://stackoverflow.com/questions/3721430/what-is-the-simplest-way-to-get-the-selected-text-of-a-combo-box-containing-only

        //        string stringValue = ((ComboBoxItem)ComboBox_JsonFileSelected.SelectedItem).Content.ToString();


        //        // string stringValue = ComboBox_JsonFileSelected.SelectedValue.ToString();

        //        //TextBox_debugOutput.Text = stringValue;
        //        Plugin.Settings.selectedJsonFileNames[indexOfSelectedPedal_u] = stringValue;

        //        Plugin.Settings.selectedJsonIndexLast[indexOfSelectedPedal_u] = ComboBox_JsonFileSelected.SelectedIndex;



        //        //ReadStructFromJson();
        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }
        //}




        //public void SaveStructToJson_click(object sender, RoutedEventArgs e)
        //{
        //    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0

        //    try
        //    {
        //        // which config file is seleced
        //        string currentDirectory = Directory.GetCurrentDirectory();
        //        string dirName = currentDirectory + "\\PluginsData\\Common";
        //        string jsonFileName = ComboBox_JsonFileSelected.Text;
        //        string fileName = dirName + "\\" + jsonFileName + ".json";

        //        this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)pedalConfigPayload_version;

        //        // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
        //        // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
        //        var stream1 = new MemoryStream();
        //        var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
        //        ser.WriteObject(stream1, dap_config_st[indexOfSelectedPedal_u]);

        //        stream1.Position = 0;
        //        StreamReader sr = new StreamReader(stream1);
        //        string jsonString = sr.ReadToEnd();

        //        // Check if file already exists. If yes, delete it.     
        //        if (File.Exists(fileName))
        //        {
        //            File.Delete(fileName);
        //        }


        //        System.IO.File.WriteAllText(fileName, jsonString);
        //        TextBox_debugOutput.Text = "Config exported!";

        //    }
        //    catch (Exception caughtEx)
        //    {

        //        string errorMessage = caughtEx.Message;
        //        TextBox_debugOutput.Text = errorMessage;
        //    }

        //}



        //public void ReadStructFromJson_click(object sender, RoutedEventArgs e)
        //{
        //    ReadStructFromJson();
        //}


        /********************************************************************************************************************/
        /*							Refind min endstop																		*/
        /********************************************************************************************************************/
        unsafe public void ResetPedalPosition_click(object sender, RoutedEventArgs e)
        {

            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {

                try
                {
                    // compute checksum
                    DAP_action_st tmp;
                    tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                    tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                    tmp.payloadHeader_.PedalTag = (byte)indexOfSelectedPedal_u;
                    tmp.payloadPedalAction_.system_action_u8 = 1;


                    DAP_action_st* v = &tmp;
                    byte* p = (byte*)v;
                    tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));


                    int length = sizeof(DAP_action_st);
                    byte[] newBuffer = new byte[length];
                    newBuffer = Plugin.getBytes_Action(tmp);


                    // clear inbuffer 
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardInBuffer();

                    // send query command
                    Plugin._serialPort[indexOfSelectedPedal_u].Write(newBuffer, 0, newBuffer.Length);
                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    TextBox_debugOutput.Text = errorMessage;
                }
            }
        }



        /********************************************************************************************************************/
        /*							System identification																	*/
        /********************************************************************************************************************/
        public void StartSystemIdentification_click(object sender, RoutedEventArgs e)
        {

            TextBox_debugOutput.Text = "Start system identification";


            try
            {

                string currentDirectory = Directory.GetCurrentDirectory();
                string dirName = currentDirectory + "\\PluginsData\\Common";
                string logFileName = "DiyActivePedalSystemIdentification";
                string fileName = dirName + "\\" + logFileName + ".txt";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }


                // This text is added only once to the file.
                if (!File.Exists(fileName))
                {
                    using (StreamWriter sw = File.CreateText(fileName))
                    {

                        // trigger system identification
                        Plugin._serialPort[indexOfSelectedPedal_u].Write("3");

                        //System.Threading.Thread.Sleep(100);


                        // read system return log
                        //while (Plugin._serialPort[indexOfSelectedPedal_u].BytesToRead > 0)
                        //{
                        //    string message = Plugin._serialPort[indexOfSelectedPedal_u].ReadLine();
                        //    sw.Write(message);

                        //    System.Threading.Thread.Sleep(20);

                        //}
                    }

                }

                TextBox_debugOutput.Text = "Finished system identification";


                ////// trigger system identification
                ////Plugin._serialPort[indexOfSelectedPedal_u].Write("3");

                ////System.Threading.Thread.Sleep(100);


                ////// read system return log
                ////while (Plugin._serialPort[indexOfSelectedPedal_u].BytesToRead > 0)
                ////{
                ////    string message = Plugin._serialPort[indexOfSelectedPedal_u].ReadLine();
                ////    using (StreamWriter sw = File.AppendText(fileName))
                ////    {
                ////        sw.WriteLine(message);
                ////    }
                ////    System.Threading.Thread.Sleep(100);
                ////}


            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }

        }






        /********************************************************************************************************************/
        /*							Send config to pedal																	*/
        /********************************************************************************************************************/
        unsafe public void Sendconfig(uint pedalIdx)
        {
            // compute checksum
            //getBytes(this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_)
            this.dap_config_st[pedalIdx].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            this.dap_config_st[pedalIdx].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
            this.dap_config_st[pedalIdx].payloadHeader_.PedalTag = (byte)pedalIdx;
            this.dap_config_st[pedalIdx].payloadHeader_.storeToEeprom = 1;
            DAP_config_st tmp = this.dap_config_st[pedalIdx];

            //payloadPedalConfig tmp = this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_;
            DAP_config_st* v = &tmp;
            byte* p = (byte*)v;
            this.dap_config_st[pedalIdx].payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalConfig));
            int length = sizeof(DAP_config_st);
            //int val = this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.checkSum;
            //string msg = "CRC value: " + val.ToString();
            byte[] newBuffer = new byte[length];
            newBuffer = getBytes(this.dap_config_st[pedalIdx]);

            //TextBox_debugOutput.Text = "CRC simhub calc: " + this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum + "    ";

            TextBox_debugOutput.Text = String.Empty;
            if (Plugin.Settings.Pedal_ESPNow_Sync_flag[pedalIdx])
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    try
                    {
                        TextBox2.Text = "Buffer sent size:" + length;
                        Plugin.ESPsync_serialPort.DiscardInBuffer();
                        Plugin.ESPsync_serialPort.DiscardOutBuffer();
                        // send data
                        Plugin.ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                        //Plugin._serialPort[indexOfSelectedPedal_u].Write("\n");
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }
                }
            }
            else
            {
                //int length2 = sizeof(DAP_config_st);
                if (Plugin._serialPort[pedalIdx].IsOpen)
                {

                    try
                    {
                        //TextBox_debugOutput.Text = "ConfigLength" + length;
                        // clear inbuffer 
                        Plugin._serialPort[pedalIdx].DiscardInBuffer();
                        Plugin._serialPort[pedalIdx].DiscardOutBuffer();
                        // send data
                        Plugin._serialPort[pedalIdx].Write(newBuffer, 0, newBuffer.Length);
                        //Plugin._serialPort[indexOfSelectedPedal_u].Write("\n");
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }

                }
            }
        }
        unsafe public void Sendconfigtopedal_shortcut()
        {

            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {
                if (Plugin.Settings.file_enable_check[profile_select, pedalIdx] == 1)
                {
                    Sendconfig(pedalIdx);
                    TextBox_debugOutput.Text = "config was sent to pedal";
                }
            }

        }
        unsafe public void SendConfigToPedal_click(object sender, RoutedEventArgs e)
        {

            Sendconfig(indexOfSelectedPedal_u);
            /*
            if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {

                // compute checksum
                //getBytes(this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_)
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
                this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.storeToEeprom = 1;
                DAP_config_st tmp = this.dap_config_st[indexOfSelectedPedal_u];

                //payloadPedalConfig tmp = this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_;
                DAP_config_st* v = &tmp;
                byte* p = (byte*)v;
                this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalConfig));


                //TextBox_debugOutput.Text = "CRC simhub calc: " + this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum + "    ";

                TextBox_debugOutput.Text = String.Empty;

                try
                {
                    int length = sizeof(DAP_config_st);
                    //int val = this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.checkSum;
                    //string msg = "CRC value: " + val.ToString();
                    byte[] newBuffer = new byte[length];
                    newBuffer = getBytes(this.dap_config_st[indexOfSelectedPedal_u]);

                    //TextBox_debugOutput.Text = "ConfigLength" + length;

                    // clear inbuffer 
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardInBuffer();
                    Plugin._serialPort[indexOfSelectedPedal_u].DiscardOutBuffer();


                    // send data
                    Plugin._serialPort[indexOfSelectedPedal_u].Write(newBuffer, 0, newBuffer.Length);
                    //Plugin._serialPort[indexOfSelectedPedal_u].Write("\n");
                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    TextBox_debugOutput.Text = errorMessage;
                }

            }
            */
        }


        unsafe public void Reading_config_auto(uint i)
        {
            // compute checksum
            DAP_action_st tmp;
            tmp.payloadPedalAction_.returnPedalConfig_u8 = 1;
            tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
            tmp.payloadHeader_.PedalTag = (byte)i;
            DAP_action_st* v = &tmp;
            byte* p = (byte*)v;
            tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
            int length = sizeof(DAP_action_st);
            byte[] newBuffer = new byte[length];
            newBuffer = Plugin.getBytes_Action(tmp);
            // tell the plugin that we expect config data
            waiting_for_pedal_config[i] = true;
            if (Plugin.Settings.Pedal_ESPNow_Sync_flag[i])
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    // try N times and check whether config has been received
                    for (int rep = 0; rep < 1; rep++)
                    {
                        // send query command
                        Plugin.ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);

                        // wait some time and check whether data has been received
                        System.Threading.Thread.Sleep(50);

                        if (waiting_for_pedal_config[i] == false)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (Plugin._serialPort[i].IsOpen)
                {
                    // try N times and check whether config has been received
                    for (int rep = 0; rep < 1; rep++)
                    {
                        // send query command
                        Plugin._serialPort[i].Write(newBuffer, 0, newBuffer.Length);

                        // wait some time and check whether data has been received
                        System.Threading.Thread.Sleep(50);

                        if (waiting_for_pedal_config[i] == false)
                        {
                            break;
                        }
                    }
                }
            }

        }

        public string[] STOPCHAR = { "\r\n" };
        public bool EndsWithStop(string incomingData)
        {
            for (int i = 0; i < STOPCHAR.Length; i++)
            {
                if (incomingData.EndsWith(STOPCHAR[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /********************************************************************************************************************/
        /*							Read config from pedal																	*/
        /********************************************************************************************************************/
        unsafe public void ReadConfigFromPedal_click(object sender, RoutedEventArgs e)
        {
            Reading_config_auto(indexOfSelectedPedal_u);
        }


        public string[] _data = { "", "", "" };// = "";

        //unsafe private void sp_DataReceived(object sender, object e)
        unsafe private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            SerialPort sp = (SerialPort)sender;
            //string _type = (string)e;

            //if (Plugin._serialPort[indexOfSelectedPedal_u].PortName = sp.PortName)

            // identify which pedal has send the data
            int pedalSelected = 255;
            for (int pedalIdx_i = 0; pedalIdx_i < 3; pedalIdx_i++)
            {
                if ((Plugin._serialPort[pedalIdx_i].PortName == sp.PortName) && (Plugin._serialPort[pedalIdx_i].IsOpen))
                {
                    pedalSelected = pedalIdx_i;
                }
            }

            // once the pedal has identified, go ahead
            if (pedalSelected < 3)
            //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {
                // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


                int length = sizeof(DAP_config_st);
                byte[] newBuffer_config = new byte[length];

                int receivedLength = sp.BytesToRead;


                string incomingData = sp.ReadExisting();
                //if the data doesn't end with a stop char this will signal to keep it in _data 
                //for appending to the following read of data
                bool endsWithStop = EndsWithStop(incomingData);

                //each array object will be sent separately to the callback
                string[] dataArray = incomingData.Split(STOPCHAR, StringSplitOptions.None);

                for (int i = 0; i < dataArray.Length; i++)
                {
                    string newData = dataArray[i];

                    //if you are at the last object in the array and this hasn't got a stopchar after
                    //it will be saved in _data
                    if (!endsWithStop && i == dataArray.Length - 1)
                    {
                        _data[pedalSelected] += newData;
                    }
                    else
                    {
                        string dataToSend = _data[pedalSelected] + newData;
                        _data[pedalSelected] = "";


                        // decode into config struct
                        if (dataToSend.Length == length)
                        {
                            DAP_config_st tmp;

                            // transform string into byte
                            fixed (byte* p = Encoding.ASCII.GetBytes(dataToSend))
                            {
                                // create a fixed size buffer
                                length = sizeof(DAP_config_st);
                                byte[] newBuffer_config_2 = new byte[length];

                                // copy the received bytes into byte array
                                for (int j = 0; j < length; j++)
                                {
                                    newBuffer_config_2[j] = p[j];
                                }

                                // parse byte array as config struct
                                DAP_config_st pedalConfig_read_st = getConfigFromBytes(newBuffer_config_2);

                                // check whether receive struct is plausible
                                DAP_config_st* v_config = &pedalConfig_read_st;
                                byte* p_config = (byte*)v_config;

                                // payload type check
                                bool check_payload_config_b = false;
                                if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
                                {
                                    check_payload_config_b = true;
                                }

                                // CRC check
                                bool check_crc_config_b = false;
                                if (Plugin.checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_config_b = true;
                                }


                                // when all checks are passed, accept the config. Otherwise discard and trow error
                                Dispatcher.Invoke(
                                new Action<DAP_config_st>((t) => this.dap_config_st[pedalSelected] = t),
                                pedalConfig_read_st);


                                this.Dispatcher.Invoke(() =>
                                {
                                    // update pedal config
                                    if (check_payload_config_b)
                                    {
                                        //this.dap_config_st[indexOfSelectedPedal_u] = pedalConfig_read_st;
                                        updateTheGuiFromConfig();
                                    }

                                    TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
                                    TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
                                });

                            }

                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                //TextBox_serialMonitor.Text += "DataArrayLength: " + dataArray.Length + "\n";
                                //TextBox_serialMonitor.Text += "DataLength: " + dataToSend.Length + "\n";
                                TextBox_serialMonitor.Text += dataToSend + "\n";

                                TextBox_serialMonitor.ScrollToEnd();
                                //TextBox_serialMonitor.Text += receivedLength + "\n";
                            });
                        }


                    }











                    //limits the data stored to 1000 to avoid using up all the memory in case of 
                    //failure to register callback or include stopchar

                    if (_data[pedalSelected].Length > 1000)
                    {
                        _data[pedalSelected] = "";
                    }


                    //////this.Dispatcher.Invoke(() =>
                    //////    {
                    //////        TextBox_serialMonitor.Text += incomingData;

                    //////        TextBox_serialMonitor.ScrollToEnd();
                    //////        //TextBox_serialMonitor.Text += receivedLength + "\n";
                    //////    });
                }

                // obtain data and check whether it is from known payload type or just debug info

            }
        }



        /********************************************************************************************************************/
        /*							read serial stream																		*/
        /********************************************************************************************************************/
        public void openSerialAndAddReadCallback(uint pedalIdx)
        {
            try
            {
                // serial port settings
                Plugin._serialPort[pedalIdx].Handshake = Handshake.None;
                Plugin._serialPort[pedalIdx].Parity = Parity.None;
                //_serialPort[pedalIdx].StopBits = StopBits.None;


                Plugin._serialPort[pedalIdx].ReadTimeout = 2000;
                Plugin._serialPort[pedalIdx].WriteTimeout = 500;

                // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
                Plugin._serialPort[pedalIdx].Encoding = System.Text.Encoding.GetEncoding(28591);

                // regular ESP
                //Plugin._serialPort[pedalIdx].DtrEnable = false;

                // ESP32 S3
                //Plugin._serialPort[pedalIdx].RtsEnable = false;
                //Plugin._serialPort[pedalIdx].DtrEnable = true;


                Plugin._serialPort[pedalIdx].NewLine = "\r\n";
                Plugin._serialPort[pedalIdx].ReadBufferSize = 10000;
                if (Plugin.Settings.auto_connect_flag[pedalIdx] == 1 & Plugin.Settings.connect_flag[pedalIdx] == 1 )
                {
                    if (Plugin.Settings.autoconnectComPortNames[pedalIdx] == "NA")
                    {
                        Plugin._serialPort[pedalIdx].PortName = Plugin.Settings.autoconnectComPortNames[pedalIdx];
                    }
                    else
                    {
                        Plugin._serialPort[pedalIdx].PortName = Plugin.Settings.selectedComPortNames[pedalIdx];
                        Plugin.Settings.autoconnectComPortNames[pedalIdx] = Plugin.Settings.selectedComPortNames[pedalIdx];
                    }
                    
                }
                else
                {
                    Plugin._serialPort[pedalIdx].PortName = Plugin.Settings.selectedComPortNames[pedalIdx];
                    Plugin.Settings.autoconnectComPortNames[pedalIdx]= Plugin.Settings.selectedComPortNames[pedalIdx];
                }
                
                if (Plugin.PortExists(Plugin._serialPort[pedalIdx].PortName))
                {
                    try
                    {
                        Plugin._serialPort[pedalIdx].Open();

                        // ESP32 S3
                        if (Plugin.Settings.RTSDTR_False[pedalIdx] == true)
                        {
                            Plugin._serialPort[pedalIdx].RtsEnable = false;
                            Plugin._serialPort[pedalIdx].DtrEnable = false;
                        }

                        System.Threading.Thread.Sleep(200);

                        // ESP32 S3
                        Plugin._serialPort[pedalIdx].RtsEnable = false;
                        Plugin._serialPort[pedalIdx].DtrEnable = true;

                        Plugin.Settings.connect_status[pedalIdx] = 1;
                        // read callback
                        if (pedal_serial_read_timer[pedalIdx] != null)
                        {
                            pedal_serial_read_timer[pedalIdx].Stop();
                            pedal_serial_read_timer[pedalIdx].Dispose();
                        }
                        pedal_serial_read_timer[pedalIdx] = new System.Windows.Forms.Timer();
                        pedal_serial_read_timer[pedalIdx].Tick += new EventHandler(timerCallback_serial);
                        pedal_serial_read_timer[pedalIdx].Tag = pedalIdx;
                        pedal_serial_read_timer[pedalIdx].Interval = 16; // in miliseconds
                        pedal_serial_read_timer[pedalIdx].Start();
                        System.Threading.Thread.Sleep(100);
                        Serial_connect_status[pedalIdx] = true;
                    }
                    catch(Exception ex)
                    { 
                        TextBox2.Text = ex.Message;
                        Serial_connect_status[pedalIdx] = false;
                    }
                    

                }
                else
                {
                    Plugin.Settings.connect_status[pedalIdx] = 0;
                    Plugin.connectSerialPort[pedalIdx] = false;
                    Serial_connect_status[pedalIdx] = false;

                }
            }
            catch (Exception ex)
            { }




        }
        private uint count_timmer_count = 0;
        private string Toast_tmp;
        public void connection_timmer_tick(object sender, EventArgs e)
        {
            //simhub action for debug
            Simhub_action_update();
            string tmp = "Connecting";
            int count_connection = ((int)count_timmer_count) % 4;
            switch (count_connection) 
            {
                case 0:
                    break;
                case 1:
                    tmp = tmp + ".";
                    break;
                case 2:
                    tmp = tmp + "..";
                    break;
                case 3:
                    tmp = tmp + "...";
                    break;
            }
            info_text_connection = tmp;



            count_timmer_count++;
            if (count_timmer_count > 1)
            {
                if (Plugin.Settings.Pedal_ESPNow_auto_connect_flag)
                {
                    if (Plugin.PortExists(Plugin.Settings.ESPNow_port))
                    {
                        if (Plugin.ESPsync_serialPort.IsOpen == false)
                        {
                            Plugin.ESPsync_serialPort.PortName = Plugin.Settings.ESPNow_port;
                            try
                            {
                                // serial port settings
                                Plugin.ESPsync_serialPort.Handshake = Handshake.None;
                                Plugin.ESPsync_serialPort.Parity = Parity.None;
                                //_serialPort[pedalIdx].StopBits = StopBits.None;
                                Plugin.ESPsync_serialPort.ReadTimeout = 2000;
                                Plugin.ESPsync_serialPort.WriteTimeout = 500;
                                // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
                                Plugin.ESPsync_serialPort.Encoding = System.Text.Encoding.GetEncoding(28591);
                                Plugin.ESPsync_serialPort.NewLine = "\r\n";
                                Plugin.ESPsync_serialPort.ReadBufferSize = 10000;
                                try
                                {
                                    Plugin.ESPsync_serialPort.Open();
                                    System.Threading.Thread.Sleep(200);
                                    // ESP32 S3
                                    Plugin.ESPsync_serialPort.RtsEnable = false;
                                    Plugin.ESPsync_serialPort.DtrEnable = true;
                                    //SystemSounds.Beep.Play();
                                    Plugin.Sync_esp_connection_flag = true;
                                    btn_connect_espnow_port.Content = "Disconnect";
                                    ESP_host_serial_timer = new System.Windows.Forms.Timer();
                                    ESP_host_serial_timer.Tick += new EventHandler(timerCallback_serial_esphost);
                                    ESP_host_serial_timer.Tag = 3;
                                    ESP_host_serial_timer.Interval = 16; // in miliseconds
                                    ESP_host_serial_timer.Start();
                                    System.Threading.Thread.Sleep(100);
                                    ToastNotification("Pedal Bridge", "Connected");
                                }
                                catch (Exception ex)
                                {
                                    TextBox2.Text = ex.Message;
                                    //Serial_connect_status[3] = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                TextBox2.Text = ex.Message;
                            }
                        }
                    }
                    else
                    {  
                        Plugin.Sync_esp_connection_flag = false;
                        btn_connect_espnow_port.Content = "Connect";
                        if (ESP_host_serial_timer != null)
                        {
                            ESP_host_serial_timer.Stop();
                            ESP_host_serial_timer.Dispose();
                        }
                            
                    }
                                            
                }

                for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
                {
                    if (Plugin.Settings.auto_connect_flag[pedalIdx] == 1)
                    {

                        if (Plugin.Settings.connect_flag[pedalIdx] == 1)
                        {
                            if (Plugin.PortExists(Plugin._serialPort[pedalIdx].PortName))
                            {
                                if (Plugin._serialPort[pedalIdx].IsOpen == false)
                                {
                                    //UpdateSerialPortList_click();
                                    openSerialAndAddReadCallback(pedalIdx);
                                    //Plugin.Settings.autoconnectComPortNames[pedalIdx] = Plugin._serialPort[pedalIdx].PortName;
                                    System.Threading.Thread.Sleep(200);
                                    if (Serial_connect_status[pedalIdx])
                                    {
                                        if (Plugin.Settings.reading_config == 1)
                                        {
                                            Reading_config_auto(pedalIdx);
                                        }
                                        System.Threading.Thread.Sleep(100);
                                        //add toast notificaiton
                                        switch (pedalIdx)
                                        {
                                            case 0:
                                                Toast_tmp = "Clutch Pedal:" + Plugin.Settings.autoconnectComPortNames[pedalIdx];
                                                break;
                                            case 1:
                                                Toast_tmp = "Brake Pedal:" + Plugin.Settings.autoconnectComPortNames[pedalIdx];
                                                break;
                                            case 2:
                                                Toast_tmp = "Throttle Pedal:" + Plugin.Settings.autoconnectComPortNames[pedalIdx];
                                                break;
                                        }
                                        ToastNotification(Toast_tmp, "Connected");
                                        updateTheGuiFromConfig();
                                        //System.Threading.Thread.Sleep(2000);
                                        //ToastNotificationManager.History.Clear("FFB Pedal Dashboard");
                                    }



                                }
                            }
                            else
                            {
                                Plugin.connectSerialPort[pedalIdx] = false;
                                Plugin.Settings.connect_status[pedalIdx] = 0;
                                updateTheGuiFromConfig();
                            }




                        }
                    }
                }
                    

                
            }
            if (count_timmer_count > 200)
            {
                count_timmer_count = 2;
            }

        }

        public void closeSerialAndStopReadCallback(uint pedalIdx)
        {
            
            if (pedal_serial_read_timer[pedalIdx] != null)
            {
                pedal_serial_read_timer[pedalIdx].Stop();
                pedal_serial_read_timer[pedalIdx].Dispose();
            }
            connect_timer.Dispose();
            connect_timer.Stop();
            if (ESP_host_serial_timer != null)
            {
                ESP_host_serial_timer.Stop();
                ESP_host_serial_timer.Dispose();
            }
            System.Threading.Thread.Sleep(300);
            
            
            if (Plugin._serialPort[pedalIdx].IsOpen)
            {
                // ESP32 S3
                if (Plugin.Settings.RTSDTR_False[pedalIdx] == true)
                {
                    Plugin._serialPort[pedalIdx].RtsEnable = false;
                    Plugin._serialPort[pedalIdx].DtrEnable = false;
                }


                Plugin._serialPort[pedalIdx].DiscardInBuffer();
                Plugin._serialPort[pedalIdx].DiscardOutBuffer();
                Plugin._serialPort[pedalIdx].Close();
                Plugin.Settings.connect_status[pedalIdx] = 0;
            }
            if (Plugin.ESPsync_serialPort.IsOpen)
            {
                Plugin.ESPsync_serialPort.DiscardInBuffer();
                Plugin.ESPsync_serialPort.DiscardOutBuffer();
                Plugin.ESPsync_serialPort.Close();
                Plugin.Sync_esp_connection_flag = false;
            }
        }

        Int64 writeCntr = 0;

        int[] timeCntr = { 0, 0, 0,0 };

        double[] timeCollector = { 0, 0, 0,0 };


        static List<int> FindAllOccurrences(byte[] source, byte[] sequence, int maxLength)
        {
            List<int> indices = new List<int>();

            int len = source.Length - sequence.Length;
            if (len > maxLength)
            {
                len = maxLength;
            }

            for (int i = 0; i <= len; i++)
            {
                bool found = true;
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (source[i + j] != sequence[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    indices.Add(i); // Sequence found, add index to the list
                }
            }



            //int i = 0;
            //while (i < len)
            //{
            //    bool found = true;
            //    for (int j = 0; j < sequence.Length; j++)
            //    {
            //        if (source[i + j] != sequence[j])
            //        {
            //            found = false;
            //            break;
            //        }
            //    }
            //    if (found)
            //    {
            //        indices.Add(i); // Sequence found, add index to the list
            //        i += sequence.Length;
            //    }
            //    else { i++; } 
            //}



            return indices;
        }

        public void Simhub_action_update()
        {
            if (Plugin.Page_update_flag == true)
            {
                Profile_change(Plugin.profile_index);
                Plugin.Page_update_flag = false;
                MyTab.SelectedIndex = (int)Plugin.Settings.table_selected;
                Plugin.pedal_select_update_flag = false;
                Plugin.simhub_theme_color = defaultcolor.ToString();
                switch (Plugin.Settings.table_selected)
                {
                    case 0:
                        Plugin.current_pedal = "Clutch";
                        break;
                    case 1:
                        Plugin.current_pedal = "Brake";
                        break;
                    case 2:
                        Plugin.current_pedal = "Throttle";
                        break;
                }
                updateTheGuiFromConfig();
            }

            if (Plugin.sendconfig_flag == 1)
            {
                Sendconfigtopedal_shortcut();
                Plugin.sendconfig_flag = 0;
            }
        }

        int[] appendedBufferOffset = { 0, 0, 0,0 };

        static int bufferSize = 10000;
        static int destBufferSize = 1000;
        byte[][] buffer_appended = { new byte[bufferSize], new byte[bufferSize], new byte[bufferSize], new byte[bufferSize] };

        unsafe public void timerCallback_serial(object sender, EventArgs e)
        {

            //action here 
            Simhub_action_update();
            
            


            int pedalSelected = Int32.Parse((sender as System.Windows.Forms.Timer).Tag.ToString());
            //int pedalSelected = (int)(sender as System.Windows.Forms.Timer).Tag;

            bool pedalStateHasAlreadyBeenUpdated_b = false;

            // once the pedal has identified, go ahead
            if (pedalSelected < 3)
            //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {



                // Create a Stopwatch instance
                Stopwatch stopwatch = new Stopwatch();

                // Start the stopwatch
                stopwatch.Start();



                SerialPort sp = Plugin._serialPort[pedalSelected];



                // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


                //int length = sizeof(DAP_config_st);




                if (sp.IsOpen)
                {
                    if (Plugin.Settings.Serial_auto_clean)
                    {
                        if (TextBox_serialMonitor.LineCount > 300)
                        {
                            TextBox_serialMonitor.Clear();
                        }
                    }

                    int receivedLength = 0;
                    try 
                    {
                        receivedLength = sp.BytesToRead;
                    }
                    catch (Exception ex)
                    {
                        TextBox_debugOutput.Text = ex.Message;
                        //ConnectToPedal.IsChecked = false;
                        return;
                    }

                

                    if (receivedLength > 0)
                    {

                        //TextBox_serialMonitor.Text += "Received:" + receivedLength + "\n";
                        //TextBox_serialMonitor.ScrollToEnd();


                        timeCntr[pedalSelected] += 1;


                        // determine byte sequence which is defined as message end --> crlf
                        byte[] byteToFind = System.Text.Encoding.GetEncoding(28591).GetBytes(STOPCHAR[0].ToCharArray());
                        int stop_char_length = byteToFind.Length;


                        // calculate current buffer length
                        int currentBufferLength = appendedBufferOffset[pedalSelected] + receivedLength;


                        // check if buffer is large enough otherwise discard in buffer and set offset to 0
                        if ((bufferSize > currentBufferLength) && (appendedBufferOffset[pedalSelected] >= 0))
                        {
                            sp.Read(buffer_appended[pedalSelected], appendedBufferOffset[pedalSelected], receivedLength);
                        }
                        else
                        {
                            sp.DiscardInBuffer();
                            appendedBufferOffset[pedalSelected] = 0;
                            return;
                        }


                        


                        // copy to local buffer
                        //byte[] localBuffer = new byte[currentBufferLength];
                        
                        //Buffer.BlockCopy(buffer_appended[pedalSelected], 0, localBuffer, 0, currentBufferLength);


                        // find all occurences of crlf as they indicate message end
                        List<int> indices = FindAllOccurrences(buffer_appended[pedalSelected], byteToFind, currentBufferLength);




                        // Destination array
                        byte[] destinationArray = new byte[destBufferSize];

                        





                        int srcBufferOffset = 0;
                        // decode every message
                        //foreach (int number in indices)
                        for (int msgId = 0; msgId < indices.Count; msgId++)
                        {
                            // computes the length of bytes to read
                            int destBuffLength = 0; //number - srcBufferOffset;

                            if (msgId == 0)
                            {
                                srcBufferOffset = 0;
                                destBuffLength = indices.ElementAt(msgId);
                            }
                            else 
                            {
                                srcBufferOffset = indices.ElementAt(msgId - 1) + stop_char_length;
                                destBuffLength = indices.ElementAt(msgId) - srcBufferOffset;
                            }

                            // check if dest buffer length is within valid length
                            if ( (destBuffLength <= 0) | (destBuffLength > destBufferSize) )
                            {
                                continue;
                            }


                 


                            // copy bytes to subarray
                            Buffer.BlockCopy(buffer_appended[pedalSelected], srcBufferOffset, destinationArray, 0, destBuffLength);


                            // check for pedal state struct
                            if ((destBuffLength == sizeof(DAP_state_basic_st)))
                            {

                                // parse byte array as config struct
                                DAP_state_basic_st pedalState_read_st = getStateFromBytes(destinationArray);

                                // check whether receive struct is plausible
                                DAP_state_basic_st* v_state = &pedalState_read_st;
                                byte* p_state = (byte*)v_state;

                                // payload type check
                                bool check_payload_state_b = false;
                                if (pedalState_read_st.payloadHeader_.payloadType == Constants.pedalStateBasicPayload_type)
                                {
                                    check_payload_state_b = true;
                                }

                                // CRC check
                                bool check_crc_state_b = false;
                                if (Plugin.checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState_Basic)) == pedalState_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_state_b = true;
                                }

                                if ((check_payload_state_b) && check_crc_state_b)
                                {

                                    // write vJoy data
                                    Pedal_position_reading[pedalSelected] = pedalState_read_st.payloadPedalBasicState_.joystickOutput_u16;
                                    //if (Plugin.Rudder_enable_flag == false)
                                    //{
                                        if (Plugin.Settings.vjoy_output_flag == 1)
                                        {
                                            switch (pedalSelected)
                                            {

                                                case 0:
                                                    //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RX);  // Center X axis
                                                    joystick.SetAxis(pedalState_read_st.payloadPedalBasicState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RX);   // HID_USAGES Enums
                                                    break;
                                                case 1:
                                                    //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RY);  // Center X axis
                                                    joystick.SetAxis(pedalState_read_st.payloadPedalBasicState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RY);   // HID_USAGES Enums
                                                    break;
                                                case 2:
                                                    //joystick.SetJoystickAxis(pedalState_read_st.payloadPedalState_.joystickOutput_u16, Axis.HID_USAGE_RZ);  // Center X axis
                                                    joystick.SetAxis(pedalState_read_st.payloadPedalBasicState_.joystickOutput_u16, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);   // HID_USAGES Enums
                                                    break;
                                                default:
                                                    break;
                                            }

                                        }
                                        
                                    //}
                                    /*
                                    else
                                    {
                                        //Brk move

                                        if (Pedal_position_reading[1] > Pedal_position_reading[2])
                                        {
                                            double Rudder_axis_value = 16384;
                                            Rudder_axis_value = Rudder_axis_value - Pedal_position_reading[1];

                                            joystick.SetAxis((int)Rudder_axis_value, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);   // HID_USAGES Enums
                                        }
                                        else
                                        {
                                            if (Pedal_position_reading[2] > Pedal_position_reading[1])
                                            {
                                                double Rudder_axis_value = 16384;
                                                Rudder_axis_value = Rudder_axis_value + Pedal_position_reading[2];

                                                joystick.SetAxis((int)Rudder_axis_value, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);   // HID_USAGES Enums
                                            }
                                            else
                                            {
                                                joystick.SetAxis(16384, Plugin.Settings.vjoy_order, HID_USAGES.HID_USAGE_RZ);

                                            }

                                        }




                                    }
                                    */



                                    // GUI update
                                    if ((pedalStateHasAlreadyBeenUpdated_b == false) && (indexOfSelectedPedal_u == pedalSelected))
                                    {
                                        //TextBox_debugOutput.Text = "Pedal pos: " + pedalState_read_st.payloadPedalState_.pedalPosition_u16;
                                        //TextBox_debugOutput.Text += "Pedal force: " + pedalState_read_st.payloadPedalState_.pedalForce_u16;
                                        //TextBox_debugOutput.Text += ",  Servo pos targe: " + pedalState_read_st.payloadPedalState_.servoPosition_i16;
                                        //TextBox_debugOutput.Text += ",  Servo pos: " + pedalState_read_st.payloadPedalState_.servoPosition_i16;



                                        pedalStateHasAlreadyBeenUpdated_b = true;

                                        text_point_pos.Visibility = Visibility.Hidden;
                                        double control_rect_value_max = 65535;
                                        double dyy = canvas.Height / control_rect_value_max;
                                        double dxx = canvas.Width / control_rect_value_max;

                                        if (debug_flag)
                                        {
                                            Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 - rect_State.Width / 2 );
                                            Canvas.SetTop(rect_State, canvas.Height - dyy * pedalState_read_st.payloadPedalBasicState_.pedalForce_u16 - rect_State.Height / 2);

                                            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) /*+ rect_State.Width*/);
                                            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                            text_state.Text = Math.Round(pedalState_read_st.payloadPedalBasicState_.pedalForce_u16 / control_rect_value_max * 100) + "%";
                                            int round_x = (int)(100 * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 / control_rect_value_max) - 1;
                                            int x_showed = round_x + 1;
                                            
                                            current_pedal_travel_state = x_showed;
                                            Plugin.pedal_state_in_ratio = (byte)current_pedal_travel_state;
                                        }
                                        else
                                        {
                                            Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 - rect_State.Width / 2 );
                                            int round_x = (int)(100 * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 / control_rect_value_max) - 1;
                                            int x_showed = round_x + 1;
                                            round_x = Math.Max(0, Math.Min(round_x, 99));
                                            current_pedal_travel_state = x_showed;
                                            Plugin.pedal_state_in_ratio = (byte)current_pedal_travel_state;
                                            Canvas.SetTop(rect_State, canvas.Height - Force_curve_Y[round_x] - rect_State.Height / 2);
                                            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) /*+ rect_State.Width*/);
                                            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                            text_state.Text = x_showed + "%";
                                            Pedal_joint_draw();
                                        }

                                    }


                                    continue;
                                }

                            }






                            // check for pedal extended state struct
                            if ((destBuffLength == sizeof(DAP_state_extended_st)))
                            {

                                // parse byte array as config struct
                                DAP_state_extended_st pedalState_ext_read_st = getStateExtFromBytes(destinationArray);

                                // check whether receive struct is plausible
                                DAP_state_extended_st* v_state = &pedalState_ext_read_st;
                                byte* p_state = (byte*)v_state;

                                // payload type check
                                bool check_payload_state_b = false;
                                if (pedalState_ext_read_st.payloadHeader_.payloadType == Constants.pedalStateExtendedPayload_type)
                                {
                                    check_payload_state_b = true;
                                }

                                // CRC check
                                bool check_crc_state_b = false;
                                if (Plugin.checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState_Extended)) == pedalState_ext_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_state_b = true;
                                }

                                if ((check_payload_state_b) && check_crc_state_b)
                                {




                                    if (indexOfSelectedPedal_u == pedalSelected)
                                    {
                                        if (dumpPedalToResponseFile[indexOfSelectedPedal_u])
                                        {
                                            // Specify the path to the file
                                            string currentDirectory = Directory.GetCurrentDirectory();
                                            string filePath = currentDirectory + "\\PluginsData\\Common" + "\\output_" + indexOfSelectedPedal_u.ToString() + ".txt";


                                            // write header
                                            if (!File.Exists(filePath))
                                            {
                                                using (StreamWriter writer = new StreamWriter(filePath, true))
                                                {
                                                    // Write the content to the file
                                                    writer.Write("cycleCtr, ");
                                                    writer.Write("time_InMs, ");
                                                    writer.Write("forceRaw_InKg, ");
                                                    writer.Write("forceFiltered_InKg, ");
                                                    writer.Write("forceVelocity_InKgPerSec, ");
                                                    writer.Write("servoPos_InSteps, ");
                                                    writer.Write("servoPosEsp_InSteps, ");
                                                    writer.Write("servoCurrent_InPercent, ");
                                                    writer.Write("servoVoltage_InV");
                                                    writer.Write("\n");
                                                }

                                            }
                                            // Use StreamWriter to write to the file
                                            using (StreamWriter writer = new StreamWriter(filePath, true))
                                            {
                                                // Write the content to the file
                                                writeCntr++;
                                                writer.Write(writeCntr);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.timeInMs_u32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.pedalForce_raw_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.pedalForce_filtered_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.forceVel_est_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servoPosition_i16);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servoPositionTarget_i16);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servo_current_percent_i16);
                                                writer.Write(", ");
                                                writer.Write(((float)pedalState_ext_read_st.payloadPedalExtendedState_.servo_voltage_0p1V_i16) / 10.0);
                                                writer.Write("\n");
                                            }
                                        }
                                    }




                                    continue;
                                }
                            }








                            // decode into config struct
                            if ((waiting_for_pedal_config[pedalSelected]) && (destBuffLength == sizeof(DAP_config_st)))
                            {

                                // parse byte array as config struct
                                DAP_config_st pedalConfig_read_st = getConfigFromBytes(destinationArray);

                                // check whether receive struct is plausible
                                DAP_config_st* v_config = &pedalConfig_read_st;
                                byte* p_config = (byte*)v_config;

                                // payload type check
                                bool check_payload_config_b = false;
                                if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
                                {
                                    check_payload_config_b = true;
                                }

                                // CRC check
                                bool check_crc_config_b = false;
                                if (Plugin.checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_config_b = true;
                                }

                                if ((check_payload_config_b) && check_crc_config_b)
                                {
                                    waiting_for_pedal_config[pedalSelected] = false;
                                    dap_config_st[pedalSelected] = pedalConfig_read_st;
                                    updateTheGuiFromConfig();

                                    continue;
                                }
                                else
                                {
                                    TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
                                    TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
                                }

                            }


                            // If non known array datatype was received, assume a text message was received and print it
                            // only print debug messages when debug mode is active as it degrades performance
                            if (Debug_check.IsChecked == true)
                            {
                                byte[] destinationArray_sub = new byte[destBuffLength];
                                Buffer.BlockCopy(destinationArray, 0, destinationArray_sub, 0, destBuffLength);
                                string resultString = Encoding.GetEncoding(28591).GetString(destinationArray_sub);

                                TextBox_serialMonitor.Text += resultString + "\n";
                                TextBox_serialMonitor.ScrollToEnd();
                            }

                            




                            // When only a few messages are received, make the counter greater than N thus every message is printed
                            //if (destBuffLength < 100)
                            //{
                            //    printCtr = 600;
                            //}

                            //if (printCtr++ > 200)
                            //{
                            //    printCtr = 0;
                            //    TextBox_serialMonitor.Text += dataToSend + "\n";
                            //    TextBox_serialMonitor.ScrollToEnd();
                            //}





                        }







                        // copy the last not finished buffer element to begining of next cycles buffer
                        // and determine buffer offset
                        if (indices.Count > 0)
                        {
                            // If at least one crlf was detected, check whether it arrieved at the last bytes
                            int lastElement = indices.Last<int>();
                            int remainingMessageLength = currentBufferLength - (lastElement + stop_char_length);
                            if (remainingMessageLength > 0)
                            {
                                appendedBufferOffset[pedalSelected] = remainingMessageLength;

                                Buffer.BlockCopy(buffer_appended[pedalSelected], lastElement + stop_char_length, buffer_appended[pedalSelected], 0, remainingMessageLength);
                            }
                            else
                            {
                                appendedBufferOffset[pedalSelected] = 0;
                            }
                        }
                        else
                        {
                            appendedBufferOffset[pedalSelected] += receivedLength;
                        }






                        // Stop the stopwatch
                        stopwatch.Stop();

                        // Get the elapsed time
                        TimeSpan elapsedTime = stopwatch.Elapsed;

                        timeCollector[pedalSelected] += elapsedTime.TotalMilliseconds;

                        if (timeCntr[pedalSelected] >= 50)
                        {


                            double avgTime = timeCollector[pedalSelected] / timeCntr[pedalSelected];
                            if (debug_flag)
                            {
                                TextBox_debugOutput.Text = "Serial callback time in ms: " + avgTime.ToString();
                            }
                            timeCntr[pedalSelected] = 0;
                            timeCollector[pedalSelected] = 0;
                        }
                    }

                }
            }
        }



        /********************************************************************************************************************/
        /*							Connect to pedal																		*/
        /********************************************************************************************************************/


        unsafe public void ConnectToPedal_click(object sender, RoutedEventArgs e)
        {

            Plugin.Settings.connect_flag[indexOfSelectedPedal_u] = 1;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedal_type = (byte)indexOfSelectedPedal_u;
            if (ConnectToPedal.IsChecked == false)
            {
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == false)
                {
                    try
                    {
                        openSerialAndAddReadCallback(indexOfSelectedPedal_u);
                        TextBox_debugOutput.Text = "Serialport open";
                        ConnectToPedal.IsChecked = true;
                        btn_pedal_connect.Content = "Disconnect From Pedal";

                        // register a callback that is triggered when serial data is received
                        // see https://gist.github.com/mini-emmy/9617732
                        //Plugin._serialPort[indexOfSelectedPedal_u].DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

                        System.Threading.Thread.Sleep(100);

                        Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 1;

                    }
                    catch (Exception ex)
                    {
                        TextBox_debugOutput.Text = ex.Message;
                        ConnectToPedal.IsChecked = false;
                    }

                }
                else
                {
                    closeSerialAndStopReadCallback(indexOfSelectedPedal_u);

                    //Plugin._serialPort[indexOfSelectedPedal_u].DataReceived -= sp_DataReceived;

                    ConnectToPedal.IsChecked = false;
                    TextBox_debugOutput.Text = "Serialport already open, close it";
                    Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
                    Plugin.Settings.connect_flag[indexOfSelectedPedal_u] = 0;
                    Plugin.connectSerialPort[indexOfSelectedPedal_u] = false;
                    btn_pedal_connect.Content = "Connect To Pedal";
                }
            }
            else
            {
                ConnectToPedal.IsChecked = false;
                closeSerialAndStopReadCallback(indexOfSelectedPedal_u);
                TextBox_debugOutput.Text = "Serialport close";
                Plugin.connectSerialPort[indexOfSelectedPedal_u] = false;
                Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
                Plugin.Settings.connect_flag[indexOfSelectedPedal_u] = 0;
                btn_pedal_connect.Content = "Connect To Pedal";

            }

            ////reading config from pedal

            if (checkbox_pedal_read.IsChecked == true)
            {
                Reading_config_auto(indexOfSelectedPedal_u);
            }
            updateTheGuiFromConfig();
        }

        /********************************************************************************************************************/
        /*							Serial port selection																	*/
        /********************************************************************************************************************/
        public void UpdateSerialPortList_click(object sender, RoutedEventArgs e)
        {
            UpdateSerialPortList_click();
        }

        public void SerialPortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tmp = (string)SerialPortSelection.SelectedValue;
            //Plugin._serialPort[indexOfSelectedPedal_u].PortName = tmp;


            //try 
            //{
            //    TextBox_debugOutput.Text = "Debug: " + Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u];
            //}
            //catch (Exception caughtEx)
            //{
            //    string errorMessage = caughtEx.Message;
            //    TextBox_debugOutput.Text = errorMessage;
            //}

            try
            {
                //if (Plugin.Settings.connect_status[indexOfSelectedPedal_u] == 0)
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen == false)
                {
                    Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u] = tmp;
                    Plugin._serialPort[indexOfSelectedPedal_u].PortName = tmp;
                }
                TextBox_debugOutput.Text = "COM port selected: " + Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u];

            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }



        }





        public void AbsPatternChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absPattern = (byte)AbsPattern.SelectedIndex;
            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }
            update_plot_ABS();
        }


        public void KF_filter_order_changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelOrder = (byte)KF_filter_order.SelectedIndex;
            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }
        }

        public void SpindlePitchChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.spindlePitch_mmPerRev_u8 = (byte)SpindlePitch.SelectedIndex;
            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }
        }








        unsafe private void RestartPedal_click(object sender, RoutedEventArgs e)
        {
            Plugin._serialPort[indexOfSelectedPedal_u].DtrEnable = true;
            Plugin._serialPort[indexOfSelectedPedal_u].RtsEnable = true;
            System.Threading.Thread.Sleep(100);
            Plugin._serialPort[indexOfSelectedPedal_u].DtrEnable = false;
            Plugin._serialPort[indexOfSelectedPedal_u].RtsEnable = false;
            if (Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u])
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    try
                    {
                        // compute checksum
                        DAP_action_st tmp;
                        tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
                        tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
                        tmp.payloadHeader_.PedalTag = (byte)indexOfSelectedPedal_u;
                        tmp.payloadPedalAction_.system_action_u8 = 2; //1=reset pedal position, 2 =restart esp.

                        DAP_action_st* v = &tmp;
                        byte* p = (byte*)v;
                        tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
                        int length = sizeof(DAP_action_st);
                        byte[] newBuffer = new byte[length];
                        newBuffer = Plugin.getBytes_Action(tmp);
                        // clear inbuffer 
                        Plugin.ESPsync_serialPort.DiscardInBuffer();

                        // send query command
                        Plugin.ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }
                }
            }
            
        }

        public void Read_for_slot(object sender, EventArgs e)
        {
            var Button = sender as SHButtonPrimary;
            
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Title = "Datei auswählen";
                openFileDialog.Filter = "Configdateien (*.json)|*.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                openFileDialog.InitialDirectory = currentDirectory + "\\PluginsData\\Common";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string content = (string)openFileDialog.FileName;


                    string filePath = openFileDialog.FileName;
                    //TextBox_debugOutput.Text =  Button.Name; 
                    if (Button.Name == "Reading_clutch")
                    {

                        Plugin.Settings.Pedal_file_string[profile_select,0] = filePath;
                        Label_clutch_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 0];
                        Plugin.Settings.file_enable_check[profile_select, 0] = 1;
                        Clutch_file_check.IsChecked = true;

                    }
                    if (Button.Name == "Reading_brake")
                    {
                        Plugin.Settings.Pedal_file_string[profile_select, 1] = filePath;
                        Label_brake_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 1];
                        Plugin.Settings.file_enable_check[profile_select, 1] = 1;
                        Brake_file_check.IsChecked = true;
                    }
                    if (Button.Name == "Reading_gas")
                    {
                        Plugin.Settings.Pedal_file_string[profile_select, 2] = filePath;
                        Label_gas_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 2];
                        Plugin.Settings.file_enable_check[profile_select, 2] = 1;
                        Gas_file_check.IsChecked = true;
                    }


                }
            }
        }

        public void Clear_slot(object sender, EventArgs e)
        {
            var Button = sender as SHButtonPrimary;
            if (Button.Name == "Clear_clutch")
            {

                Plugin.Settings.Pedal_file_string[profile_select, 0] = "";
                Label_clutch_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 0];
                Plugin.Settings.file_enable_check[profile_select, 0] = 0;
                Clutch_file_check.IsChecked = false;

            }
            if (Button.Name == "Clear_brake")
            {

                Plugin.Settings.Pedal_file_string[profile_select, 1] = "";
                Label_brake_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 1];
                Plugin.Settings.file_enable_check[profile_select, 1] = 0;
                Brake_file_check.IsChecked = false;

            }
            if (Button.Name == "Clear_gas")
            {

                Plugin.Settings.Pedal_file_string[profile_select, 2] = "";
                Label_gas_file.Content = Plugin.Settings.Pedal_file_string[profile_select, 2];
                Plugin.Settings.file_enable_check[profile_select, 2] = 0;
                Gas_file_check.IsChecked = false;

            }
            //updateTheGuiFromConfig();
        }
        void Parsefile(uint profile_index)
        {
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization


            // c# code to iterate over all fields of struct and set values from json file
            for (uint pedalIdx = 0; pedalIdx < 3; pedalIdx++)
            {
                if (Plugin.Settings.file_enable_check[profile_select, pedalIdx] == 1)
                {
                    payloadPedalConfig payloadPedalConfig_fromJson_st = dap_config_st[pedalIdx].payloadPedalConfig_;
                    // Read the entire JSON file
                    string jsonString = File.ReadAllText(Plugin.Settings.Pedal_file_string[profile_index, pedalIdx]);
                    // Parse all of the JSON.
                    //JsonNode forecastNode = JsonNode.Parse(jsonString);
                    dynamic data = JsonConvert.DeserializeObject(jsonString);
                    //var s = default(payloadPedalConfig);
                    Object obj = payloadPedalConfig_fromJson_st;// s;
                    FieldInfo[] fi = payloadPedalConfig_fromJson_st.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    // Iterate over each field and print its name and value
                    foreach (var field in fi)
                    {

                        if (data["payloadPedalConfig_"][field.Name] != null)
                        //if (forecastNode["payloadPedalConfig_"][field.Name] != null)
                        {
                            try
                            {
                                if (field.FieldType == typeof(float))
                                {
                                    //float value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<float>();
                                    float value = (float)data["payloadPedalConfig_"][field.Name];
                                    field.SetValue(obj, value);
                                }

                                if (field.FieldType == typeof(byte))
                                {
                                    //byte value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<byte>();
                                    byte value = (byte)data["payloadPedalConfig_"][field.Name];
                                    field.SetValue(obj, value);
                                }

                            }
                            catch (Exception)
                            {

                            }

                        }
                    }

                    // set values in global structure
                    dap_config_st[pedalIdx].payloadPedalConfig_ = (payloadPedalConfig)obj;// payloadPedalConfig_fromJson_st;
                    if (dap_config_st[pedalIdx].payloadPedalConfig_.spindlePitch_mmPerRev_u8 == 0)
                    {
                        dap_config_st[pedalIdx].payloadPedalConfig_.spindlePitch_mmPerRev_u8 = 5;
                    }
                    if (dap_config_st[pedalIdx].payloadPedalConfig_.kf_modelNoise == 0)
                    {
                        dap_config_st[pedalIdx].payloadPedalConfig_.kf_modelNoise = 5;
                    }
                    dap_config_st[pedalIdx].payloadPedalConfig_.pedal_type = (byte)pedalIdx;

                }
                
            }
            


            updateTheGuiFromConfig();
        }
    

        

        
        private void OpenButton_Click(object sender, EventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Title = "Datei auswählen";
                openFileDialog.Filter = "Configdateien (*.json)|*.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                openFileDialog.InitialDirectory = currentDirectory + "\\PluginsData\\Common";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string content = (string)openFileDialog.FileName;
                    TextBox_debugOutput.Text = content;

                    string filePath = openFileDialog.FileName;


                    if (false)
                    {
                        string text1 = System.IO.File.ReadAllText(filePath);
                        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                        var ms = new MemoryStream(Encoding.UTF8.GetBytes(text1));
                        dap_config_st[indexOfSelectedPedal_u] = (DAP_config_st)deserializer.ReadObject(ms);
                    }
                    else
                    {
                        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization


                        // c# code to iterate over all fields of struct and set values from json file

                        // Read the entire JSON file
                        string jsonString = File.ReadAllText(filePath);

                        // Parse all of the JSON.
                        //JsonNode forecastNode = JsonNode.Parse(jsonString);
                        dynamic data = JsonConvert.DeserializeObject(jsonString);



                        payloadPedalConfig payloadPedalConfig_fromJson_st = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_;
                        //var s = default(payloadPedalConfig);
                        Object obj = payloadPedalConfig_fromJson_st;// s;



                        FieldInfo[] fi = payloadPedalConfig_fromJson_st.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                        // Iterate over each field and print its name and value
                        foreach (var field in fi)
                        {

                            if (data["payloadPedalConfig_"][field.Name] != null)
                            //if (forecastNode["payloadPedalConfig_"][field.Name] != null)
                            {
                                try
                                {
                                    if (field.FieldType == typeof(float))
                                    {
                                        //float value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<float>();
                                        float value = (float)data["payloadPedalConfig_"][field.Name];
                                        field.SetValue(obj, value);
                                    }

                                    if (field.FieldType == typeof(byte))
                                    {
                                        //byte value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<byte>();
                                        byte value = (byte)data["payloadPedalConfig_"][field.Name];
                                        field.SetValue(obj, value);
                                    }

                                    if (field.FieldType == typeof(Int16))
                                    {
                                        //byte value = forecastNode["payloadPedalConfig_"][field.Name].GetValue<byte>();
                                        Int16 value = (Int16)data["payloadPedalConfig_"][field.Name];
                                        field.SetValue(obj, value);
                                    }


                                }
                                catch (Exception)
                                {

                                }

                            }
                        }

                        // set values in global structure
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_ = (payloadPedalConfig)obj;// payloadPedalConfig_fromJson_st;
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.spindlePitch_mmPerRev_u8 == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.spindlePitch_mmPerRev_u8 = 5;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise = 5;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedal_type != indexOfSelectedPedal_u)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedal_type = (byte)indexOfSelectedPedal_u;

                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a==0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a = 205;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b = 220;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d = 60;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal = 215;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical = 60;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel == 0)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = 100;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition < 5)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition = 5;
                        }
                        if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition > 95)
                        {
                            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition = 95;
                        }
                    }

                    updateTheGuiFromConfig();
                    TextBox_debugOutput.Text = "Config new imported!";
                    TextBox2.Text = "Open " + openFileDialog.FileName;
                }
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.Title = "Datei speichern";
                saveFileDialog.Filter = "Textdateien (*.json)|*.json";
                string currentDirectory = Directory.GetCurrentDirectory();
                saveFileDialog.InitialDirectory = currentDirectory + "\\PluginsData\\Common";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                     string fileName = saveFileDialog.FileName;


                    this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;

                    // https://stackoverflow.com/questions/3275863/does-net-4-have-a-built-in-json-serializer-deserializer
                    // https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-serialize-and-deserialize-json-data?redirectedfrom=MSDN
                    var stream1 = new MemoryStream();
                    //var ser = new DataContractJsonSerializer(typeof(DAP_config_st));
                    //ser.WriteObject(stream1, dap_config_st[indexOfSelectedPedal_u]);


                    // formatted JSON see https://stackoverflow.com/a/38538454
                    var writer = JsonReaderWriterFactory.CreateJsonWriter(stream1, Encoding.UTF8, true, true, "  ");
                    var serializer = new DataContractJsonSerializer(typeof(DAP_config_st));
                    serializer.WriteObject(writer, dap_config_st[indexOfSelectedPedal_u]);
                    writer.Flush();

                    stream1.Position = 0;
                    StreamReader sr = new StreamReader(stream1);
                    string jsonString = sr.ReadToEnd();

                    // Check if file already exists. If yes, delete it.     
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }


                    System.IO.File.WriteAllText(fileName, jsonString);
                    TextBox_debugOutput.Text = "Config new exported!";
                    TextBox2.Text = "Save " + saveFileDialog.FileName;
                    }
            }
        }

        
        private void DisconnectToPedal_click(object sender, RoutedEventArgs e)
        {

            closeSerialAndStopReadCallback(indexOfSelectedPedal_u);
            Plugin.Settings.connect_flag[indexOfSelectedPedal_u] = 0;

            if (ConnectToPedal.IsChecked == true)
            {
                ConnectToPedal.IsChecked = false;
                TextBox_debugOutput.Text = "Serialport close";
                Plugin.Settings.connect_status[indexOfSelectedPedal_u] = 0;
            }           
            else
            {
                ConnectToPedal.IsChecked = false;
                TextBox_debugOutput.Text = "Not Checked Serialport close";
            }
            updateTheGuiFromConfig();

        }

        private void dump_pedal_response_to_file_checked(object sender, RoutedEventArgs e)
        {
            dumpPedalToResponseFile[indexOfSelectedPedal_u] = true;
        }

        private void dump_pedal_response_to_file_unchecked(object sender, RoutedEventArgs e)
        {
            dumpPedalToResponseFile[indexOfSelectedPedal_u] = false;
        }



        private void Simulate_ABS_check_Checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger = 1;
            TextBox_debugOutput.Text = "simulateABS: on";
            rect_SABS.Visibility = Visibility.Visible;
            rect_SABS_Control.Visibility = Visibility.Visible;
            text_SABS.Visibility = Visibility.Visible;

        }
        private void Simulate_ABS_check_Unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_trigger = 0;
            TextBox_debugOutput.Text = "simulateABS: off";
            rect_SABS.Visibility = Visibility.Hidden;
            rect_SABS_Control.Visibility = Visibility.Hidden;
            text_SABS.Visibility = Visibility.Hidden;

        }



        //dragable control rect.

        /*private void InitializeRectanglePositions()
        {
            rectanglePositions.Add("rect1", new Point(75, 75));
            rectanglePositions.Add("rect2", new Point(155, 55));
            rectanglePositions.Add("rect3", new Point(235, 35));
            rectanglePositions.Add("rect4", new Point(315, 15));
        }*/

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var rectangle = sender as Rectangle;
            offset = e.GetPosition(rectangle);
            rectangle.CaptureMouse();
            if(rectangle.Name != "rect_SABS_Control" & rectangle.Name != "rect_BP_Control")
            {
                var dropShadowEffect = new DropShadowEffect
                {
                    ShadowDepth = 0,
                    BlurRadius = 15,
                    Color = Colors.White,
                    Opacity = 1
                };
                rectangle.Fill = lightcolor;
                rectangle.Effect = dropShadowEffect;
            }

        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                //double x = e.GetPosition(canvas).X - offset.X;
                double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas
                //x = Math.Max(0, Math.Min(x, canvas.ActualWidth - rectangle.ActualWidth));
                y = Math.Max(-1*rectangle.Height/2, Math.Min(y, canvas.Height - rectangle.Height/2));

                //Canvas.SetLeft(rectangle, x);
                Canvas.SetTop(rectangle, y);
                double y_max = 100;
                double dx = canvas.Height / y_max;
                double y_actual = (canvas.Height - y -rectangle.Height/2)/dx;
                if (rectangle.Name == "rect0")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:0%";
                    text_point_pos.Text += "\nForce: "+(int)y_actual+"%";
                    
                }
                if (rectangle.Name == "rect1")
                {

                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:20%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect2")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:40%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect3")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:60%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect4")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:80%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                if (rectangle.Name == "rect5")
                {
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = Convert.ToByte(y_actual);
                    text_point_pos.Text = "Travel:100%";
                    text_point_pos.Text += "\nForce: " + (int)y_actual + "%";
                }
                text_point_pos.Visibility = Visibility.Visible; ;

                Update_BrakeForceCurve();



                // Update the position in the dictionary
                //rectanglePositions[rectangle.Name] = new Point(x, y);
            }
        }

       
        

        private void Rectangle_MouseMove_ABS(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                //double x = e.GetPosition(canvas).X - offset.X;
                double y = e.GetPosition(canvas).Y - offset.Y;

                // Ensure the rectangle stays within the canvas
                double dy = canvas.Height / 100;
                double min_posiiton = 5 * dy;
                double max_position = 50 * dy;
                //min position: 50%, max 95%
                //double dx = 100 / (canvas_horz_slider.Width - 10);
                y = Math.Max(min_posiiton, Math.Min(y, max_position));
                //Canvas.SetTop(rect_SABS, y);
                rect_SABS.Height = y;
                double actual_y = (canvas.Height -y)/dy;
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value = Convert.ToByte(actual_y);
                TextBox_debugOutput.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value+"%";
                text_SABS.Text = "ABS trigger value: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Simulate_ABS_value + "%";
                Canvas.SetTop(text_SABS, y - rect_SABS_Control.Height-text_SABS.Height);
                Canvas.SetTop(rectangle, y-rect_SABS_Control.Height/2);

            }
        }
        private void Rectangle_MouseMove_sigle_slider_H(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                //Bite point control
                if (rectangle.Name == "rect_BP_Control")
                {
                    // Ensure the rectangle stays within the canvas
                    double x = e.GetPosition(canvas).X - offset.X;
                    double BP_max = 100;
                    double dx = (canvas.Width) / BP_max;
                    double min_position = 10 * dx - rect_BP_Control.Width / 2;
                    double max_position = (BP_max - 10) * dx - rect_BP_Control.Width / 2;

                    x = Math.Max(min_position, Math.Min(x, max_position));
                    double actual_x = (x + rect_BP_Control.Width / 2) / dx;
                    dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value = (byte)(actual_x);

                    text_BP.Text = "Bite Point:\n" + ((float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger_value) + "%";
                    Canvas.SetLeft(rectangle, x);
                    Canvas.SetLeft(text_BP, Canvas.GetLeft(rect_BP_Control) + rect_BP_Control.Width + 3);
                    Canvas.SetTop(text_BP, canvas.Height - text_BP.Height-15);

                }
            }
        }
        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                isDragging = false;
                rectangle.ReleaseMouseCapture();
                text_point_pos.Visibility = Visibility.Hidden;
                //SolidColorBrush buttonBackground = btn_update.Background as SolidColorBrush;
                //Color color = Color.FromArgb(150, buttonBackground.Color.R, buttonBackground.Color.G, buttonBackground.Color.B);
                //rectangle.Fill = btn_update.Background;
                if (rectangle.Name != "rect_SABS_Control" & rectangle.Name != "rect_BP_Control")
                {
                    var dropShadowEffect = new DropShadowEffect
                    {
                        ShadowDepth = 0,
                        BlurRadius = 20,
                        Color = Colors.White,
                        Opacity = 0
                    };
                    rectangle.Fill = defaultcolor;
                    rectangle.Effect = dropShadowEffect;
                }

                //rectangle.Fill = new SolidColorBrush(color);
            }
        }
        private void Debug_checkbox_Checked(object sender, RoutedEventArgs e)
        {

            text_debug_flag.Visibility = Visibility.Visible; 
            text_serial.Visibility = Visibility.Visible;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Visible;
            textBox_debug_Flag_0.Visibility = Visibility.Visible;
            //btn_serial.Visibility = System.Windows.Visibility.Visible;
            btn_system_id.Visibility = System.Windows.Visibility.Visible;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Visible;
            button_pedal_restart.Visibility = System.Windows.Visibility.Visible;
            btn_reset_default.Visibility = System.Windows.Visibility.Visible;
            dump_pedal_response_to_file.Visibility = System.Windows.Visibility.Visible;
            //InvertLoadcellReading_check.Visibility = Visibility.Visible;
            //InvertMotorDir_check.Visibility = Visibility.Visible;
            //text_state.Visibility = Visibility.Hidden;
            debug_flag = true;
            Border_serial_monitor.Visibility = Visibility.Visible;
            Advanced_Tab.Visibility = Visibility.Visible;
            
           // Label_reverse_LC.Visibility = Visibility.Visible;
            //Label_reverse_servo.Visibility = Visibility.Visible;
            btn_test.Visibility = Visibility.Visible;
            Line_H_HeaderTab.X2 = 1088;
            CustomEffectTab.Visibility = Visibility.Visible;
            Slider_LC_rate.TickFrequency = 1;

        }
        private void Debug_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            text_debug_flag.Visibility = Visibility.Hidden; ;
            text_serial.Visibility = Visibility.Hidden;
            TextBox_serialMonitor.Visibility = System.Windows.Visibility.Hidden;
            textBox_debug_Flag_0.Visibility = Visibility.Hidden;
            //btn_serial.Visibility = System.Windows.Visibility.Hidden;
            btn_system_id.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_position_reset.Visibility = System.Windows.Visibility.Hidden;
            button_pedal_restart.Visibility = System.Windows.Visibility.Hidden;
            btn_reset_default.Visibility = System.Windows.Visibility.Hidden;
            dump_pedal_response_to_file.Visibility = System.Windows.Visibility.Hidden;
            //InvertLoadcellReading_check.Visibility = Visibility.Hidden;
            //InvertMotorDir_check.Visibility = Visibility.Hidden;
            //text_state.Visibility = Visibility.Visible;
            debug_flag = false;
            Border_serial_monitor.Visibility = Visibility.Hidden;
            Advanced_Tab.Visibility = Visibility.Hidden;
            
            //Label_reverse_LC.Visibility = Visibility.Hidden;
            //Label_reverse_servo.Visibility = Visibility.Hidden;
            btn_test.Visibility = Visibility.Hidden;
            Line_H_HeaderTab.X2 = 723;
            CustomEffectTab.Visibility = Visibility.Hidden;
            Slider_LC_rate.TickFrequency = 10;
        }


        private void JoystickOutput_checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 = 1;

        }
        private void JoystickOutput_unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.travelAsJoystickOutput_u8 = 0;
        }


        private void InvertLoadcellReading_checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 = 1;
        }
        private void InvertLoadcellReading_unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertLoadcellReading_u8 = 0;
        }


        private void InvertMotorDir_checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertMotorDirection_u8 = 1;
        }
        private void InvertMotorDir_unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.invertMotorDirection_u8 = 0;
        }



        private void CheckBox_Reading_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.reading_config = 1;
        }
        private void CheckBox_Reading_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.reading_config = 0;
        }

        private void checkbox_auto_connect_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.auto_connect_flag[indexOfSelectedPedal_u] = 1;
        }

        private void checkbox_auto_connect_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.auto_connect_flag[indexOfSelectedPedal_u] = 0 ;
        }

        private void checkbox_enable_ABS_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] = 1;
            checkbox_enable_ABS.Content = "ABS/TC Effect Enabled";
        }
        private void checkbox_enable_ABS_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.ABS_enable_flag[indexOfSelectedPedal_u] = 0;
            checkbox_enable_ABS.Content = "ABS/TC Effect Disabled";
        }

        private void checkbox_enable_RPM_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] = 1;
            checkbox_enable_RPM.Content = "Effect Enabled";
        }

        private void checkbox_enable_RPM_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RPM_enable_flag[indexOfSelectedPedal_u] = 0;
            checkbox_enable_RPM.Content = "Effect Disabled";
        }

        private void Vjoy_out_check_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_output_flag = 1;
            ////// vJoy c# wrapper, see https://github.com/bobhelander/vJoy.Wrapper
            ////uint vJoystickId = Plugin.Settings.vjoy_order;
            ////joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
            ////joystick.Aquire();
            ////vjoy_axis_initialize();

            uint vJoystickId = Plugin.Settings.vjoy_order;
            //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
            joystick = new vJoyInterfaceWrap.vJoy();

            joystick.AcquireVJD(vJoystickId);
            //joystick.Aquire();
            vjoy_axis_initialize();
            //CheckBox_rudder.IsEnabled = true;

        }


        private void Vjoy_out_check_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_output_flag = 0;
            //joystick.Release();
            joystick.RelinquishVJD(Plugin.Settings.vjoy_order);
            //CheckBox_rudder.IsEnabled = false;
        }


        public void EffectAppliedOnForceOrTravel_combobox_changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit = (byte)EffectAppliedOnForceOrTravel_combobox.SelectedIndex;

                if (label_ABS_AMP != null)
                {
                    switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
                    {
                        case 0:
                            label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "kg";
                            break;
                        case 1:
                            label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "%";
                            break;
                        default:
                            break;
                    }
                }
                

            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }

        }

        private void vjoy_plus_click(object sender, RoutedEventArgs e)
        {
            // release old joystick
            joystick.RelinquishVJD(Plugin.Settings.vjoy_order);

            Plugin.Settings.vjoy_order += 1;
            uint max = 16;
            uint min = 1;
            Plugin.Settings.vjoy_order = Math.Max(min, Math.Min(Plugin.Settings.vjoy_order, max));
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                //joystick.Release();
                
                //VjdStat status;
                VjdStat status = joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                //status = joystick.Joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        TextBox_debugOutput.Text = "vjoy already aquaried";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        break;
                    case VjdStat.VJD_STAT_FREE:

                        TextBox_debugOutput.Text = "vjoy aquaried";
                        //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                        //joystick.Aquire();
                        joystick.AcquireVJD(Plugin.Settings.vjoy_order);
                        if (Vjoy_out_check.IsChecked == false)
                        {
                            Vjoy_out_check.IsChecked = true;
                        }
                        //Console.WriteLine("vJoy Device {0} is free\n", id);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        TextBox_debugOutput.Text = "vjoy was aquaried by other program";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        TextBox_debugOutput.Text = "the selected vjoy device not enabled";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                        return;
                    default:
                        TextBox_debugOutput.Text = "vjoy device error";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                        return;
                };
            }
            

        }

        private void vjoy_minus_click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.vjoy_order -= 1;
            uint max = 16;
            uint min = 1;
            Plugin.Settings.vjoy_order = Math.Max(min, Math.Min(Plugin.Settings.vjoy_order, max));
            Label_vjoy_order.Content = Plugin.Settings.vjoy_order;
            if (Plugin.Settings.vjoy_output_flag == 1)
            {
                //joystick.Release();
                joystick.RelinquishVJD(Plugin.Settings.vjoy_order);
                VjdStat status;
                status = joystick.GetVJDStatus(Plugin.Settings.vjoy_order);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        TextBox_debugOutput.Text = "vjoy already aquaried";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        break;
                    case VjdStat.VJD_STAT_FREE:

                        TextBox_debugOutput.Text = "vjoy aquaried";
                        //joystick = new VirtualJoystick(Plugin.Settings.vjoy_order);
                        joystick.AcquireVJD(Plugin.Settings.vjoy_order);
                        //joystick.Aquire();
                        if (Vjoy_out_check.IsChecked == false)
                        {
                            Vjoy_out_check.IsChecked = true;
                        }
                        //Console.WriteLine("vJoy Device {0} is free\n", id);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        TextBox_debugOutput.Text = "vjoy was aquaried by other program";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                        return;
                    case VjdStat.VJD_STAT_MISS:
                        TextBox_debugOutput.Text = "the selected vjoy device not enabled";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                        return;
                    default:
                        TextBox_debugOutput.Text = "vjoy device error";
                        Plugin.Settings.vjoy_output_flag = 0;
                        Vjoy_out_check.IsChecked = false;
                        //Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                        return;
                };
            }




        }
        private void checkbox_enable_bite_point_Checked(object sender, RoutedEventArgs e)
        {

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger = 1;
            text_BP.Visibility = Visibility.Visible;
            rect_BP_Control.Visibility = Visibility.Visible;
            checkbox_enable_bite_point.Content = "Bite Point Vibration Enabled";


        }

        private void checkbox_enable_bite_point_Unchecked(object sender, RoutedEventArgs e)
        {

            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_trigger = 0;
            text_BP.Visibility = Visibility.Hidden;
            rect_BP_Control.Visibility = Visibility.Hidden;
            checkbox_enable_bite_point.Content = "Bite Point Vibration Disabled";


        }

        private void btn_reset_default_Click(object sender, RoutedEventArgs e)
        {
            DAP_config_set_default(indexOfSelectedPedal_u);
            updateTheGuiFromConfig();
        }

        private void checkbox_enable_G_force_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.G_force_enable_flag[indexOfSelectedPedal_u] = 0;
            checkbox_enable_G_force.Content = "G Force Effect Disabled";


        }
        private void checkbox_enable_G_force_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.G_force_enable_flag[indexOfSelectedPedal_u] = 1;
            checkbox_enable_G_force.Content = "G Force Effect Enabled";


        }

        // for ocntrol strategy
        private void StrategySel(object sender, RoutedEventArgs e)
        {
            if (ControlStrategy_Sel_1.IsChecked == true)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b = 0;
            }

            if (ControlStrategy_Sel_2.IsChecked == true)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b = 1;
            }

            if (ControlStrategy_Sel_3.IsChecked == true)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.control_strategy_b = 2;
            }

        }
        // RPM effect select
        private void RPMeffecttype_Sel_1_Checked(object sender, RoutedEventArgs e)
        {
            if (RPMeffecttype_Sel_1.IsChecked == true)
            {
                Plugin.Settings.RPM_effect_type = 0;
                //TextBox_debugOutput.Text = "" + Plugin.Settings.RPM_effect_type;
            }
            if (RPMeffecttype_Sel_2.IsChecked == true)
            {
                Plugin.Settings.RPM_effect_type = 1;
                //TextBox_debugOutput.Text = "" + Plugin.Settings.RPM_effect_type;
            }
        }

        private void TabControl_file_path(object sender, SelectionChangedEventArgs e)
        {
            profile_select = (uint)ProfileTab.SelectedIndex;
            Plugin.profile_index = profile_select;
            //Profile_change(profile_select);
            //Plugin.Settings.table_selected = (uint)MyTab.SelectedIndex;
            // update the sliders & serial port selection accordingly
            updateTheGuiFromConfig();
        }

        private void file_check_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as System.Windows.Controls.CheckBox;
            if (checkbox.Name == "Clutch_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 0] = 1;
            }

            if (checkbox.Name == "Brake_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 1] = 1;
            }

            if (checkbox.Name == "Gas_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 2] = 1;
            }
        }

        private void file_check_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as System.Windows.Controls.CheckBox;
            if (checkbox.Name == "Clutch_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 0] = 0;
            }

            if (checkbox.Name == "Brake_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 1] = 0;
            }

            if (checkbox.Name == "Gas_file_check")
            {
                Plugin.Settings.file_enable_check[profile_select, 2] = 0;
            }
        }

        public void Profile_change(uint profile_index)
        {
            profile_select = profile_index;
            ProfileTab.SelectedIndex = (int)profile_index;
            //if (Plugin.Settings.file_enable_check[profile_select])
            Parsefile(profile_index);
            string tmp;
            switch (profile_index)
            {
                case 0:
                    tmp = "A:" +Plugin.Settings.Profile_name[profile_index];
                    break;
                case 1:
                    tmp = "B:" + Plugin.Settings.Profile_name[profile_index];
                    break;
                case 2:
                    tmp = "C:" + Plugin.Settings.Profile_name[profile_index];
                    break;
                case 3:
                    tmp = "D:" + Plugin.Settings.Profile_name[profile_index];
                    break;
                case 4:
                    tmp = "E:" + Plugin.Settings.Profile_name[profile_index];
                    break;
                case 5:
                    tmp = "F:" + Plugin.Settings.Profile_name[profile_index];
                    break;
                default:
                    tmp = "No Profile";
                    break;
            }
            Plugin.current_profile = tmp;
        }

        private void effect_bind_click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.WSeffect_bind = (string)textBox_wheelslip_effect_string.Text;
            Plugin.Settings.WS_enable_flag[indexOfSelectedPedal_u] = 1;
            updateTheGuiFromConfig();
        }
        private void effect_clear_click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.WSeffect_bind = "";
            textBox_wheelslip_effect_string.Text = "";
            Plugin.Settings.WS_enable_flag[indexOfSelectedPedal_u] = 0;
            updateTheGuiFromConfig();
        }

        private void checkbox_enable_WS_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.WS_enable_flag[indexOfSelectedPedal_u] = 1;
            //checkbox_enable_RPM.Content = "Effect Enabled";
        }

        private void checkbox_enable_WS_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.WS_enable_flag[indexOfSelectedPedal_u] = 0;
            //checkbox_enable_RPM.Content = "Effect Disabled";
        }

        private void btn_apply_profile_Click(object sender, RoutedEventArgs e)
        {
            Profile_change((uint)ProfileTab.SelectedIndex);
            Parsefile((uint)ProfileTab.SelectedIndex);
        }

        private void btn_send_profile_Click(object sender, RoutedEventArgs e)
        {
            Sendconfigtopedal_shortcut();
        }

        private void textbox_profile_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as System.Windows.Controls.TextBox;
            Plugin.Settings.Profile_name[profile_select]= textbox.Text;
        }

        private void btn_toast_Click(object sender, RoutedEventArgs e)
        {
            ToastNotification("Rudder Brake","Test");
            Plugin.Rudder_brake_enable_flag = true;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // for .NET Core you need to add UseShellExecute = true
            // see https://learn.microsoft.com/dotnet/api/system.diagnostics.processstartinfo.useshellexecute#property-value
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        private void btn_scurve_Click(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = 7;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = 28;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = 70;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = 93;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = 100;
            Update_BrakeForceCurve();
            updateTheGuiFromConfig();
        }
        private void btn_10xcurve_Click(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = 43;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = 69;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = 85;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = 95;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = 100;
            Update_BrakeForceCurve();
            updateTheGuiFromConfig();
        }
        private void btn_logcurve_Click(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = 6;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = 17;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = 33;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = 59;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = 100;
            Update_BrakeForceCurve();
            updateTheGuiFromConfig();
        }
        private void btn_linearcurve_Click(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p000 = 0;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p020 = 20;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p040 = 40;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p060 = 60;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p080 = 80;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.relativeForce_p100 = 100;
            Update_BrakeForceCurve();
            updateTheGuiFromConfig();
        }

        private void OTA_update_check_Unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.OTA_flag = 0;
        }

        private void OTA_update_check_Checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.OTA_flag = 1;
        }

        private void EnableReboot_check_Unchecked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.enableReboot_u8 = 0;
        }

        private void EnableReboot_check_Checked(object sender, RoutedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.enableReboot_u8 = 1;
        }

        private void btn_serial_clear_Click(object sender, RoutedEventArgs e)
        {
            TextBox_serialMonitor.Clear();
        }

        private void checkbox_enable_impact_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.Road_impact_enable_flag[indexOfSelectedPedal_u] = 1;
        }

        private void checkbox_enable_impact_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.Road_impact_enable_flag[indexOfSelectedPedal_u] = 0;
        }

        private void Bind_Impacteffect_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.Road_impact_bind = (string)textBox_impact_effect_string.Text;
            Plugin.Settings.Road_impact_enable_flag[indexOfSelectedPedal_u] = 1;
            updateTheGuiFromConfig();
        }

        private void Clear_Impacteffect_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.Road_impact_bind = "";
            Plugin.Settings.Road_impact_enable_flag[indexOfSelectedPedal_u] = 0;
            updateTheGuiFromConfig();
        }



        private void rect_joint_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                double y = e.GetPosition(canvas_kinematic).Y - offset.Y;
                double x = e.GetPosition(canvas_kinematic).X - offset.X;
                //double y = e.GetPosition(canvas).Y - offset.Y;


                /*
                if (rectangle.Name == "rect8")
                {
                    // Ensure the rectangle stays within the canvas
                    double dy = 250 / (canvas_vert_slider.Height);
                    double min_position = Canvas.GetTop(rect8) - rectangle.Height / 2;
                    double max_position = Canvas.GetTop(rect9) + rectangle.Height / 2;
                    double min_limit = canvas_vert_slider.Height - 0 / dy;
                    double max_limit = canvas_vert_slider.Height - 250 / dy;
                }
                */
            }
        }

        private void Pedal_joint_draw()
        {
            //A= kinematic joint C
            //B= Kinematic joint A
            //C= Kinematic joint B
            //O=O
            //D=D

            Label_kinematic_b_canvas.Text = ""+dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            Label_kinematic_c_hort_canvas.Text = ""+dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            Label_kinematic_c_vert_canvas.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            Label_kinematic_a_canvas.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            Label_kinematic_d_canvas.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d;
            Label_travel_canvas.Text = "" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel;
            Label_kinematic_scale.Content = Math.Round(Plugin.Settings.kinematicDiagram_zeroPos_scale,1);
            
            //parameter calculation
            double OA_length = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB_length = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC_length = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double Travel_length = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel;
            double CA_length = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            double OD_length = OA_length + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d;
            double Current_travel_position;
            double OC_length;
            double pedal_angle_1;
            double pedal_angle_2;
            double pedal_angle;
            if (OA_length != 0 && OB_length != 0 && BC_length != 0 && CA_length != 0)
            {

                Current_travel_position = Travel_length / 100 * (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition - dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition) / 100 * current_pedal_travel_state + Travel_length / 100 * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                OC_length = Math.Sqrt((OB_length + Current_travel_position) * (OB_length + Current_travel_position) + BC_length * BC_length);
                pedal_angle_1 = Math.Acos((OA_length * OA_length + OC_length * OC_length - CA_length * CA_length) / (2 * OA_length * OC_length));
                pedal_angle_2 = Math.Atan2(BC_length, (OB_length + Current_travel_position));
                pedal_angle = pedal_angle_1 + pedal_angle_2;
                // only draw when pedal kinematic tab is selected
                if (pedalKinematicTab.IsSelected == false)
                {
                    return;
                }
            }
            else 
            {
                OA_length = 220;
                OB_length = 215;
                BC_length = 60;
                CA_length = 220;
                Travel_length = 60;
                Current_travel_position = Travel_length / 100 * (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition - dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition) / 100 * current_pedal_travel_state + Travel_length / 100 * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition;
                OC_length = Math.Sqrt((OB_length + Current_travel_position) * (OB_length + Current_travel_position) + BC_length * BC_length);
                pedal_angle_1 = Math.Acos((OA_length * OA_length + OC_length * OC_length - CA_length * CA_length) / (2 * OA_length * OC_length));
                pedal_angle_2 = Math.Atan2(BC_length, (OB_length + Current_travel_position));
                pedal_angle = pedal_angle_1 + pedal_angle_2;
            }
            double OB_Max = OB_length + Travel_length;
            double OC_Max = Math.Sqrt((OB_Max) * (OB_Max) + BC_length * BC_length);
            double min_angle_1= Math.Acos((OA_length * OA_length + OC_Max * OC_Max - CA_length * CA_length) / (2 * OA_length * OC_Max));
            double min_angle_2= Math.Atan2(BC_length, OB_Max);
            double OC_Min = Math.Sqrt((OB_length) * (OB_length) + BC_length * BC_length);
            double max_angle_1= Math.Acos((OA_length * OA_length + OC_Min * OC_Min - CA_length * CA_length) / (2 * OA_length * OC_Min));
            double max_angle_2= Math.Atan2(BC_length, OB_length);
            Label_kinematic_pedal_angle.Content = "Current Pedal Angle: " + Math.Round(pedal_angle/Math.PI*180)+ "°,";
            Label_kinematic_pedal_angle.Content = Label_kinematic_pedal_angle.Content + " Max Pedal Angle:" + Math.Round((max_angle_1+max_angle_2) / Math.PI * 180) + "°,";
            Label_kinematic_pedal_angle.Content = Label_kinematic_pedal_angle.Content + " Min Pedal Angle:" + Math.Round((min_angle_1 + min_angle_2) / Math.PI * 180) + "°,";
            Label_kinematic_pedal_angle.Content = Label_kinematic_pedal_angle.Content + " Angle Travel:" + Math.Round((max_angle_1 + max_angle_2-min_angle_1-min_angle_2) / Math.PI * 180) + "°";


            double A_X = OA_length * Math.Cos(pedal_angle);
            double A_Y = OA_length * Math.Sin(pedal_angle);
            double D_X = OD_length * Math.Cos(pedal_angle);
            double D_Y = OD_length * Math.Sin(pedal_angle);
            double scale_factor = Plugin.Settings.kinematicDiagram_zeroPos_scale;
            double shifting_OX = Plugin.Settings.kinematicDiagram_zeroPos_OX;
            double shifting_OY = Plugin.Settings.kinematicDiagram_zeroPos_OY;
            //set rect position
            Canvas.SetLeft(rect_joint_O, shifting_OX - rect_joint_O.Width / 2);
            Canvas.SetTop(rect_joint_O, canvas_kinematic.Height - shifting_OY - rect_joint_O.Height / 2);
            Canvas.SetLeft(rect_joint_C, A_X / scale_factor - rect_joint_C.Width / 2 + shifting_OX);
            Canvas.SetTop(rect_joint_C, canvas_kinematic.Height - A_Y / scale_factor - rect_joint_C.Height / 2 - shifting_OY);
            Canvas.SetLeft(rect_joint_A, OB_length / scale_factor - rect_joint_A.Width / 2 + shifting_OX);
            Canvas.SetTop(rect_joint_A, canvas_kinematic.Height - 0 / scale_factor - rect_joint_A.Height / 2 - shifting_OY);
            Canvas.SetLeft(rect_joint_B, OB_length / scale_factor - rect_joint_B.Width / 2 + Current_travel_position / scale_factor + shifting_OX);
            Canvas.SetTop(rect_joint_B, canvas_kinematic.Height - BC_length / scale_factor - rect_joint_B.Height / 2 - shifting_OY);
            Canvas.SetLeft(rect_joint_D, D_X / scale_factor - rect_joint_A.Width / 2 + shifting_OX);
            Canvas.SetTop(rect_joint_D, canvas_kinematic.Height - D_Y / scale_factor - rect_joint_A.Height / 2 - shifting_OY);

            Canvas.SetLeft(Label_joint_C, Canvas.GetLeft(rect_joint_C) - Label_joint_C.Width);
            Canvas.SetTop(Label_joint_C, Canvas.GetTop(rect_joint_C));
            Canvas.SetLeft(Label_joint_A, Canvas.GetLeft(rect_joint_A)+rect_joint_A.Width/2-Label_joint_A.Width/2);
            Canvas.SetTop(Label_joint_A, Canvas.GetTop(rect_joint_A)-Label_joint_A.Height);
            Canvas.SetLeft(Label_joint_B, Canvas.GetLeft(rect_joint_B) + Label_joint_B.Width);
            Canvas.SetTop(Label_joint_B, Canvas.GetTop(rect_joint_B));
            Canvas.SetLeft(Label_joint_D, Canvas.GetLeft(rect_joint_D) - Label_joint_D.Width);
            Canvas.SetTop(Label_joint_D, Canvas.GetTop(rect_joint_D));
            Canvas.SetLeft(Label_joint_O, Canvas.GetLeft(rect_joint_O) - Label_joint_O.Width);
            Canvas.SetTop(Label_joint_O, Canvas.GetTop(rect_joint_O));

            Canvas.SetLeft(SP_kinematic_b_canvas, (Canvas.GetLeft(rect_joint_C) + shifting_OX) / 2 - SP_kinematic_b_canvas.Width / 2 - Label_kinematic_b_canvas.Width / 2);
            Canvas.SetTop(SP_kinematic_b_canvas, (Canvas.GetTop(rect_joint_C) + canvas_kinematic.Height - shifting_OY) / 2 - Label_kinematic_b_canvas.Height / 2);
            Canvas.SetLeft(SP_kinematic_c_hort_canvas, (Canvas.GetLeft(rect_joint_A) + shifting_OX) / 2 - SP_kinematic_c_hort_canvas.Width / 2 -5);
            Canvas.SetTop(SP_kinematic_c_hort_canvas, (Canvas.GetTop(rect_joint_A) + canvas_kinematic.Height - shifting_OY) / 2 + Label_kinematic_c_hort_canvas.Height / 2 -5);
            Canvas.SetLeft(SP_kinematic_c_vert_canvas, Canvas.GetLeft(rect_joint_B) - rect_joint_B.Width - SP_kinematic_c_vert_canvas.Width / 2 + Label_kinematic_c_vert_canvas.Width);
            Canvas.SetTop(SP_kinematic_c_vert_canvas, (Canvas.GetTop(rect_joint_A) + Canvas.GetTop(rect_joint_B)) / 2 - Label_kinematic_c_vert_canvas.Height / 2 +5);
            Canvas.SetLeft(SP_kinematic_a_canvas, (Canvas.GetLeft(rect_joint_A) + Canvas.GetLeft(rect_joint_C)) / 2 - SP_kinematic_a_canvas.Width / 2 + Label_kinematic_a_canvas.Width / 2);
            Canvas.SetTop(SP_kinematic_a_canvas, (Canvas.GetTop(rect_joint_A) + Canvas.GetTop(rect_joint_C)) / 2 - Label_kinematic_a_canvas.Height);
            Canvas.SetLeft(SP_kinematic_d_canvas, (Canvas.GetLeft(rect_joint_C) + Canvas.GetLeft(rect_joint_D)) / 2 - SP_kinematic_d_canvas.Width / 2 - Label_kinematic_d_canvas.Width / 2);
            Canvas.SetTop(SP_kinematic_d_canvas, (Canvas.GetTop(rect_joint_C) + +Canvas.GetTop(rect_joint_D)) / 2 - Label_kinematic_d_canvas.Height / 2 );
            Canvas.SetLeft(SP_travel_canvas, (Canvas.GetLeft(rect_joint_A) + (OB_length + Travel_length) / scale_factor + shifting_OX) / 2 - SP_travel_canvas.Width / 2);
            Canvas.SetTop(SP_travel_canvas, (Canvas.GetTop(rect_joint_A) + canvas_kinematic.Height - shifting_OY) / 2 + Label_travel_canvas.Height / 2 -5);

            this.Line_kinematic_b.X1 = shifting_OX;
            this.Line_kinematic_b.Y1 = canvas_kinematic.Height - shifting_OY;
            this.Line_kinematic_b.X2 = A_X / scale_factor + shifting_OX;
            this.Line_kinematic_b.Y2 = canvas_kinematic.Height - A_Y / scale_factor - shifting_OY;

            this.Line_kinematic_c_hort.X1 = shifting_OX;
            this.Line_kinematic_c_hort.Y1 = canvas_kinematic.Height - shifting_OY;
            this.Line_kinematic_c_hort.X2 = OB_length / scale_factor + shifting_OX;
            this.Line_kinematic_c_hort.Y2 = canvas_kinematic.Height - shifting_OY;

            this.Line_kinematic_c_vert.X1 = (OB_length + Current_travel_position) / scale_factor + shifting_OX;
            this.Line_kinematic_c_vert.Y1 = canvas_kinematic.Height - shifting_OY;
            this.Line_kinematic_c_vert.X2 = (OB_length + Current_travel_position) / scale_factor + shifting_OX;
            this.Line_kinematic_c_vert.Y2 = canvas_kinematic.Height - BC_length / scale_factor - shifting_OY;

            this.Line_kinematic_a.X1 = (OB_length + Current_travel_position) / scale_factor + shifting_OX;
            this.Line_kinematic_a.Y1 = canvas_kinematic.Height - BC_length / scale_factor - shifting_OY;
            this.Line_kinematic_a.X2 = A_X / scale_factor + shifting_OX;
            this.Line_kinematic_a.Y2 = canvas_kinematic.Height - A_Y / scale_factor - shifting_OY;

            this.Line_kinematic_d.X1 = A_X / scale_factor + shifting_OX;
            this.Line_kinematic_d.Y1 = canvas_kinematic.Height - A_Y / scale_factor - shifting_OY;
            this.Line_kinematic_d.X2 = D_X / scale_factor + shifting_OX;
            this.Line_kinematic_d.Y2 = canvas_kinematic.Height - D_Y / scale_factor - shifting_OY;

            this.Line_Pedal_Travel.X1 = OB_length / scale_factor + shifting_OX;
            this.Line_Pedal_Travel.Y1 = canvas_kinematic.Height - shifting_OY;
            this.Line_Pedal_Travel.X2 = (OB_length + Travel_length) / scale_factor + shifting_OX;
            this.Line_Pedal_Travel.Y2 = canvas_kinematic.Height - shifting_OY;









        }

        private bool Kinematic_check(double OA, double OB, double BC, double CA, double travel)
        {
            
            double OC= Math.Sqrt((OB+travel) * (OB + travel) + BC * BC);
            double pedal_angle_1 = Math.Acos((OA * OA + OC * OC - CA * CA) / (2 * OA * OC));
            double pedal_angle_2 = Math.Atan2(BC, (OB + travel));


            double pedal_angle = pedal_angle_1 + pedal_angle_2;
            if (pedal_angle_1 != double.NaN && pedal_angle_2 != double.NaN)
            {
                if (pedal_angle <= Math.PI * 0.6)
                {
                    if ((OA + CA) > OC)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            

            
        }

        private void btn_plus_OA_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA + 1, OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_minus_OA_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA -1 , OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_plus_OB_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA , OB+1, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_minus_OB_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA , OB-1, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_plus_BC_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA, OB, BC+1, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_minus_BC_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA, OB, BC-1, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_plus_CA_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA, OB, BC, CA+1, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_minus_CA_Click(object sender, RoutedEventArgs e)
        {
            double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
            double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
            double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
            double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
            if (Kinematic_check(OA, OB, BC, CA-1, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_plus_AD_Click(object sender, RoutedEventArgs e)
        {
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d > 1)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }

        }

        private void btn_minus_AD_Click(object sender, RoutedEventArgs e)
        {
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d > 2)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
            }
        }

        private void btn_plus_travel_Click(object sender, RoutedEventArgs e)
        {
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel <= 100)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel + 1);
                updateTheGuiFromConfig();
            }
            else
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = 100;
            }

        }

        private void btn_minus_travel_Click(object sender, RoutedEventArgs e)
        {
            if (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel >= 30)
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = (Int16)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel - 1);
                updateTheGuiFromConfig();
            }
            else
            {
                dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = 30;
            }

        }


        private void SP_canvas_MouseEnter(object sender, MouseEventArgs e)
        {
            btn_plus_kinematic_b_canvas.Visibility = Visibility.Visible;
            btn_minus_kinematic_b_canvas.Visibility = Visibility.Visible;
            btn_plus_kinematic_c_hort_canvas.Visibility = Visibility.Visible;
            btn_minus_kinematic_c_hort_canvas.Visibility = Visibility.Visible;
            btn_plus_kinematic_c_vert_canvas.Visibility = Visibility.Visible;
            btn_minus_kinematic_c_vert_canvas.Visibility = Visibility.Visible;
            btn_plus_kinematic_a_canvas.Visibility = Visibility.Visible;
            btn_minus_kinematic_a_canvas.Visibility = Visibility.Visible;
            btn_plus_kinematic_d_canvas.Visibility = Visibility.Visible;
            btn_minus_kinematic_d_canvas.Visibility = Visibility.Visible;
            btn_plus_travel_canvas.Visibility = Visibility.Visible;
            btn_minus_travel_canvas.Visibility = Visibility.Visible;
        }
        private void SP_canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            btn_plus_kinematic_b_canvas.Visibility = Visibility.Hidden;
            btn_minus_kinematic_b_canvas.Visibility = Visibility.Hidden;
            btn_plus_kinematic_c_hort_canvas.Visibility = Visibility.Hidden;
            btn_minus_kinematic_c_hort_canvas.Visibility = Visibility.Hidden;
            btn_plus_kinematic_c_vert_canvas.Visibility = Visibility.Hidden;
            btn_minus_kinematic_c_vert_canvas.Visibility = Visibility.Hidden;
            btn_plus_kinematic_a_canvas.Visibility = Visibility.Hidden;
            btn_minus_kinematic_a_canvas.Visibility = Visibility.Hidden;
            btn_plus_kinematic_d_canvas.Visibility = Visibility.Hidden;
            btn_minus_kinematic_d_canvas.Visibility = Visibility.Hidden;
            btn_plus_travel_canvas.Visibility = Visibility.Hidden;
            btn_minus_travel_canvas.Visibility = Visibility.Hidden;
        }

        private void btn_minus_kinematic_scale_Click(object sender, RoutedEventArgs e)
        {
            if (Plugin.Settings.kinematicDiagram_zeroPos_scale > 0.7)
            {
                Plugin.Settings.kinematicDiagram_zeroPos_scale = Plugin.Settings.kinematicDiagram_zeroPos_scale - 0.1;
                DrawGridLines_kinematicCanvas(Plugin.Settings.kinematicDiagram_zeroPos_OX, Plugin.Settings.kinematicDiagram_zeroPos_OY, Plugin.Settings.kinematicDiagram_zeroPos_scale);
                Pedal_joint_draw();
                //Label_kinematic_scale.Content = Plugin.Settings.kinematicDiagram_zeroPos_scale;
            }
        }

        private void btn_plus_kinematic_scale_Click(object sender, RoutedEventArgs e)
        {
            if (Plugin.Settings.kinematicDiagram_zeroPos_scale < 2)
            {
                Plugin.Settings.kinematicDiagram_zeroPos_scale = Plugin.Settings.kinematicDiagram_zeroPos_scale + 0.1;
                DrawGridLines_kinematicCanvas(Plugin.Settings.kinematicDiagram_zeroPos_OX, Plugin.Settings.kinematicDiagram_zeroPos_OY, Plugin.Settings.kinematicDiagram_zeroPos_scale);
                Pedal_joint_draw();
                //Label_kinematic_scale.Content = Plugin.Settings.kinematicDiagram_zeroPos_scale;
            }
        }

        private void Kinematic_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as System.Windows.Controls.TextBox;
            if (textbox.Name == "Label_kinematic_b_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    double OA = result;
                    double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
                    double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
                    double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
                    if (Kinematic_check(OA, OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b = (Int16)(result);
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
            if (textbox.Name == "Label_kinematic_c_hort_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
                    double OB = result;
                    double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
                    double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
                    if (Kinematic_check(OA, OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal = (Int16)(result);
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
            if (textbox.Name == "Label_kinematic_c_vert_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
                    double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
                    double BC = result;
                    double CA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a;
                    if (Kinematic_check(OA, OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical = (Int16)(result);
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
            if (textbox.Name == "Label_kinematic_a_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    double OA = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_b;
                    double OB = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_horizontal;
                    double BC = dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_c_vertical;
                    double CA = result;
                    if (Kinematic_check(OA, OB, BC, CA, dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel))
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_a = (Int16)(result);
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
            if (textbox.Name == "Label_kinematic_d_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    if (result >= 0 && result <= 100)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_d = (Int16)result;
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
            if (textbox.Name == "Label_travel_canvas")
            {
                if (int.TryParse(textbox.Text, out int result))
                {
                    if (result >= 10 && result <= 100)
                    {
                        dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel = (Int16)result;
                        Pedal_joint_draw();
                    }
                    else
                    {
                        TextBox_debugOutput.Text = "Pedal Kinematic calculation error";
                    }
                }
            }
        }

        private void Tab_main_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateTheGuiFromConfig();
        }

        private void Slider_impact_smoothness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_window = (Byte)Slider_impact_smoothness.Value;
            label_impact_window.Content = "Impact Smoothness: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_window;


        }

        private void Slider_impact_multi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_multi = (Byte)e.NewValue;
            label_impact_multi.Content = "Impact Multiplier: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.Impact_multi+"%";
        }

        private void Slider_WS_freq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_freq = (Byte)e.NewValue;
            label_WS_freq.Content = "Notification Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_freq + "Hz";
            update_plot_WS();
        }

        private void Slider_WS_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_amp = (Byte)(e.NewValue*20);
            label_WS_AMP.Content = "Notification Amplitude: " + (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.WS_amp)/20.0f + "kg";
            update_plot_WS();
        }

        private void Slider_WS_trigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Plugin!= null)
            {
                Plugin.Settings.WS_trigger = (int)e.NewValue;
                label_WS_trigger.Content = "Notification Trigger: "+(Plugin.Settings.WS_trigger + 50) + "%";
            }

        }

        private void Slider_G_force_smoothness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_window=(Byte)e.NewValue;
            label_G_force_window.Content = "G Force Smoothness: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_window;

        }

        private void Slider_G_force_multi_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_multi=(Byte)e.NewValue;
            label_G_force_multi.Content = "G Force Multiplier: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.G_multi + "%";
        }

        private void Slider_BP_freq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq = (Byte)e.NewValue;
            label_BP_freq.Content = "Bite Point Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_freq + "Hz";
            update_plot_BP();
        }

        private void Slider_BP_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp = (Byte)(e.NewValue * 100);
            label_BP_AMP.Content = "Bite Point Amplitude: " + (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.BP_amp) / 100.0f + "kg";
            update_plot_BP();
        }

        private void Slider_RPM_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP = (Byte)(e.NewValue * 100);
            label_RPM_AMP.Content = "Effect Amplitude: " + (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_AMP) / 100.0f + "kg";
            update_plot_RPM();
        }

        private void Rangeslider_RPM_freq_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq = (byte)e.NewValue;
            label_RPM_freq_min.Content = "MIN:"+ dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_min_freq+"Hz";

            update_plot_RPM();
        }

        private void Rangeslider_RPM_freq_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq = (byte)e.NewValue;
            label_RPM_freq_max.Content = "MAX:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.RPM_max_freq + "Hz";
            update_plot_RPM();
        }

        private void Slider_ABS_freq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency = (Byte)(e.NewValue);
            label_ABS_freq.Content = "ABS/TC Frequency: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absFrequency + "Hz";
            update_plot_ABS();
        }

        private void Slider_ABS_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude = (Byte)(e.NewValue * 20);
            switch (dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absForceOrTarvelBit)
            {
                case 0:
                    label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "kg";
                    break;
                case 1:
                    label_ABS_AMP.Content = "ABS/TC Amplitude: " + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.absAmplitude / 20.0f + "%";
                    break;
                default:
                    break;
            }
            update_plot_ABS();
        }

        private void Rangeslider_travel_range_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition = (byte)e.NewValue;
            if (Plugin != null)
            {
                Label_min_pos.Content = "MIN\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition + "%\n" + Math.Round((float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalStartPosition) / 100) + "mm";         
            }
            

        }

        private void Rangeslider_travel_range_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition = (byte)e.NewValue;
            if (Plugin != null)
            { 
                Label_max_pos.Content = "MAX\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition + "%\n" + Math.Round((float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.lengthPedal_travel * dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedalEndPosition) / 100) + "mm";

            }
        }

        private void Rangeslider_force_range_UpperValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce = (byte)e.NewValue;
            if (Plugin != null)
            {
                Label_max_force.Content = "Max force:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxForce + "kg";
            }
        }

        private void Rangeslider_force_range_LowerValueChanged(object sender, RangeParameterChangedEventArgs e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce = (byte)e.NewValue;
            if (Plugin != null)
            {
                Label_min_force.Content = "Preload:\n" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.preloadForce + "kg";
            }
        }

        private void Slider_damping_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress = (Byte)e.NewValue;
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPull = (Byte)e.NewValue;
            label_damping.Content = "Damping factor: " + (float)(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.dampingPress * 0.00015f) + "s";
        }

        private void Slider_LC_rate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating= (Byte)(e.NewValue/2);
            label_LC_rate.Content = "Loadcell rate: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.loadcell_rating*2+"kg";
        }

        private void Slider_maxgame_output_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput = (Byte)(e.NewValue);
            label_maxgame_output.Content = "Max Game Output: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.maxGameOutput + "%";
        }

        private void Slider_KF_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise = (byte)e.NewValue;
            label_KF.Content = "KF: " + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.kf_modelNoise;
        }

        private void Slider_MPC_0th_gain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.MPC_0th_order_gain = (float)e.NewValue;
            label_MPC_0th_gain.Content = "Foot spring stiffness: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.MPC_0th_order_gain, 2)+ "kg/mm";
        }

        private void Slider_Pgain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain = (float)e.NewValue;
            label_Pgain.Content = "P-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_p_gain, 2);
        }

        private void Slider_Igain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain = (float)e.NewValue;
            label_Igain.Content = "I-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_i_gain, 2);
        }

        private void Slider_Dgain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain = (float)e.NewValue;
            label_Dgain.Content = "D-Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_d_gain, 4);
        }

        private void Slider_VFgain_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_velocity_feedforward_gain = (float)e.NewValue;
            label_VFgain.Content = "Feed Forward Gain: " + Math.Round(dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.PID_velocity_feedforward_gain, 1);
        }

        private void CheckBox_rudder_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Rudder_enable_flag = true;
            

        }

        private void CheckBox_rudder_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Rudder_enable_flag = true;
        }

        private void CheckBox_RTSDTR_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RTSDTR_False[indexOfSelectedPedal_u] = true;
        }

        private void CheckBox_RTSDTR_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.RTSDTR_False[indexOfSelectedPedal_u] = false;
        }

        private void Bind_CV1_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV1_bindings[indexOfSelectedPedal_u] = (string)textBox_CV1_string.Text;
            Plugin.Settings.CV1_enable_flag[indexOfSelectedPedal_u] = true;
            //updateTheGuiFromConfig();
        }

        private void checkbox_enable_CV_1_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV1_enable_flag[indexOfSelectedPedal_u] = true;
        }

        private void checkbox_enable_CV_1_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV1_enable_flag[indexOfSelectedPedal_u] = false;
        }

        private void Clear_CV1_Click(object sender, RoutedEventArgs e)
        {
            textBox_CV1_string.Text = "";
            Plugin.Settings.CV1_bindings[indexOfSelectedPedal_u] = (string)textBox_CV1_string.Text;
            Plugin.Settings.CV1_enable_flag[indexOfSelectedPedal_u] = false;
            //updateTheGuiFromConfig();
        }

        private void Slider_CV2_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_2 = (Byte)(e.NewValue*20);
            label_CV2_AMP.Content = "Effect Amplitude:" + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_2/20.0f + "kg";
        }

        private void Slider_CV1_AMP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_1 = (Byte)(e.NewValue*20);
            label_CV1_AMP.Content = "Effect Amplitude:" + (float)dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_amp_1/20.0f+"kg";
        }

        private void Slider_CV1_trigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Plugin.Settings.CV1_trigger[indexOfSelectedPedal_u]= (Byte)e.NewValue;
            label_CV1_trigger.Content = "Effect Trigger:" + Plugin.Settings.CV1_trigger[indexOfSelectedPedal_u];
        }

        private void Slider_CV1_freq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_1 = (Byte)e.NewValue;
            label_CV1_freq.Content = "Effect Frequency:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_1 + "Hz";
        }

        private void checkbox_enable_CV2_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV2_enable_flag[indexOfSelectedPedal_u] = true;
        }

        private void checkbox_enable_CV2_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV2_enable_flag[indexOfSelectedPedal_u] = false;
        }

        private void Bind_CV2_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.CV2_bindings[indexOfSelectedPedal_u] = (string)textBox_CV2_string.Text;
            Plugin.Settings.CV2_enable_flag[indexOfSelectedPedal_u] = true;
        }

        private void Clear_CV2_Click(object sender, RoutedEventArgs e)
        {
            textBox_CV2_string.Text = "";
            Plugin.Settings.CV2_bindings[indexOfSelectedPedal_u] = (string)textBox_CV2_string.Text;
            Plugin.Settings.CV2_enable_flag[indexOfSelectedPedal_u] = false;
        }

        private void Slider_CV2_trigger_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Plugin.Settings.CV2_trigger[indexOfSelectedPedal_u] = (Byte)e.NewValue;
            label_CV2_trigger.Content = "Effect Trigger:" + Plugin.Settings.CV2_trigger[indexOfSelectedPedal_u];
        }

        private void Slider_CV2_freq_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_2 = (Byte)e.NewValue;
            label_CV2_freq.Content = "Effect Frequency:" + dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.CV_freq_2 + "Hz";
        }

        private void btn_rudder_Click(object sender, RoutedEventArgs e)
        {
            if (Plugin.Rudder_status)
            {
                Plugin.Rudder_enable_flag = true;
                Plugin.Rudder_status = false;
                btn_rudder.Content = "Rudder On";
            }
            else
            {
                Plugin.Rudder_enable_flag = true;
                Plugin.Rudder_status = true;
                btn_rudder.Content = "Rudder Off";
            }
        }
        public void ESPNow_SerialPortSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tmp = (string)SerialPortSelection_ESPNow.SelectedValue;
            //Plugin._serialPort[indexOfSelectedPedal_u].PortName = tmp;


            //try 
            //{
            //    TextBox_debugOutput.Text = "Debug: " + Plugin.Settings.selectedComPortNames[indexOfSelectedPedal_u];
            //}
            //catch (Exception caughtEx)
            //{
            //    string errorMessage = caughtEx.Message;
            //    TextBox_debugOutput.Text = errorMessage;
            //}

            try
            {
                //if (Plugin.Settings.connect_status[indexOfSelectedPedal_u] == 0)
                if (Plugin.ESPsync_serialPort.IsOpen == false)
                {
                    Plugin.Settings.ESPNow_port = tmp;
                    Plugin.ESPsync_serialPort.PortName = tmp;
                }
                TextBox_debugOutput.Text = "COM port selected: " + Plugin.Settings.ESPNow_port;

            }
            catch (Exception caughtEx)
            {
                string errorMessage = caughtEx.Message;
                TextBox_debugOutput.Text = errorMessage;
            }



        }

        private void btn_connect_espnow_port_Click(object sender, RoutedEventArgs e)
        {
            if (Plugin.Sync_esp_connection_flag)
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    if (ESP_host_serial_timer != null)
                    {
                        ESP_host_serial_timer.Stop();
                        ESP_host_serial_timer.Dispose();
                    }
                    Plugin.ESPsync_serialPort.DiscardInBuffer();
                    Plugin.ESPsync_serialPort.DiscardOutBuffer();
                    Plugin.ESPsync_serialPort.Close();
                    Plugin.Sync_esp_connection_flag = false;
                    btn_connect_espnow_port.Content = "Connect";
                    SystemSounds.Beep.Play();
                    Plugin.Settings.Pedal_ESPNow_auto_connect_flag = false;
                    updateTheGuiFromConfig();
                }
            }
            else
            {
                try
                {
                    // serial port settings
                    Plugin.ESPsync_serialPort.Handshake = Handshake.None;
                    Plugin.ESPsync_serialPort.Parity = Parity.None;
                    //_serialPort[pedalIdx].StopBits = StopBits.None;


                    Plugin.ESPsync_serialPort.ReadTimeout = 2000;
                    Plugin.ESPsync_serialPort.WriteTimeout = 500;

                    // https://stackoverflow.com/questions/7178655/serialport-encoding-how-do-i-get-8-bit-ascii
                    Plugin.ESPsync_serialPort.Encoding = System.Text.Encoding.GetEncoding(28591);
                    Plugin.ESPsync_serialPort.NewLine = "\r\n";
                    Plugin.ESPsync_serialPort.ReadBufferSize = 10000;
                    if (Plugin.PortExists(Plugin.ESPsync_serialPort.PortName))
                    {
                        try
                        {
                            Plugin.ESPsync_serialPort.Open();
                            System.Threading.Thread.Sleep(200);
                            // ESP32 S3
                            Plugin.ESPsync_serialPort.RtsEnable = false;
                            Plugin.ESPsync_serialPort.DtrEnable = true;
                            SystemSounds.Beep.Play();
                            Plugin.Sync_esp_connection_flag = true;
                            btn_connect_espnow_port.Content = "Disconnect";
                            //Plugin.Settings.connect_status[3] = 1;
                            // read callback
                            /*
                            if (pedal_serial_read_timer[3] != null)
                            {
                                pedal_serial_read_timer[3].Stop();
                                pedal_serial_read_timer[3].Dispose();
                            }
                            */
                            ESP_host_serial_timer = new System.Windows.Forms.Timer();
                            ESP_host_serial_timer.Tick += new EventHandler(timerCallback_serial_esphost);
                            ESP_host_serial_timer.Tag = 3;
                            ESP_host_serial_timer.Interval = 7; // in miliseconds
                            ESP_host_serial_timer.Start();
                            System.Threading.Thread.Sleep(100);
                            if (Plugin.Settings.Pedal_ESPNow_auto_connect_flag)
                            {
                                Plugin.Settings.ESPNow_port = Plugin.ESPsync_serialPort.PortName;
                            }
                            
                            
                        }
                        catch (Exception ex)
                        {
                            TextBox2.Text = ex.Message;
                            //Serial_connect_status[3] = false;
                        }


                    }
                    else
                    {
                        //Plugin.Settings.connect_status[pedalIdx] = 0;
                        //Plugin.connectSerialPort[pedalIdx] = false;
                        //Serial_connect_status[pedalIdx] = false;

                    }
                }
                catch (Exception ex)
                {
                    TextBox2.Text = ex.Message;
                }
            }
            
        }



        private void CheckBox_Pedal_ESPNow_SyncFlag_Checked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            {
                Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u] = true;
            }
        }

        private void CheckBox_Pedal_ESPNow_SyncFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            {
                Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u] = false;
            }
        }


        unsafe public void timerCallback_serial_esphost(object sender, EventArgs e)
        {

            //action here 
            Simhub_action_update();




            //int pedalSelected = Int32.Parse((sender as System.Windows.Forms.Timer).Tag.ToString());
            //int pedalSelected = (int)(sender as System.Windows.Forms.Timer).Tag;

            bool pedalStateHasAlreadyBeenUpdated_b = false;

            // once the pedal has identified, go ahead
            //if (pedalSelected < 3)
            //if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
            {



                // Create a Stopwatch instance
                Stopwatch stopwatch = new Stopwatch();

                // Start the stopwatch
                stopwatch.Start();



                SerialPort sp = Plugin.ESPsync_serialPort;



                // https://stackoverflow.com/questions/9732709/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it


                //int length = sizeof(DAP_config_st);




                if (sp.IsOpen)
                {

                    if (Plugin.Settings.Serial_auto_clean)
                    {
                        if (TextBox_serialMonitor.LineCount > 100)
                        {
                            TextBox_serialMonitor.Clear();
                        }
                    }

                    int receivedLength = 0;
                    try
                    {
                        receivedLength = sp.BytesToRead;
                    }
                    catch (Exception ex)
                    {
                        TextBox_debugOutput.Text = ex.Message;
                        //ConnectToPedal.IsChecked = false;
                        return;
                    }



                    if (receivedLength > 0)
                    {

                        //TextBox_serialMonitor.Text += "Received:" + receivedLength + "\n";
                        //TextBox_serialMonitor.ScrollToEnd();


                        timeCntr[3] += 1;


                        // determine byte sequence which is defined as message end --> crlf
                        byte[] byteToFind = System.Text.Encoding.GetEncoding(28591).GetBytes(STOPCHAR[0].ToCharArray());
                        int stop_char_length = byteToFind.Length;


                        // calculate current buffer length
                        int currentBufferLength = appendedBufferOffset[3] + receivedLength;


                        // check if buffer is large enough otherwise discard in buffer and set offset to 0
                        if ((bufferSize > currentBufferLength) && (appendedBufferOffset[3] >= 0))
                        {
                            sp.Read(buffer_appended[3], appendedBufferOffset[3], receivedLength);
                        }
                        else
                        {
                            sp.DiscardInBuffer();
                            appendedBufferOffset[3] = 0;
                            return;
                        }





                        // copy to local buffer
                        //byte[] localBuffer = new byte[currentBufferLength];

                        //Buffer.BlockCopy(buffer_appended[pedalSelected], 0, localBuffer, 0, currentBufferLength);


                        // find all occurences of crlf as they indicate message end
                        List<int> indices = FindAllOccurrences(buffer_appended[3], byteToFind, currentBufferLength);




                        // Destination array
                        byte[] destinationArray = new byte[destBufferSize];







                        int srcBufferOffset = 0;
                        // decode every message
                        //foreach (int number in indices)
                        for (int msgId = 0; msgId < indices.Count; msgId++)
                        {
                            // computes the length of bytes to read
                            int destBuffLength = 0; //number - srcBufferOffset;

                            if (msgId == 0)
                            {
                                srcBufferOffset = 0;
                                destBuffLength = indices.ElementAt(msgId);
                            }
                            else
                            {
                                srcBufferOffset = indices.ElementAt(msgId - 1) + stop_char_length;
                                destBuffLength = indices.ElementAt(msgId) - srcBufferOffset;
                            }

                            // check if dest buffer length is within valid length
                            if ((destBuffLength <= 0) | (destBuffLength > destBufferSize))
                            {
                                continue;
                            }





                            // copy bytes to subarray
                            Buffer.BlockCopy(buffer_appended[3], srcBufferOffset, destinationArray, 0, destBuffLength);


                            // check for pedal state struct
                            if ((destBuffLength == sizeof(DAP_state_basic_st)))
                            {

                                // parse byte array as config struct
                                DAP_state_basic_st pedalState_read_st = getStateFromBytes(destinationArray);
                                
                                // check whether receive struct is plausible
                                DAP_state_basic_st* v_state = &pedalState_read_st;
                                byte* p_state = (byte*)v_state;
                                UInt16 pedalSelected= pedalState_read_st.payloadHeader_.PedalTag;
                                // payload type check
                                bool check_payload_state_b = false;
                                if (pedalState_read_st.payloadHeader_.payloadType == Constants.pedalStateBasicPayload_type)
                                {
                                    check_payload_state_b = true;
                                }
                                
                                // CRC check
                                bool check_crc_state_b = false;
                                if (Plugin.checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState_Basic)) == pedalState_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_state_b = true;
                                }

                                if ((check_payload_state_b) && check_crc_state_b)
                                { 

                                    // write vJoy data
                                    Pedal_position_reading[pedalSelected] = pedalState_read_st.payloadPedalBasicState_.joystickOutput_u16;
                                    // GUI update
                                    if (pedalState_read_st.payloadPedalBasicState_.error_code_u8 != 0)
                                    {
                                        Plugin.PedalErrorCode = pedalState_read_st.payloadPedalBasicState_.error_code_u8;
                                        Plugin.PedalErrorIndex = pedalState_read_st.payloadHeader_.PedalTag;
                                        TextBox2.Text = "Pedal:" + pedalState_read_st.payloadHeader_.PedalTag + " ErrorCode" + pedalState_read_st.payloadPedalBasicState_.error_code_u8;

                                    }
                                    if ((pedalStateHasAlreadyBeenUpdated_b == false) && (indexOfSelectedPedal_u == pedalSelected))
                                    {



                                        pedalStateHasAlreadyBeenUpdated_b = true;

                                        text_point_pos.Visibility = Visibility.Hidden;
                                        double control_rect_value_max = 65535;
                                        double dyy = canvas.Height / control_rect_value_max;
                                        double dxx = canvas.Width / control_rect_value_max;

                                        if (debug_flag)
                                        {
                                            Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 - rect_State.Width / 2);
                                            Canvas.SetTop(rect_State, canvas.Height - dyy * pedalState_read_st.payloadPedalBasicState_.pedalForce_u16 - rect_State.Height / 2);

                                            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) );
                                            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                            text_state.Text = Math.Round(pedalState_read_st.payloadPedalBasicState_.pedalForce_u16 / control_rect_value_max * 100) + "%";
                                            int round_x = (int)(100 * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 / control_rect_value_max) - 1;
                                            int x_showed = round_x + 1;

                                            current_pedal_travel_state = x_showed;
                                            Plugin.pedal_state_in_ratio = (byte)current_pedal_travel_state;
                                        }
                                        else
                                        {
                                            Canvas.SetLeft(rect_State, dxx * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 - rect_State.Width / 2);
                                            int round_x = (int)(100 * pedalState_read_st.payloadPedalBasicState_.pedalPosition_u16 / control_rect_value_max) - 1;
                                            int x_showed = round_x + 1;
                                            round_x = Math.Max(0, Math.Min(round_x, 99));
                                            current_pedal_travel_state = x_showed;
                                            Plugin.pedal_state_in_ratio = (byte)current_pedal_travel_state;
                                            Canvas.SetTop(rect_State, canvas.Height - Force_curve_Y[round_x] - rect_State.Height / 2);
                                            Canvas.SetLeft(text_state, Canvas.GetLeft(rect_State) );
                                            Canvas.SetTop(text_state, Canvas.GetTop(rect_State) - rect_State.Height);
                                            text_state.Text = x_showed + "%";
                                            Pedal_joint_draw();
                                        }

                                    }
                                    

                                    continue;
                                }

                            }






                            // check for pedal extended state struct
                            if ((destBuffLength == sizeof(DAP_state_extended_st)))
                            {

                                // parse byte array as config struct
                                DAP_state_extended_st pedalState_ext_read_st = getStateExtFromBytes(destinationArray);

                                // check whether receive struct is plausible
                                DAP_state_extended_st* v_state = &pedalState_ext_read_st;
                                byte* p_state = (byte*)v_state;
                                UInt16 pedalSelected = pedalState_ext_read_st.payloadHeader_.PedalTag;
                                // payload type check
                                bool check_payload_state_b = false;
                                if (pedalState_ext_read_st.payloadHeader_.payloadType == Constants.pedalStateExtendedPayload_type)
                                {
                                    check_payload_state_b = true;
                                }

                                // CRC check
                                bool check_crc_state_b = false;
                                if (Plugin.checksumCalc(p_state, sizeof(payloadHeader) + sizeof(payloadPedalState_Extended)) == pedalState_ext_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_state_b = true;
                                }

                                if ((check_payload_state_b) && check_crc_state_b)
                                {



                                    
                                    //if (indexOfSelectedPedal_u == pedalSelected)
                                    {
                                        if (dumpPedalToResponseFile[indexOfSelectedPedal_u])
                                        {
                                            // Specify the path to the file
                                            string currentDirectory = Directory.GetCurrentDirectory();
                                            string filePath = currentDirectory + "\\PluginsData\\Common" + "\\output_" + pedalSelected + ".txt";


                                            // write header
                                            if (!File.Exists(filePath))
                                            {
                                                using (StreamWriter writer = new StreamWriter(filePath, true))
                                                {
                                                    // Write the content to the file
                                                    writer.Write("cycleCtr, ");
                                                    writer.Write("time_InMs, ");
                                                    writer.Write("forceRaw_InKg, ");
                                                    writer.Write("forceFiltered_InKg, ");
                                                    writer.Write("forceVelocity_InKgPerSec, ");
                                                    writer.Write("servoPos_InSteps, ");
                                                    writer.Write("servoPosEsp_InSteps, ");
                                                    writer.Write("servoCurrent_InPercent, ");
                                                    writer.Write("servoVoltage_InV");
                                                    writer.Write("\n");
                                                }

                                            }
                                            // Use StreamWriter to write to the file
                                            using (StreamWriter writer = new StreamWriter(filePath, true))
                                            {
                                                // Write the content to the file
                                                writeCntr++;
                                                writer.Write(writeCntr);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.timeInMs_u32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.pedalForce_raw_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.pedalForce_filtered_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.forceVel_est_fl32);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servoPosition_i16);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servoPositionTarget_i16);
                                                writer.Write(", ");
                                                writer.Write(pedalState_ext_read_st.payloadPedalExtendedState_.servo_current_percent_i16);
                                                writer.Write(", ");
                                                writer.Write(((float)pedalState_ext_read_st.payloadPedalExtendedState_.servo_voltage_0p1V_i16) / 10.0);
                                                writer.Write("\n");
                                            }
                                        }
                                    }
                                    




                                    continue;
                                }
                            }








                            // decode into config struct
                            
                            if ( destBuffLength == sizeof(DAP_config_st))
                            {

                                // parse byte array as config struct
                                DAP_config_st pedalConfig_read_st = getConfigFromBytes(destinationArray);

                                // check whether receive struct is plausible
                                DAP_config_st* v_config = &pedalConfig_read_st;
                                byte* p_config = (byte*)v_config;

                                // payload type check
                                bool check_payload_config_b = false;
                                if (pedalConfig_read_st.payloadHeader_.payloadType == Constants.pedalConfigPayload_type)
                                {
                                    check_payload_config_b = true;
                                }

                                // CRC check
                                bool check_crc_config_b = false;
                                if (Plugin.checksumCalc(p_config, sizeof(payloadHeader) + sizeof(payloadPedalConfig)) == pedalConfig_read_st.payloadFooter_.checkSum)
                                {
                                    check_crc_config_b = true;
                                }
                                UInt16 pedalSelected = pedalConfig_read_st.payloadHeader_.PedalTag;
                                if (waiting_for_pedal_config[pedalSelected])
                                {
                                    if ((check_payload_config_b) && check_crc_config_b)
                                    {
                                        waiting_for_pedal_config[pedalSelected] = false;
                                        dap_config_st[pedalSelected] = pedalConfig_read_st;
                                        updateTheGuiFromConfig();

                                        continue;
                                    }
                                    else
                                    {
                                        TextBox_debugOutput.Text = "Payload config test 1: " + check_payload_config_b;
                                        TextBox_debugOutput.Text += "Payload config test 2: " + check_crc_config_b;
                                    }
                                }


                            }
                            


                            // If non known array datatype was received, assume a text message was received and print it
                            // only print debug messages when debug mode is active as it degrades performance
                            if (Debug_check.IsChecked == true)
                            {
                                byte[] destinationArray_sub = new byte[destBuffLength];
                                Buffer.BlockCopy(destinationArray, 0, destinationArray_sub, 0, destBuffLength);
                                string resultString = Encoding.GetEncoding(28591).GetString(destinationArray_sub);

                                TextBox_serialMonitor.Text += resultString + "\n";
                                TextBox_serialMonitor.ScrollToEnd();
                                
                            }






                            // When only a few messages are received, make the counter greater than N thus every message is printed
                            //if (destBuffLength < 100)
                            //{
                            //    printCtr = 600;
                            //}

                            //if (printCtr++ > 200)
                            //{
                            //    printCtr = 0;
                            //    TextBox_serialMonitor.Text += dataToSend + "\n";
                            //    TextBox_serialMonitor.ScrollToEnd();
                            //}





                        }







                        // copy the last not finished buffer element to begining of next cycles buffer
                        // and determine buffer offset
                        if (indices.Count > 0)
                        {
                            // If at least one crlf was detected, check whether it arrieved at the last bytes
                            int lastElement = indices.Last<int>();
                            int remainingMessageLength = currentBufferLength - (lastElement + stop_char_length);
                            if (remainingMessageLength > 0)
                            {
                                appendedBufferOffset[3] = remainingMessageLength;

                                Buffer.BlockCopy(buffer_appended[3], lastElement + stop_char_length, buffer_appended[3], 0, remainingMessageLength);
                            }
                            else
                            {
                                appendedBufferOffset[3] = 0;
                            }
                        }
                        else
                        {
                            appendedBufferOffset[3] += receivedLength;
                        }






                        // Stop the stopwatch
                        stopwatch.Stop();

                        // Get the elapsed time
                        TimeSpan elapsedTime = stopwatch.Elapsed;

                        timeCollector[3] += elapsedTime.TotalMilliseconds;

                        if (timeCntr[3] >= 50)
                        {


                            double avgTime = timeCollector[3] / timeCntr[3];
                            if (debug_flag)
                            {
                                TextBox_debugOutput.Text = "Serial callback time in ms: " + avgTime.ToString();
                            }
                            timeCntr[3] = 0;
                            timeCollector[3] = 0;
                        }
                    }

                }
            }
        }

        private void CheckBox_Pedal_ESPNow_autoconnect_Checked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            { 
                Plugin.Settings.Pedal_ESPNow_auto_connect_flag = true;
            }
        }

        private void CheckBox_Pedal_ESPNow_autoconnect_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            {
                Plugin.Settings.Pedal_ESPNow_auto_connect_flag = false;
            }
        }

        private void Checkbox_auto_remove_serial_line_Checked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            {
                Plugin.Settings.Serial_auto_clean = true;
            }
        }

        private void Checkbox_auto_remove_serial_line_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Plugin != null)
            {
                Plugin.Settings.Serial_auto_clean = false;
            }
        }

 unsafe private void btn_OTA_enable_Click(object sender, RoutedEventArgs e)
        {
            // compute checksum
            DAP_action_st tmp;
            tmp.payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            tmp.payloadHeader_.payloadType = (byte)Constants.pedalActionPayload_type;
            tmp.payloadHeader_.PedalTag = (byte)indexOfSelectedPedal_u;
            tmp.payloadPedalAction_.system_action_u8 = 3; //1=reset pedal position, 2 =restart esp, 3=enable wifi OTA

            DAP_action_st* v = &tmp;
            byte* p = (byte*)v;
            tmp.payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalAction));
            int length = sizeof(DAP_action_st);
            byte[] newBuffer = new byte[length];
            newBuffer = Plugin.getBytes_Action(tmp);
            if (Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u])
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    try
                    {
                        // clear inbuffer 
                        Plugin.ESPsync_serialPort.DiscardInBuffer();

                        // send query command
                        Plugin.ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }
                }
            }
            else
            {
                if (Plugin._serialPort[indexOfSelectedPedal_u].IsOpen)
                {
                    try
                    {
                        // clear inbuffer 
                        Plugin._serialPort[indexOfSelectedPedal_u].DiscardInBuffer();

                        // send query command
                        Plugin._serialPort[indexOfSelectedPedal_u].Write(newBuffer, 0, newBuffer.Length);
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }
                }
            }
            string MSG_tmp = "Connect to ";
            if (indexOfSelectedPedal_u == 0)
            {
                MSG_tmp += "FFBPedalClutch";
            }
            if (indexOfSelectedPedal_u == 1)
            {
                MSG_tmp += "FFBPedalBrake";
            }
            if (indexOfSelectedPedal_u == 2)
            {
                MSG_tmp += "FFBPedalGas";
            }
            MSG_tmp += " wifi hotspot, then go to 192.168.2.1 to upload firmware.bin";

            System.Windows.MessageBox.Show(MSG_tmp, "OTA warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btn_Bridge_restart_Click(object sender, RoutedEventArgs e)
        {
            Plugin.ESPsync_serialPort.DtrEnable = true;
            Plugin.ESPsync_serialPort.RtsEnable = true;
            System.Threading.Thread.Sleep(100);
            Plugin.ESPsync_serialPort.DtrEnable = false;
            Plugin.ESPsync_serialPort.RtsEnable = false;
        }

        unsafe private void btn_PedalID_Reset_Click(object sender, RoutedEventArgs e)
        {
            // compute checksum
            //getBytes(this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_)
            this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.payloadType = (byte)Constants.pedalConfigPayload_type;
            this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.PedalTag = (byte)indexOfSelectedPedal_u;
            this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.storeToEeprom = 1;
            this.dap_config_st[indexOfSelectedPedal_u].payloadPedalConfig_.pedal_type = 4;//back to default value 4.
            DAP_config_st tmp = this.dap_config_st[indexOfSelectedPedal_u];
            DAP_config_st* v = &tmp;
            byte* p = (byte*)v;
            this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum = Plugin.checksumCalc(p, sizeof(payloadHeader) + sizeof(payloadPedalConfig));
            int length = sizeof(DAP_config_st);
            //int val = this.dap_config_st[indexOfSelectedPedal_u].payloadHeader_.checkSum;
            //string msg = "CRC value: " + val.ToString();
            byte[] newBuffer = new byte[length];
            newBuffer = getBytes(this.dap_config_st[indexOfSelectedPedal_u]);

            //TextBox_debugOutput.Text = "CRC simhub calc: " + this.dap_config_st[indexOfSelectedPedal_u].payloadFooter_.checkSum + "    ";

            TextBox_debugOutput.Text = String.Empty;
            if (Plugin.Settings.Pedal_ESPNow_Sync_flag[indexOfSelectedPedal_u])
            {
                if (Plugin.ESPsync_serialPort.IsOpen)
                {
                    try
                    {
                        TextBox2.Text = "Buffer sent size:" + length;
                        Plugin.ESPsync_serialPort.DiscardInBuffer();
                        Plugin.ESPsync_serialPort.DiscardOutBuffer();
                        // send data
                        Plugin.ESPsync_serialPort.Write(newBuffer, 0, newBuffer.Length);
                        //Plugin._serialPort[indexOfSelectedPedal_u].Write("\n");
                        System.Threading.Thread.Sleep(100);
                        string MSG_tmp = "Pedal:"+indexOfSelectedPedal_u+" ID is reset, please adjust jumpper on the control board then re-send config in.";
                        System.Windows.MessageBox.Show(MSG_tmp, "OTA warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception caughtEx)
                    {
                        string errorMessage = caughtEx.Message;
                        TextBox_debugOutput.Text = errorMessage;
                    }
                }
            }
        }
    }
    
}
