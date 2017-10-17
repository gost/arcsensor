using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArcSensor
{
    public class FeatureClassCreator
    {
        public static Task<int> GetCount(FeatureLayer featureLayer)
        {
            return QueuedTask.Run(() =>
            {
                var table = featureLayer.GetTable();
                var qf = new QueryFilter { WhereClause = "1=1" };
                var anotherSelection = table.Select(qf, SelectionType.ObjectID, SelectionOption.Normal);
                var count = anotherSelection.GetCount();
                return count;
            });
        }

        public static FeatureLayer GetFeatureLayer(Map map, string TableName)
        {
            var featureLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.Name == TableName).FirstOrDefault();
            return featureLayer;
        }


        public static async Task CreateFeatureClass(string geodatabasepath, SpatialReference spatialReference, string featureclassName, string featureclassType)
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

        public static async Task<bool> ExecuteAddFieldTool(FeatureLayer theLayer, KeyValuePair<string, string> field, string fieldType, int? fieldLength = null, bool isNullable = true)
        {
            try
            {
                return await QueuedTask.Run(() =>
                {
                    var inTable = theLayer.Name;
                    var table = theLayer.GetTable();
                    var dataStore = table.GetDatastore();
                    var workspaceNameDef = dataStore.GetConnectionString();
                    var workspaceName = workspaceNameDef.Split('=')[1];

                    var fullSpec = System.IO.Path.Combine(workspaceName, inTable);
                    System.Diagnostics.Debug.WriteLine($@"Add {field.Key} from {fullSpec}");

                    var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, fieldType.ToUpper(), null, null,
                        fieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");
                    var cts = new CancellationTokenSource();
                    var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, null,
                        (eventName, o) =>
                        {
                            System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                        });
                    return true;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
    }
}
