using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OttawaStreetCameras {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class CameraPage : Page {
        public static Boolean RUNNING;
        Camera camera;
        public CameraPage() {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }
        public async void getImage(Camera camera) {
            string url = "https://traffic.ottawa.ca/map/camera?id=" + camera.id;

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            byte[] img = await response.Content.ReadAsByteArrayAsync();
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(randomAccessStream.GetOutputStreamAt(0));

            writer.WriteBytes(img);
            await writer.StoreAsync();

            BitmapImage b = new BitmapImage();
            b.SetSource(randomAccessStream);

            image.Source = b;

            if (RUNNING) {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                getImage(camera);
            }
            else {
                Debug.WriteLine(url);
            }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            RUNNING = true;

            camera = (Camera)e.Parameter;
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = camera.name;
            getImage(camera);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }
        private void App_BackRequested(object sender, BackRequestedEventArgs e) {
            RUNNING = false;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack && e.Handled == false) {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

    }
}
