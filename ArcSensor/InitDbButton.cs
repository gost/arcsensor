using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcSensor
{
    internal class InitDbButton : Button
    {
        protected async override void OnClick()
        {
            if (MapView.Active.Map != null)
            {
                var tablename = Settings.StLocationsTablename;

                var projGDBPath = Settings.STGdb;
                var gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(projGDBPath)));

                var pointFeatureLayer = FeatureClassCreator.GetFeatureLayer(MapView.Active.Map, tablename);

                if (pointFeatureLayer == null)
                {
                    var urltable = projGDBPath + @"\" + tablename;
                    await QueuedTask.Run(() =>
                    {
                        pointFeatureLayer = LayerFactory.Instance.CreateFeatureLayer(new Uri(urltable),
                          MapView.Active.Map, layerName: tablename);
                    });
                }
            }
            else
            {
                MessageBox.Show("Activate mapview first");
            }

        }

        private async Task<bool> InitDb(Map map, string projGDBPath, string tablename)
        {
            await FeatureClassCreator.CreateFeatureClass(projGDBPath, map.SpatialReference, tablename, "point");
            var fl = FeatureClassCreator.GetFeatureLayer(map, tablename);
            var fld = new KeyValuePair<string, string>("name", "name");

            await FeatureClassCreator.ExecuteAddFieldTool(fl, fld, "text");
            return true;
        }
    }
}
