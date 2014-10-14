using System;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Cells;
using GitHubSharp.Models;

namespace CodeHub.iOS.Views.Source
{
    public class BranchesAndTagsView : ReactiveTableViewController<BranchesAndTagsViewModel>
	{
        private readonly UISegmentedControl _viewSegment = new UISegmentedControl(new object[] {"Branches", "Tags"});

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            _viewSegment.ValueChanged += (sender, args) => ViewModel.SelectedFilter = (BranchesAndTagsViewModel.ShowIndex) _viewSegment.SelectedSegment;
            ViewModel.WhenAnyValue(x => x.SelectedFilter).Subscribe(x => _viewSegment.SelectedSegment = (int)x);
            NavigationItem.TitleView = _viewSegment;

            TableView.RegisterClassForCellReuse(typeof(BranchCellView), BranchCellView.Key);
            TableView.RegisterClassForCellReuse(typeof(TagCellView), TagCellView.Key);
 
            ViewModel.WhenAnyValue(x => x.SelectedFilter)
                .Subscribe(x =>
                {
                    if (TableView.Source != null)
                    {
                        TableView.Source.Dispose();
                        TableView.Source = null;
                    }

                    if (x == BranchesAndTagsViewModel.ShowIndex.Branches)
                    {
                        var source = new ReactiveTableViewSource<BranchModel>(TableView, ViewModel.Branches, BranchCellView.Key, 44f);
                        source.ElementSelected.Subscribe(ViewModel.GoToSourceCommand.ExecuteIfCan);
                        TableView.Source = source;
                    }
                    else
                    {
                        var source = new ReactiveTableViewSource<TagModel>(TableView, ViewModel.Tags, TagCellView.Key, 44f);
                        source.ElementSelected.Subscribe(ViewModel.GoToSourceCommand.ExecuteIfCan);
                        TableView.Source = source;
                    }
                });

            ViewModel.LoadCommand.ExecuteIfCan();
		}
	}
}

