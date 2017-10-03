using ArcGIS.Desktop.Framework.Contracts;
using Newtonsoft.Json;
using SensorThings.Client;
using System.Windows;

namespace ArcSensor
{
    internal class Button1 : Button
    {
        protected override void OnClick()
        {
            var server = "http://black-pearl:8080/v1.0";
            var client = new SensorThingsClient(server);
            var locations = client.GetLocationCollection().Items;

            var firstfeature = locations[0].Feature;
            var p = JsonConvert.DeserializeObject<GeoJSON.Net.Geometry.Point>(firstfeature.ToString());

            MessageBox.Show($"location: {p.Coordinates.Longitude} , {p.Coordinates.Latitude}");

            // todo: draw locations in map

        }
    }
}
