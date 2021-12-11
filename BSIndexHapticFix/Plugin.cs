using HarmonyLib;
using IPA;
using System.Reflection;
using UnityEngine;

namespace BSIndexHapticFix
{
    /// <summary>
    /// Plugin main class
    /// </summary>
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        internal static IPA.Logging.Logger Logger { get; private set; }
        /// <summary>
        /// Harmony ID for patches
        /// </summary>
        internal const string HarmonyID = "com.github.hardcpp.bsindexhapticfix";
        /// <summary>
        /// Left controller haptic emulator
        /// </summary>
        internal static Components.HapticEmulator LeftHapticEmulator;
        /// <summary>
        /// Right controller haptic emulator
        /// </summary>
        internal static Components.HapticEmulator RightHapticEmulator;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Harmony patch holder
        /// </summary>
        private static Harmony m_Harmony;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        [Init]
        public void Init(IPA.Logging.Logger p_Logger)
        {
            Logger = p_Logger;

            Logger.Info("BSIndexHapticFix initialized.");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On application start
        /// </summary>
        [OnStart]
        public void OnApplicationStart()
        {
            try
            {
                Logger.Debug("OnApplicationStart");

                m_Harmony = new Harmony(HarmonyID);
                m_Harmony.PatchAll(Assembly.GetExecutingAssembly());

                LeftHapticEmulator = new GameObject("BSIndexHapticFix_Left").AddComponent<Components.HapticEmulator>();
                LeftHapticEmulator.Init(UnityEngine.XR.XRNode.LeftHand);

                RightHapticEmulator = new GameObject("BSIndexHapticFix_Right").AddComponent<Components.HapticEmulator>();
                RightHapticEmulator.Init(UnityEngine.XR.XRNode.RightHand);
            }
            catch (System.Exception p_Exception)
            {
                Logger.Critical(p_Exception);
            }
        }
        /// <summary>
        /// On application quit
        /// </summary>
        [OnExit]
        public void OnApplicationQuit()
        {
            try
            {
                Logger.Debug("OnApplicationQuit");

                m_Harmony.UnpatchAll();

                GameObject.Destroy(LeftHapticEmulator);
                GameObject.Destroy(RightHapticEmulator);
            }
            catch (System.Exception p_Exception)
            {
                Logger.Critical(p_Exception);
            }
        }
    }
}
