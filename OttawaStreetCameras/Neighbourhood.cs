using System.Collections.Generic;
using Windows.Data.Json;

namespace OttawaStreetCameras {
    public class Neighbourhood {
        private string name, nameFr;
        public readonly int id;
        List<List<LatLng>> boundaries = new List<List<LatLng>>();

        public Neighbourhood(JsonObject vals) {
            JsonObject props = vals.GetNamedObject("properties");
            name = props.GetNamedString("Name");
            nameFr = props.GetNamedString("Name_FR");
            id = (int)props.GetNamedNumber("ONS_ID");

            JsonObject geo = vals.GetNamedObject("geometry");
            JsonArray neighbourhoodZones = new JsonArray();


            if (geo.GetNamedString("type") == "Polygon") {
                neighbourhoodZones.Add(geo.GetNamedArray("coordinates"));
            } else {
                neighbourhoodZones = geo.GetNamedArray("coordinates");
            }

            for (uint i = 0; i < neighbourhoodZones.Count; i++) {
                JsonArray neighbourhoodPoints = neighbourhoodZones.GetArrayAt(i).GetArrayAt(0);
                List<LatLng> list = new List<LatLng>();
                for (uint it = 0; it < neighbourhoodPoints.Count; it++) {
                    list.Add(new LatLng(neighbourhoodPoints.GetArrayAt(it).GetNumberAt(1), neighbourhoodPoints.GetArrayAt(it).GetNumberAt(0)));
                }
                boundaries.Add(list);
            }
        }

        public bool ContainsCamera(Camera camera) {
            int intersectCount = 0;
            LatLng cameraLocation = new LatLng(camera.lat, camera.lng);

            foreach (List<LatLng> vertices in boundaries) {
                for (int j = 0; j < vertices.Count - 1; j++) {
                    if (RayCastIntersect(cameraLocation, vertices[j], vertices[j + 1])) {
                        intersectCount++;
                    }
                }
            }
            return ((intersectCount % 2) == 1); // odd = inside, even = outside
        }

        private bool RayCastIntersect(LatLng location, LatLng vertA, LatLng vertB) {

            double aY = vertA.latitude;
            double bY = vertB.latitude;
            double aX = vertA.longitude;
            double bX = vertB.longitude;
            double pY = location.latitude;
            double pX = location.longitude;

            if ((aY > pY && bY > pY) || (aY < pY && bY < pY) || (aX < pX && bX < pX)) {
                return false; // a and b can't both be above or below pt.y, and a or
                              // b must be east of pt.x
            }

            double m = (aY - bY) / (aX - bX); // Rise over run
            double bee = (-aX) * m + aY; // y = mx + b
            double x = (pY - bee) / m; // algebra is neat!

            return x > pX;
        }

        public string GetName() {
            return true ? nameFr : name;
        }

        public override string ToString() {
            return GetName();
        }

    }
}