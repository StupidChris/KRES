using UnityEngine;
using System.Linq;

namespace KRES.Extensions
{
    public static class ConfigNodeExtensions
    {
        #region TryGetNode
        public static bool TryGetNode(this ConfigNode cfg, string name, ref ConfigNode node)
        {
            if (cfg.HasNode(name))
            {
                node = cfg.GetNode(name);
                return true;
            }
            return false;
        }
        #endregion

        #region TryGetValue
        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref string value)
        {
            if (node.HasValue(name))
            {
                string result = node.GetValue(name);
                if (!string.IsNullOrEmpty(result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was null or empty");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref string[] value)
        {
            if (node.HasValue(name))
            {
                string result = node.GetValue(name);
                if (!string.IsNullOrEmpty(result))
                {
                    value = KRESUtils.ParseArray(result);
                    return true;
                }
                DebugWindow.Log(name + " was null or empty");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref int value)
        {
            if (node.HasValue(name))
            {
                int result;
                if (int.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as Int32");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref ushort value)
        {
            if (node.HasValue(name))
            {
                ushort result;
                if (ushort.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as UInt16");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref short value)
        {
            if (node.HasValue(name))
            {
                short result;
                if (short.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as UInt16");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref float value)
        {
            if (node.HasValue(name))
            {
                float result;
                if (float.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as float");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref double value)
        {
            if (node.HasValue(name))
            {
                double result;
                if (double.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as double");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref Color value)
        {
            if (node.HasValue(name))
            {
                if (KRESUtils.TryStringToColor(node.GetValue(name), ref value))
                {
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as Color");
            }
            return false;
        }

        /// <summary>
        /// Get a value and place it into the ref variable and return true. Otherwise returns false and leaves the ref variable untouched.
        /// </summary>
        public static bool TryGetValue(this ConfigNode node, string name, ref bool value)
        {
            if (node.HasValue(name))
            {
                bool result;

                if (bool.TryParse(node.GetValue(name), out result))
                {
                    value = result;
                    return true;
                }
                DebugWindow.Log(name + " was not parsable as bool");
            }
            return false;
        }
        #endregion
    }
}
