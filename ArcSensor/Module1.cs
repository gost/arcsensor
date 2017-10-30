using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ArcSensor
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ArcSensor_Module"));
            }
        }

        protected override bool CanUnload()
        {
            return true;
        }
    }
}
