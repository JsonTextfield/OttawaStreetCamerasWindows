﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace OttawaStreetCameras {

    public sealed partial class CameraPage : Page {
        private bool RUNNING;
        private string sessionId;

        public CameraPage() {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }
        public async void getSessionId(Camera camera) {
            HttpClient client = new HttpClient();
            HttpResponseMessage res = await client.GetAsync("https://traffic.ottawa.ca/map");

            IEnumerable<string> values;
            if (res.Headers.TryGetValues("Set-Cookie", out values)) {
                sessionId = values.First();
                getImage(camera);
            }
        }
        public async void getImage(Camera camera) {
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

            Camera camera = (Camera)e.Parameter;
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = camera.name;

            getSessionId(camera);

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
