using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace KRES
{
    /// <summary>
    /// All the different resource types to be found
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Ore type, mined from the ground
        /// </summary>
        ORE,

        /// <summary>
        /// Liquid type, found in oceans
        /// </summary>
        LIQUID,

        /// <summary>
        /// Gas type, found in atmospheres
        /// </summary>
        GAS
    }

    public static class KRESUtils
    {
        #region Constants
        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        public const double degToRad = Math.PI / 180d;

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        public const double radToDeg = 180d / Math.PI;

        /// <summary>
        /// Local URL to the PluginData folder
        /// </summary>
        public const string localPluginDataURL = "GameData/KRES/Plugins/PluginData";

        /// <summary>
        /// Fast conversion from ResourceType to string
        /// </summary>
        public static readonly Dictionary<ResourceType, string> types = new Dictionary<ResourceType, string>(3)
        {
            #region Values
            { ResourceType.ORE, "ore" },
            { ResourceType.LIQUID, "liquid" },
            { ResourceType.GAS, "gas" }
            #endregion
        };
        #endregion

        #region Properties
        private static bool colourSet = false;
        private static Color _blankColour;
        /// <summary>
        /// Default blank colour
        /// </summary>
        public static Color blankColour
        {
            get
            {
                if (!colourSet) { _blankColour = new Color(1, 1, 1, 0); }
                return _blankColour;
            }
        }

        private static Texture2D _blankTexture = null;
        /// <summary>
        /// Default empty texture
        /// </summary>
        public static Texture2D blankTexture
        {
            get
            {
                if (_blankTexture == null)
                { 
                   _blankTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                   _blankTexture.SetPixel(1, 1, blankColour);
                   _blankTexture.Apply();
                   _blankTexture.Compress(false);
                }
                return _blankTexture;
            }
        }

        private static GUIStyle _boldLabel = null;
        /// <summary>
        /// A bolded GUI label
        /// </summary>
        public static GUIStyle BoldLabel
        {
            get
            {
                if (_boldLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                    style.fontStyle = FontStyle.Bold;
                    _boldLabel = style;
                }
                return _boldLabel;
            }
        }

        private static GUIStyle _dottyFontStyle = null;
        /// <summary>
        /// Returns the text style used in ScreenMessages
        /// </summary>
        public static GUIStyle dottyFontStyle
        {
            get
            {
                if (_dottyFontStyle == null)
                {
                    ScreenMessages messages = (GameObject.FindObjectOfType(typeof(ScreenMessages)) as ScreenMessages);

                    foreach (GUIStyle style in messages.textStyles)
                    {
                        if (style.font.name == "dotty")
                        {
                            _dottyFontStyle = style;
                            return _dottyFontStyle;
                        }
                    }
                    _dottyFontStyle = HighLogic.Skin.label;
                }
                return _dottyFontStyle;
            }
        }

        /// <summary>
        /// Returns the absolute path to the currently used Save folder
        /// </summary>
        public static string savePath
        {
            get { return Path.Combine(KSPUtil.ApplicationRootPath, "saves/" + HighLogic.fetch.GameSaveFolder); }
        }

        /// <summary>
        /// Returns the URL to the curently used data config file
        /// </summary>
        public static string dataURL
        {
            get { return Path.Combine(savePath, "KRESData.cfg"); }
        }

        /// <summary>
        /// Absolute path to the PluginData folder
        /// </summary>
        public static string pluginDataURL
        {
            get { return Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL); }
        }

        /// <summary>
        /// Current version of KRES
        /// </summary>
        public static string assemblyVersion
        {
            get
            {
                System.Version version = new System.Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                if (version.Revision == 0)
                {
                    if (version.Build == 0) { return "v" + version.ToString(2); }
                    return "v" + version.ToString(3);
                }
                return "v" + version.ToString();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// If the following string is the name of a given CelstialBody or not
        /// </summary>
        /// <param name="name">Name of the body to find</param>
        public static bool IsCelestialBody(string name)
        {
            return FlightGlobals.Bodies.Any(body => body.bodyName == name);
        }

        /// <summary>
        /// Finds and returns the CelestialBody of the given name, returns false if it fails
        /// </summary>
        /// <param name="name">Name of the CelestialBody to find</param>
        /// <param name="result">Value to store the result into</param>
        public static bool TryParseCelestialBody(string name, out CelestialBody result)
        {
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (body.bodyName == name)
                {
                    result = body;
                    return true;
                }
            }
            result = null;
            return false;
        }

        /// <summary>
        /// Transforms a given cartesian Vector3d into a spherical Vector3d
        /// </summary>
        /// <param name="cartesian">Cartesian vector to transform</param>
        public static Vector3d CartesianToSpherical(Vector3d cartesian)
        {
            double rho = cartesian.magnitude;
            double theta = Math.Atan(cartesian.y / cartesian.x) * radToDeg;
            double phi = Math.Acos(cartesian.z / rho) * radToDeg;
            return new Vector3d(rho, theta, phi);
        }

        /// <summary>
        /// Transforms cartesian set of coordinates into a spherical Vector3d
        /// </summary>
        /// <param name="x">X cartesian coordinate</param>
        /// <param name="y">Y cartesian coordinate</param>
        /// <param name="z">Z cartesian coordinate</param>
        public static Vector3d CartesianToSpherical(double x, double y, double z)
        {
            double rho = Math.Sqrt((x * x) + (y * y) + (z * z));
            double theta = Math.Atan(y / x) * radToDeg;
            double phi = Math.Acos(z / rho) * radToDeg;
            return new Vector3d(rho, theta, phi);
        }

        /// <summary>
        /// Transforms a sp^herical Vector3d into a cartesian Vector3d
        /// </summary>
        /// <param name="spherical">Spherical vector3d to transform</param>
        public static Vector3d SphericalToCartesian(Vector3d spherical)
        {
            double radY = spherical.y * degToRad, radZ = spherical.z * degToRad;
            double x = spherical.x * Math.Sin(radZ) * Math.Cos(radY);
            double y = spherical.x * Math.Sin(radZ) * Math.Sin(radY);
            double z = spherical.x * Math.Cos(radZ);
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Transforms a spherical set of coordinate into a cartesian Vector3d
        /// </summary>
        /// <param name="rho">Length from the origin to the point</param>
        /// <param name="theta">Angle on the horizontal plane in degrees</param>
        /// <param name="phi">Angle on the vertical plane in degrees</param>
        public static Vector3d SphericalToCartesian(double rho, double theta, double phi)
        {
            double phiRad = phi * degToRad, thetaRad = theta * degToRad;
            double x = rho * Math.Sin(phiRad) * Math.Cos(thetaRad);
            double y = rho * Math.Sin(phiRad) * Math.Sin(thetaRad);
            double z = rho * Math.Cos(phiRad);
            return new Vector3d(x, y, z);
        }

        /// <summary>
        /// Returns a nicely formatted string associated to a Color value
        /// </summary>
        /// <param name="value">Color to get the string of</param>
        public static string ColourToString(Color value)
        {
            return String.Concat(value.r, ", ", value.g, ", ", value.b, ", ", value.a);
        }

        /// <summary>
        /// Parses a single of multiple elements separated by a comma into an array of string elements
        /// </summary>
        /// <param name="text">String to get the array from</param>
        public static string[] ParseArray(string text)
        {
            return text.Split(',').Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Gets a colour from a string, returns a blank Color object if could not parse
        /// </summary>
        /// <param name="vectorString">Text to get the color from</param>
        public static Color StringToColour(string vectorString)
        {
            string[] splitValue = ParseArray(vectorString);

            if (splitValue.Length == 3)
            {
                float r, g, b;

                if (!float.TryParse(splitValue[0], out r)) return blankColour;
                if (!float.TryParse(splitValue[1], out g)) return blankColour;
                if (!float.TryParse(splitValue[2], out b)) return blankColour;

                return new Color(r, g, b);
            }
            else if (splitValue.Length == 4)
            {
                float r, g, b, a;

                if (!float.TryParse(splitValue[0], out r)) return blankColour;
                if (!float.TryParse(splitValue[1], out g)) return blankColour;
                if (!float.TryParse(splitValue[2], out b)) return blankColour;
                if (!float.TryParse(splitValue[3], out a)) return blankColour;

                return new Color(r, g, b, a);
            }
            return blankColour;
        }

        /// <summary>
        /// Tries to parse a Color from a given string, returns false if it fails
        /// </summary>
        /// <param name="vectorString">Text to get the Color from</param>
        /// <param name="value">Color value to store the result into</param>
        public static bool TryStringToColor(string vectorString, ref Color value)
        {
            string[] splitValue = ParseArray(vectorString);

            if (splitValue.Length == 3)
            {
                float r, g, b;

                if (!float.TryParse(splitValue[0], out r)) return false;
                if (!float.TryParse(splitValue[1], out g)) return false;
                if (!float.TryParse(splitValue[2], out b)) return false;

                value = new Color(r, g, b);
                return true;
            }
            else if (splitValue.Length == 4)
            {
                float r, g, b, a;

                if (!float.TryParse(splitValue[0], out r)) return false;
                if (!float.TryParse(splitValue[1], out g)) return false;
                if (!float.TryParse(splitValue[2], out b)) return false;
                if (!float.TryParse(splitValue[3], out a)) return false;

                value = new Color(r, g, b, a);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the associated ResourceType to the given string
        /// </summary>
        /// <param name="name">String repersentation of the ResourceType to get</param>
        public static ResourceType GetResourceType(string name)
        {
            return types.First(pair => pair.Value == name).Key;
        }

        /// <summary>
        /// Returns the string associated to a given ResourceType
        /// </summary>
        /// <param name="type">ResourceType to get the string from</param>
        public static string GetTypeString(ResourceType type)
        {
            return types.First(pair => pair.Key == type).Value;
        }

        /// <summary>
        /// Transforms a certain amount of seconds into a formatted string
        /// </summary>
        /// <param name="seconds">Seconds to get time string from</param>
        public static string SecondsToTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            StringBuilder builder = new StringBuilder();
            if (time.Seconds > 0) { builder.Append(time.Seconds + "s"); }
            if (time.Minutes > 0)
            {
                if (builder.Length > 0) { builder.Insert(0, ' '); }
                builder.Insert(0, time.Minutes + "m");
            }
            if (time.Hours > 0)
            {
                if (builder.Length > 0) { builder.Insert(0, ' '); }
                builder.Insert(0, time.Hours + "h");
            }
            if (time.Days > 0)
            {
                if (builder.Length > 0) { builder.Insert(0, ' '); }
                builder.Insert(0, time.Days + "d");
            }
            if (builder.Length == 0) { builder.Append("0s"); }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the relevant list of CelestialBody for the given resource type
        /// </summary>
        /// <param name="type">String representation of the ResourceType wished for, must be "ore", "gas, or "liquid"</param>
        public static List<CelestialBody> GetRelevantBodies(string type)
        {
            switch (type)
            {
                case "ore":
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.pqsController != null));

                case "gas":
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.atmosphere));

                case "liquid":
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.ocean));

                default:
                    return new List<CelestialBody>();
            }
        }

        /// <summary>
        /// Returns the relevant list of CelestialBody for the given resource type
        /// </summary>
        /// <param name="type">ResourceType to match</param>
        public static List<CelestialBody> GetRelevantBodies(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.ORE:
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.pqsController != null));

                case ResourceType.GAS:
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.atmosphere));

                case ResourceType.LIQUID:
                    return new List<CelestialBody>(FlightGlobals.Bodies.Where(b => b.ocean));

                default:
                    return new List<CelestialBody>();
            }
        }

        /// <summary>
        /// Returns a coloured GUIStyle label for a given resource
        /// </summary>
        /// <param name="name">Name of the resource to get the colour from</param>
        public static GUIStyle GetLabelOfColour(string name)
        {
            Color colour = ResourceInfoLibrary.instance.GetResource(name).colour;
            GUIStyle style = new GUIStyle(HighLogic.Skin.label);
            style.normal.textColor = colour;
            style.hover.textColor = colour;
            style.fontStyle = FontStyle.Bold;
            return style;
        }

        /// <summary>
        /// Clamps a double value between 0 and 1, if the value is out of the boundaries, it jumps back to 0 or 1
        /// </summary>
        /// <param name="value">Value to clamp</param>
        public static double Clamp01(double value)
        {
            if (value > 1) { return 2 - value; }
            else if (value < 0) { return -value; }
            return value;
        }
        #endregion
    }
}