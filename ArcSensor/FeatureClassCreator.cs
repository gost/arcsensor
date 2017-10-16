using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcSensor
{
    public class FeatureClassCreator
    {
        public static async Task CreateLayer(string geodatabasepath, SpatialReference spatialReference, string featureclassName, string featureclassType)
        {
            List<object> arguments = new List<object>
                  {
                    geodatabasepath,
                    featureclassName,
                    featureclassType,
                    "",
                    "DISABLED",
                    "DISABLED"
                  };
            await QueuedTask.Run(() =>
            {
                // spatial reference
                arguments.Add(spatialReference);
            });

            IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()));
        }
    }
}
