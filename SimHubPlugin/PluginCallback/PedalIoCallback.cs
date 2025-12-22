using log4net.Plugin;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IPlugin = SimHub.Plugins.IPlugin;

namespace User.PluginSdkDemo
{
    public partial class DIY_FFB_Pedal : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public async Task SendBridgeAction(DAP_bridge_state_st tmp)
        {
            int length;
            tmp.payLoadHeader_.version = (byte)Constants.pedalConfigPayload_version;
            tmp.payLoadHeader_.payloadType = (byte)Constants.bridgeStatePayloadType;
            tmp.payLoadHeader_.PedalTag = (byte)99;
            tmp.payloadFooter_.enfOfFrame0_u8 = ENDOFFRAMCHAR[0];
            tmp.payloadFooter_.enfOfFrame1_u8 = ENDOFFRAMCHAR[1];
            tmp.payLoadHeader_.startOfFrame0_u8 = STARTOFFRAMCHAR[0];
            tmp.payLoadHeader_.startOfFrame1_u8 = STARTOFFRAMCHAR[1];
            tmp.payloadBridgeState_.unassignedPedalCount = 0;
            tmp.payloadBridgeState_.Pedal_availability_0 = 0;
            tmp.payloadBridgeState_.Pedal_availability_1 = 0;
            tmp.payloadBridgeState_.Pedal_availability_2 = 0;


            byte[] newBuffer_2;
            unsafe
            {
                length = sizeof(DAP_bridge_state_st);
                newBuffer_2 = new byte[length];
                DAP_bridge_state_st* v_2 = &tmp;
                byte* p_2 = (byte*)v_2;
                tmp.payloadFooter_.checkSum = checksumCalc(p_2, sizeof(payloadHeader) + sizeof(payloadBridgeState));
            }
            newBuffer_2 = getBytes_Bridge(tmp);
            if (ESPsync_serialPort.IsOpen)
            {
                try
                {
                    if (BridgeHidService.IsConnected)
                    {
                        await Task.Delay(10);
                        await BridgeHidService.SendLargeDataAsync(newBuffer_2);
                    }
                    else
                    {
                        // clear inbuffer 
                        ESPsync_serialPort.DiscardInBuffer();
                        // send query command
                        ESPsync_serialPort.Write(newBuffer_2, 0, newBuffer_2.Length);
                    }

                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    wpfHandle.TextBox2.Text = errorMessage;
                }
            }
        }

        public async Task SendOTAAction(DAP_action_ota_st tmp)
        {
            int length;
            byte[] newBuffer_2;
            unsafe
            {
                length = sizeof(DAP_action_ota_st);
                newBuffer_2 = new byte[length];
                DAP_action_ota_st* v_2 = &tmp;
                byte* p_2 = (byte*)v_2;
                tmp.payloadFooter_.checkSum = checksumCalc(p_2, sizeof(payloadHeader) + sizeof(payloadOtaInfo));
            }
            newBuffer_2 = getBytes_Action_Ota(tmp);
            if (ESPsync_serialPort.IsOpen)
            {
                try
                {
                    if (BridgeHidService.IsConnected)
                    {
                        await Task.Delay(10);
                        await BridgeHidService.SendLargeDataAsync(newBuffer_2);
                    }
                    else
                    {
                        // clear inbuffer 
                        ESPsync_serialPort.DiscardInBuffer();
                        // send query command
                        ESPsync_serialPort.Write(newBuffer_2, 0, newBuffer_2.Length);
                    }

                }
                catch (Exception caughtEx)
                {
                    string errorMessage = caughtEx.Message;
                    wpfHandle.TextBox2.Text = errorMessage;
                }
            }
        }
    }
}
