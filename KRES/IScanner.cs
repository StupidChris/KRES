using System;
using System.Collections.Generic;
using System.Linq;

namespace KRES
{
    public interface IScanner
    {
        double Scan();
        void Complete();
        void NoResources();
        bool CanActivate();
    }

    public class OrbitalScanner : IScanner
    {
        #region Fields
        private readonly ModuleKresScanner scanner;
        #endregion

        #region Constructor
        public OrbitalScanner(ModuleKresScanner scanner)
        {
            this.scanner = scanner;
        }
        #endregion

        #region Methods
        public double Scan()
        {
            return Math.Pow(2, -Math.Abs(this.scanner.ASL - this.scanner.optimalAltitude) / this.scanner.scaleFactor);
        }

        public void Complete()
        {
            ScreenMessages.PostScreenMessage("Surface scan of " + this.scanner.body.name + " complete, scanner turned off.", 5, ScreenMessageStyle.UPPER_CENTER); ;
        }

        public void NoResources()
        {
            ScreenMessages.PostScreenMessage("No resources on " + this.scanner.body.name + "'s surface", 5, ScreenMessageStyle.UPPER_CENTER);
        }

        public bool CanActivate()
        {
            if (this.scanner.vessel.mainBody.pqsController == null)
            {
                ScreenMessages.PostScreenMessage("No planetary surface to scan", 5, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            return true;
        }
        #endregion
    }

    public class AtmosphericScanner : IScanner
    {
        #region Fields
        private readonly ModuleKresScanner scanner;
        #endregion

        #region Constructor
        public AtmosphericScanner(ModuleKresScanner scanner)
        {
            this.scanner = scanner;
        }
        #endregion

        #region Methods
        public double Scan()
        {
            return Math.Pow(2, -Math.Abs(this.scanner.atmosphericPressure - this.scanner.optimalPressure) / this.scanner.scaleFactor);
        }

        public void Complete()
        {
            ScreenMessages.PostScreenMessage("Atmospheric scan of " + this.scanner.body.name + " complete, scanner turned off.", 5, ScreenMessageStyle.UPPER_CENTER);
        }

        public void NoResources()
        {
            ScreenMessages.PostScreenMessage("No resources in " + this.scanner.body.name + "'s atmosphere", 5, ScreenMessageStyle.UPPER_CENTER);
        }

        public bool CanActivate()
        {
            if (this.scanner.atmosphericPressure == 0)
            {
                ScreenMessages.PostScreenMessage("No atmosphere to scan", 5, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            return true;
        }
        #endregion
    }

    public class OceanicScanner : IScanner
    {
        #region Fields
        private readonly ModuleKresScanner scanner;
        #endregion

        #region Constructor
        public OceanicScanner(ModuleKresScanner scanner)
        {
            this.scanner = scanner;
        }
        #endregion

        #region Methods
        public double Scan()
        {
            return 1;
        }

        public void Complete()
        {
            ScreenMessages.PostScreenMessage("Oceanic scan of " + this.scanner.body.name + " complete, scanner turned off.", 5, ScreenMessageStyle.UPPER_CENTER);
        }

        public void NoResources()
        {
            ScreenMessages.PostScreenMessage("No resources in " + this.scanner.body.name + "'s oceans", 5, ScreenMessageStyle.UPPER_CENTER);
        }

        public bool CanActivate()
        {
            if (!this.scanner.vessel.Splashed)
            {
                ScreenMessages.PostScreenMessage("No ocean to scan", 5, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }
            return true;
        }
        #endregion
    }
}
