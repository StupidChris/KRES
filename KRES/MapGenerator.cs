using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using KRES.Defaults;
using KRES.Extensions;
using HeightmapManager;

namespace KRES
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class MapGenerator : MonoBehaviour
    {
        #region Fields
        //Generation
        private string texturePath = Path.Combine(KRESUtils.savePath, "KRESTextures");
        private string path = string.Empty;
        private ushort height = 0, width = 0;
        private ConfigNode currentBody = null;
        private string bodyName = string.Empty;
        private string resourceName = string.Empty;
        private DefaultBody defaultBody = null;
        private DefaultResource currentResource = null;
        private Color currentColor = KRESUtils.blankColour;
        private Color[] rawMap = new Color[0];
        private Texture2D map = new Texture2D(1, 1);
        private ConfigNode[] dataNodes = new ConfigNode[0];
        private Stopwatch timer = new Stopwatch(), mapTimer = new Stopwatch();
        private DefaultConfig config = null;
        private Heightmap heightmap = null;
        private SettingsLibrary set = null;
        private ConfigNode settings = new ConfigNode(), ores = null;
        private List<CelestialBody> relevantBodies = new List<CelestialBody>();
        private Simplex simplex = new Simplex();
        private double amountComplete = 0, max = 0, current = 0;

        //GUI
        private int id = Guid.NewGuid().GetHashCode();
        private bool visible = false;
        private Rect window = new Rect();
        private GUISkin skins = HighLogic.Skin;
        private Texture2D background = new Texture2D(1, 1);
        private Texture2D bar = new Texture2D(1, 1);
        private Rect bgPos = new Rect();
        private Rect barPos = new Rect();
        private double time = 0d;
        private Progressbar progressbar = new Progressbar();
        #endregion

        #region Propreties
        private static bool _generated = false;
        /// <summary>
        /// If the 
        /// </summary>
        public static bool generated
        {
            get { return _generated; }
        }

        private static string _defaultName = string.Empty;
        public static string defaultName
        {
            get { return _defaultName; }
        }
        #endregion

        #region Initialization
        private void Start()
        {
            if (!generated)
            {
                ConfigNode test = null;

                if (!File.Exists(KRESUtils.dataURL))
                {
                    DebugWindow.Log("The data config file is missing, creating a new version");
                    test = new ConfigNode();
                    DefaultsLibrary.SaveSelectedDefault(test, KRESUtils.dataURL);
                    settings = test.GetNode("KRES");
                    settings.TryGetValue("name", ref _defaultName);
                    config = DefaultsLibrary.GetSelectedDefault();
                    DebugWindow.Log("Created and saved new data config");
                }
                else
                {
                    test = ConfigNode.Load(KRESUtils.dataURL);
                    if (!test.TryGetNode("KRES", ref this.settings))
                    {
                        DebugWindow.Log("Failed to correctly load the settings file, aborting");
                        return;
                    }
                    else
                    {
                        settings.TryGetValue("name", ref _defaultName);
                        if (!DefaultsLibrary.TryGetDefault(_defaultName, ref config))
                        {
                            DebugWindow.Log("Loaded default does not exist, aborting");
                            return;
                        }
                        settings.TryGetValue("generated", ref _generated);
                        DebugWindow.Log("Successfully loaded data config");
                    }
                }

                if (!Directory.Exists(texturePath))
                {
                    DebugWindow.Log("Textures directory does not exist in the save file");
                    Directory.CreateDirectory(texturePath);
                    _generated = false;
                    DebugWindow.Log("Successfully created texture directory");
                }

                if (!generated)
                {
                    DebugWindow.Log("Generating resource maps");
                    if (!settings.TryGetNode("ore", ref ores))
                    {
                        DebugWindow.Log("No ore nodes detected, aborting generation process");
                        return;
                    }
                    this.relevantBodies = new List<CelestialBody>(KRESUtils.GetRelevantBodies("ore").Where(b => this.ores.HasNode(b.bodyName) && this.ores.GetNode(b.bodyName).nodes.Count > 0));
                    max = this.relevantBodies.Select(b => ores.GetNode(b.bodyName)).SelectMany(n => n.GetNodes("KRES_DATA")).Count();
                    LoadProgressbar();
                    this.set = SettingsLibrary.instance;
                    this.width = this.set.mapWidth;
                    this.height = this.set.mapHeight;
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "KRESMapGenerator");
                    StartCoroutine(GenerateAllMaps());
                    timer.Start();
                }
                else if (!ResourceLoader.loaded)
                {
                    this.amountComplete = 1;
                    timer.Start();
                    LoadProgressbar();
                    ResourceLoader.instance.Load();
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "KRESMapGenerator");
                }
            }
        }
        #endregion

        #region Methods
        private void LoadProgressbar()
        {
            this.bgPos = new Rect(0, 0, 374, 19);
            this.barPos = new Rect(2, 2, 370, 15);
            this.background.LoadImage(File.ReadAllBytes(Path.Combine(KRESUtils.pluginDataURL, "Progressbar/progressBackground.png")));
            this.bar.LoadImage(File.ReadAllBytes(Path.Combine(KRESUtils.pluginDataURL, "Progressbar/progressbar.png")));
            this.progressbar = new Progressbar(bgPos, barPos, background, bar);
            this.progressbar.SetValue(0);
            this.window = new Rect((Screen.width / 2) - 200, (Screen.height / 2) - 75, 400, 150);
            this.visible = true;
        }

        private IEnumerator<YieldInstruction> GenerateAllMaps()
        {
            foreach (CelestialBody body in this.relevantBodies)
            {
                this.bodyName = body.bodyName;
                this.currentBody = this.ores.GetNode(this.bodyName);
                this.defaultBody = this.config.GetBody(this.bodyName);
                this.path = Path.Combine(texturePath, this.bodyName);
                if (!Directory.Exists(this.path))
                {
                    Directory.CreateDirectory(this.path);
                    DebugWindow.Log("Created texture folder for " + this.bodyName);
                }
                this.dataNodes = currentBody.GetNodes("KRES_DATA");
                foreach (ConfigNode resource in dataNodes)
                {
                    this.current++;
                    this.amountComplete = this.current / this.max;
                    this.progressbar.SetValue(this.amountComplete);
                    if (!resource.TryGetValue("name", ref this.resourceName))
                    {
                        DebugWindow.Log("Unnamed resource for " + this.currentBody + ", cycling to next resource");
                        continue;
                    }
                    this.currentResource = this.defaultBody.GetResourceOfType(resourceName, ResourceType.ORE);
                    this.currentColor = ResourceInfoLibrary.instance.GetResource(resourceName).colour;

                    //Simplex values check
                    if (this.currentResource.density == 0 || this.currentResource.octaves == 0 || this.currentResource.persistence == 0 || this.currentResource.frequency == 0)
                    {
                        DebugWindow.Log("Invalid simplex values for " + this.resourceName + " on " + this.bodyName + ", cycling to next resource");
                        continue;
                    }
                    yield return new WaitForEndOfFrame();

                    //Heightmap
                    if (!double.IsNaN(this.currentResource.minAltitude) || !double.IsNaN(this.currentResource.maxAltitude))
                    {
                        string path = Path.Combine(KRESUtils.pluginDataURL, "Heightmaps/" + this.bodyName + "_raw.bin");
                        if (File.Exists(path))
                        {
                            try
                            {
                                this.heightmap = new Heightmap(path);
                            }
                            catch (Exception e)
                            {
                                DebugWindow.Log(String.Format("Error loading the heightmap for {0}\n{1}\n{2}", this.bodyName, e.GetType().Name, e.StackTrace));
                                this.heightmap = null;
                            }
                            if (this.heightmap.width != this.width || this.heightmap.height != this.height)
                            {
                                DebugWindow.Log(String.Format("Heightmap for {0} os not of the correct dimention, must be same dimention as the created resource map ({1}, {2})", this.bodyName, this, width, this.height));
                                this.heightmap = null;
                            }
                        }
                        else
                        {
                            DebugWindow.Log("No heightmap found for " + this.bodyName);
                            this.heightmap = null;
                        }
                    }

                    //Map generation
                    this.simplex = new Simplex(this.currentResource.seed, this.currentResource.octaves, this.currentResource.persistence, this.currentResource.frequency);
                    this.map = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
                    this.rawMap = new Color[this.width * this.height];
                    DebugWindow.Log(String.Format("Creating texture for {0} on {1}", this.resourceName, this.bodyName));
                    this.mapTimer = Stopwatch.StartNew();

                    for (int y = 0; y < this.height; y++)
                    {
                        double latitude = ((y * 180) / (double)this.height) - 90;
                        for (int x = 0; x < this.width; x++)
                        {
                            double longitude = 90 - ((x * 360) / (double)this.width);
                            int index = (y * this.width) + x;

                            if (this.heightmap != null)
                            {
                                double alt = this.heightmap[x, y];
                                if (alt > this.currentResource.maxAltitude || alt < this.currentResource.minAltitude)
                                {
                                    continue;
                                }
                            }

                            if (this.currentResource.biomes.Length > 0 || this.currentResource.excludedBiomes.Length > 0)
                            {
                                string biome = body.GetBiome(latitude, longitude);
                                if (!this.currentResource.biomes.Contains(biome) || this.currentResource.excludedBiomes.Contains(biome))
                                {
                                    continue;
                                }
                            }

                            double density = simplex.noiseNormalized(KRESUtils.SphericalToCartesian(10, 90 - longitude, latitude + 90)) - 1d + this.currentResource.density;
                            if (density > 0)
                            {
                                float a = Mathf.Lerp(0.2f, 1f, (float)(density / this.currentResource.density));
                                if (a > 0.8f) { a = 0.8f; }
                                this.rawMap[index] = this.currentColor.A(a);
                            }
                        }
                        if (y % this.set.maxLinesPerFrame == 0) { yield return new WaitForEndOfFrame(); }
                    }

                    //Save texture
                    this.map.SetPixels(this.rawMap);
                    this.map.Apply();
                    File.WriteAllBytes(Path.Combine(this.path, this.resourceName + ".png"), this.map.EncodeToPNG());
                    Texture2D.Destroy(this.map);
                    this.mapTimer.Stop();
                    DebugWindow.Log(String.Format("Map for {0} on {1} created in {2}ms", this.resourceName, this.bodyName, this.mapTimer.ElapsedMilliseconds));
                }
            }
            Resources.UnloadUnusedAssets();
            this.progressbar.SetValue(1);
            this.amountComplete = 1;
            DebugWindow.Log(String.Format("Map generation took a total of {0:0.000}s", this.time));
            _generated = true;
            this.settings.SetValue("generated", bool.TrueString);
            ConfigNode node = new ConfigNode();
            node.AddNode(this.settings);
            node.Save(KRESUtils.dataURL);
            ResourceLoader.instance.Load();
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (this.visible)
            {
                this.window = GUI.Window(this.id, this.window, Window, "KRES Resource Loader", skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.BeginGroup(new Rect(10, 10, 380, 130));
            if (amountComplete == 1 && ResourceLoader.loaded) { GUI.Label(new Rect(0, 20, 380, 15), "Complete!", skins.label); }
            else if (amountComplete != 1) { GUI.Label(new Rect(0, 20, 380, 15), String.Concat("Currently generating: ", this.bodyName, " - ", this.resourceName), skins.label); }
            else if (!ResourceLoader.loaded) { GUI.Label(new Rect(0, 20, 380, 15), "Loading resources...", skins.label); }
            GUI.BeginGroup(new Rect(5, 50, 380, 30));
            if (amountComplete == 1 && !ResourceLoader.loaded) { this.progressbar.SetValue(ResourceLoader.instance.loadPercent); }
            this.progressbar.Draw();
            GUI.EndGroup();
            if (this.amountComplete == 1 && ResourceLoader.loaded)
            {
                if (this.timer.IsRunning)
                {
                    this.progressbar.SetValue(1);
                    this.timer.Stop();
                    this.time = this.timer.Elapsed.TotalSeconds;
                }
                if (GUI.Button(new Rect(155, 80, 80, 25), "Close", skins.button))
                {
                    this.visible = false;
                    InputLockManager.RemoveControlLock("KRESMapGenerator");
                }
                GUI.Label(new Rect(240, 80, 140, 15), String.Format("Elapsed time: {0:0.000}s", this.time), skins.label);
            }
            else
            {
                if (amountComplete == 1d)
                {
                    GUI.Label(new Rect(0, 80, 380, 15), String.Format("Please wait for resources to load. Do not leave the scene.\nElapsed time: {0:0.000}s", this.timer.Elapsed.TotalSeconds), skins.label);
                }
                else { GUI.Label(new Rect(0, 80, 380, 20), String.Format("Please wait for map generation to finish. Do not leave the scene.\nElapsed time: {0:0.000}s", this.timer.Elapsed.TotalSeconds), skins.label); }
            }
            GUI.EndGroup();
        }
        #endregion

        #region Unloading
        private void OnDestroy()
        {
            if (HighLogic.LoadedScene == GameScenes.MAINMENU && generated)
            {
                _generated = false;
            }
        }
        #endregion
    }
}
