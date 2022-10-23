using Microsoft.Win32;
using Offmodel.FFXIV.Log.Model;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Action = Offmodel.FFXIV.Log.Model.Action;

namespace viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? Path;
        private ObservableCollection<LogEvent> Data = new();
        private Func<LogEvent, bool> Filter = e => true;

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
                DataGrid.ItemsSource = Data.Where(Filter);
            }
        }

        private ObservableCollection<LogEvent> Reload()
        {
            Parser parser = new(new StreamReader(File.OpenRead(Path)));
            return new ObservableCollection<LogEvent>(parser.Events.Where(Filter));
        }

        private void AllEvents()
        {
            Filter = e => true;

            DataGrid.Columns.Clear();
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "EventType", Binding = new Binding("EventId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Time", Binding = new Binding("EventTime") });
            DataGrid.ItemsSource = Data.Where(Filter);
        }

        private void Actors()
        {
            Filter = e => e is Actor;

            DataGrid.Columns.Clear();
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "EventType", Binding = new Binding("EventId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Time", Binding = new Binding("EventTime") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Id", Binding = new Binding("Id") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Name", Binding = new Binding("Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Job", Binding = new Binding("Job") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Owner", Binding = new Binding("Owner.Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "World", Binding = new Binding("World") });
            DataGrid.ItemsSource = Data.Where(Filter);
        }

        private void Actions()
        {
            Filter = e => e is Action;

            DataGrid.Columns.Clear();
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "EventType", Binding = new Binding("EventId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Time", Binding = new Binding("EventTime") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Initiator", Binding = new Binding("Initiator.Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "AbilityName", Binding = new Binding("AbilityName") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Target", Binding = new Binding("Target.Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "ActionId", Binding = new Binding("ActionId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "HitIndex", Binding = new Binding("HitIndex") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "HitTotal", Binding = new Binding("HitTotal") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "IsDamage", Binding = new Binding("Effects[0].IsDamage") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Damage", Binding = new Binding("Effects[0].Damage") });
            DataGrid.ItemsSource = Data.Where(Filter);
        }

        private void EventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object tag = ((ComboBoxItem) EventType.SelectedItem).Tag;
            switch (uint.Parse((string) tag))
            {
                case 1:
                    AllEvents();
                    break;

                case 2:
                    Actors();
                    break;

                case 3:
                    Actions();
                    break;
            }
        }
    }
}
