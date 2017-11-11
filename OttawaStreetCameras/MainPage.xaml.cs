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
        private HashSet<Camera> selectedCameras = new HashSet<Camera>();
        public MainPage() {
            this.InitializeComponent();
            getFile();
        }

        public async void getFile() {
            string url = "http://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            //DataWriter writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0));

            //writer.WriteBytes(img);
            //await writer.StoreAsync();
            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++) {
                Camera camera = new Camera();

                camera.nameFr = root.GetObjectAt(i).GetNamedString("descriptionFr");
                camera.name = root.GetObjectAt(i).GetNamedString("description");
                camera.type = root.GetObjectAt(i).GetNamedString("type");
                camera.num = camera.type.Equals("MTO")? (int)root.GetObjectAt(i).GetNamedNumber("number") +2000: (int) root.GetObjectAt(i).GetNamedNumber("number");
                camera.id = (int) root.GetObjectAt(i).GetNamedNumber("id");
                camera.lng = (double) root.GetObjectAt(i).GetNamedNumber("longitude");
                camera.lat = (double) root.GetObjectAt(i).GetNamedNumber("latitude");
                

                listOfCameras.Add(camera);
                
            }
            searchBox.PlaceholderText = string.Format("Search from {0} locations", root.Count);
            refresh();

        }

        /*public async void getFile() {

            string filename = "camera_list.db";
            StorageFile sFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + filename);
            SqliteConnection db = new SqliteConnection("Filename="+sFile.Path);

            db.Open();
            SqliteCommand selectCommand = new SqliteCommand("SELECT * from cameras", db);
            SqliteDataReader query = selectCommand.ExecuteReader();
            while (query.Read()) {
                Debug.WriteLine(query.GetString(3));
                Camera cam = new Camera(query);
                listOfCameras.Add(cam);
            }
            db.Close();
            refresh();
        }*/

        public void refresh() {
            listView.ItemsSource = listOfCameras;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                selectedCameras.Add((Camera)e.AddedItems[0]);
            }
            if (e.RemovedItems.Count > 0) {
                selectedCameras.Remove((Camera)e.RemovedItems[0]);
            }

        }
        public void openCameras() {
            this.Frame.Navigate(typeof(CameraPage), selectedCameras);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = "";
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
            // Only get results when it was a user typing,
            // otherwise assume the value got filled in by TextMemberPath
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
                sender.ItemsSource = listOfCameras.FindAll(delegate (Camera cam) {
                    return cam.name.ToLower().Contains(searchBox.Text.ToLower());
                });
            }
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            Camera param = (Camera)args.SelectedItem;
            this.Frame.Navigate(typeof(CameraPage), param);
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
