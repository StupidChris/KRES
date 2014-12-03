using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KRES.Extensions;

namespace KRES.Data
{
    public class DataBody
    {
        #region Propreties
        private string _name = string.Empty;
        public string name
        {
            get { return this._name; }
        }

        private ResourceType _type = ResourceType.ORE;
        public ResourceType type
        {
            get { return this._type; }
        }

        private double _currentError = 1d;
        public double currentError
        {
            get { return this._currentError; }
            set { this._currentError = value; }
        }
        #endregion

        #region Constructor
        public DataBody(string body, ResourceType type)
        {
            this._name = body;
            this._type = type;
           this. _currentError = 1d;
        }

        public DataBody(ConfigNode body, ResourceType type)
        {
            this._name = body.name;
            this._type = type;
            if (!body.TryGetValue("currentError", ref _currentError))
            {
                _currentError = 1d;
                body.AddValue("currentError", _currentError.ToString("0.00000"));
            }
        }
        #endregion
    }
}
