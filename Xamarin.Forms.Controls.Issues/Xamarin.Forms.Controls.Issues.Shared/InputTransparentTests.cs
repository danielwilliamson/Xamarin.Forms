﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 8675309, "Test InputTransparent true/false on various controls")]
	public class InputTransparentTests : TestNavigationPage
	{
		const string TargetAutomationId = "inputtransparenttarget";
		ContentPage _menu;

#if UITEST
		[Test, TestCaseSource(nameof(TestCases))]
		public void VerifyInputTransparent(string menuItem)
		{
			RunningApp.WaitForElement(q => q.Marked(menuItem));
			RunningApp.Tap(q => q.Marked(menuItem));

			// Find the start label
			RunningApp.WaitForElement(q => q.Marked("Start"));

			// Find the control we're testing
			var result = RunningApp.WaitForElement(q => q.Marked(TargetAutomationId));
			var target = result.First().Rect;

			// Tap the control
			RunningApp.TapCoordinates(target.CenterX, target.CenterY);

			// Since InputTransparent is set to false, the start label should not have changed
			RunningApp.WaitForElement(q => q.Marked("Start"));

			// Switch to InputTransparent == true
			RunningApp.Tap(q => q.Marked("Toggle"));

			// Tap the control
			RunningApp.TapCoordinates(target.CenterX, target.CenterY);

			// Since InputTransparent is set to true, the start label should now show a single tap
			RunningApp.WaitForElement(q => q.Marked("Taps registered: 1"));
		}
#endif

		ContentPage CreateTestPage(View view)
		{
			var layout = new Grid();
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition());

			var abs = new AbsoluteLayout();
			var box = new BoxView { Color = Color.BlanchedAlmond };

			var label = new Label { BackgroundColor = Color.Chocolate, Text = "Start", Margin = 5 };

			var taps = 0;

			abs.Children.Add(box, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

			abs.Children.Add(label, new Rectangle(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);

			box.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					taps += 1;
					label.Text = $"Taps registered: {taps}";
				})
			});

			view.InputTransparent = false;
			abs.Children.Add(view, new Rectangle(.5, .5, .5, .5), AbsoluteLayoutFlags.All);

			var toggleButton = new Button { AutomationId = "Toggle", Text = $"Toggle InputTransparent (now {view.InputTransparent})" };
			toggleButton.Clicked += (sender, args) =>
			{
				view.InputTransparent = !view.InputTransparent;
				toggleButton.Text = $"Toggle InputTransparent (now {view.InputTransparent})";
			};

			layout.Children.Add(toggleButton);
			layout.Children.Add(abs);

			Grid.SetRow(abs, 1);

			return new ContentPage {Content = layout};
		}

		Button MenuButton(string label, Func<View> view)
		{
			var button = new Button { Text = label };

			var testView = view();
			testView.AutomationId = TargetAutomationId;

			button.Clicked += (sender, args) => PushAsync(CreateTestPage(testView));

			return button;
		}

		IEnumerable<string> TestCases
		{
			get
			{
				return (BuildMenu().Content as Layout).InternalChildren.Select(view => (view as Button).Text);
			}
		}

		ContentPage BuildMenu()
		{
			if (_menu != null)
			{
				return _menu;
			}

			var layout = new StackLayout();

			layout.Children.Add(MenuButton(nameof(Image), () => new Image { Source = ImageSource.FromFile("oasis.jpg") }));
			layout.Children.Add(MenuButton(nameof(Frame), () => new Frame { BackgroundColor = Color.DarkGoldenrod }));
			layout.Children.Add(MenuButton(nameof(Entry), () => new Entry()));
			layout.Children.Add(MenuButton(nameof(Editor), () => new Editor()));
			layout.Children.Add(MenuButton(nameof(Button), () => new Button { Text = "Test" }));
			layout.Children.Add(MenuButton(nameof(Label), () => new Label
			{
				LineBreakMode = LineBreakMode.WordWrap,
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
			}));
			layout.Children.Add(MenuButton(nameof(SearchBar), () => new SearchBar()));
			layout.Children.Add(MenuButton(nameof(DatePicker), () => new DatePicker()));
			layout.Children.Add(MenuButton(nameof(TimePicker), () => new TimePicker()));
			layout.Children.Add(MenuButton(nameof(Slider), () => new Switch()));
			layout.Children.Add(MenuButton(nameof(Switch), () => new Slider()));
			layout.Children.Add(MenuButton(nameof(Stepper), () => new Stepper()));
			layout.Children.Add(MenuButton(nameof(BoxView), () => new BoxView { BackgroundColor = Color.DarkMagenta, WidthRequest = 100, HeightRequest = 100 }));

			return new ContentPage { Content = layout };
		}

		protected override void Init()
		{
			PushAsync(BuildMenu());
		}
	}
}
