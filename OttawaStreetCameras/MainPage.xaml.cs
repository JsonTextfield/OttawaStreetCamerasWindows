using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OttawaStreetCameras {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page {
        private List<Neighbourhood> neighbourhoods = new List<Neighbourhood>();
        private List<Camera> cameras = new List<Camera>();
        private List<Camera> selectedCameras = new List<Camera>();

        private const int maxCameras = int.MaxValue;

        public MainPage() {
            this.InitializeComponent();
            DownloadJson();
        }

        private void Handle_Menu_Click(object sender, RoutedEventArgs e) {
            AppBarButton button = sender as AppBarButton;
            if (button == openCams) {
                OpenCameras();
            } else if (button == favouriteBtn) {
                searchBox.Text = "f: ";
            } else if (button == neighbourhoodBtn) {
                searchBox.Text = "n: ";
            } else if (button == hiddenBtn) {
                searchBox.Text = "h: ";
            } else if (button == addFav) {
                foreach (Camera c in selectedCameras) {
                    c.isFavourite = !c.isFavourite;
                }
            } else if (button == hide) {
                foreach (Camera c in selectedCameras) {
                    c.isVisible = !c.isVisible;
                }
            } else if (button == sortDistance) {
            } else if (button == sortName) {
                cameras.Sort();
                listView.ItemsSource = cameras;
            } else if (button == random) {
                selectedCameras.Clear();
                Random ran = new Random();
                selectedCameras.Add(cameras[ran.Next(cameras.Count)]);
                OpenCameras();
            } else if (button == cancel) {
                listView.SelectedItems.Clear();
            } else if (button == select_all) {
                listView.SelectAll();
            } else {
                if (listView.Visibility == Visibility.Collapsed) {
                    listView.Visibility = Visibility.Visible;
                    mapView.Visibility = Visibility.Collapsed;
                } else {
                    listView.Visibility = Visibility.Collapsed;
                    mapView.Visibility = Visibility.Visible;
                    PositionMap();
                }
            }
        }

        private async void GetNeighbourhoods() {
            string url = "http://data.ottawa.ca/dataset/302ade92-51ec-4b26-a715-627802aa62a8/resource/f1163794-de80-4682-bda5-b13034984087/download/onsboundariesgen1.shp.json";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetObject().GetNamedArray("features");

            for (uint i = 0; i < root.Count; i++) {
                neighbourhoods.Add(new Neighbourhood(root.GetObjectAt(i)));
            }
            foreach (Camera camera in cameras) {
                foreach (Neighbourhood neighbourhood in neighbourhoods) {
                    if (neighbourhood.ContainsCamera(camera)) {
                        camera.neighbourhood = neighbourhood.GetName();
                        neighbourhood.cameras.Add(camera);
                        break;
                    }
                }
            }
            neighbourhoods.Sort();

            searchBox.PlaceholderText = string.Format("Search from {0} locations", cameras.Count);
            listView.ItemsSource = cameras.FindAll(camera => camera.isVisible);
        }

        private async void PositionMap() {
            IEnumerable<BasicGeoposition> positions = cameras.Select(x => x.gp);
            GeoboundingBox bounds = GeoboundingBox.TryCompute(positions);
            await mapView.TrySetViewBoundsAsync(bounds, new Thickness(50), MapAnimationKind.None);
        }

        private async void DownloadJson() {
            string url = "https://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++) {
                Camera camera = new Camera(root.GetObjectAt(i));
                cameras.Add(camera);

                mapView.MapElements.Add(camera.mapIcon);

            }

            cameras.Sort();
            GetNeighbourhoods();

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listView.SelectedItems.Count > maxCameras) {
            }
            if (e.AddedItems.Count > 0) {
                select_menu.Visibility = Visibility.Visible;
                menu.Visibility = Visibility.Collapsed;
                List<Camera> list = new List<Camera>();
                for (int i = 0; i < e.AddedItems.Count; i++) {
                    list.Add((Camera)e.AddedItems[i]);
                }
                selectedCameras.AddRange(list);
            }
            if (e.RemovedItems.Count > 0) {
                selectedCameras.RemoveAll(camera => e.RemovedItems.Contains(camera));
                if (selectedCameras.Count == 0) {
                    select_menu.Visibility = Visibility.Collapsed;
                    menu.Visibility = Visibility.Visible;
                }
            }
        }
        private void OpenCameras() {
            this.Frame.Navigate(typeof(CameraPage), selectedCameras);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            selectedCameras.Clear();
            listView.SelectedItems.Clear();
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
            if (searchBox.Text.ToLower().StartsWith("n: ")) {
                sender.ItemsSource = neighbourhoods.FindAll(n => n.cameras.Count > 0 && n.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower()));
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    bool b = cam.neighbourhood.ToLower().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            } else if (searchBox.Text.ToLower().StartsWith("f: ")) {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    bool b = cam.isFavourite && cam.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            } else if (searchBox.Text.ToLower().StartsWith("h: ")) {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    bool b = !cam.isVisible && cam.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });

            } else {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    bool b = cam.isVisible && cam.GetSortableName().Contains(searchBox.Text.ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            }

        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            //Camera param = (Camera)args.SelectedItem;
            //this.Frame.Navigate(typeof(CameraPage), param);
            searchBox.Text = "n: " + args.SelectedItem;

        }

        private void MapView_MapElementClick(MapControl sender, MapElementClickEventArgs args) {
            try {
                selectedCameras.Add(args.MapElements[0].Tag as Camera);
                OpenCameras();
            } catch {

            }
        }
    }
}
