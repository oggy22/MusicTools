using Microsoft.Win32;
using MusicCore;
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

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Midi Files|*.mid";
            openFileDialog.Title = "Select a Midi File";

            if (openFileDialog.ShowDialog().Value)
            {
                string filename = openFileDialog.FileName;

                txt.Text += $"File {filename} loading...\r\n";
                var lists = MidiFileReader.Read(filename);
                txt.Text += "File loaded.\r\n";

                txt.Text += "Analysing...\r\n";
                var allNodes = ChomskyGrammarAnalysis.Reduce(lists);
                txt.Text += $"Analysis finished!\r\n";

                txt.Text += "Post analysis...\r\n";
                ChomskyGrammarAnalysis.PostAnalysis(lists);
                txt.Text += "Post analysis finished!\r\n";

                listView.ItemsSource = allNodes;
            }
        }
    }
}
