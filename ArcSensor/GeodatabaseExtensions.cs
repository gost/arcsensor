using ArcGIS.Core.Data;

namespace ArcSensor
{
    public static class GeodatabaseExtensions
    {
        public static bool HasFeatureClass(this Geodatabase gdb, string name)
        {
            try
            {
                var table = gdb.OpenDataset<FeatureClass>(name);
                return true;

            }
            catch (GeodatabaseTableException)
            {
            }
            return false;
        }

        public static FeatureClass GetFeatureClass(this Geodatabase gdb, string name)
        {
            FeatureClass fc = null;
            try
            {
                fc = gdb.OpenDataset<FeatureClass>(name);
            }
            catch (GeodatabaseTableException)
            {
            }
            return fc;
        }

    }
}
