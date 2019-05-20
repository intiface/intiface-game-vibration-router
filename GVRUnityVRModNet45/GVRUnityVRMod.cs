﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using Harmony;

namespace GVRUnityVRMod
{
    public class GVRUnityVRModFuncs
    {
        private static bool _useOutputFile = true;
        private static StreamWriter _outFile;
        private static NamedPipeClientStream _stream;
        static void WriteToOutput(string aMsg)
        {
            if (_useOutputFile)
            {
                _outFile.WriteLine(aMsg);
            }
        }

        static void Load()
        {
            if (_useOutputFile)
            {
                _outFile = new StreamWriter("C:\\Users\\qdot\\bstest.txt");
                _outFile.AutoFlush = true;
                WriteToOutput("Created log file.");
            }
            /*
            try
            {
                _stream = new NamedPipeClientStream("GVRPipe");
                _stream.Connect();
            }
            catch (Exception ex)
            {
                WriteToOutput(ex.ToString());
            }
            */

            HarmonyInstance harmony;
            try
            {
                harmony = HarmonyInstance.Create("com.nonpolynomial.buttsaber");
            }
            catch (Exception ex)
            {
                WriteToOutput(ex.ToString());
                return;
            }

            WriteToOutput("Patching assemblies");
            try
            {
                var original = AccessTools.TypeByName("CVRSystem");
                if (original == null)
                {
                    WriteToOutput("Can't find CVRSystem!");
                    return;
                }
                var method = original.GetMethod("TriggerHapticPulse");
                if (method == null)
                {
                    WriteToOutput("Can't find TriggerHapticPulse!");
                    return;
                }

                var postfix = typeof(TriggerHapticPulse_Exfiltration_Patch).GetMethod("ValvePostfix", BindingFlags.Public | BindingFlags.Static);
                if (postfix == null)
                {
                    WriteToOutput("Can't find ValvePostfix!");
                    return;
                }

                harmony.Patch(method, null, new HarmonyMethod(postfix));
                /*
                var original2 = AccessTools.TypeByName("OVRPlugin");
                if (original2 == null)
                {
                    WriteToOutput("Can't find OVRPlugin!");
                    return;
                }
                var method2 = original.GetMethod("SetControllerHaptics");
                if (method2 == null)
                {
                    WriteToOutput("Can't find SetControllerHaptics!");
                    return;
                }

                var postfix2 = typeof(TriggerHapticPulse_Oculus_Exfiltration_Patch).GetMethod("OculusPostfix", BindingFlags.Public | BindingFlags.Static);
                if (postfix2 == null)
                {
                    WriteToOutput("Can't find OculusPostfix!");
                    return;
                }

                harmony.Patch(method2, null, new HarmonyMethod(postfix2));
                */
                //harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                WriteToOutput(ex.ToString());
                return;
            }

            WriteToOutput("Patched assemblies");
        }

        [HarmonyPatch()]
        static class TriggerHapticPulse_Exfiltration_Patch
        {
            [HarmonyPostfix]
            static public void ValvePostfix(uint unControllerDeviceIndex, uint unAxisId, char usDurationMicroSec)
            {
                WriteToOutput($"VALVE: Writing ${usDurationMicroSec} duration to ${unControllerDeviceIndex}");
                /*
                var hand = node == XRNode.LeftHand ? "l" : "r";
                var msg = $"{hand},{strength.ToString()}\n";
                var b = Encoding.ASCII.GetBytes(msg);
                _stream.Write(b, 0, b.Length);
                */
            }
        }

        public struct HapticsBuffer
        {
            public IntPtr Samples;

            public int SamplesCount;
        }

        [HarmonyPatch()]
        static class TriggerHapticPulse_Oculus_Exfiltration_Patch
        {
            [HarmonyPostfix]
            static void OculusPostfix(uint controllerMask, HapticsBuffer hapticsBuffer)
            {
                WriteToOutput($"OCULUS: Writing ${hapticsBuffer.SamplesCount} samples to ${controllerMask}");
            }
        }

        // Output a string of "[l|r],[number]\n" over IPC. No reason to deal with
        // something like pbufs for this.
        //
        // [number] for BeatSaber will be a float between 0-1. We'll use the Vive
        // interpretation of this, which is a multiplier against 4000 microseconds.
        // See https://github.com/ValveSoftware/openvr/wiki/IVRSystem::TriggerHapticPulse
        // for more info. This is weird.
        //
        // It might be worth trying to hook this at the OVR/Oculus API level at some
        // point to make this a more generic solution, but that will mean translating
        // Oculus haptic clips for games that haven't moved to the new API and I don't
        // wanna.
        /*
        [HarmonyPatch(typeof(VRPlatformHelper), "TriggerHapticPulse")]
        static class TriggerHapticPulse_Exfiltration_Patch
        {
            static void Postfix(XRNode node, float strength = 1f)
            {
                WriteToOutput($"BS: Writing ${strength} to ${node}");
                var hand = node == XRNode.LeftHand ? "l" : "r";
                var msg = $"{hand},{strength.ToString()}\n";
                var b = Encoding.ASCII.GetBytes(msg);
                _stream.Write(b, 0, b.Length);
            }
        }
        */
        /* [HarmonyPatch(typeof(OVRPlugin), "SetControllerHaptics")]
        static class TriggerHapticPulse_Oculus_Exfiltration_Patch
        {
            static void Postfix(uint controllerMask, OVRPlugin.HapticsBuffer hapticsBuffer)
            {
                WriteToOutput($"OCULUS: Writing ${hapticsBuffer.SamplesCount} samples to ${controllerMask}");
            }
        }

        
        [HarmonyPatch(typeof(Valve.VR.CVRSystem), "TriggerHapticPulse")]
        static class TriggerHapticPulse_Valve_Exfiltration_Patch
        {
            static void Postfix(uint unControllerDeviceIndex, uint unAxisId, char usDurationMicroSec)
            {
                WriteToOutput($"VALVE: Writing ${usDurationMicroSec} duration to ${unControllerDeviceIndex}");
                var hand = node == XRNode.LeftHand ? "l" : "r";
                var msg = $"{hand},{strength.ToString()}\n";
                var b = Encoding.ASCII.GetBytes(msg);
                _stream.Write(b, 0, b.Length);
            }
        }
        */
    }
}