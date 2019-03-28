using MusicCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicAnalysisWPF
{
    /// <summary>
    /// Interaction logic for MusicalNodeWPF.xaml
    /// </summary>
    public partial class MusicalNodeWPF : UserControl
    {
        public MusicalNodeWPF()
        {
            InitializeComponent();
        }

        MelodyPartList mpl;

        public void Present(MelodyPartList mpl)
        {
            this.mpl = mpl;

            // Clear the grid content
            grid.Children.Clear();

            if (mpl == null)
                return;

            var notes = mpl.GetNotes().ToList();

            // Grid Definitions
            int min = notes.Min(
                nwd => nwd.IsPause ? int.MaxValue : nwd.note);
            int max = notes.Max(
                nwd => nwd.IsPause ? int.MinValue : nwd.note);

            // Row Defintions
            grid.RowDefinitions.Clear();
            for (int i=min; i<=max; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(1, GridUnitType.Star);
                grid.RowDefinitions.Add(rd);
            }

            // The last row for durations (in fractions)
            grid.RowDefinitions.Add(new RowDefinition()
            { Height = new GridLength(1, GridUnitType.Auto) });

            // Column Defintions
            grid.ColumnDefinitions.Clear();

            grid.ColumnDefinitions.Add(new ColumnDefinition()
            { Width = new GridLength(1, GridUnitType.Auto) });

            // Populate the first column with pitches [min, max]
            for (int i = max; i >= min; i--)
            {
                Label label = new Label()
                { Content = $"{i}", VerticalAlignment = VerticalAlignment.Center };
                Grid.SetColumn(label, 0);
                Grid.SetRow(label, max - i);
                grid.Children.Add(label);
            }

            int iColumn = 0;
            foreach (var nwd in notes)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength((double)(nwd.Duration), GridUnitType.Star);
                grid.ColumnDefinitions.Add(cd);
                if (!nwd.IsPause)
                {
                    Label label = new Label()
                    { Content="", Background = Brushes.Blue };
                    Grid.SetColumn(label, iColumn + 1);
                    Grid.SetRow(label, max - nwd.note);
                    grid.Children.Add(label);
                }

                // Duration part in the last row
                Label lblDur = new Label()
                { Content = nwd.Duration, HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetColumn(lblDur, iColumn + 1);
                Grid.SetRow(lblDur, max - min + 1);
                grid.Children.Add(lblDur);

                iColumn++;
            }
        }
    }
}