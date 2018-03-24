using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace OttawaStreetCameras {
    public sealed partial class CameraItem : UserControl {
        public CameraItem() {
            this.InitializeComponent();
            image.CacheMode = new BitmapCache();
            image.Stretch = Stretch.Uniform;
        }
        public Image Source { get { return image; } }
        public TextBlock Label { get { return label; } }
    }
    
}
