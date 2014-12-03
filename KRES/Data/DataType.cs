using System.Collections.Generic;
using System.Linq;

namespace KRES.Data
{
    public class DataType
    {
        #region Propreties
        private ResourceType _type = ResourceType.ORE;
        public ResourceType type
        {
            get { return this._type; }
        }

        private List<DataBody> _bodies = new List<DataBody>();
        public List<DataBody> bodies
        {
            get { return this._bodies; }
        }
        #endregion

        #region Contructor
        public DataType(ResourceType type)
        {
            this._bodies = new List<DataBody>();
            foreach (CelestialBody body in KRESUtils.GetRelevantBodies(type))
            {
                this._bodies.Add(new DataBody(body.bodyName, type));
            }
        }

        public DataType(ConfigNode type)
        {
            this._type = KRESUtils.GetResourceType(type.name);
            foreach (ConfigNode body in type.nodes)
            {
                _bodies.Add(new DataBody(body, this._type));
            }
        }
        #endregion

        #region Methods
        public DataBody GetBody(string name)
        {
            if (!this.bodies.Any(b => b.name == name)) { return null; }
            return this.bodies.Find(b => b.name == name);
        }
        #endregion
    }
}
