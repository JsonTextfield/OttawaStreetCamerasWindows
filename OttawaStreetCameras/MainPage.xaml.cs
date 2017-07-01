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
            getFile();
            getSessionId();
        }

        public async void getSessionId() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://traffic.ottawa.ca/map");
            WebResponse response = await request.GetResponseAsync();
            SESSION_ID = response.Headers["Set-Cookie"];
        }
        public async void getFile() {
            string filename = "camera_list.json";
            StorageFile sFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\" + filename);
            string text = await Windows.Storage.FileIO.ReadTextAsync(sFile);

            JsonArray array = JsonValue.Parse(text).GetArray();
            for (uint i = 0; i < array.Count; i++) {
                Camera cam = new Camera(array.GetObjectAt(i));
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
    }
}
