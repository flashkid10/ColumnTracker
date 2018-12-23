# ColumnTracker
Allow the ability  to set a Listview column width to GridUnitType.Star/GridUnitType.Pixel (Avoid GridUnitType.Auto)

ListView.GetColumnTracker();
ListView.GetColumnTracker(ColumnUnitType.Star, 2);
ListView.GetColumnTracker().GetColumenItem(GridViewColumnItem, ColumnUnitType.Star, 1);
ListView.GetColumnTracker().GetColumenItem(ColumnIndexInt).IsFixed = false;
