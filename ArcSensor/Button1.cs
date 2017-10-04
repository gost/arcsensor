using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensorThings.Client;
using System;

namespace ArcSensor
{
    internal class Button1 : Button
    {
        private CIMPointSymbol _pointSymbol = null;

        protected async override void OnClick()
        {
            if (_pointSymbol == null)
                _pointSymbol = await Module1.CreatePointSymbolAsync();

            var server = "http://scratchpad.sensorup.com/OGCSensorThings/v1.0/";
            var client = new SensorThingsClient(server);
            var locations = client.GetLocationCollection().Items;

            foreach(var location in locations)
            {
                var firstfeature = location.Feature;
                var jfeature = firstfeature as JObject;
                var token = jfeature.GetValue("type").ToString();

                if (token == "Point") {
                    var p = jfeature.ToObject<GeoJSON.Net.Geometry.Point>();
                    var mapView = MapView.Active;

                    await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                    {
                        var newMapPoint = MapPointBuilder.CreateMapPoint(p.Coordinates.Longitude, p.Coordinates.Latitude, SpatialReferences.WGS84);

                        var _graphic = MapView.Active.AddOverlay(newMapPoint, _pointSymbol.MakeSymbolReference());
                    });
                }
            }
        }
    }
}
