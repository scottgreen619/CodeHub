using System;
using System.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueAssignedToView : ViewModelCollectionDrivenDialogViewController
    {

        public override void ViewDidLoad()
        {
            Title = "Assignees";
            NoItemsText = "No Assignees";

            base.ViewDidLoad();

            var vm = (IssueAssignedToViewModel)ViewModel;
            BindCollection(vm.Users, x =>
            {
                var el = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                el.Clicked.Subscribe(_ => {
                    if (vm.SelectedUser != null && string.Equals(vm.SelectedUser.Login, x.Login))
                        vm.SelectedUser = null;
                    else
                        vm.SelectedUser = x;
                });

                if (vm.SelectedUser != null && string.Equals(vm.SelectedUser.Login, x.Login, StringComparison.OrdinalIgnoreCase))
                    el.Accessory = UITableViewCellAccessory.Checkmark;
                else
                    el.Accessory = UITableViewCellAccessory.None;
                return el;
            });

            vm.Bind(x => x.SelectedUser).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<UserElement>())
                    m.Accessory = (x != null && string.Equals(vm.SelectedUser.Login, m.Caption, StringComparison.OrdinalIgnoreCase)) ? 
                                     UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });

            vm.Bind(x => x.IsSaving).SubscribeStatus("Saving...");
        }
    }
}

