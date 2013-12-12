using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views
{
	public class NotificationsView : ViewModelCollectionDrivenViewController
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;
		private IHud _markHud;

		public NotificationsView()
        {
            _viewSegment = new UISegmentedControl(new object[] { "Unread".t(), "Participating".t(), "All".t() });
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }

        public override void ViewDidLoad()
        {
            NoItemsText = "No Notifications".t();
            Title = "Notifications".t();

            base.ViewDidLoad();

			_markHud = new Hud(View);
            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };

			var vm = (NotificationsViewModel)ViewModel;
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.CheckButton, UIBarButtonItemStyle.Plain, (s, e) => vm.ReadAllCommand.Execute(null));
			vm.ReadAllCommand.CanExecuteChanged += (sender, e) => NavigationItem.RightBarButtonItem.Enabled = vm.ReadAllCommand.CanExecute(null);

			vm.Bind(x => x.IsMarking, x =>
			{
				if (x)
					_markHud.Show("Marking...");
				else
					_markHud.Hide();
			});

			BindCollection(vm.Notifications, x =>
            {
                var el = new StyledStringElement(x.Subject.Title, x.UpdatedAt.ToDaysAgo(), UITableViewCellStyle.Subtitle) { Accessory = UITableViewCellAccessory.DisclosureIndicator };

                var subject = x.Subject.Type.ToLower();
                if (subject.Equals("issue"))
                    el.Image = Images.Flag;
                else if (subject.Equals("pullrequest"))
                    el.Image = Images.Hand;
                else if (subject.Equals("commit"))
                    el.Image = Images.Commit;

				el.Tapped += () => vm.GoToNotificationCommand.Execute(x);
                return el;
            });

			var set = this.CreateBindingSet<NotificationsView, NotificationsViewModel>();
			set.Bind(_viewSegment).To(x => x.ShownIndex);
			set.Apply();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
            base.ViewWillDisappear(animated);
        }
    }
}

