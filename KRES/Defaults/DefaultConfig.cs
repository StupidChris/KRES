using System;
using System.Collections.Generic;
using KRES.Extensions;

namespace KRES.Defaults
{
    public class DefaultConfig
    {
        #region Properties
        private string _name = string.Empty;
        public string name
        {
            get { return this._name; }
        }

        private string _description = string.Empty;
        public string description
        {
            get { return this._description; }
        }

        private List<DefaultBody> _bodies = new List<DefaultBody>();
        public List<DefaultBody> bodies
        {
            get { return this._bodies; }
        }
        #endregion

        #region Initialisation
        public DefaultConfig() { }

        public DefaultConfig(ConfigNode configNode)
        {
            configNode.TryGetValue("name", ref this._name);
            configNode.TryGetValue("description", ref this._description);
            Random random = new Random();
            foreach (ConfigNode bodyNode in configNode.GetNodes("KRES_BODY"))
            {
                if (KRESUtils.IsCelestialBody(bodyNode.GetValue("name")))
                {
                    this._bodies.Add(new DefaultBody(bodyNode, random));
                }
            }
        }
        #endregion

        #region Public Methods
        public DefaultBody GetBody(string name)
        {
            foreach (DefaultBody body in this._bodies)
            {
                if (body.name == name)
                {
                    return body;
                }
            }
            return null;
        }

        public bool HasBody(string name)
        {
            foreach (DefaultBody body in this._bodies)
            {
                if (body.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetBody(string name, ref DefaultBody body)
        {
            foreach (DefaultBody b in this._bodies)
            {
                if (b.name == name)
                {
                    body = b;
                    return true;
                }
            }
            return false;
        }

        public ConfigNode CreateConfigNode(bool addRootNode = false)
        {
            ConfigNode configNode = addRootNode ? new ConfigNode("KRES") : new ConfigNode();

            configNode.AddValue("name", this._name);
            configNode.AddValue("description", this._description);
            configNode.AddValue("generated", false);
            foreach (ResourceType type in KRESUtils.types.Keys)
            {
                DebugWindow.Log("Creating " + type + " node");
                ConfigNode t = configNode.AddNode(KRESUtils.GetTypeString(type));
                foreach (CelestialBody body in KRESUtils.GetRelevantBodies(type))
                {
                    if (HasBody(body.bodyName)) { t.AddNode(this._bodies.Find(b => b.name == body.bodyName).CreateConfigNode(type)); }
                    else { DebugWindow.Log("The " + this._name + " defaults file does not contain a definition in " + type + " for " + body.bodyName); }
                }
            }

            return configNode;
        }
        #endregion
    }
}
