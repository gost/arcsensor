using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using SensorThings.Client;
using SensorThings.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcSensor
{
    internal class Button1 : Button
    {
        private CIMPointSymbol _pointSymbol = null;

        protected async override void OnClick()
        {
            var tablename = "st_locations";

            var projGDBPath = Project.Current.DefaultGeodatabasePath;
            var gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(projGDBPath)));

            if (!gdb.HasFeatureClass("st_locations"))
            {
                await FeatureClassCreator.CreateLayer(projGDBPath, MapView.Active.Map.SpatialReference, tablename, "point");
            }

            var pointFeatureLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Where(
                lyr => lyr.Name == tablename).FirstOrDefault();


            if (_pointSymbol == null)
                _pointSymbol = await Module1.CreatePointSymbolAsync();

            if (pointFeatureLayer == null)
            {
                var urltable = projGDBPath + @"\" + tablename;
                await QueuedTask.Run(() =>
                {
                    pointFeatureLayer = LayerFactory.Instance.CreateFeatureLayer(new Uri(urltable),
                      MapView.Active.Map, layerName: tablename);
                });
            }

            var server = "http://scratchpad.sensorup.com/OGCSensorThings/v1.0/";
            var client = new SensorThingsClient(server);
            var locations = client.GetLocationCollection().Items;

            await CreateLocations(pointFeatureLayer, locations);
        }


        private Task<bool> CreateLocations(FeatureLayer pointFeatureLayer, IReadOnlyList<Location> locations)
        {
            return QueuedTask.Run(() =>
            {
                var createOperation = new EditOperation()
                {
                    Name = "Generate points",
                    SelectNewFeatures = false
                };

                foreach (var location in locations)
                {
                    var firstfeature = location.Feature;
                    var jfeature = firstfeature as JObject;
                    var token = jfeature.GetValue("type").ToString();

                    if (token == "Point")
                    {
                        var p = jfeature.ToObject<GeoJSON.Net.Geometry.Point>();
                        var mapView = MapView.Active;

                        var newMapPoint = MapPointBuilder.CreateMapPoint(p.Coordinates.Longitude, p.Coordinates.Latitude, SpatialReferences.WGS84);
                        createOperation.Create(pointFeatureLayer, newMapPoint);
                    }
                }
                return createOperation.ExecuteAsync();
            });
        }
    }
}
