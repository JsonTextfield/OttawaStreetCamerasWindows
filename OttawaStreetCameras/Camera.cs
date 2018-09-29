using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace OttawaStreetCameras {
    public class Camera : BilingualObject {
        public string type;
        public int num;
        public Geopoint location;
        public MapIcon mapIcon;
        public BasicGeoposition gp;
        public string neighbourhood = "";
        public bool isFavourite = false;
        public bool isVisible = true;

        public Camera(JsonObject jsonObject) {
            nameFr = jsonObject.GetNamedString("descriptionFr");
            name = jsonObject.GetNamedString("description");
            type = jsonObject.GetNamedString("type");
            num = (int)jsonObject.GetNamedNumber("number");
            if (type.Equals("MTO")) {
                num += 2000;
            }
            id = (int)jsonObject.GetNamedNumber("id");

            gp = new BasicGeoposition {
                Longitude = jsonObject.GetNamedNumber("longitude"),
                Latitude = jsonObject.GetNamedNumber("latitude")
            };
            location = new Geopoint(gp);
            mapIcon = new MapIcon {
                Location = location,
                Title = GetName(),
                Tag = this
            };
            
        }
        public void SetVisibility(bool b) {
            isVisible = b;
            mapIcon.Visible = b;
        }
    }
}