using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BSIndexHapticFix.Components
{
    /// <summary>
    /// Haptic emulator for a single controller
    /// </summary>
    public class HapticEmulator : MonoBehaviour
    {
        /// <summary>
        /// Target XR Node
        /// </summary>
        public UnityEngine.XR.XRNode Node { get; private set; }
        /// <summary>
        /// Is it an Index knuckle?
        /// </summary>
        public bool IsIndexKnuckle { get; private set; } = false;
        /// <summary>
        /// Is left hand
        /// </summary>
        public bool IsLeftHand => Node == UnityEngine.XR.XRNode.LeftHand;
        /// <summary>
        /// Is right hand
        /// </summary>
        public bool IsRightHand => Node == UnityEngine.XR.XRNode.RightHand;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Matching XR input device
        /// </summary>
        private UnityEngine.XR.InputDevice? m_Device = null;
        /// <summary>
        /// Remaining haptic time
        /// </summary>
        private float m_OpenVREmulatedHapticRemainingTime = 0f;
        /// <summary>
        /// Remaining haptic strength
        /// </summary>
        private float m_OpenVREmulatedHapticAmplitude = 0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        public void Init(UnityEngine.XR.XRNode p_Node)
        {
            Node = p_Node;

            /// Bind callbacks
            UnityEngine.XR.InputDevices.deviceConnected     += InputDevices_deviceConnected;
            UnityEngine.XR.InputDevices.deviceDisconnected  += InputDevices_deviceDisconnected;

            /// In case of late init, search device manually
            var l_AvailableDevicesAtNode = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(Node, l_AvailableDevicesAtNode);
            l_AvailableDevicesAtNode.ForEach(x => InputDevices_deviceConnected(x));

            GameObject.DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// On component destroy
        /// </summary>
        private void OnDestroy()
        {
            /// Remove callbacks
            UnityEngine.XR.InputDevices.deviceDisconnected  -= InputDevices_deviceDisconnected;
            UnityEngine.XR.InputDevices.deviceConnected     -= InputDevices_deviceConnected;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void SetHaptics(float p_Amplitude, float p_Duration)
        {
            float l_VibrationStrength = 2f;

            p_Duration  *= l_VibrationStrength;
            p_Amplitude *= l_VibrationStrength;

            if (p_Amplitude <= 0.01f)
                return;

            m_OpenVREmulatedHapticRemainingTime = p_Duration;
            m_OpenVREmulatedHapticAmplitude     = Mathf.Clamp01(p_Amplitude);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On XR device connected
        /// </summary>
        /// <param name="p_Device">New device</param>
        private void InputDevices_deviceConnected(UnityEngine.XR.InputDevice p_Device)
        {
            var l_RequiredCharacteristics = UnityEngine.XR.InputDeviceCharacteristics.Controller
                | UnityEngine.XR.InputDeviceCharacteristics.TrackedDevice
                | UnityEngine.XR.InputDeviceCharacteristics.HeldInHand
                | (IsLeftHand ? UnityEngine.XR.InputDeviceCharacteristics.Left : UnityEngine.XR.InputDeviceCharacteristics.Right);

            /// Check for matching role
            if (!p_Device.isValid || (p_Device.characteristics & l_RequiredCharacteristics) != l_RequiredCharacteristics)
                return;

            m_Device        = p_Device;
            IsIndexKnuckle  = m_Device.Value.name.ToLower().Contains("knuckles");

            if (IsIndexKnuckle)
                StartCoroutine(Coroutine_OpenVRHaptics());

            Plugin.Logger.Debug($"Device found \"{p_Device.manufacturer}\"-\"{p_Device.name}\" with role \"{p_Device.characteristics}\" serial {p_Device.serialNumber} is index knuckle? {IsIndexKnuckle}");
        }
        /// <summary>
        /// On XR device disconnected
        /// </summary>
        /// <param name="p_Device">Disconnected device</param>
        private void InputDevices_deviceDisconnected(UnityEngine.XR.InputDevice p_Device)
        {
            /// Check for matching role
            if (!m_Device.HasValue || m_Device != p_Device)
                return;

            m_Device = null;
            StopAllCoroutines();

            Plugin.Logger.Debug($"[OPVR.VRController] Device lost \"{p_Device.manufacturer}\"-\"{p_Device.name}\" with role \"{p_Device.characteristics}\"");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handles the haptic process every 1/80 second.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Coroutine_OpenVRHaptics()
        {
            const float s_Rate = 1 / 80f;

            var l_Waiter = new WaitForSecondsRealtime(s_Rate);
            while (true)
            {
                m_OpenVREmulatedHapticRemainingTime -= s_Rate;

                if (m_OpenVREmulatedHapticRemainingTime > 0f)
                    m_Device?.SendHapticImpulse(0, m_OpenVREmulatedHapticAmplitude);

                yield return l_Waiter;
            }
        }
    }
}
