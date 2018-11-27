using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using System;
using System.Threading;
using System.Globalization;
using System.Windows.Media;

namespace Project
{
    public static partial class Utils
    {
        public static ColumnTracker GetColumnTracker(this ListView LST)
        {
            return GetColumnTracker(LST, GridUnitType.Pixel);
        }

        public static ColumnTracker GetColumnTracker(this ListView LST, GridUnitType GNU)
        {
            return GetColumnTracker(LST, GNU, 1);
        }

        public static ColumnTracker GetColumnTracker(this ListView LST, GridUnitType GNU, double Width)
        {
            if (LST.Tag is ColumnTracker) return LST.Tag as ColumnTracker;
            else if (LST.View is GridView) return (ColumnTracker)(LST.Tag = new ColumnTracker(LST, GNU, Width));
            else return null;
        }
    }

    public class ColumnTracker
    {
        private ListView LST { get; set; }
        private GridView GRD { get; set; }
        public Trictionary<GridViewColumn, GridUnitType, double> Columns = new Trictionary<GridViewColumn, GridUnitType, double>();

        private GridUnitType DefaultGUT = GridUnitType.Pixel;
        private double DefaultWidth = 1;

        //public ColumnTracker(ListView _LST)
        //{
        //    SetLSTGRD(_LST);
        //    CheckColumens(GridUnitType.Pixel);
        //}

        public ColumnTracker(ListView _LST, GridUnitType GNU)
        {
            SetLSTGRD(_LST);
            CheckColumens(DefaultGUT = GNU, 1);
        }

        public ColumnTracker(ListView _LST, GridUnitType GNU, double Width)
        {
            SetLSTGRD(_LST);
            CheckColumens(DefaultGUT = GNU, DefaultWidth = Width);
        }

        public void SetLSTGRD(ListView _LST)
        {
            LST = _LST;
            LST.SizeChanged += LST_SizeChanged;
            GRD = LST.View as GridView;
            GRD.Columns.CollectionChanged += Columns_CollectionChanged;
        }

        public void CheckColumens(GridUnitType GNU, double Width)
        {
            foreach (GridViewColumn x in GRD.Columns)
            {
                if (!Columns.Keys.Contains(x))
                {
                    INotifyPropertyChanged INP = x as INotifyPropertyChanged;
                    INP.PropertyChanged += INP_PropertyChanged;

                    switch (GNU)
                    {
                        case GridUnitType.Auto:
                        case GridUnitType.Pixel:

                            //if (GNU == GridUnitType.Auto)
                            //{
                            //  var formattedText = new FormattedText(x.Header.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LST.FontFamily, LST.FontStyle, LST.FontWeight, LST.FontStretch), LST.FontSize, LST.Foreground, new NumberSubstitution(), TextFormattingMode.Ideal);
                            //   Width = formattedText.Width + 2;
                            //Width = GetAutoWidth(x);
                            //}
                            //else
                            //{
                            Width = x.ActualWidth;
                            if (x.Width > Width) Width = x.Width;
                            //}

                            Columns.Add(x, GridUnitType.Pixel, Width);
                            break;

                        case GridUnitType.Star:
                            Columns.Add(x, GridUnitType.Star, Width);
                            break;
                    }
                }
            }
        }

        #region Set Columen

        public void SetColumen(GridViewColumn GVS, GridUnitType GUT) => SetColumen(Columns.IndexOf(GVS), GUT);

        public void SetColumen(GridViewColumn GVS, double Width) => SetColumen(Columns.IndexOf(GVS), Width);

        public void SetColumen(GridViewColumn GVS, GridUnitType GUT, double Width) => SetColumen(Columns.IndexOf(GVS), GUT, Width);

        public void SetColumen(int CN, GridUnitType GUT)
        {
            KeyValueTriple<GridViewColumn, GridUnitType, double> CNKVT = Columns.GetAtIndex(CN);

            switch (GUT)
            {
                case GridUnitType.Auto:
                    //double Wid = CNKVT.Key.Width;
                    //if (LSTWidth > 0) CNKVT.Control = Wid / LSTWidth;
                    //else CNKVT.Control = Wid;
                    //CNKVT.Control = GetAutoWidth(CNKVT.Key);
                    break;

                case GridUnitType.Pixel: CNKVT.Control = CNKVT.Key.Width; break;
                case GridUnitType.Star: CNKVT.Control = 1; break;
            }
            SetColumen(CN, GUT, CNKVT.Control);
        }

        public void SetColumen(int CN, double Width) => SetColumen(CN, Columns.GetAtIndex(CN).Value, Width);

        public void SetColumen(int CN, GridUnitType GUT, double Width)
        {
            Columns.GetAtIndex(CN).Value = GUT;
            Columns.GetAtIndex(CN).Control = Width;
            if (!IsDead) UpdateColumnWidths();
        }

        #endregion Set Columen

        private void INP_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsDead)
                if (!BlockColumnUpdate)
                    if (e.PropertyName == "Width" || e.PropertyName == "ActualWidth")
                    {
                        GridViewColumn GVC = sender as GridViewColumn;
                        BlockUpdate = System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed;
                        int Index = Columns.IndexOf(GVC);
                        if (Columns.GetAtIndex(Index).Value == GridUnitType.Auto) SetColumen(Index, GridUnitType.Auto);
                        else UpdateColumnWidths();
                        BlockUpdate = false;
                    }
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (GridViewColumn GVC in e.NewItems)
                {
                    if (!Columns.Keys.Contains(GVC))
                    {
                        CheckColumens(DefaultGUT, DefaultWidth);
                        break;
                    }
                }
            }
            UpdateColumnWidths();
        }

        private void LST_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateColumnWidths();

        private bool BlockUpdate = false;
        private bool BlockColumnUpdate = false;

        public void UpdateColumnWidths()
        {
            if (!IsDead)
            {
                if (!BlockUpdate)
                {
                    if (Columns.Count > 0)
                    {
                        new Thread(() =>
                    {
                        LST.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            double Width = LSTWidth;
                            if (Width > 0)
                            {
                                Dictionary<GridUnitType, List<GridViewColumn>> GUTColumn = new Dictionary<GridUnitType, List<GridViewColumn>>();
                                GUTColumn.Add(GridUnitType.Auto, new List<GridViewColumn>());
                                GUTColumn.Add(GridUnitType.Pixel, new List<GridViewColumn>());
                                GUTColumn.Add(GridUnitType.Star, new List<GridViewColumn>());

                                foreach (KeyValueTriple<GridViewColumn, GridUnitType, double> x in Columns)
                                {
                                    if (x.Value == GridUnitType.Auto && x.Control > 1)
                                    {
                                        BlockUpdate = true;
                                        SetColumen(Columns.IndexOf(x.Key), GridUnitType.Auto);
                                    }
                                    if (GRD.Columns.Contains(x.Key)) GUTColumn[x.Value].Add(x.Key);
                                }

                                if (BlockUpdate)
                                {
                                    BlockUpdate = false;
                                    UpdateColumnWidths();
                                }
                                else
                                {
                                    double ReservedWidth = ReturnControl(GUTColumn, GridUnitType.Pixel);
                                    BlockColumnUpdate = true;
                                    //foreach (GridViewColumn GVC in GUTColumn[GridUnitType.Auto]) GVC.Width = GetAutoWidth(GVC);
                                    //ReservedWidth += ReturnControl(GUTColumn, GridUnitType.Auto);
                                    double WidthRemaining = Width - ReservedWidth;
                                    if (WidthRemaining < 0) WidthRemaining = 0;
                                    List<GridViewColumn> StarAuto = new List<GridViewColumn>();
                                    StarAuto.AddRange(GUTColumn[GridUnitType.Star]);
                                    //StarAuto.AddRange(GUTColumn[GridUnitType.Auto]);
                                    double StarWidth = ReturnControl(GUTColumn, GridUnitType.Star) + ReturnControl(GUTColumn, GridUnitType.Auto);
                                    foreach (GridViewColumn GVC in StarAuto)
                                    {
                                        double Star = Columns[GVC].Control;
                                        double StarPercent = Star / StarWidth;
                                        GVC.Width = StarPercent * WidthRemaining;
                                    }
                                    BlockColumnUpdate = false;
                                }
                            }
                        }));
                    }).Start();
                    }
                }
            }
        }

        private double ReturnControl(Dictionary<GridUnitType, List<GridViewColumn>> GUTColumn, GridUnitType GUT)
        {
            double Width = 0;
            foreach (GridViewColumn GVC in GUTColumn[GUT]) Width += Columns[GVC].Control;
            return Width;
        }

        public double LSTWidth
        {
            get
            {
                //LST.Dispatcher.BeginInvoke(new Action(delegate ()
                //{
                double Width = LST.ActualWidth;
                if (LST.Width > Width) Width = LST.Width;
                return Width;
                //}));
            }
        }

        //private double GetAutoWidth(GridViewColumn GVC)
        //{
        //    var formattedText = new FormattedText(GVC.Header.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LST.FontFamily, LST.FontStyle, LST.FontWeight, LST.FontStretch), LST.FontSize, LST.Foreground, new NumberSubstitution(), TextFormattingMode.Display);
        //    return formattedText.Width;// + 2;
        //}

        private bool IsDead = false;

        public void Kill()
        {
            IsDead = true;
            LST.SizeChanged -= LST_SizeChanged;
            GRD.Columns.CollectionChanged -= Columns_CollectionChanged;
            foreach (GridViewColumn x in Columns.Keys)
            {
                INotifyPropertyChanged INP = x as INotifyPropertyChanged;
                INP.PropertyChanged -= INP_PropertyChanged;
            }
            Columns = new Trictionary<GridViewColumn, GridUnitType, double>();
        }
    }
}
