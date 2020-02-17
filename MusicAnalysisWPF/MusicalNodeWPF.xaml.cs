using MusicCore;
using System;
using System.Collections.Generic;
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
        private readonly List<string> romanNums = new List<string>()
            { "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix", "x",
             "xi", "xii", "xiii", "xiv", "xv", "xvi", "xvii", "xviii", "xix", "xx" };

        public MusicalNodeWPF()
        {
            InitializeComponent();
        }

        MelodyPartList mpl;

        public void Present(MelodyPartList mpl)
        {
            // unsubscribe the previous mpl
            if (this.mpl != null)
                this.mpl.noteTriggered -= Mpl_noteTriggered;

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
                string number = mpl.IsDiatonic ? romanNums[Math.Abs(i)] : i.ToString();
                Label label = new Label()
                { Content = $"{number}", VerticalAlignment = VerticalAlignment.Center };
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
                { Content = nwd.Duration, HorizontalAlignment = HorizontalAlignment.Center, Background = Brushes.White };
                Grid.SetColumn(lblDur, iColumn + 1);
                Grid.SetRow(lblDur, max - min + 1);
                grid.Children.Add(lblDur);

                iColumn++;
            }

            sgrid = grid;
            mpl.noteTriggered += Mpl_noteTriggered;
            mpl.GetNotes().ToList();
        }

        static Grid sgrid; //kludge

        private static void Mpl_noteTriggered(int note)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                int lastRow = sgrid.RowDefinitions.Count - 1;
                
                // Color each label to blue
                foreach (UIElement uie in sgrid.Children)
                {
                    if (Grid.GetColumn(uie) == 0 || Grid.GetRow(uie) == lastRow)
                        continue;

                    Label label = uie as Label;

                    label.Background = Brushes.Blue;
                }

                // -1 signals end
                if (note == -1)
                    return;

                // Color the current note to red
                foreach (UIElement uie in sgrid.Children)
                {
                    int column = Grid.GetColumn(uie);
                    int row = Grid.GetRow(uie);
                    
                    if (column == note + 1 && row != lastRow)
                    {
                        Label label = uie as Label;
                        label.Background = Brushes.Red;
                        return;
                    }
                }
            }));
        }
    }
}