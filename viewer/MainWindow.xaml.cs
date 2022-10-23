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
                Parser parser = new(new StreamReader(File.OpenRead(Path)));
                Data = new ObservableCollection<LogEvent>(parser.Events.Where(Filter));
                DataGrid.ItemsSource = Data.Where(Filter);
            }
        }

        private void AllEvents()
        {
            Filter = e => true;

            DataGrid.Columns.Clear();
            DetailGrid.Columns.Clear();
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "EventType", Binding = new Binding("EventId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Time", Binding = new Binding("EventTime") });
            DataGrid.ItemsSource = Data.Where(Filter);
        }

        private void Actors()
        {
            Filter = e => e is Actor;

            DataGrid.Columns.Clear();
            DetailGrid.Columns.Clear();
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
            DetailGrid.Columns.Clear();
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "EventType", Binding = new Binding("EventId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Time", Binding = new Binding("EventTime") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Initiator", Binding = new Binding("Initiator.Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "AbilityName", Binding = new Binding("AbilityName") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Target", Binding = new Binding("Target.Name") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "ActionId", Binding = new Binding("ActionId") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "HitIndex", Binding = new Binding("HitIndex") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "HitTotal", Binding = new Binding("HitTotal") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "IsDamage", Binding = new Binding("FirstEffect.IsDamage") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "Effect", Binding = new Binding("FirstEffect.EffectValue") });
            DataGrid.Columns.Add(new DataGridTextColumn() { Header = "IsMagic", Binding = new Binding("FirstEffect.IsMagic") });

            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "Type", Binding = new Binding("TypeName") });
            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "IsDamage", Binding = new Binding("IsDamage") });
            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "IsHeal", Binding = new Binding("IsHeal") });
            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "IsCritical", Binding = new Binding("IsCritical") });
            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "IsDirectHit", Binding = new Binding("IsDirectHit") });
            DetailGrid.Columns.Add(new DataGridTextColumn() { Header = "Effect", Binding = new Binding("EffectValue") });

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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGrid.SelectedItem != null)
            {
                LogEvent ev = (LogEvent) DataGrid.SelectedItem;
                if (ev is Action)
                {
                    DetailGrid.ItemsSource = ((Action)ev).Effects;
                }
                else
                {
                    DetailGrid.ItemsSource = new ObservableCollection<LogEvent>();
                }
            } else
            {
                DetailGrid.ItemsSource = new ObservableCollection<LogEvent>();
            }
        }
    }
}
