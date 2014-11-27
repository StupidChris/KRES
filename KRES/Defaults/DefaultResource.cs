using System;
using System.Collections.Generic;
using System.Linq;
using KRES.Extensions;

namespace KRES.Defaults
{
    public class DefaultResource
    {
        #region Properties
        private string _name = string.Empty;
        public string name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        private string _type = string.Empty;
        public string type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        private string[] _biomes = new string[] { };
        public string[] biomes
        {
            get { return this._biomes; }
            set { this._biomes = value; }
        }

        private string[] _excludedBiomes = new string[] { };
        public string[] excludedBiomes
        {
            get { return this._excludedBiomes; }
        }

        private double _minAltitude = double.NaN;
        public double minAltitude
        {
            get { return this._minAltitude; }
            set { this._minAltitude = value; }
        }

        private double _maxAltitude = double.NaN;
        public double maxAltitude
        {
            get { return this._maxAltitude; }
            set { this._maxAltitude = value; }
        }

        private double _density = 0;
        public double density
        {
            get { return this._density; }
            set { this._density = value; }
        }

        private double _octaves = 0;
        public double octaves
        {
            get { return this._octaves; }
            set { this._octaves = value; }
        }

        private double _persistence = 0;
        public double persistence
        {
            get { return this._persistence; }
            set { this._persistence = value; }
        }

        private double _frequency = 0;
        public double frequency
        {
            get { return this._frequency; }
            set { this._frequency = value; }
        }

        private int _seed = 0;
        public int seed
        {
            get { return this._seed; }
            set { this._seed = value; }
        }
        #endregion

        #region Initialisation
        public DefaultResource(ConfigNode configNode, Random random)
        {
            configNode.TryGetValue("name", ref this._name);
            configNode.TryGetValue("type", ref this._type);
            configNode.TryGetValue("density", ref this._density);
            configNode.TryGetValue("octaves", ref this._octaves);
            configNode.TryGetValue("persistence", ref this._persistence);
            configNode.TryGetValue("frequency", ref this._frequency);
            configNode.TryGetValue("minAltitude", ref this._minAltitude);
            configNode.TryGetValue("maxAltitude", ref this._maxAltitude);
            configNode.TryGetValue("biomes", ref this._biomes);
            configNode.TryGetValue("excludedBiomes", ref this._excludedBiomes);
            if (this._type == "ore") { this._seed = random.Next(999999999); }
        }
        #endregion

        #region Public Methods
        public ConfigNode CreateConfigNode()
        {
            ConfigNode configNode = new ConfigNode("KRES_DATA");
            configNode.AddValue("name", this._name);
            return configNode;
        }
        #endregion
    }
}
