using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KRES
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class DebugWindow : MonoBehaviour
    {
        #region Instance
        public static DebugWindow instance { get; private set; }
        #endregion

        #region Fields
        private Vector2 windowSize = new Vector2(400f, 0f);
        private Rect windowPosition = new Rect();
        private int windowID = Guid.NewGuid().GetHashCode();
        private int numberOfEntries = 5;
        private Queue<string> logEntries = new Queue<string>();
        private bool showTexture = false;
        private string textureName = string.Empty;
        private Texture textureImage = null;
        private float textureScale = 1f;
        private int i = 0;
        private string body = string.Empty;
        private ResourceItem[] items = null;
        #endregion

        #region Properties
        private bool _visible = false;
        /// <summary>
        /// Gets and sets whether the window is visible.
        /// </summary>
        public bool visible
        {
            get { return this._visible; }
            set { this._visible = value; }
        }
        #endregion

        #region Initialisation
        private void Awake()
        {
            // Check for current instance.
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            this.windowPosition = new Rect(this.windowPosition.x, this.windowPosition.y, this.windowSize.x, this.windowSize.y);
            Print("Debug window started.");
        }
        #endregion

        #region Update and Drawing
        private void Update()
        {
            // Toggle the window visibility with the F11 key.
            if (Input.GetKeyDown(KeyCode.F11))
            {
                this._visible = !this._visible;
            }

            if (ResourceLoader.loaded && HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.mainBody.bodyName != body)
            {
                if (items != null)
                {
                    items[i].map.HideTexture(body);
                    Print("Hid " + items[i].name + " around " + body);
                    ClearTexture();
                    i = 0;
                }
                if (FlightGlobals.ActiveVessel.mainBody.bodyName == "Sun")
                {
                    body = string.Empty;
                    Print("Body is now Sun");
                    items = null;
                }
                else
                {
                    body = FlightGlobals.ActiveVessel.mainBody.bodyName;
                    Print("Body is now " + body);
                    items = ResourceController.instance.resourceBodies.Find(b => b.name == body).resourceItems.Where(i => i.hasMap).ToArray();
                }
            }

            else if (!HighLogic.LoadedSceneIsFlight && this.textureImage != null)
            {
                ResourceController.instance.HideAllResources();
                ClearTexture();
                i = 0;
                items = null;
            }
        }

        private void OnGUI()
        {
            if (this._visible)
            {
                this.windowPosition = GUILayout.Window(this.windowID, this.windowPosition, Window, "KRES (Debug)", HighLogic.Skin.window);
            }
        }

        private void Window(int windowID)
        {
            // Draw the log entries.
            GUILayout.BeginVertical(HighLogic.Skin.box);
            foreach (string entry in logEntries)
            {
                GUILayout.Label(entry);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Show next map", HighLogic.Skin.button))
            {
                if (ResourceLoader.loaded)
                {
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        if (body != string.Empty && items.Length > 0)
                        {
                            if (items[i].map.HideTexture(body))
                            {
                                if (textureImage != null) { i++; }
                                if (i > items.Length - 1) { i = 0; }
                                items[i].map.ShowTexture(body);
                                SetTexture(items[i]);
                                Print("Showing " + items[i].resource.name + " around " + body);
                            }
                        }
                    }
                    else { Print("Cannot display map, not in flight mode"); }
                }
                else { Print("Cannot display map, resources are not loaded"); }
            }

            if (GUILayout.Button("Show previous map", HighLogic.Skin.button))
            {
                if (ResourceLoader.loaded)
                {
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        if (body != string.Empty && items.Length > 0)
                        {
                            if (items[i].map.HideTexture(body))
                            {
                                if (textureImage != null) { i--; }
                                if (i < 0) { i = items.Length - 1; }
                                items[i].map.ShowTexture(body);
                                SetTexture(items[i]);
                                Print("Showing " + items[i].resource.name + " around " + body);
                            }
                        }
                    }
                    else { Print("Cannot display map, not in flight mode"); }
                }
                else { Print("Cannot display map, resources are not loaded"); }
            } 

            if (GUILayout.Button("Hide map", HighLogic.Skin.button))
            {
                if (ResourceLoader.loaded)
                {
                    if (HighLogic.LoadedSceneIsFlight && items.Length > 0)
                    {
                        if (body != string.Empty)
                        {
                            if (items[i].map.HideTexture(body))
                            {
                                Print("Hid " + items[i].resource.name + " around " + body);
                                ClearTexture();
                                i = 0;
                            }
                        }
                    }
                    else { Print("Cannot hide map, not in flight mode"); }
                }
                else { Print("Cannot hide map, resources are not loaded"); }
            }

            // If a texture has been set allow it to be displayed.
            if (this.textureImage != null)
            {
                // Draw the toggle button to display the texture.
                if (GUILayout.Toggle(this.showTexture, "Show Texture", HighLogic.Skin.button) != this.showTexture)
                {
                    this.showTexture = !this.showTexture;

                    // If toggled reset the window size.
                    this.windowPosition.width = this.windowSize.x;
                    this.windowPosition.height = this.windowSize.y;
                }

                if (this.showTexture)
                {
                    // Draw the texture name if one has been supplied.
                    if (this.textureName.Length > 0)
                    {
                        GUILayout.Label(this.textureName);
                    }

                    GUILayout.BeginHorizontal();
                    float previousScale = this.textureScale;
                    this.textureScale = GUILayout.HorizontalSlider(this.textureScale, 0.1f, 1f, GUILayout.ExpandWidth(true));
                    GUILayout.Label((this.textureScale * 100f).ToString("F0") + "%");
                    GUILayout.EndHorizontal();

                    if (this.textureScale != previousScale)
                    {
                        this.windowPosition.width = this.windowSize.x;
                        this.windowPosition.height = this.windowSize.y;
                    }
                    GUILayoutOption[] boxOptions = 
                    {
                        GUILayout.Width(this.textureImage.width * this.textureScale),
                        GUILayout.Height(this.textureImage.height * this.textureScale)
                    };
                    GUILayout.Box(this.textureImage, boxOptions);
                }
            }

            GUI.DragWindow();
        }
        #endregion

        #region Print
        /// <summary>
        /// Prints a log entry to the debug window and KSP log file.
        /// </summary>
        public void Print(string entry)
        {
            if (this.logEntries.Count == this.numberOfEntries)
            {
                this.logEntries.Dequeue();
            }

            this.logEntries.Enqueue(entry);
            print("[KRES]: " + entry);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file.
        /// </summary>
        public static void Log(string entry)
        {
            instance.Print(entry);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Bool)
        /// </summary>
        public static void Log(bool value)
        {
            instance.Print("Boolean: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Byte)
        /// </summary>
        public static void Log(byte value)
        {
            instance.Print("Byte: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Signed Byte)
        /// </summary>
        public static void Log(sbyte value)
        {
            instance.Print("SByte: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Short)
        /// </summary>
        public static void Log(short value)
        {
            instance.Print("Int16: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Unsigned short)
        /// </summary>
        public static void Log(ushort value)
        {
            instance.Print("UInt16: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Int)
        /// </summary>
        public static void Log(int value)
        {
            instance.Print("Int32: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Unsigned int)
        /// </summary>
        public static void Log(uint value)
        {
            instance.Print("UInt32: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Long)
        /// </summary>
        public static void Log(long value)
        {
            instance.Print("Int64: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Unsigned long)
        /// </summary>
        public static void Log(ulong value)
        {
            instance.Print("UInt64: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Float)
        /// </summary>
        public static void Log(float value)
        {
            instance.Print("Float: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Double)
        /// </summary>
        public static void Log(double value)
        {
            instance.Print("Double: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Decimal)
        /// </summary>
        public static void Log(decimal value)
        {
            instance.Print("Decimal: " + value);
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector2)
        /// </summary>
        public static void Log(Vector2 value)
        {
            instance.Print("Vector2: XY(" + value.x + ", " + value.y + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector2d)
        /// </summary>
        public static void Log(Vector2d value)
        {
            instance.Print("Vector2d: XY(" + value.x + ", " + value.y + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector3)
        /// </summary>
        public static void Log(Vector3 value)
        {
            instance.Print("Vector3: XYZ(" + value.x + ", " + value.y + ", " + value.z + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector3d)
        /// </summary>
        public static void Log(Vector3d value)
        {
            instance.Print("Vector3d: XYZ(" + value.x + ", " + value.y + ", " + value.z + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector4)
        /// </summary>
        public static void Log(Vector4 value)
        {
            instance.Print("Vector4: XYZW(" + value.x + ", " + value.y + ", " + value.z + ", " + value.w + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Vector4d)
        /// </summary>
        public static void Log(Vector4d value)
        {
            instance.Print("Vector4d: XYZW(" + value.x + ", " + value.y + ", " + value.z + ", " + value.w + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Color)
        /// </summary>
        public static void Log(Color value)
        {
            instance.Print("Color: " + KRESUtils.ColourToString(value));
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Rect)
        /// </summary>
        public static void Log(Rect value)
        {
            instance.Print("Rect: XYWH(" + value.x + ", " + value.y + ", " + value.width + ", " + value.height + ")");
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (String[])
        /// </summary>
        public static void Log(string[] value)
        {
            instance.Print("String[]: " + string.Join(", ", value));
        }

        /// <summary>
        /// Prints a log entry to the debug window and KSP log file. (Array)
        /// </summary>
        public static void Log(Array value)
        {
            instance.Print(value.GetType().Name + "[]: " + value.LongLength);
        }
        #endregion

        #region Texture
        /// <summary>
        /// Sets a texture that can be viewed from within the debug window.
        /// </summary>
        public void SetTexture(ResourceItem item)
        {
            this.textureImage = item.map.GetTexture();
            this.textureName = item.resource.name;
        }

        /// <summary>
        /// Clears the currently displayed texture.
        /// </summary>
        public void ClearTexture()
        {
            this.textureImage = null;
            this.textureName = string.Empty;
        }
        #endregion
    }
}
