using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KRES.Data;

namespace KRES
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class ResourceController : MonoBehaviour
    {
        #region Instance
        public static ResourceController instance { get; private set; }
        #endregion

        #region Properties
        private List<ResourceBody> _resourceBodies = new List<ResourceBody>();
        /// <summary>
        /// Gets and sets the global list of resource bodies.
        /// </summary>
        public List<ResourceBody> resourceBodies
        {
            get { return this._resourceBodies; }
            set { this._resourceBodies = value; }
        }

        public bool dataSet
        {
            get
            {
                if (DataManager.Current == null) { return false; }
                return DataManager.Current.data.Count > 0;
            }
        }
        #endregion

        #region Initialisation
        private void Awake()
        {
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows all resource map textures for the specified resource type.
        /// </summary>
        public void ShowResource(string resourceName)
        {
            if (ResourceLoader.loaded)
            {
                DebugWindow.instance.Print("Showing: " + resourceName);
                GetCurrentBody().ResourceItems.Find(i => i.hasMap && i.name == resourceName).map.ShowTexture(GetCurrentBody().Name);
            }
        }

        /// <summary>
        /// Hides all resource map textures for the specified resource type.
        /// </summary>
        public void HideResource(string resourceName)
        {
            if (ResourceLoader.loaded)
            {
                DebugWindow.instance.Print("Hiding: " + resourceName);
                GetCurrentBody().ResourceItems.Find(i => i.hasMap && i.name == resourceName).map.HideTexture(GetCurrentBody().Name);
            }
        }

        /// <summary>
        /// Hides all resource map textures.
        /// </summary>
        public void HideAllResources()
        {
            if (ResourceLoader.loaded)
            {
                DebugWindow.instance.Print("Hiding: All Resources");
                foreach (ResourceBody body in this._resourceBodies)
                {
                    foreach (ResourceItem item in body.ResourceItems.Where(i => i.hasMap))
                    {
                        item.map.HideTexture(body.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Unloads all the resource maps.
        /// </summary>
        public void UnloadResources()
        {
            if (ResourceLoader.loaded)
            {
                foreach (Transform transform in ScaledSpace.Instance.scaledSpaceTransforms)
                {
                    List<Material> materials = new List<Material>(transform.renderer.materials);
                    materials.RemoveAll(m => m.name.Contains("KRESResourceMap"));
                    transform.renderer.materials = materials.ToArray();
                }
            }

            this._resourceBodies.Clear();
            this._resourceBodies.TrimExcess();
        }

        /// <summary>
        /// Returns the ResourceBody of the given name
        /// </summary>
        /// <param name="name">Name of the body to find</param>
        public ResourceBody GetBody(string name)
        {
            return resourceBodies.Find(b => b.Name == name);
        }

        /// <summary>
        /// Returns the ResourceBody associated to the current main body
        /// </summary>
        public ResourceBody GetCurrentBody()
        {
            return resourceBodies.Find(b => b.Name == FlightGlobals.currentMainBody.bodyName);
        }

        /// <summary>
        /// Returns the DataBody associated to the current scanner
        /// </summary>
        /// <param name="scanner">Scanner module to get the body from</param>
        public DataBody GetDataBody(ModuleKresScanner scanner)
        {
            if (dataSet) { return DataManager.Current.data.Find(d => d.type == scanner.scannerType).GetBody(scanner.body.Name); }
            else
            {
                return new DataType(scanner.type).GetBody(scanner.body.Name);
            }
        }
        #endregion
    }
}
