using UnityEngine;


namespace SigmaAdvancedTraitsPlugin
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class Version : MonoBehaviour
    {
        public static readonly System.Version number = new System.Version("0.0.0.1");

        void Awake()
        {
            UnityEngine.Debug.Log("[SigmaLog] Version Check:   Sigma AdvancedTraits v" + number);
        }
    }
}
