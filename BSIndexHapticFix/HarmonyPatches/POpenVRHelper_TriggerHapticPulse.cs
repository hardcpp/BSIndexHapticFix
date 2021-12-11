using HarmonyLib;
using UnityEngine.XR;

namespace BSIndexHapticFix.HarmonyPatches
{
    [HarmonyPatch(typeof(OpenVRHelper))]
    [HarmonyPatch(nameof(OpenVRHelper.TriggerHapticPulse))]
    internal class POpenVRHelper_TriggerHapticPulse
    {
        static bool Prefix(XRNode node, float duration, float strength, float frequency)
        {
            if (node == XRNode.LeftHand && Plugin.LeftHapticEmulator && Plugin.LeftHapticEmulator.IsIndexKnuckle)
            {
                Plugin.LeftHapticEmulator.SetHaptics(strength, duration);

                /// Skip original method
                return false;
            }

            if (node == XRNode.RightHand && Plugin.RightHapticEmulator && Plugin.RightHapticEmulator.IsIndexKnuckle)
            {
                Plugin.RightHapticEmulator.SetHaptics(strength, duration);

                /// Skip original method
                return false;
            }

            /// Forward to base method
            return true;
        }
    }
}
