using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;

namespace Project
{
    public static partial class Utils
    {
        public static ColumnTracker GetColumnTracker(this ListView LST)
        {
            return GetColumnTracker(LST, ColumnUnitType.Pixel);
        }

        public static ColumnTracker GetColumnTracker(this ListView LST, ColumnUnitType CUT)
        {
            return GetColumnTracker(LST, CUT, 1);
        }

        public static ColumnTracker GetColumnTracker(this ListView LST, ColumnUnitType CUT, double Width)
        {
            if (LST.Tag is ColumnTracker) return LST.Tag as ColumnTracker;
            else if (LST.View is GridView) return (ColumnTracker)(LST.Tag = new ColumnTracker(LST, CUT, Width));
            else return null;
        }

        public static Size MeasureString(string candidate, FontFamily FontFamily, FontStyle FontStyle, FontWeight FontWeight, FontStretch FontStretch, double FontSize, TextFormattingMode TextFormattingMode)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                TextFormattingMode);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }

    public class ColumnTracker
    {
        private ListView LST { get; set; }
        private GridView GRD { get; set; }
        public double StarWidth { get; set; }
        public Dictionary<GridViewColumn, ColumnController> Columns = new Dictionary<GridViewColumn, ColumnController>();

        public ColumnTracker(ListView _LST)
        {
            Set_LST_GRD(_LST);
            CheckColumens(ColumnUnitType.Star, 1);
        }

        public ColumnTracker(ListView _LST, ColumnUnitType CUT)
        {
            Set_LST_GRD(_LST);
            CheckColumens(CUT, 1);
        }

        public ColumnTracker(ListView _LST, ColumnUnitType CUT, double Width)
        {
            Set_LST_GRD(_LST);
            CheckColumens(CUT, Width);
        }

        public void Set_LST_GRD(ListView _LST)
        {
            (LST = _LST).SizeChanged += LST_SizeChanged;
            (GRD = LST.View as GridView).Columns.CollectionChanged += Columns_CollectionChanged;
            //LST.Items.CurrentChanged += Items_CurrentChanged;

            LST.Tag = this;
            LST.MouseDoubleClick += LST_MouseDoubleClick;
        }

        public void CheckColumens(ColumnUnitType CUT, double Width)
        {
            foreach (GridViewColumn x in GRD.Columns)
            {
                try { ColumnController CI = Columns[x]; }
                catch { Columns.Add(x, new ColumnController(x, this, CUT, Width)); }
            }
        }

        #region Get

        public ColumnController GetColumenItem(int ColumenIndex)
        {
            return GetColumenItem(GetColumnAt(ColumenIndex));
        }

        public ColumnController GetColumenItem(GridViewColumn GVC)
        {
            if (!Columns.ContainsKey(GVC)) Columns.Add(GVC, new ColumnController(GVC, this, ColumnUnitType.Star, 1));
            return Columns[GVC];
        }

        public ColumnController GetColumenItem(int index, ColumnUnitType CUT)
        {
            return GetColumenItem(GetColumnAt(index), CUT);
        }

        public ColumnController GetColumenItem(GridViewColumn GVC, ColumnUnitType CUT)
        {
            switch (CUT)
            {
                case ColumnUnitType.Pixel: return GetColumenItem(GVC, CUT, GVC.Width);
                default: return GetColumenItem(GVC, CUT, 1);
            }
        }

        public ColumnController GetColumenItem(int index, ColumnUnitType CUT, double Width)
        {
            return GetColumenItem(GetColumnAt(index), CUT, Width);
        }

        public ColumnController GetColumenItem(GridViewColumn GVC, ColumnUnitType CUT, double Width)
        {
            if (!Columns.ContainsKey(GVC)) Columns.Add(GVC, new ColumnController(GVC, this, CUT, Width));
            else
            {
                Columns[GVC].ColumnUnitType = CUT;
                Columns[GVC].Width = Width;
                Columns[GVC].HWSet();
            }
            Refresh();
            return Columns[GVC];
        }

        #endregion Get

        #region ListView Controller

        private void LST_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => UpdateColumnWidths();

        private void LST_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateColumnWidths();

        //private void Items_CurrentChanged(object sender, EventArgs e) => UpdateColumnWidths();

        private void UpdateColumnWidths()
        {
            if (Columns.Count > 0)
            {
                new Thread(() =>
                {
                    LST.Dispatcher.BeginInvoke(new Action(delegate ()
                    {
                        double StarDiv = 0;
                        double PixelWidth = 0;
                        if (LSTOverFlow) PixelWidth = ScrollBarInt;
                        PixelWidth += 6; //>=6

                        List<ColumnController> StarController = new List<ColumnController>();
                        foreach (ColumnController CC in Columns.Values)
                        {
                            switch (CC.ColumnUnitType)
                            {
                                case ColumnUnitType.HeaderWidth:
                                case ColumnUnitType.Pixel: PixelWidth += CC.GetWidth(); break;
                                case ColumnUnitType.Star:
                                    if (GRD.Columns.Contains(CC.Column))
                                    {
                                        StarDiv += CC.Width;
                                        StarController.Add(CC);
                                    }
                                    break;
                            }
                        }

                        StarWidth = (LSTWidth - PixelWidth) / StarDiv;
                        if (StarWidth < 0) StarWidth = 0;
                        foreach (ColumnController CC in StarController) CC.SetWidth();
                    }));
                }).Start();
            }
        }

        public double LSTWidth
        {
            get
            {
                double Width = LST.ActualWidth;
                if (LST.Width > Width) Width = LST.Width;
                return Width;
            }
        }

        public double LSTHeight
        {
            get
            {
                double Height = LST.ActualHeight;
                if (LST.Height > Height) Height = LST.Height;
                return Height;
            }
        }

        public bool LSTOverFlow
        {
            get
            {
                //15
                double Inner_Height = 14;// 13;// 11.887555556;
                foreach (object listBoxItem in LST.Items)
                {
                    try
                    {
                        ListViewItem container = LST.ItemContainerGenerator.ContainerFromItem(listBoxItem) as ListViewItem;
                        Border listBoxItemBorder = VisualTreeHelper.GetChild(container, 0) as Border;
                        Inner_Height += listBoxItemBorder.ActualHeight;
                    }
                    catch { }
                }
                return Inner_Height > LSTHeight - 22;
            }
        }

        #endregion ListView Controller

        #region GridView Controller

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (GridViewColumn GVC in e.NewItems)
                {
                    if (!Columns.ContainsKey(GVC))
                    {
                        CheckColumens(ColumnUnitType.Star, 1);
                        break;
                    }
                }
            }
            UpdateColumnWidths();
        }

        #endregion GridView Controller

        private int ScrollBarInt = 17 + 3;

        public GridViewColumn GetColumnAt(int ColumenIndex)
        {
            int Count = 0;
            foreach (GridViewColumn GVC in GRD.Columns)
            {
                if (Count == ColumenIndex) return GVC;
                else if (Count > ColumenIndex) break;
                Count++;
            }
            return null;
        }

        public void Refresh() => UpdateColumnWidths();
    }

    public class ColumnController
    {
        public ColumnTracker ColumnTracker { get; set; }
        public GridViewColumn Column { get; set; }
        public double Width { get; set; }
        public bool IsFixed { get; set; } = false;

        public ColumnUnitType ColumnUnitType { get; set; }

        public ColumnController(GridViewColumn GVC, ColumnTracker CT, ColumnUnitType GUT, double _Width)
        {
            ColumnTracker = CT;
            Width = _Width;

            switch (ColumnUnitType = GUT)
            {
                case ColumnUnitType.Pixel: IsFixed = false; break;
                case ColumnUnitType.HeaderWidth: Column.Width = GetWidth(); goto case ColumnUnitType.Star;
                case ColumnUnitType.Star: IsFixed = true; break;
            }
            INotifyPropertyChanged INP = (Column = GVC) as INotifyPropertyChanged;
            INP.PropertyChanged += Columen_SizeChanged;
        }

        private bool BlockColumnUpdate = false;

        private void Columen_SizeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!BlockColumnUpdate)
            {
                if (e.PropertyName == "Width" || e.PropertyName == "ActualWidth") SetWidth();
            }
        }

        private bool BlockSetWidth = false;

        public void SetWidth()
        {
            if (!BlockSetWidth)
            {
                BlockColumnUpdate = true;
                switch (ColumnUnitType)
                {
                    case ColumnUnitType.Pixel:
                        if (IsFixed) Column.Width = Width;
                        else
                        {
                            Width = Column.Width;
                            ColumnTracker.Refresh();
                        }

                        break;

                    case ColumnUnitType.HeaderWidth: Column.Width = GetHeaderWidth(); break;

                    case ColumnUnitType.Star:
                        //if (IsFixed)
                        //{
                        Column.Width = Width * ColumnTracker.StarWidth;
                        //}
                        //else
                        //{
                        //    double HypoWid = ColumnTracker.LSTWidth / GetWidth();
                        //    if (HypoWid == Width)
                        //        Width = ColumnTracker.LSTWidth / GetWidth();
                        //    BlockSetWidth = true;
                        //    ColumnTracker.UpdateColumnWidths();
                        //    BlockSetWidth = false;
                        //}
                        break;
                }
            }
            BlockColumnUpdate = false;
        }

        public void HWSet()
        {
            switch (ColumnUnitType)
            {
                case ColumnUnitType.Pixel: break;
                case ColumnUnitType.Star: break;
                case ColumnUnitType.HeaderWidth: Column.Width = Width; break;
            }
        }

        public double GetWidth()
        {
            switch (ColumnUnitType)
            {
                case ColumnUnitType.HeaderWidth: return GetHeaderWidth();
                default:
                    if (Column.Width != double.NaN) return Column.Width;
                    else if (Column.ActualWidth != double.NaN) return Column.ActualWidth;
                    else return double.NaN;
            }
        }

        public double GetHeaderWidth()
        {
            return Width + 10 + Utils.MeasureString(Column.Header.ToString(), new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, 12, TextFormattingMode.Ideal).Width;
        }
    }

    public enum ColumnUnitType { Pixel, Star, HeaderWidth }
}
