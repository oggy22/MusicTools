using Microsoft.Win32;
using MusicCore;
using NAudio.Midi;
using System.Collections.Generic;
using System.Windows;

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

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Midi Files|*.mid";
            openFileDialog.Title = "Select a Midi File";

            if (openFileDialog.ShowDialog().Value)
            {
                string filename = openFileDialog.FileName;

                //todo: using Dispatcher.Invoke doesn't work
                Dispatcher.Invoke(() => { txt.Text += $"File {filename} loading...\r\n"; });
                composition = MidiFileReader.Read(filename);
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
        
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            composition.PlayBack(midiOut);
            //todo: make it asynchronous
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
