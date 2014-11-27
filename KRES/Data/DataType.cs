using System.Collections.Generic;

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
        public DataType(string type)
        {
            this._type = KRESUtils.GetResourceType(type);
            foreach (CelestialBody body in KRESUtils.GetRelevantBodies(type))
            {
                _bodies.Add(new DataBody(body.bodyName, type));
            }
        }

        public DataType(ConfigNode type)
        {
            this._type = KRESUtils.GetResourceType(type.name);
            foreach (ConfigNode body in type.nodes)
            {
                _bodies.Add(new DataBody(body, type.name));
            }
        }
        #endregion

        #region Methods
        public DataBody GetBody(string name)
        {
            return this.bodies.Find(b => b.name == name);
        }
        #endregion
    }
}
