using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using KRES.Defaults;

namespace KRES.MainMenu
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class MenuOverlay : MonoBehaviour
    {
        #region Fields
        private Rect windowPosition = new Rect(Screen.width - 500f, 10f, 500f, 0);
        private int windowID = Guid.NewGuid().GetHashCode();
        private GUIStyle windowStyle, headingStyle, normalStyle, buttonStyle;

        private string version = string.Empty;

        private Texture2D buttonTextureNormal = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        private Texture2D buttonTextureHover = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        private Texture2D buttonTextureActive = new Texture2D(32, 32, TextureFormat.ARGB32, false);
        #endregion

        private void Start()
        {
            GUIStyle dottyFontStyle = KRESUtils.dottyFontStyle;

            this.buttonTextureNormal.LoadImage(File.ReadAllBytes(Path.Combine(KRESUtils.pluginDataURL, "Button/Normal.png")));
            this.buttonTextureHover.LoadImage(File.ReadAllBytes(Path.Combine(KRESUtils.pluginDataURL, "Button/Hover.png")));
            this.buttonTextureActive.LoadImage(File.ReadAllBytes(Path.Combine(KRESUtils.pluginDataURL, "Button/Active.png")));

            this.version = KRESUtils.assemblyVersion;
            this.windowStyle = new GUIStyle();

            this.headingStyle = new GUIStyle(dottyFontStyle);
            this.headingStyle.normal.textColor = Color.yellow;
            this.headingStyle.alignment = TextAnchor.UpperCenter;
            this.headingStyle.fontSize = 50;
            this.headingStyle.stretchWidth = true;

            this.normalStyle = new GUIStyle(dottyFontStyle);
            this.normalStyle.normal.textColor = Color.white;
            this.normalStyle.alignment = TextAnchor.UpperCenter;
            this.normalStyle.fontSize = 24;
            this.normalStyle.stretchWidth = true;

            this.buttonStyle = new GUIStyle(HighLogic.Skin.button);
            this.buttonStyle.normal.textColor = this.buttonStyle.hover.textColor = this.buttonStyle.active.textColor = Color.yellow;
            this.buttonStyle.normal.background = this.buttonTextureNormal;
            this.buttonStyle.hover.background = this.buttonTextureHover;
            this.buttonStyle.active.background = this.buttonTextureActive;
        }

        private void OnGUI()
        {
            this.windowPosition = GUILayout.Window(this.windowID, this.windowPosition, Window, string.Empty, this.windowStyle);
        }

        private void Window(int windowID)
        {
            GUILayout.Label("KSP Resource Expansion System", this.headingStyle);
            GUILayout.Label("Currently Selected Resource Pack: " + DefaultsLibrary.GetSelectedDefault().name, this.normalStyle);
            if (GUILayout.Button("Select Resource Pack", this.buttonStyle) && !PackSelector.isBeingDisplayed)
            {
                new GameObject("PackSelector", typeof(PackSelector));
            }
        }
    }
}