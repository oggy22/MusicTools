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

                txt.Text += "Post-analysing...\r\n";
                ChomskyGrammarAnalysis.PostAnalysis(lists);
                txt.Text += $"Post-analysis finished!\r\n";

                List<double> x=new List<double>(), y = new List<double>(), c = new List<double>(), d = new List<double>();
                foreach (var node in allNodes)
                {
                    if (lists.Contains(node))
                        continue;

                    x.Add(node.RecursiveCount);
                    y.Add(node.TotalOccurances);
                    c.Add(node.IsLeaf ? 0.5 : 0.9);
                    d.Add(50);
                }

                circles.PlotColor(x.ToArray(), y.ToArray(), c.ToArray());
            }
        }
    }
}