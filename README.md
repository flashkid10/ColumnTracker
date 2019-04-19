# ColumnTracker
Allow the ability  to set a Listview column width to GridUnitType.Star/GridUnitType.Pixel (Avoid GridUnitType.Auto)

* ListView.GetColumnTracker();
* ListView.GetColumnTracker(ColumnUnitType.Star, 2);
* ListView.GetColumnTracker(ColumnUnitType.Pixel, 100);
* ListView.GetColumnTracker(ColumnUnitType.HeaderWidth, (double)LRMargin);
* ListView.GetColumnTracker().GetColumenItem(GridViewColumnItem, ColumnUnitType.Star, 1);
* ListView.GetColumnTracker().GetColumenItem(ColumnIndexInt).IsFixed = false;

Or Use "  https://www.codeproject.com/Articles/25058/ListView-Layout-Manager "
It Is Frankly Better
