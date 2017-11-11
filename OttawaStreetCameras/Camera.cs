using Microsoft.Data.Sqlite;
using System.Diagnostics;
using Windows.Data.Json;

namespace OttawaStreetCameras {
    public class Camera {
        public string name, nameFr, type;
        public int id, num;
        public double lat, lng;

        public Camera() { }
        public Camera(SqliteDataReader query) {
            name = query.GetString(query.GetOrdinal("name"));
            nameFr = query.GetString(query.GetOrdinal("nameFr"));
            type = query.GetString(query.GetOrdinal("owner"));
            id = query.GetInt32(query.GetOrdinal("id"));
            num = query.GetInt32(query.GetOrdinal("num"));
            if (type.Equals("MTO")) {
                num += 2000;
            }
            lat = query.GetDouble(query.GetOrdinal("latitude"));
            lng = query.GetDouble(query.GetOrdinal("longitude"));
        }

        public override string ToString() {
            return name;
        }
    }
}
