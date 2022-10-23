using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace viewer
{
    public class Track
    {
        private String _t;
        private String _a;
        private int _n;

        public String title
        {
            get { return _t; }
            set { _t = value; }
        }
        public String artist
        {
            get { return _a; }
            set { _a = value; }
        }
        public int number
        {
            get { return _n; }
            set { _n = value; }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? Path;
        private ObservableCollection<Track>? Data;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new() { FileName = Path };
            if (file.ShowDialog() == true)
            {
                Path = file.FileName;
                Data = Reload();
                DataGrid.ItemsSource = Data;
            }
        }

        private ObservableCollection<Track> Reload()
        {
            ObservableCollection<Track> data = new() { };
            data.Add(new Track() { title = "Think", artist = "Aretha Franklin", number = 7 });
            data.Add(new Track() { title = "Minnie The Moocher", artist = "Cab Calloway", number = 9 });
            data.Add(new Track() { title = "Shake A Tail Feather", artist = "Ray Charles", number = 4 });
            return data;
        }
    }
}
