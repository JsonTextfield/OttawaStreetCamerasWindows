using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OttawaStreetCameras {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class CameraPage : Page {
        BitmapImage bmp = new BitmapImage();
        string[] param;
        public CameraPage() {
            this.InitializeComponent();
        }
        public async void getImage(string[] id) {
            Uri url = new Uri("https://traffic.ottawa.ca/map/camera?id=" + id[0], UriKind.Absolute);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers["Cookie"] = id[1];

            HttpWebResponse webResponse = (HttpWebResponse) await request.GetResponseAsync();
            Stream stream = webResponse.GetResponseStream();

            bmp = new BitmapImage(url);
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.Source = bmp;
            Debug.WriteLine(url);
            await Task.Delay(TimeSpan.FromSeconds(1));
            getImage(id);

        }
        public void setImage(BitmapImage bmp) {
            //image.Source = RandomAccessStreamReference.CreateFromUri(new Uri(url));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            string[] param = (string[])e.Parameter;
            this.param = param;
            getImage(param);
        }
    }
}
