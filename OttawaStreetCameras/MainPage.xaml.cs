using System;
using System.Collections.Generic;
using System.Net;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OttawaStreetCameras {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private List<Camera> listOfCameras = new List<Camera>();
        public static string SESSION_ID;

        public MainPage() {
            this.InitializeComponent();
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = "Street Cameras";
            getFile();
            getSessionId();
        }

        public async void getSessionId() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://traffic.ottawa.ca/map");
            WebResponse response = await request.GetResponseAsync();
            SESSION_ID =  response.Headers["Set-Cookie"];
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
            Camera param = (Camera)e.AddedItems[0];
            this.Frame.Navigate(typeof(CameraPage), param);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
    }
}
