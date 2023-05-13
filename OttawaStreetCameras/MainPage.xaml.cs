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

namespace OttawaStreetCameras
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {
        private List<Neighbourhood> neighbourhoods = new List<Neighbourhood>();
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
            if (button == openCams)
            {
                OpenCameras();
            }
            else if (button == favouriteBtn)
            {
                searchBox.Text = "f: ";
            }
            else if (button == neighbourhoodBtn)
            {
                searchBox.Text = "n: ";
            }
            else if (button == hiddenBtn)
            {
                searchBox.Text = "h: ";
            }
            else if (button == shuffleBtn)
            {
                selectedCameras = cameras;
                OpenCameras(true);
            }
            else if (button == addFav)
            {
                foreach (Camera c in selectedCameras)
                {
                    c.isFavourite = !c.isFavourite;
                }
            }
            else if (button == hide)
            {
                foreach (Camera c in selectedCameras)
                {
                    c.isVisible = !c.isVisible;
                }
            }
            else if (button == sortDistance)
            {
            }
            else if (button == sortName)
            {
                cameras.Sort();
                cameras.ForEach((Camera camera) => {
                    listView.Items.Add(new ListItem(camera));
                });
            }
            else if (button == random)
            {
                selectedCameras.Clear();
                Random ran = new Random();
                selectedCameras.Add(cameras[ran.Next(cameras.Count)]);
                OpenCameras();
            }
            else if (button == cancel)
            {
                listView.SelectedItems.Clear();
            }
            else if (button == select_all)
            {
                listView.SelectAll();
            }
            else
            {
                if (listView.Visibility == Visibility.Collapsed)
                {
                    listView.Visibility = Visibility.Visible;
                    mapView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    listView.Visibility = Visibility.Collapsed;
                    mapView.Visibility = Visibility.Visible;
                    PositionMap();
                }
            }
        }

        private async void GetNeighbourhoods()
        {
            string url = "https://services.arcgis.com/G6F8XLCl5KtAlZ2G/arcgis/rest/services/Gen_2_ONS_Boundaries/FeatureServer/0/query?outFields=*&where=1%3D1&f=geojson";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetObject().GetNamedArray("features");

            for (uint i = 0; i < root.Count; i++)
            {
                Neighbourhood neighbourhood = new Neighbourhood(root.GetObjectAt(i));
                neighbourhoods.Add(neighbourhood);

                foreach (Camera camera in cameras)
                {
                    if (neighbourhood.ContainsCamera(camera))
                    {
                        camera.neighbourhood = neighbourhood.GetName();
                        neighbourhood.cameras.Add(camera);
                    }
                }
            }
            neighbourhoods.Sort();

            searchBox.PlaceholderText = string.Format("Search from {0} locations", cameras.Count);
            foreach (Camera camera in cameras)
            {
                if (camera.isVisible)
                {
                    listView.Items.Add(new ListItem(camera));
                }
            }
        }

        private async void PositionMap()
        {
            IEnumerable<BasicGeoposition> positions = cameras.Select(x => x.gp);
            GeoboundingBox bounds = GeoboundingBox.TryCompute(positions);
            await mapView.TrySetViewBoundsAsync(bounds, new Thickness(50), MapAnimationKind.None);
        }

        private async void DownloadJson()
        {
            string url = "https://traffic.ottawa.ca/map/camera_list";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            JsonArray root = JsonValue.Parse(jsonString).GetArray();
            for (uint i = 0; i < root.Count; i++)
            {
                Camera camera = new Camera(root.GetObjectAt(i));
                cameras.Add(camera);

                mapView.MapElements.Add(camera.mapIcon);

            }

            cameras.Sort();
            GetNeighbourhoods();

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedItems.Count > maxCameras)
            {
            }
            if (e.AddedItems.Count > 0)
            {
                select_menu.Visibility = Visibility.Visible;
                menu.Visibility = Visibility.Collapsed;
                List<Camera> list = new List<Camera>();
                for (int i = 0; i < e.AddedItems.Count; i++)
                {
                    list.Add(((ListItem)e.AddedItems[i]).camera);
                }
                selectedCameras.AddRange(list);
            }
            if (e.RemovedItems.Count > 0)
            {
                selectedCameras.RemoveAll(camera => e.RemovedItems.Contains(camera));
                if (selectedCameras.Count == 0)
                {
                    select_menu.Visibility = Visibility.Collapsed;
                    menu.Visibility = Visibility.Visible;
                }
            }
        }
        private void OpenCameras(bool shuffleOn = false)
        {
            Frame.Navigate(typeof(CameraPage), new Object[] { selectedCameras, shuffleOn });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            selectedCameras.Clear();
            listView.SelectedItems.Clear();
        }
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            listView.Items.Clear();
            List<Camera> filteredCameras = new List<Camera>();
            if (searchBox.Text.ToLower().StartsWith("n: "))
            {
                sender.ItemsSource = neighbourhoods.FindAll(n => n.cameras.Count > 0 && n.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower()));
                filteredCameras = cameras.FindAll((Camera cam) =>
                {
                    bool b = cam.neighbourhood.ToLower().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            }
            else if (searchBox.Text.ToLower().StartsWith("f: "))
            {
                filteredCameras = cameras.FindAll((Camera cam) =>
                {
                    bool b = cam.isFavourite && cam.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            }
            else if (searchBox.Text.ToLower().StartsWith("h: "))
            {
                filteredCameras = cameras.FindAll((Camera cam) =>
                {
                    bool b = !cam.isVisible && cam.GetSortableName().Contains(searchBox.Text.Substring(3).ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });

            }
            else
            {
                filteredCameras = cameras.FindAll((Camera cam) =>
                {
                    bool b = cam.isVisible && cam.GetSortableName().Contains(searchBox.Text.ToLower());
                    cam.mapIcon.Visible = b;
                    return b;
                });
            }
            filteredCameras.ForEach(camera => listView.Items.Add(new ListItem(camera)));

        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
            //Camera param = (Camera)args.SelectedItem;
            //this.Frame.Navigate(typeof(CameraPage), param);
            searchBox.Text = "n: " + args.SelectedItem;

        }

        private void MapView_MapElementClick(MapControl sender, MapElementClickEventArgs args)
        {
            try
            {
                selectedCameras.Add(args.MapElements[0].Tag as Camera);
                OpenCameras();
            }
            catch
            {

            }
        }
    }
}
