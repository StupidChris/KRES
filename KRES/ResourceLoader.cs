using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KRES
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class ResourceLoader : MonoBehaviour
    {
        #region Propreties
        private static ResourceLoader _instance = null;
        /// <summary>
        /// Returns the current instance
        /// </summary>
        public static ResourceLoader instance
        {
            get { return _instance; }
        }

        private static bool _loaded = false;
        /// <summary>
        /// Gets the loaded state of the resource system.
        /// </summary>
        public static bool loaded
        {
            get { return _loaded; }
        }

        private double _loadPercent = 0d;
        /// <summary>
        /// How much of the resources are loaded if loading
        /// </summary>
        public double loadPercent
        {
            get { return _loadPercent; }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (_instance == null) { _instance = this; }
            else { Destroy(this); }
        }
        #endregion

        #region Methods
        public void Load()
        {
            StartCoroutine(LoadBodies());
        }

        private IEnumerator<YieldInstruction> LoadBodies()
        {
            ConfigNode settings = ConfigNode.Load(KRESUtils.dataURL);
            double max = FlightGlobals.Bodies.Count;
            double current = -1d;
            System.Random random = new System.Random();
            foreach (CelestialBody planet in FlightGlobals.Bodies)
            {
                current++;
                _loadPercent = current / max;
                ResourceBody body = new ResourceBody(planet.bodyName);
                var b = body.LoadItems(settings.GetNode("KRES"), random);
                while (b.MoveNext()) { yield return b.Current; }
                ResourceController.instance.resourceBodies.Add(body);
            }
            settings.Save(KRESUtils.dataURL);
            _loadPercent = 1;
            _loaded = true;
            DebugWindow.instance.Print("- Loaded Resources -");
            Resources.UnloadUnusedAssets();
        }
        #endregion

        #region Unloading
        private void OnDestroy()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU && _loaded)
            {
                _instance = null;
                ResourceController.instance.UnloadResources();
                _loaded = false;
                DebugWindow.instance.Print("- Unloaded Resources -");
                Resources.UnloadUnusedAssets();
            }
        }
        #endregion
    }
}
