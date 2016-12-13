using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OttawaStreetCameras {
    public class Camera {
        public string name;
        public string id;
        public Camera(string name, string id) {
            this.name = name;
            this.id = id;
        }
        public override string ToString() {
            return name;
        }
    }
}
