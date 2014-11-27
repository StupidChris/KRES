using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KRES.Extensions;

namespace KRES
{
    public class ResourceBody
    {
        #region Properties
        private string name = string.Empty;
        /// <summary>
        /// Gets the name of the celestial body.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        private List<ResourceItem> resourceItems = new List<ResourceItem>();
        /// <summary>
        /// Gets the resource items associated with this body
        /// </summary>
        public List<ResourceItem> ResourceItems
        {
            get { return this.resourceItems; }
        }
        #endregion

        #region Initialisation
        public ResourceBody() { }

        public ResourceBody(string body)
        {
            this.name = body;
        }
        #endregion

        #region Methods
        internal IEnumerator<YieldInstruction> LoadItems(ConfigNode settings, System.Random random)
        {
            foreach (ResourceType type in KRESUtils.types.Keys)
            {
                if (KRESUtils.GetRelevantBodies(type).Any(b => b.bodyName == this.Name))
                {
                    string typeString = KRESUtils.GetTypeString(type);
                    foreach (ConfigNode data in settings.GetNode(typeString).GetNode(this.Name).GetNodes("KRES_DATA"))
                    {
                        string resourceName = string.Empty;
                        data.TryGetValue("name", ref resourceName);
                        if (!PartResourceLibrary.Instance.resourceDefinitions.Contains(resourceName)) { continue; }
                        switch (type)
                        {
                            case ResourceType.ORE:
                                {
                                    string path = Path.Combine(KRESUtils.savePath, "KRESTextures/" + name + "/" + resourceName + ".png");
                                    if (File.Exists(path))
                                    {
                                        ResourceItem item = new ResourceItem(data, resourceName, this.Name, random);
                                        resourceItems.Add(item);
                                    }
                                    break;
                                }

                            case ResourceType.GAS:
                            case ResourceType.LIQUID:
                                {
                                    ResourceItem item = new ResourceItem(data, resourceName, this.Name, typeString, random);
                                    resourceItems.Add(item);
                                    break;
                                }

                            default:
                                break;
                        }
                        yield return new WaitForEndOfFrame();
                    }
                }
            }
        }

        public ResourceItem GetItem(string name, string type)
        {
            return ResourceItems.Find(i => i.name == name && i.type == KRESUtils.GetResourceType(type));
        }

        public List<ResourceItem> GetItemsOfType(string type)
        {
            return ResourceItems.Where(i => i.type == KRESUtils.GetResourceType(type)).ToList();
        }

        public List<ResourceItem> GetItemsOfType(ResourceType type)
        {
            return ResourceItems.Where(i => i.type == type).ToList();
        }

        public List<ResourceItem> GetItemsOfName(string name)
        {
            return ResourceItems.Where(i => i.name == name).ToList();
        }
        #endregion
    }
}
