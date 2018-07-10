using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
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

        private const int maxCameras = 10;

        public MainPage() {
            this.InitializeComponent();
            downloadJson();
        }

        public async void getNeighbourhoods() {
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
        }

        public async void downloadJson() {
            string url = "https://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++) {
                cameras.Add(new Camera(root.GetObjectAt(i)));
            }
            cameras.Sort();
            searchBox.PlaceholderText = string.Format("Search from {0} locations", root.Count);
            refresh();

        }

        public void refresh() {
            listView.ItemsSource = cameras;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listView.SelectedItems.Count > maxCameras) {
            }
            if (e.AddedItems.Count > 0) {
                openCams.Visibility = Windows.UI.Xaml.Visibility.Visible;
                List<Camera> list = new List<Camera>();
                for (int i = 0; i < e.AddedItems.Count; i++){
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
        public void openCameras() {
            this.Frame.Navigate(typeof(CameraPage), selectedCameras);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
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
            listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                return cam.name.ToLower().Contains(searchBox.Text.ToLower());
            });
            // Use args.QueryText to determine what to do.
            //}
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            //Camera param = (Camera)args.SelectedItem;
            //this.Frame.Navigate(typeof(CameraPage), param);
        }


        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
            if (args.ChosenSuggestion != null) {
                // User selected an item from the suggestion list, take an action on it here.
            } else {
                listView.ItemsSource = cameras.FindAll(delegate (Camera cam) {
                    return cam.name.ToLower().Contains(searchBox.Text.ToLower());
                });
                // Use args.QueryText to determine what to do.
            }
        }

        private void listView_ItemClick(object sender, ItemClickEventArgs e) {
            Debug.WriteLine("click");
            Camera param = (Camera)e.ClickedItem;
            this.Frame.Navigate(typeof(CameraPage), param);
        }

        private void openCams_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            openCameras();
        }

    }
}
