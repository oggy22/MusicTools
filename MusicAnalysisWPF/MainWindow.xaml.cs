using Microsoft.Win32;
using MusicCore;
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
                lists = MidiFileReader.Read(filename);
                Dispatcher.Invoke(() => { txt.Text += "File loaded.\r\n"; this.UpdateLayout(); });

                txt.Text += "Analysing...\r\n";
                var allNodes = ChomskyGrammarAnalysis.Reduce(lists);
                txt.Text += $"Analysis finished!\r\n";

                txt.Text += "Post analysis...\r\n";
                ChomskyGrammarAnalysis.PostAnalysis(lists);
                txt.Text += "Post analysis finished!\r\n";

                listView.ItemsSource = allNodes;
            }
        }

        List<MelodyPartList> lists;

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

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            //todo: play lists
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
