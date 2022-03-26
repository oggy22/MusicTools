using Microsoft.Win32;
using MusicCore;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MusicAnalysisWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Midi Files|*.mid|Melody Files|*.csv";
            openFileDialog.Title = "Select a Midi File";

            if (openFileDialog.ShowDialog().Value)
            {
                string filename = openFileDialog.FileName;

                //todo: using Dispatcher.Invoke doesn't work
                Dispatcher.Invoke(() => { txt.Text += $"File {filename} loading...\r\n"; });
                composition = filename.EndsWith(".mid") ? MidiFileReader.Read(filename) : CSVReader.Read(filename);
                Dispatcher.Invoke(() => { txt.Text += "File loaded.\r\n"; this.UpdateLayout(); });

                txt.Text += "Analysing...\r\n";
                var allNodes = ChomskyGrammarAnalysis.Reduce(composition.GetVoices(), new List<TwelveToneSet>() { TwelveToneSet.majorScale});
                txt.Text += $"Analysis finished!\r\n";

                txt.Text += "Post analysis...\r\n";
                ChomskyGrammarAnalysis.PostAnalysis(composition.GetVoices());
                txt.Text += "Post analysis finished!\r\n";

                this.Title = $"{filename} - Music Analysis";

                listView.ItemsSource = allNodes;
            }
        }

        Composition composition;

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listView = sender as System.Windows.Controls.ListView;
            //listView.Sele
            var mpl = listView.SelectedValue as MusicCore.MelodyPartList;

            musicalNodeWPF.Present(mpl);
        }

        static MidiOut midiOut = new MidiOut(0);
        bool isPlaying = false;
        private async void MenuItemPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                isPlaying = false;
                return;
            }

            MenuItemStop.IsEnabled = true;
            isPlaying = true;
            await Task.Run(() =>
            {
                composition.PlayBack(midiOut, composition.millisecondsPerNote.Value);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MenuItemStop.IsEnabled = false;
                }));
                isPlaying = false;
            });
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_Reshuffle_notes(object sender, RoutedEventArgs e)
        {
            foreach (var item in selection)
            {
                item.ReshuffleNoteBlocks();
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(listView.ItemsSource);
            view.Refresh();
        }

        HashSet<MelodyPartList> selection = new HashSet<MelodyPartList>();

        private void MenuItem_All_leafs(object sender, RoutedEventArgs e)
        {
            selection = new HashSet<MelodyPartList>();
            foreach (var item in listView.ItemsSource)
            {
                MelodyPartList mpl = item as MelodyPartList;
                if (mpl.IsLeaf)
                    selection.Add(mpl);
            }
        }

        private void MenuItem_All_nodes(object sender, RoutedEventArgs e)
        {
            selection = new HashSet<MelodyPartList>();
            foreach (var item in listView.ItemsSource)
            {
                MelodyPartList mpl = item as MelodyPartList;
                selection.Add(mpl);
            }
        }

        private void MenuItem_All_roots(object sender, RoutedEventArgs e)
        {
            selection = new HashSet<MelodyPartList>(composition.GetVoices());
        }

        private void MenuItem_All_none_leafs(object sender, RoutedEventArgs e)
        {
            selection = new HashSet<MelodyPartList>();
            foreach (var item in listView.ItemsSource)
            {
                MelodyPartList mpl = item as MelodyPartList;
                if (mpl.IsLeaf)
                    selection.Add(mpl);
            }
        }

        private void MenuItem_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Music Analyzer by Ognjen Sobajic\nFeb 2020");
        }

        private void MenuItemStop_Click(object sender, RoutedEventArgs e)
        {
            composition.StopPlaybak();
            MenuItemStop.IsEnabled = false;
        }
    }
}
