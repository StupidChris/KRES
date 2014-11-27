using System;
using System.Collections.Generic;
using System.Linq;
using KRES.Extensions;

namespace KRES.Defaults
{
    public class DefaultBody
    {
        #region Properties
        private string _name = string.Empty;
        public string name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        private List<DefaultResource> _resources = new List<DefaultResource>();
        public List<DefaultResource> resources
        {
            get { return this._resources; }
            set { this._resources = value; }
        }
        #endregion

        #region Initialisation
        public DefaultBody(ConfigNode configNode, Random random)
        {
            configNode.TryGetValue("name", ref this._name);
            foreach (ConfigNode resourceNode in configNode.GetNodes("KRES_RESOURCE"))
            {
                this._resources.Add(new DefaultResource(resourceNode, random));
            }
        }
        #endregion
        
        #region Public Methods
        public DefaultResource GetResource(string name)
        {
            foreach (DefaultResource resource in this._resources)
            {
                if (resource.name == name)
                {
                    return resource;
                }
            }
            return null;
        }

        public DefaultResource GetResourceOfType(string name, string type)
        {
            return this.resources.Find(r => r.name == name && r.type == type);
        }

        public bool HasResource(string name)
        {
            foreach (DefaultResource resource in this._resources)
            {
                if (resource.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public ConfigNode CreateConfigNode(string type)
        {
            ConfigNode configNode = new ConfigNode(this.name);
            foreach (DefaultResource resource in this.resources.Where(r => r.type == type))
            {
                configNode.AddNode(resource.CreateConfigNode());
            }
            return configNode;
        }
        #endregion 
    }
}
