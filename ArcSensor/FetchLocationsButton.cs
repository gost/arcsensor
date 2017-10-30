using System.Collections.Generic;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using SensorThings.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using Newtonsoft.Json.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using SensorThings.Client;
using ArcSensor.Core;
using ArcGIS.Core.Data;
using System;

namespace ArcSensor
{
    internal class FetchLocationsButton : Button
    {
        protected async override void OnClick()
        {
            var map = MapView.Active.Map;


            var fl = FeatureClassCreator.GetFeatureLayer(map, Settings.StLocationsTablename);

            if (fl == null)
            {
                fl = await QueuedTask.Run(() => AddLocationsLayer()); 
            }

            SetRenderer(fl);
            var count = await FeatureClassCreator.GetCount(fl);
            if (count == 0)
            {
                var server = Settings.STServer; ;
                var client = new SensorThingsClient(server);
                var locations = client.GetLocationCollection().Items;

                await CreateLocations(fl, locations);
            }
        }


        private async Task<FeatureLayer> AddLocationsLayer()
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
            return pointFeatureLayer;
        }


        private void SetRenderer(FeatureLayer featurelayer)
        {
            QueuedTask.Run(() =>
            {
                var symbol = SymbolFactory.Instance.ConstructPointSymbol(ColorFactory.Instance.RedRGB, 22, SimpleMarkerStyle.Star);
                var renderer = (CIMSimpleRenderer)featurelayer.GetRenderer();
                renderer.Symbol = symbol.MakeSymbolReference();

                featurelayer.SetRenderer(renderer);
            });
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
                        var atts = new Dictionary<string, object>();
                        atts.Add("id", location.Id);
                        atts.Add("Shape", newMapPoint);
                        atts.Add("description", location.Description);
                        createOperation.Create(pointFeatureLayer,atts);
                    }
                }
                return createOperation.ExecuteAsync();
            });
        }
    }

}
