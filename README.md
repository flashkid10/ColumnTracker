# ColumnTracker
Allow the ability  to set a Listview column width to GridUnitType.Star/GridUnitType.Pixel (Avoid GridUnitType.Auto)

ListView.GetColumnTracker(GridUnitType.Star);
ListView.GetColumnTracker(GridUnitType.Star,1);
ListView.GetColumnTracker().SetColumen(GridViewColumn, GridUnitType.Star, 1);
ListView.GetColumnTracker().SetColumen(GridViewColumnIndex, GridUnitType.Star, 1);
