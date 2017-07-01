using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace OttawaStreetCameras {
    public class Camera {
        public string name, nameFr, type;
        public int id, num;
        public double lat, lng;

        
        public Camera(JsonObject jsonObject) {
            name = jsonObject.GetNamedString("description");
            nameFr = jsonObject.GetNamedString("descriptionFr");
            type = jsonObject.GetNamedString("type");
            id = (int) jsonObject.GetNamedNumber("id");
            num = (int)jsonObject.GetNamedNumber("number");
            lat = jsonObject.GetNamedNumber("latitude");
            lng = jsonObject.GetNamedNumber("longitude");
        }

        public override string ToString() {
            return name;
        }
    }
}
