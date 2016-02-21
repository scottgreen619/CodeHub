﻿using UIKit;
using System;

namespace CodeHub.iOS.ViewControllers.Walkthrough
{
    public partial class OrgViewController : BaseViewController
    {
        public OrgViewController()
            : base("OrgViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TellMeMoreButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            TellMeMoreButton.Layer.CornerRadius = 6f;

            OnActivation(d => d(TellMeMoreButton.GetClickedObservable().Subscribe(_ => TellMeMore())));
        }

        private void TellMeMore()
        {
            const string url = "https://help.github.com/articles/about-third-party-application-restrictions/";
            var view = new WebBrowserViewController(url);
            view.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.CancelButton, UIBarButtonItemStyle.Done, 
                (s, e) => DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(view), true, null);
        }
    }
}

