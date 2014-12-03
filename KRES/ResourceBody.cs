using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KRES.Extensions;

namespace KRES
{
    public class ResourceBody
    {
        #region Fields
        public double bodyPercent = 0;
        #endregion

        #region Properties
        private string _name = string.Empty;
        /// <summary>
        /// Gets the name of the celestial body.
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private List<ResourceItem> _resourceItems = new List<ResourceItem>();
        /// <summary>
        /// Gets the resource items associated with this body
        /// </summary>
        public List<ResourceItem> resourceItems
        {
            get { return this._resourceItems; }
        }
        #endregion

        #region Initialisation
        public ResourceBody() { }

        public ResourceBody(string body)
        {
            this._name = body;
        }
        #endregion

        #region Methods
        internal IEnumerator<YieldInstruction> LoadItems(ConfigNode settings, System.Random random)
        {
            double index = -1;
            foreach (ResourceType type in KRESUtils.types.Keys)
            {
                index++;
                if (KRESUtils.GetRelevantBodies(type).Any(b => b.bodyName == this.name))
                {
                    ConfigNode[] nodes = settings.GetNode(KRESUtils.GetTypeString(type)).GetNode(this.name).GetNodes("KRES_DATA");
                    double current = -1;
                    foreach (ConfigNode data in nodes)
                    {
                        current++;
                        string resourceName = string.Empty;
                        data.TryGetValue("name", ref resourceName);
                        if (!PartResourceLibrary.Instance.resourceDefinitions.Contains(resourceName)) { continue; }
                        switch (type)
                        {
                            case ResourceType.ORE:
                                {
                                    string path = Path.Combine(KRESUtils.savePath, "KRESTextures/" + _name + "/" + resourceName + ".png");
                                    if (File.Exists(path))
                                    {
                                        ResourceItem item = new ResourceItem(data, resourceName, this.name, random);
                                        _resourceItems.Add(item);
                                    }
                                    break;
                                }

                            case ResourceType.GAS:
                            case ResourceType.LIQUID:
                                {
                                    ResourceItem item = new ResourceItem(data, resourceName, this.name, type, random);
                                    _resourceItems.Add(item);
                                    break;
                                }

                            default:
                                break;
                        }
                        this.bodyPercent =  (index + (current / (double)nodes.Length)) / 3d;
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }

        public ResourceItem GetItem(string name, string type)
        {
            return resourceItems.Find(i => i.name == name && i.type == KRESUtils.GetResourceType(type));
        }

        public List<ResourceItem> GetItemsOfType(string type)
        {
            return resourceItems.Where(i => i.type == KRESUtils.GetResourceType(type)).ToList();
        }

        public List<ResourceItem> GetItemsOfType(ResourceType type)
        {
            return resourceItems.Where(i => i.type == type).ToList();
        }

        public List<ResourceItem> GetItemsOfName(string name)
        {
            return resourceItems.Where(i => i.name == name).ToList();
        }
        #endregion
    }
}
