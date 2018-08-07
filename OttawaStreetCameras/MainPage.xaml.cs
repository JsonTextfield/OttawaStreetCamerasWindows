using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OttawaStreetCameras
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private List<Neighbourhood> neighbourhoods = new List<Neighbourhood>();
        private List<MapIcon> markers = new List<MapIcon>();
        private List<Camera> cameras = new List<Camera>();
        private List<Camera> selectedCameras = new List<Camera>();

        private const int maxCameras = int.MaxValue;

        public MainPage()
        {
            this.InitializeComponent();
            DownloadJson();
        }

        private void Handle_Menu_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = sender as AppBarButton;
            if (button == openCams) {
                OpenCameras();
            } else if (button == favouriteBtn) {
                searchBox.Text = "f: ";
            } else if (button == neighbourhoodBtn) {
                searchBox.Text = "n: ";
            } else if (button == hiddenBtn) {
                searchBox.Text = "h: ";
            } else {
                if (listView.Visibility == Visibility.Collapsed) {
                    listView.Visibility = Visibility.Visible;
                    mapView.Visibility = Visibility.Collapsed;
                } else {
                    listView.Visibility = Visibility.Collapsed;
                    mapView.Visibility = Visibility.Visible;

                }
            }
        }

        private async void GetNeighbourhoods()
        {
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
                        break;
                    }
                }
            }
            neighbourhoods.Sort();
        }

        private async void DownloadJson()
        {
            string url = "https://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++) {
                Camera camera = new Camera(root.GetObjectAt(i));
                cameras.Add(camera);

                MapIcon mapIcon;
                mapIcon = new MapIcon();

                BasicGeoposition location = new BasicGeoposition();  
                location.Latitude = camera.lat;  
                location.Longitude = camera.lng;  

                mapIcon.Location = new Geopoint(location);  
                mapIcon.Title = camera.GetName();
                markers.Add(mapIcon);
                mapView.MapElements.Add(mapIcon);

            }
            cameras.Sort();
            GetNeighbourhoods();
            searchBox.PlaceholderText = string.Format("Search from {0} locations", root.Count);
            listView.ItemsSource = cameras;

        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedItems.Count > maxCameras) {
            }
            if (e.AddedItems.Count > 0) {
                openCams.Visibility = Windows.UI.Xaml.Visibility.Visible;
                List<Camera> list = new List<Camera>();
                for (int i = 0; i < e.AddedItems.Count; i++) {
                    list.Add((Camera)e.AddedItems[i]);
                }
                selectedCameras.AddRange(list);
            }
            if (e.RemovedItems.Count > 0) {
                selectedCameras.RemoveAll(camera => e.RemovedItems.Contains(camera));
                if (selectedCameras.Count == 0) {
                    openCams.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }
        private void OpenCameras()
        {
            this.Frame.Navigate(typeof(CameraPage), selectedCameras);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            
            /*if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                sender.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.name.ToLower().Contains(searchBox.Text.ToLower());
                });
            }
            if (args.ChosenSuggestion != null) {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else {*/
            if (searchBox.Text.ToLower().StartsWith("n: ")) {
                sender.ItemsSource = neighbourhoods.FindAll(delegate (Neighbourhood n) {
                    return n.GetName().ToLower().Contains(searchBox.Text.Substring(3).ToLower());
                });
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.neighbourhood.ToLower().Contains(searchBox.Text.Substring(3).ToLower());
                });
            } else if (searchBox.Text.ToLower().StartsWith("f: ")) {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.isFavourite && cam.GetSortableName().ToLower().Contains(searchBox.Text.ToLower());
                });
            } else if (searchBox.Text.ToLower().StartsWith("h: ")) {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.isVisible && cam.GetSortableName().ToLower().Contains(searchBox.Text.ToLower());
                });
            } else {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.GetSortableName().ToLower().Contains(searchBox.Text.ToLower());
                });
            }

            // Use args.QueryText to determine what to do.
            //}
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            //Camera param = (Camera)args.SelectedItem;
            //this.Frame.Navigate(typeof(CameraPage), param);
            listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.neighbourhood.ToLower().Contains(searchBox.Text.ToLower());
            });
            searchBox.Text = "n: " + args.SelectedItem.ToString();
            
        }
    }
}
