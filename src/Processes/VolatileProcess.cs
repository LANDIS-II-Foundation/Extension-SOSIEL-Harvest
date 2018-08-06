using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Processes
{
    public abstract class VolatileProcess
    {
        protected abstract void EqualToOrAboveFocalValue();
        protected abstract void EqualToOrBelowFocalValue();
        protected abstract void Maximize();
        protected abstract void Minimize();


        protected void SpecificLogic(string tendency)
        {
            switch(tendency)
            {
                case "EqualToOrAboveFocalValue":
                    {
                        EqualToOrAboveFocalValue();
                        break;
                    }
                case "EqualToOrBelowFocalValue":
                    {
                        EqualToOrBelowFocalValue();
                        break;
                    }
                case "Maximize":
                    {
                        Maximize();
                        break;
                    }
                case "Minimize":
                    {
                        Minimize();
                        break;
                    }
                default:
                    throw new Exception("Unknown managing of goal");
            }
        }
    }
}
