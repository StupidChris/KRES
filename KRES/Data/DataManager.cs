using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KRES.Extensions;

namespace KRES.Data
{
    public class DataManager : ScenarioModule
    {
        #region Propreties
        private static DataManager _current = null;
        /// <summary>
        /// Gets the current KRESDataManager for this save
        /// </summary>
        public static DataManager current
        {
            get
            {
                if (_current == null)
                {
                    //Borrowed from https://github.com/Majiir/Kethane/blob/master/Plugin/KethaneData.cs#L10-L28
                    Game game = HighLogic.CurrentGame;
                    if (game == null) { return null; }
                    if (!game.scenarios.Any(p => p.moduleName == typeof(DataManager).Name))
                    {
                        ProtoScenarioModule scenario = game.AddProtoScenarioModule(typeof(DataManager), GameScenes.FLIGHT);
                        if (scenario.targetScenes.Contains(HighLogic.LoadedScene)) { scenario.Load(ScenarioRunner.fetch); }
                    }
                    _current =  game.scenarios.Select(s => s.moduleRef).OfType<DataManager>().SingleOrDefault();
                }
                return _current;
            }
        }
        #endregion

        #region Fields
        internal List<DataType> data = new List<DataType>();
        #endregion

        #region Overrides
        public override void OnSave(ConfigNode node)
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            foreach(DataType type in data)
            {
                ConfigNode t = node.AddNode(KRESUtils.GetTypeString(type.type));
                foreach (DataBody body in type.bodies)
                {
                    ConfigNode b = t.AddNode(body.name);
                    b.AddValue("currentError", body.currentError);
                }
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            CheckForDataNodes(node);
            GetNodes(node);
        }
        #endregion

        #region Methods
        private bool CheckForDataNodes(ConfigNode node)
        {
            foreach (string type in KRESUtils.types.Values)
            {
                if (!node.HasNode(type)) { AddNodes(node); }
                ConfigNode t = node.GetNode(type);
                foreach (CelestialBody body in KRESUtils.GetRelevantBodies(type))
                {
                    if (!t.HasNode(body.bodyName)) { AddNodes(node); return false; }
                }
            }
            return true;
        }

        private void AddNodes(ConfigNode node)
        {
            node.ClearNodes();
            foreach (string type in KRESUtils.types.Values)
            {
                ConfigNode t = node.AddNode(type);
                foreach (CelestialBody body in KRESUtils.GetRelevantBodies(type))
                {
                    ConfigNode b = t.AddNode(body.bodyName);
                    b.AddValue("currentError", 1);
                }
            }
        }

        private void GetNodes(ConfigNode node)
        {
            data.Clear();
            foreach (ConfigNode type in node.nodes)
            {
                data.Add(new DataType(type));
            }
        }
        #endregion
    }
}
