using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;
using System;
using System.Text.RegularExpressions;

namespace OttawaStreetCameras {
    public class Camera : IComparable<Camera> {
        public string name, nameFr, type;
        public int id, num;
        public double lat, lng;

        public Camera() { }

        public Camera(JsonObject jsonObject) {

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

        public int CompareTo(Camera other) {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            return rgx.Replace(name, "").CompareTo(rgx.Replace(other.name, ""));
        }
    }
}
