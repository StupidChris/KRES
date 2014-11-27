using System.Collections.Generic;
using System.Linq;
using KRES.Extensions;

namespace KRES.Defaults
{
    public class DefaultsLibrary
    {
        #region Instance
        private static DefaultsLibrary _instance;
        public static DefaultsLibrary instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DefaultsLibrary();
                }
                return _instance;
            }
        }
        #endregion

        #region Properties
        private Dictionary<string, DefaultConfig> _defaults = new Dictionary<string, DefaultConfig>();
        public Dictionary<string, DefaultConfig> defaults
        {
            get { return this._defaults; }
        }

        private DefaultConfig _selectedDefault = null;
        public DefaultConfig selectedDefault
        {
            get { return this._selectedDefault; }
            set { this._selectedDefault = value; }
        }
        #endregion

        #region Initialisation
        private DefaultsLibrary()
        {
            Load();
        }
        #endregion

        #region Public Methods
        public void Load()
        {
            this._defaults.Clear();
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("KRES_DEFAULTS"))
            {
                DefaultConfig defaultConfig = new DefaultConfig(node);
                this._defaults.Add(defaultConfig.name, defaultConfig);

                if (defaultConfig.name == "Default")
                {
                    this._selectedDefault = defaultConfig;
                }
            }
            if (this._selectedDefault == null && this._defaults.Count > 0) { this._selectedDefault = this._defaults.Values.First(); }
        }
        #endregion

        #region Static Methods
        public static List<DefaultConfig> GetDefaults()
        {
            return new List<DefaultConfig>(instance.defaults.Values);
        }

        public static List<string> GetDefaultNames()
        {
            return new List<string>(instance.defaults.Keys);
        }

        public static DefaultConfig GetDefault(string name)
        {
            if (_instance.defaults.ContainsKey(name))
            {
                return instance.defaults[name];
            }
            return null;
        }

        public static bool TryGetDefault(string name, ref DefaultConfig config)
        {
            if (instance._defaults.ContainsKey(name))
            {
                config = instance._defaults[name];
                return true;
            }
            return false;
        }

        public static DefaultConfig GetSelectedDefault()
        {
            return instance.selectedDefault;
        }

        public static void SetSelectedDefault(DefaultConfig defaultConfig)
        {
            instance.selectedDefault = defaultConfig;
        }

        public static void SaveSelectedDefault(ConfigNode config, string path)
        {
            config.AddNode(GetSelectedDefault().CreateConfigNode(true));
            config.Save(path);
        }
        #endregion
    }
}
