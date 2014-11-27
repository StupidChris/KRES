using System.IO;
using UnityEngine;
using KRES.Extensions;

namespace KRES
{
    public class SettingsLibrary
    {
        #region Instance
        private static SettingsLibrary _instance;
        /// <summary>
        /// Current instance of the settings library
        /// </summary>
        public static SettingsLibrary instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsLibrary();
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        private ushort _mapWidth = 1440;
        /// <summary>
        /// Resource map generation width
        /// </summary>
        public ushort mapWidth
        {
            get { return this._mapWidth; }
        }

        private ushort _mapHeight = 720;
        /// <summary>
        /// Resource map generation height
        /// </summary>
        public ushort mapHeight
        {
            get { return this._mapHeight; }
        }

        /// <summary>
        /// Resolution in pixels of the resource maps
        /// </summary>
        public uint mapResolution
        {
            get { return (uint)this._mapWidth * (uint)this._mapHeight; }
        }

        private ushort _maxLinesPerFrame = 90;
        /// <summary>
        /// Max lines itterated per frame when creating resource maps
        /// </summary>
        public ushort maxLinesPerFrame
        {
            get { return this._maxLinesPerFrame; }
        }
        #endregion

        #region Initialisation
        private SettingsLibrary()
        {
            Load();
        }
        #endregion

        #region Methods
        private void Load()
        {
            string path = Path.Combine(KRESUtils.pluginDataURL, "KRESSettings.cfg");
            if (!File.Exists(path)) { throw new FileNotFoundException("KRES Settings file could not be found", path); }
            ConfigNode node = ConfigNode.Load(path), settings = null;
            if (!node.TryGetNode("SETTINGS", ref settings)) { throw new FileNotFoundException("KRES Settings file did not have a SETTINGS node", path); }
            settings.TryGetValue("mapWidth", ref this._mapWidth);
            settings.TryGetValue("mapHeight", ref this._mapHeight);
            settings.TryGetValue("maxLinesPerFrame", ref this._maxLinesPerFrame);
        }

        public static void Save()
        {
            ConfigNode node = new ConfigNode(), settings = new ConfigNode("SETTINGS");
            settings.AddValue("mapWidth", _instance._mapWidth);
            settings.AddValue("mapHeight", _instance._mapHeight);
            settings.AddValue("maxLinesPerFrame", _instance._maxLinesPerFrame);
            node.AddNode(settings);
            node.Save(Path.Combine(KRESUtils.pluginDataURL, "KRESSettings.cfg"));
        }
        #endregion
    }
}
