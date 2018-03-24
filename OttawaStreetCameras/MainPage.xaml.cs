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
        private List<Camera> listOfCameras = new List<Camera>();
        private List<Camera> selectedCameras = new List<Camera>();

        private const int maxCameras = 10;

        public MainPage() {
            this.InitializeComponent();
            downloadJson();
        }

        public bool selectCamera(Camera camera) {
            if (selectedCameras.Contains(camera)) {
                selectedCameras.Remove(camera);
            } else if (selectedCameras.Count < maxCameras) {
                selectedCameras.Add(camera);
            }
            return selectedCameras.Contains(camera);
        }

        public async void downloadJson() {
            string url = "https://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++) {
                listOfCameras.Add(new Camera(root.GetObjectAt(i)));
            }
            listOfCameras.Sort();
            searchBox.PlaceholderText = string.Format("Search from {0} locations", root.Count);
            refresh();

        }

        public void refresh() {
            listView.ItemsSource = listOfCameras;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (listView.SelectedItems.Count > maxCameras) {
            }
            if (e.AddedItems.Count > 0) {
                openCams.Visibility = Windows.UI.Xaml.Visibility.Visible;
                selectedCameras.Add((Camera)e.AddedItems[0]);
            }
            if (e.RemovedItems.Count > 0) {
                selectedCameras.Remove((Camera)e.RemovedItems[0]);
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
                sender.ItemsSource = listOfCameras.FindAll(delegate (Camera cam) {
                    return cam.name.ToLower().Contains(searchBox.Text.ToLower());
                });
            }
            if (args.ChosenSuggestion != null) {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else {*/
            listView.ItemsSource = listOfCameras.FindAll(delegate (Camera cam) {
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
            }
            else {
                listView.ItemsSource = listOfCameras.FindAll(delegate (Camera cam) {
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
