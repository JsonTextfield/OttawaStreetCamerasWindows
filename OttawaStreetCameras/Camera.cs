using Windows.Data.Json;

namespace OttawaStreetCameras
{
    public class Camera : BilingualObject
    {
        public string type;
        public int id, num;
        public double lat, lng;
        public string neighbourhood = "";
        public bool isFavourite = false;
        public bool isVisible = true;

        public Camera(JsonObject jsonObject)
        {
            nameFr = jsonObject.GetNamedString("descriptionFr");
            name = jsonObject.GetNamedString("description");
            type = jsonObject.GetNamedString("type");
            num = (int)jsonObject.GetNamedNumber("number");
            if (type.Equals("MTO")) {
                num += 2000;
            }
            id = (int)jsonObject.GetNamedNumber("id");
            lng = (double)jsonObject.GetNamedNumber("longitude");
            lat = (double)jsonObject.GetNamedNumber("latitude");
        }
    }
}