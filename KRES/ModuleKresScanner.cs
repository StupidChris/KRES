using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KRES.Extensions;
using KRES.Data;

namespace KRES
{
    public class ModuleKresScanner : PartModule, IModuleInfo
    {
        public class InputResource
        {
            #region Properties
            private readonly string _name = string.Empty;
            public string name
            {
                get { return this._name; }
            }

            private readonly double _rate = 0;
            public double rate
            {
                get { return this._rate; }
            }
            #endregion

            #region Constructor
            public InputResource(ConfigNode node)
            {
                node.TryGetValue("name", ref this._name);
                if (string.IsNullOrEmpty(this._name))
                {
                    DebugWindow.Log("Nameless scanner input resource, skipping");
                    return;
                }
                if (!PartResourceLibrary.Instance.resourceDefinitions.Contains(this._name))
                {
                    DebugWindow.Log("Scanner could not find resource definition for " + this._name);
                    this._name = string.Empty;
                    return;
                }
                node.TryGetValue("rate", ref this._rate);
                if (rate <= 0)
                {
                    DebugWindow.Log("Resource draining rate for " + this._name + " must be superior to zero");
                    this._name = string.Empty;
                    return;
                }
            }
            #endregion
        }

        #region Constants
        private const double delta = 0.005;
        #endregion

        #region KSPFields
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Optimal alt"), UI_FloatRange(minValue = 100000f, maxValue = 2000000f, stepIncrement = 5000f)]
        public float optimalAltitude = 100000f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Optimal pressure"), UI_FloatRange(minValue = 0.1f, maxValue = 1f, stepIncrement = 0.05f)]
        public float optimalPressure = 0.1f;
        [KSPField]
        public float scaleFactor = 0.02f;
        [KSPField]
        public float maxPrecision = 0.05f;
        [KSPField]
        public float scanningSpeed = 3600f;
        [KSPField]
        public bool isTweakable = true;
        [KSPField]
        public string type = "ore";
        [KSPField(isPersistant = true)]
        public bool scanning = false;
        [KSPField(guiActive = true, guiName = "Status")]
        public string status = "Idle";
        [KSPField(guiActive = false, guiFormat = "0.000", guiName = "Pressure")]
        public float pressure = 0f;
        #endregion

        #region Fields
        //Scanning
        internal ResourceBody body = new ResourceBody();
        internal ResourceType scannerType = ResourceType.ORE;
        internal List<ResourceItem> items = new List<ResourceItem>();
        public List<InputResource> resources = new List<InputResource>();
        public ConfigNode node = null;
        DataBody data = null;
        internal double currentError = 1d;
        internal IScanner scanner = null;
        private double a = 0, b = 0;

        //GUI
        private int id = Guid.NewGuid().GetHashCode();
        internal bool visible = false;
        internal Rect window = new Rect();
        internal string location = string.Empty;
        internal string presence = string.Empty;
        private GUISkin skins = HighLogic.Skin;
        private Vector2 scroll = new Vector2();
        #endregion

        #region Propreties
        internal double ASL
        {
            get { return FlightGlobals.getAltitudeAtPos(this.part.transform.position); }
        }

        internal double atmosphericPressure
        {
            get
            {
                double pressure = FlightGlobals.getStaticPressure(ASL, this.vessel.mainBody);
                return pressure > 1E-6 ? pressure : 0;
            }
        }

        private bool dataVisible
        {
            get { return this.data.currentError <= 0.75; }
        }

        private bool percentageVisible
        {
            get { return this.data.currentError <= 0.5; }
        }

        private double scanFactor
        {
            get { return this.a * (this.currentError - this.b) * (double)TimeWarp.fixedDeltaTime; }
        }
        #endregion

        #region Part GUI
        [KSPEvent(guiName = "Start scanning", active = true, guiActive = true, externalToEVAOnly = false, guiActiveUnfocused = true, unfocusedRange = 5f)]
        public void GUIToggleScanning()
        {
            if (this.scanning) { DeactivateScanner(false); }
            else { ActivateScanner(); }
        }

        [KSPEvent(guiName = "Toggle window", active = true, guiActive = true)]
        public void GUIToggleWindow()
        {
            if (!this.visible)
            {
                List<ModuleKresScanner> scanners = new List<ModuleKresScanner>(this.vessel.FindPartModulesImplementing<ModuleKresScanner>().Where(m => m != this));
                if (scanners.Any(m => m.visible))
                {
                    ModuleKresScanner scanner = scanners.Single(m => m.visible);
                    scanner.visible = false;
                    this.window.x = scanner.window.x;
                    this.window.y = scanner.window.y;
                }
                this.visible = true;
            }
            else { this.visible = false; }
        }
        #endregion

        #region Functions
        private void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || this.resources.Count <= 0 || FlightGlobals.currentMainBody == null || FlightGlobals.ActiveVessel == null || !this.vessel.loaded || !ResourceController.instance.dataSet) { return; }
            if (this.body.name != this.vessel.mainBody.bodyName)
            {
                LoadData();
            }
        }

        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight || this.resources.Count <= 0 || FlightGlobals.currentMainBody == null || FlightGlobals.ActiveVessel == null || !this.vessel.loaded || !ResourceController.instance.dataSet) { return; }
            
            if (this.scanning)
            {
                if (this.currentError > this.maxPrecision)
                {
                    double scan = Scan();
                    if (scan != 0)
                    {
                        this.status = "Scanning...";
                        this.currentError += scan;
                        this.data.currentError = this.currentError;
                    }
                    else { this.status = "Not enough resources"; }
                }

                if (this.items.Count > 0 && this.currentError <= this.maxPrecision)
                {
                    this.currentError = this.maxPrecision;
                    this.data.currentError = this.maxPrecision;
                    DeactivateScanner(true);
                    this.scanner.Complete();
                }
                else if (this.items.Count <= 0 && this.percentageVisible)
                {
                    DeactivateScanner(false);
                    this.currentError = -1;
                    this.data.currentError = -1;
                    this.scanner.NoResources();
                    return;
                }
            }
            if (Fields["pressure"].guiActive) { this.pressure = (float)atmosphericPressure; }
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }
            LoadScanner();

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.scanning) { Events["GUIToggleScanning"].guiName = "Stop scanning"; }
                this.window = new Rect(200, 200, 250, 400);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (this.node == null) { this.node = node; }

            if (this.resources.Count == 0 && this.node.HasNode("INPUT"))
            {
                this.resources = new List<InputResource>(this.node.GetNodes("INPUT").Select(n => new InputResource(n)).Where(r => !string.IsNullOrEmpty(r.name)));
            }
        }

        public override string GetInfo()
        {
            StringBuilder builder = new StringBuilder();
            switch (this.type)
            {
                case "ore":
                    builder.AppendLine(String.Format("Orbital scanner")); break;

                case "gas":
                    builder.AppendLine(String.Format("Atmospheric scanner")); break;

                case "liquid":
                   builder.AppendLine(String.Format("Oceanic scanner")); break;

                default:
                    return string.Empty;
            }

            builder.AppendLine(String.Format("Minimal scanning period: {0}", KRESUtils.SecondsToTime(scanningSpeed)));
            builder.Append(String.Format("Minimum error margin: {0:0.00}%", maxPrecision * 100));
            if (type == "ore")
            {
                builder.AppendLine(String.Format("\nOptimal scanning altitude: {0}m", optimalAltitude));
                builder.Append(String.Format("Scale altitude: {0:0.000}m", scaleFactor * optimalAltitude));
            }
            else if (type == "gas")
            {
                builder.AppendLine(String.Format("\nOptimal scanning pressure: {0}atm", optimalPressure));
                builder.Append(String.Format("Scale pressure: {0:0.000}atm", scaleFactor * optimalPressure));
            }

            if (this.resources.Count > 0)
            {
                foreach (InputResource resource in this.resources)
                {
                    builder.AppendLine("\n\n<b><color=#99ff00ff>Input:</color></b>");
                    builder.AppendLine(String.Format("Resource: {0}", resource.name));
                    builder.Append(String.Format("Rate: {0:0.0}/m", resource.rate));
                }
            }

            return builder.ToString();
        }
        #endregion

        #region Methods
        private void ActivateScanner()
        {
            if (this.resources.Count == 0)
            {
                DebugWindow.Log("Cannot activate scanner that has no input resource");
                return;
            }
            else if (!this.scanner.CanActivate()) { return; }

            this.scanning = true;
            Events["GUIToggleScanning"].guiName = "Stop scanning";
            this.status = "Scanning...";
        }

        private void DeactivateScanner(bool complete)
        {
            this.scanning = false;
            Events["GUIToggleScanning"].guiName = "Start scanning";
            if (complete)
            {
                this.status = "Complete";
                Events["GUIToggleScanning"].guiActive = false;
            }
            else { this.status = "Idle"; }
        }

        private void ToggleAllScanners()
        {
            GUIToggleScanning();
            List<ModuleKresScanner> scanners = new List<ModuleKresScanner>(this.vessel.FindPartModulesImplementing<ModuleKresScanner>().Where(m => m != this && m.scannerType == this.scannerType));
            if (scanners.Count > 0)
            {
                foreach (ModuleKresScanner scanner in scanners)
                {
                    if (this.scanning) { scanner.DeactivateScanner(false); }
                    else { scanner.ActivateScanner(); }
                }
            }
        }

        private void LoadScanner()
        {
            try
            {
                this.scannerType = KRESUtils.GetResourceType(this.type);
            }
            catch (Exception)
            {
                DebugWindow.Log(String.Format("Could not load scanner type correctly, must be \"ore\", \"gas\", or \"liquid\". {0} is an invalid type."));
                return;
            }

            this.a = Math.Log(delta / (1 - (double)this.maxPrecision)) / (double)this.scanningSpeed;
            this.b = (double)this.maxPrecision - delta;

            switch (this.scannerType)
            {
                case ResourceType.ORE:
                    {
                        scanner = new OrbitalScanner(this);
                        this.presence = " (surface%):";
                        this.location = "Extractable";
                        Fields["optimalAltitude"].guiActiveEditor = this.isTweakable;
                        Fields["optimalPressure"].guiActiveEditor = false;
                        return;
                    }

                case ResourceType.GAS:
                    {
                        scanner = new AtmosphericScanner(this);
                        this.presence = " (vol/vol):";
                        this.location = "Atmospheric";
                        Fields["pressure"].guiActive = true;
                        Fields["optimalAltitude"].guiActiveEditor = false;
                        Fields["optimalPressure"].guiActiveEditor = this.isTweakable;
                        return;
                    }

                case ResourceType.LIQUID:
                    {
                        scanner = new OceanicScanner(this);
                        this.presence = " (vol/vol):";
                        this.location = "Oceanic";
                        Fields["optimalAltitude"].guiActiveEditor = false;
                        Fields["optimalPressure"].guiActiveEditor = false;
                        return;
                    }

                default:
                    return;
            }
        }

        private void LoadData()
        {
            this.body = ResourceController.instance.GetBody(this.vessel.mainBody.bodyName);
            this.items = body.GetItemsOfType(scannerType);
            this.data = ResourceController.instance.GetDataBody(this.scannerType, this.vessel.mainBody);
            this.currentError = this.data.currentError;
            DeactivateScanner(false);
        }

        private string ItemPercentage(ResourceItem item)
        {
            return (((data.currentError * item.actualError) + 1) * item.actualDensity * 100).ToString("0.00");
        }

        private string ItemError(ResourceItem item)
        {
            return (item.actualDensity * data.currentError * 100).ToString("0.00");
        }

        internal double Scan()
        {
            if (!CheatOptions.InfiniteFuel)
            {
                double[] amounts = new double[this.resources.Count];
                for (int i = 0; i < this.resources.Count; i++)
                {
                    InputResource resource = this.resources[i];
                    double amount = this.part.RequestResource(resource.name, resource.rate * TimeWarp.fixedDeltaTime);
                    if (amount <= 0)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            InputResource r = this.resources[j];
                            this.part.RequestResource(r.name, -amounts[j]);
                        }
                        return 0;
                    }
                    else { amounts[i] = amount; }
                }
            }
            return this.scanner.Scan() * this.scanFactor;
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetModuleTitle()
        {
            return "KRES Scanner";
        }

        public string GetPrimaryField()
        {
            return string.Empty;
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (this.visible)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "KRES Scan Data", skins.window);
            }
        }

        private void Window(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(location + " resources:", KRESUtils.BoldLabel, GUILayout.Width(150));
            scroll = GUILayout.BeginScrollView(scroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box);
            if (this.dataVisible)
            {
                foreach (ResourceItem item in items)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(item.name + presence, KRESUtils.GetLabelOfColour(item.name), GUILayout.Width(120));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(this.percentageVisible ? String.Format("{0} ± {1}%", ItemPercentage(item), ItemError(item)) : "-- ± --%");
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                if (this.scanning) { GUILayout.Label(status, skins.label); }
                else if (this.currentError == -1) GUILayout.Label("No resources detected.", skins.label);
                else { GUILayout.Label("Nothing to show.", skins.label); }
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max error:", skins.label);
            GUILayout.Label(this.percentageVisible ? String.Format("   ± {0}%", (this.data.currentError * 100d).ToString("0.00")) : "   ± --%");
            GUILayout.EndHorizontal();
           if (GUILayout.Button((this.scanning ? "Stop" : "Start") + " scanning", skins.button))
           {
               GUIToggleScanning();
           }
           if (GUILayout.Button("Close", skins.button))
           {
                this.visible = false;
           }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
        #endregion
    }
}
