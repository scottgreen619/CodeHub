﻿using ReactiveUI;
using CodeHub.Core.ViewModels.Users;
using CodeHub.iOS.Views.Users;

namespace CodeHub.iOS.TableViewSources
{
    public class UserTableViewSource : ReactiveTableViewSource<UserViewModel>
    {
        public UserTableViewSource(MonoTouch.UIKit.UITableView tableView, IReactiveNotifyCollectionChanged<UserViewModel> collection) 
            : base(tableView, collection,  UserTableViewCell.Key, 44.0f)
        {
            tableView.RegisterClassForCellReuse(typeof(UserTableViewCell), UserTableViewCell.Key);
        }

        public override void RowSelected(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
        {
            base.RowSelected(tableView, indexPath);
            var item = ItemAt(indexPath) as UserViewModel;
            if (item != null)
                item.GoToCommand.ExecuteIfCan();
        }
    }
}

