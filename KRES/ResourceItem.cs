using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using KRES.Extensions;
using KRES.Defaults;

namespace KRES
{
    public class ResourceItem
    {
        public class ResourceMap
        {
            #region Fields
            private bool fullShowing = false;
            private bool scannedShowing = false;
            #endregion

            #region Propreties
            private string _texturePath = string.Empty;
            /// <summary>
            /// Gets the path to the resource texture
            /// </summary>
            public string texturePath
            {
                get { return this._texturePath; }
            }

            private string _scannedTexturePath = string.Empty;
            /// <summary>
            /// Path to the Scanned texture of this instance
            /// </summary>
            public string scannedTexturePath
            {
                get { return this._scannedTexturePath; }
            }

            private Color _colour = KRESUtils.blankColour;
            /// <summary>
            /// Gets and sets the colour used when generating the texture.
            /// </summary>
            public Color colour
            {
                get { return this._colour; }
            }

            private double _minAltitude = double.NegativeInfinity;
            /// <summary>
            /// Minimum altitude at which this resource can be found
            /// </summary>
            public double minAltitude
            {
                get { return this._minAltitude; }
            }

            private double _maxAltitude = double.PositiveInfinity;
            /// <summary>
            /// Maximum altitude at which this resource can be found
            /// </summary>
            public double maxAltitude
            {
                get { return this._maxAltitude; }
            }

            private string[] _biomes = new string[] { };
            /// <summary>
            /// Contains the only biomes where this resource can be found in
            /// </summary>
            public string[] biomes
            {
                get { return this._biomes; }
            }

            private string[] _excludedBiomes = new string[] { };
            /// <summary>
            /// Contains the only biomes this resource cannot be found in
            /// </summary>
            public string[] excludedBiomes
            {
                get { return this._excludedBiomes; }
            }
            #endregion

            #region Initialisation
            public ResourceMap(DefaultResource resource, string body)
            {
                string path = Path.Combine(KRESUtils.savePath, "KRESTextures/" + body + "/" + resource.name);
                this._texturePath = path + ".png";
                this._scannedTexturePath = path + "_scanned.png";
                this._colour = ResourceInfoLibrary.instance.GetResource(resource.name).colour;
                this._minAltitude = resource.minAltitude;
                this._maxAltitude = resource.maxAltitude;
                this._biomes = resource.biomes;
                this._excludedBiomes = resource.excludedBiomes;
                if (!File.Exists(scannedTexturePath))
                {
                    Texture2D texture = new Texture2D(1440, 720, TextureFormat.ARGB32, false);
                    File.WriteAllBytes(_scannedTexturePath, texture.EncodeToPNG());
                    Texture2D.Destroy(texture);
                }
            }
            #endregion

            #region Public Methods
            public Texture2D GetTexture()
            {
                Texture2D texture = new Texture2D(1440, 720, TextureFormat.ARGB32, false);
                texture.LoadImage(File.ReadAllBytes(_texturePath));
                return texture;
            }

            /// <summary>
            /// Shows the texture in scaled space.
            /// </summary>
            public bool ShowTexture(string bodyName)
            {
                if (!this.fullShowing)
                {
                    foreach (Transform transform in ScaledSpace.Instance.scaledSpaceTransforms)
                    {
                        if (transform.name == bodyName)
                        {
                            bool containsMaterial = false;

                            foreach (Material material in transform.renderer.materials)
                            {
                                if (material.name.Contains("KRESResourceMap" + bodyName))
                                {
                                    containsMaterial = true;
                                    material.mainTexture = GetTexture();
                                    break;
                                }
                            }

                            if (!containsMaterial)
                            {
                                Material material = new Material(Shader.Find("Unlit/Transparent"));
                                material.name = "KRESResourceMap" + bodyName;
                                material.mainTexture = GetTexture();

                                List<Material> materials = new List<Material>(transform.renderer.materials);
                                materials.Add(material);
                                transform.renderer.materials = materials.ToArray();
                            }
                            this.fullShowing = true;
                            return true;
                        }
                    }
                }
                return false;
            }

            /// <summary>
            /// Hide the texture in scaled space.
            /// </summary>
            public bool HideTexture(string bodyName)
            {
                if (this.fullShowing)
                {
                    foreach (Transform transform in ScaledSpace.Instance.scaledSpaceTransforms)
                    {
                        if (transform.name == bodyName)
                        {
                            foreach (Material material in transform.renderer.materials)
                            {
                                if (material.name.Contains("KRESResourceMap" + bodyName))
                                {
                                    this.fullShowing = false;
                                    Texture t = material.mainTexture;
                                    material.mainTexture = KRESUtils.blankTexture;
                                    Texture.Destroy(t);
                                    Resources.UnloadUnusedAssets();
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }

            public void SaveScannedTexture(Texture2D texture)
            {
                if (!string.IsNullOrEmpty(scannedTexturePath))
                {
                    File.WriteAllBytes(scannedTexturePath, texture.EncodeToPNG());
                    Texture2D.Destroy(texture);
                }
            }

            public Texture2D GetScannedTexture()
            {
                Texture2D texture = new Texture2D(1440, 720, TextureFormat.ARGB32, false);
                texture.LoadImage(File.ReadAllBytes(scannedTexturePath));
                return texture;
            }
            #endregion
        }

        #region Propreties
        private ResourceType _type = ResourceType.LIQUID;
        /// <summary>
        /// The type of resource this is
        /// </summary>
        public ResourceType type
        {
            get { return this._type; }
        }

        private PartResourceDefinition _resource = new PartResourceDefinition();
        /// <summary>
        /// Resource associated with this item
        /// </summary>
        public PartResourceDefinition resource
        {
            get { return this._resource; }
        }

        /// <summary>
        /// Name of the resource
        /// </summary>
        public string name
        {
            get { return this.resource.name; }
        }

        private double _actualError = 0;
        /// <summary>
        /// How far away from the real value will this appear
        /// </summary>
        public double actualError
        {
            get { return this._actualError; }
        }

        private double _actualDensity = 0;
        /// <summary>
        /// Actual density of this resource on the planet
        /// </summary>
        public double actualDensity
        {
            get { return this._actualDensity; }
        }

        private ResourceMap _map = null;
        /// <summary>
        /// ResourceMap associated with this ResourceItem
        /// </summary>
        public ResourceMap map
        {
            get { return this._map; }
        }

        /// <summary>
        /// Whether this ResourceItem has a ResourceMap or not
        /// </summary>
        public bool hasMap
        {
            get { return this._map != null; }
        }
        #endregion

        #region Initialisation
        public ResourceItem(ConfigNode data, string resource, string body, ResourceType type, System.Random random)
        {
            this._resource = PartResourceLibrary.Instance.GetDefinition(data.GetValue("name"));
            this._type = type;
            this._map = null;
            if (!data.TryGetValue("actualDensity", ref _actualDensity))
            {
                _actualDensity = KRESUtils.Clamp01(DefaultsLibrary.GetDefault(MapGenerator.defaultName).GetBody(body).GetResourceOfType(resource, type).density * (0.97 + (random.NextDouble() * 0.06d)));
                data.AddValue("actualDensity", _actualDensity);
            }

            if (!data.TryGetValue("actualError", ref _actualError))
            {
                _actualError = (random.NextDouble() * 2d) - 1d;
                data.AddValue("actualError", _actualError);
            }
        }

        public ResourceItem(ConfigNode data, string resource, string body, System.Random random)
        {
            DefaultResource defaultResource = DefaultsLibrary.GetDefault(MapGenerator.defaultName).GetBody(body).GetResourceOfType(resource, ResourceType.ORE);
            this._resource = PartResourceLibrary.Instance.GetDefinition(resource);
            this._type = ResourceType.ORE;
            double density = defaultResource.density;
            this._map = new ResourceMap(defaultResource, body);
            if (!data.TryGetValue("actualDensity", ref _actualDensity))
            {
                Texture2D texture = map.GetTexture();
                _actualDensity = (double)texture.GetPixels().Count(p => p.a > 0) / (double)SettingsLibrary.instance.mapResolution;
                Texture2D.Destroy(texture);
                Resources.UnloadUnusedAssets();
                data.AddValue("actualDensity", _actualDensity);
            }

            if (!data.TryGetValue("actualError", ref _actualError))
            {
                _actualError = (random.NextDouble() * 2d) - 1d;
                data.AddValue("actualError", _actualError);
            }
        }
        #endregion
    }
}
