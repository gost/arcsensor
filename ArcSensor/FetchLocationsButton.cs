using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using SensorThings.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Editing;
using Newtonsoft.Json.Linq;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.CIM;
using SensorThings.Client;

namespace ArcSensor
{
    internal class FetchLocationsButton : Button
    {
        protected async override void OnClick()
        {
            var map = MapView.Active.Map;

            var fl = FeatureClassCreator.GetFeatureLayer(map, Settings.Tablename);

            SetRenderer(fl);
            var count = await FeatureClassCreator.GetCount(fl);
            if (count == 0)
            {
                var server = "http://scratchpad.sensorup.com/OGCSensorThings/v1.0/";
                var client = new SensorThingsClient(server);
                var locations = client.GetLocationCollection().Items;

                await CreateLocations(fl, locations);
            }
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
                        createOperation.Create(pointFeatureLayer, newMapPoint);
                    }
                }
                return createOperation.ExecuteAsync();
            });
        }
    }

}
