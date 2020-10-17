using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace OttawaStreetCameras
{

    public sealed partial class CameraPage : Page
    {
        private bool isRunning;
        private bool isShuffleOn;
        private string sessionId;
        private List<Camera> cameras;
        private Random random = new Random();

        public CameraPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }

        public async Task getSessionId()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage res = await client.GetAsync("http://traffic.ottawa.ca/map");

            if (res.Headers.TryGetValues("Set-Cookie", out IEnumerable<string> values))
            {
                sessionId = values.First();
            }
        }

        private CameraView loadCameraView()
        {
            CameraView camItem = new CameraView();
            if (isShuffleOn || cameras.Count < 2)
            {
                camItem.Source.MaxHeight = ((Frame)Window.Current.Content).ActualHeight - 1;
            }
            else if (cameras.Count() < 4)
            {
                camItem.MaxWidth = ((Frame)Window.Current.Content).ActualWidth / cameras.Count - 1;
            }
            else
            {
                camItem.MaxWidth = ((Frame)Window.Current.Content).ActualWidth / 4 - 1;
            }
            grid.Children.Add(camItem);
            return camItem;
        }

        public async void getImage(Camera camera, CameraView camView)
        {
            Image image = camView.Source;
            camView.Label.Text = camera.GetName();
            string url = "https://traffic.ottawa.ca/map/camera?id=" + camera.num;
            HttpClient outClient = new HttpClient();

            outClient.DefaultRequestHeaders.Add("Cookie", sessionId);

            byte[] img = await outClient.GetByteArrayAsync(url);
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0));

            writer.WriteBytes(img);
            await writer.StoreAsync();

            BitmapImage b = new BitmapImage();
            b.SetSource(randomAccessStream);

            image.Source = b;
            await Task.Delay(TimeSpan.FromMilliseconds(6000));
            if (isRunning)
            {
                if (isShuffleOn)
                {
                    camera = cameras[random.Next(0, cameras.Count)];
                }
                getImage(camera, camView);
            }
            else
            {
                Debug.WriteLine(url);
            }

        }

        private async void setup()
        {
            await getSessionId();

            if (isShuffleOn)
            {
                getImage(cameras[random.Next(0, cameras.Count)], loadCameraView());
            }
            else
            {
                foreach (Camera camera in cameras)
                {
                    getImage(camera, loadCameraView());
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            isRunning = true;
            cameras = (List<Camera>)((Object[])e.Parameter)[0];
            isShuffleOn = (bool)((Object[])e.Parameter)[1];
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            setup();
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            isRunning = false;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;

            // Navigate back if possible, and if the event has not already been handled.
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

    }
}
