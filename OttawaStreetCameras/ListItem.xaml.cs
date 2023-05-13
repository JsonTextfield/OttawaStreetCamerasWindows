using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OttawaStreetCameras
{
    public sealed partial class ListItem : UserControl
    {
        public Camera camera { get; }
        public ListItem(Camera camera)
        {
            this.InitializeComponent();
            title.Text = camera.GetName();
            neighbourhood.Text = camera.neighbourhood;
            this.camera = camera;
        }
    }
}
