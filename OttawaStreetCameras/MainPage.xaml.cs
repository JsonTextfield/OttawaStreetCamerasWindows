using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OttawaStreetCameras {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private List<Camera> listOfCameras = new List<Camera>();
        static string SESSION_ID = "";
        public MainPage() {
            this.InitializeComponent();
            getFile();
            getSessionId();
            //WebRequest request = WebRequest.Create("http://www.contoso.com/");


        }
        public async void getSessionId() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://traffic.ottawa.ca/map");
            WebResponse response = await request.GetResponseAsync();
            Debug.WriteLine(response.Headers["Set-Cookie"]);
        }
        public async void getFile() {
            string filename = "ints.json";
            StorageFile sFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + filename);
            string text = await Windows.Storage.FileIO.ReadTextAsync(sFile);

            JsonArray array = JsonValue.Parse(text).GetArray();
            for (uint i = 0; i < array.Count; i++) {
                string name = array.GetObjectAt(i).GetNamedString("name");
                string id = array.GetObjectAt(i).GetNamedString("id");
                Camera cam = new Camera(name, id);
                listOfCameras.Add(cam);
            }
            refresh();
        }
        public void refresh() {
            listView.ItemsSource = listOfCameras;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            loadImage(((Camera)e.AddedItems[0]).id);
            

        }
        public async void loadImage(string id){
            Uri url = new Uri("https://traffic.ottawa.ca/map/camera?id=" + id);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            Cookie cookie = new Cookie("Cookie", SESSION_ID);
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(url, cookie);
            System.Net.WebResponse response = await request.GetResponseAsync();
            System.IO.Stream responseStream = response.GetResponseStream();
            update(new BitmapImage(url));
        }
        public void update(BitmapImage bmp) {
            image.Source = bmp;
        }
    }
}
