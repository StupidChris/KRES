using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KRES.Extensions;

namespace KRES
{
    public class ResourceInfoLibrary
    {
        public class ResourceInfo
        {
            #region Propreties
            private PartResourceDefinition _resource = new PartResourceDefinition();
            public PartResourceDefinition resource
            {
                get { return this._resource; }
            }

            private string _name = string.Empty;
            public string name
            {
                get { return this._name; }
            }

            private string _realName = string.Empty;
            public string realName
            {
                get { return this._realName; }
            }

            private Color _colour = KRESUtils.blankColour;
            public Color colour
            {
                get { return this._colour; }
            }
            #endregion

            #region Constructor
            public ResourceInfo(ConfigNode node)
            {
                node.TryGetValue("name", ref this._name);
                _resource = PartResourceLibrary.Instance.GetDefinition(this.name);
                node.TryGetValue("realName", ref this._realName);
                node.TryGetValue("colour", ref this._colour);
            }
            #endregion
        }

        #region Instance
        private static ResourceInfoLibrary _instance;
        public static ResourceInfoLibrary instance 
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResourceInfoLibrary();
                }
                return _instance;
            }
        }
        #endregion

        #region Propreties
        private List<ResourceInfo> _resources = new List<ResourceInfo>();
        /// <summary>
        /// Contains the resources info
        /// </summary>
        public List<ResourceInfo> resources
        {
            get { return this._resources; }
            set { this._resources = value; }
        }
        #endregion

        #region Initialisation
        private ResourceInfoLibrary()
        {
            _resources.Clear();
            _resources.AddRange(GameDatabase.Instance.GetConfigNodes("RESOURCE_DEFINITION").Select(n => new ResourceInfo(n)));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gets the ResourceInfo of the given name
        /// </summary>
        /// <param name="name">Name of the resource to find</param>
        public ResourceInfo GetResource(string name)
        {
            return _resources.Find(r => r.name == name);
        }
        #endregion
    }
}
