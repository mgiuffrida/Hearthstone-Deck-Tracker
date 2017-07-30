#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options
{
	/// <summary>
	/// Interaction logic for OptionsSearch.xaml
	/// </summary>
	public partial class OptionsSearch : UserControl
	{
		private List<OptionWrapper> _optionWrappers;

		public OptionsSearch()
		{
			InitializeComponent();
		}
		
		private void TextBoxSearchLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			((TextBox) sender).Focus();
		}

		private List<OptionWrapper> OptionWrappers => _optionWrappers ?? (_optionWrappers = LoadWrappers());

		private void ButtonSearch_OnClick(object sender, RoutedEventArgs e) => UpdateSearchResult(TextBoxSearch.Text);

		private List<OptionWrapper> LoadWrappers()
		{
			var optionsMenuItems = new[]
			{
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayDeckWindows, nameof(Core.MainWindow.Options.OptionsOverlayDeckWindows)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayGeneral, nameof(Core.MainWindow.Options.OptionsOverlayGeneral)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayInteractivity, nameof(Core.MainWindow.Options.OptionsOverlayInteractivity)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayOpponent, nameof(Core.MainWindow.Options.OptionsOverlayOpponent)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsOverlayPlayer, nameof(Core.MainWindow.Options.OptionsOverlayPlayer)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerAppearance, nameof(Core.MainWindow.Options.OptionsTrackerAppearance)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerBackups, nameof(Core.MainWindow.Options.OptionsTrackerBackups)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerGeneral, nameof(Core.MainWindow.Options.OptionsTrackerGeneral)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerHotKeys, nameof(Core.MainWindow.Options.OptionsTrackerHotKeys)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerImporting, nameof(Core.MainWindow.Options.OptionsTrackerImporting)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerNotifications, nameof(Core.MainWindow.Options.OptionsTrackerNotifications)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerPlugins, nameof(Core.MainWindow.Options.OptionsTrackerPlugins)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerSettings, nameof(Core.MainWindow.Options.OptionsTrackerSettings)),
				new UserControlWrapper(Core.MainWindow.Options.OptionsTrackerStats, nameof(Core.MainWindow.Options.OptionsTrackerStats))
			};
			var optionWrappers = new List<OptionWrapper>();
			foreach(var optionsMenuItem in optionsMenuItems)
			{
				var option = Helper.FindLogicalDescendants<DependencyObject>(IsSearchableOption, optionsMenuItem.UserControl);
				optionWrappers.AddRange(option.Select(cb => WrapOption(optionsMenuItem, cb)));
			}
			return optionWrappers;
		}

		private bool IsSearchableOption(DependencyObject depObj)
		{
			var basicTypes = new List<Type>{typeof(CheckBox)};
			if(basicTypes.Contains(depObj.GetType())) return true;
			if(depObj is DockPanel)
			{
				var dockPanel = depObj as DockPanel;
				foreach(var child in dockPanel.Children)
				{
					if(basicTypes.Contains(child.GetType())) return true;
				}
			}
			return false;
		}

		private OptionWrapper WrapOption(UserControlWrapper menuItem, DependencyObject depObj)
		{
			if(depObj is DockPanel)
			{
				var panel = depObj as DockPanel;
				foreach(var child in panel.Children)
				{
					if(child is CheckBox)
						return WrapSimpleOption(menuItem, child as CheckBox);
				}
			}
			return WrapSimpleOption(menuItem, depObj as ContentControl);
		}

		private OptionWrapper WrapSimpleOption(UserControlWrapper menuItem, ContentControl control)
		{
			if(control == null)
				throw new ArgumentNullException("control");
			if(control is CheckBox)
				return new CheckBoxWrapper(menuItem, control as CheckBox);
			throw new ArgumentException("Argument must be a wrappable option type.", "control");
		}

		private void UpdateSearchResult(string text)
		{
			ListBoxSearchResult.Items.Clear();
			if(string.IsNullOrEmpty(text))
				return;
			foreach(var wrapper in OptionWrappers.Where(x => x.Matches(text)))
				ListBoxSearchResult.Items.Add(wrapper);
		}

		private void ListBoxSearchResult_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var selected = (sender as ListBox)?.SelectedItem as OptionWrapper;
			if(selected == null)
				return;
			var tvis = Helper.FindLogicalDescendantsDeep<TreeViewItem>(Core.MainWindow.Options.TreeViewOptions);
			var target = tvis.FirstOrDefault(x => x.Name.Contains(selected.MenuItem.Name.Substring(7)));
			if(target != null)
			{
				if(selected.OptionControl.Visibility == Visibility.Collapsed)
					AdvancedOptions.Instance.Show = true;
				target.IsSelected = true;
			}
		}

		public abstract class OptionWrapper
		{
			public OptionWrapper(UserControlWrapper menuItem)
			{
				MenuItem = menuItem;
			}

			public UserControlWrapper MenuItem { get; }

			public abstract Control OptionControl { get; }

			// Returns true if the option matches the search query.
			public abstract bool Matches(string query);

			// Returns the user-friendly string describing the option.
			public override abstract string ToString();
		}

		public class CheckBoxWrapper : OptionWrapper
		{
			public CheckBoxWrapper(UserControlWrapper menuItem, CheckBox checkBox) : base(menuItem)
			{
				CheckBox = checkBox;
			}

			private CheckBox CheckBox { get; }
			public override Control OptionControl { get => CheckBox; }

			public override bool Matches(string query)
			{
				return CheckBox.Content.ToString().ToUpperInvariant().Contains(query.ToUpperInvariant())
					|| CheckBox.Name.ToUpperInvariant().Replace("CHECKBOX", "").Contains(query.ToUpperInvariant());
			}

			public override string ToString() => $"{(CheckBox.Visibility == Visibility.Collapsed ? "[Adv.] " : "")}{MenuItem.Name.Substring(7).Insert(7, " > ")}: {CheckBox.Content}";
		}

		public class UserControlWrapper
		{
			public UserControlWrapper(UserControl userControl, string name)
			{
				UserControl = userControl;
				Name = name;
			}

			public UserControl UserControl { get; set; }
			public string Name { get; set; }
		}

		private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (TextBoxSearch.Text.Length < 3)
				return;
			UpdateSearchResult(TextBoxSearch.Text);
		}
	}
}
