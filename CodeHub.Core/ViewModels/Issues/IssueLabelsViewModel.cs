using System;
using CodeFramework.Core.ViewModels;
using System.Threading.Tasks;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Core.Messages;
using System.Linq;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace CodeHub.Core.ViewModels.Issues
{
	public class IssueLabelsViewModel : LoadableViewModel
    {
		private bool _isSaving;
		public bool IsSaving
		{
			get { return _isSaving; }
			private set {
				_isSaving = value;
				RaisePropertyChanged(() => IsSaving);
			}
		}

		private readonly CollectionViewModel<LabelModel> _labels = new CollectionViewModel<LabelModel>();
		public CollectionViewModel<LabelModel> Labels
		{
			get { return _labels; }
		}

		private readonly CollectionViewModel<LabelModel> _selectedLabels = new CollectionViewModel<LabelModel>();
		public CollectionViewModel<LabelModel> SelectedLabels
		{
			get { return _selectedLabels; }
		}

		public string Username  { get; private set; }

		public string Repository { get; private set; }

		public ulong Id { get; private set; }

		public bool SaveOnSelect { get; private set; }

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
			Id = navObject.Id;
			SaveOnSelect = navObject.SaveOnSelect;
			SelectedLabels.Items.Reset(GetService<CodeFramework.Core.Services.IViewModelTxService>().Get() as IEnumerable<LabelModel>);
		}

		public ICommand SaveLabelChoices
		{
			get { return new MvxCommand(() => SelectLabels(SelectedLabels)); }
		}

		private async Task SelectLabels(IEnumerable<LabelModel> x)
		{
			if (SaveOnSelect)
			{
				try
				{
					IsSaving = true;
					var labels = x != null ? x.Select(y => y.Name).ToArray() : null;
					var updateReq = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues[Id].UpdateLabels(labels);
					var newIssue = await this.GetApplication().Client.ExecuteAsync(updateReq);
					Messenger.Publish(new IssueEditMessage(this) { Issue = newIssue.Data });
				}
				catch (Exception e)
				{
					DisplayException(e);
				}
				finally
				{
					IsSaving = false;
				}
			}
			else
			{
				Messenger.Publish(new SelectIssueLabelsMessage(this) { Labels = SelectedLabels.Items.ToArray() });
			}

			ChangePresentation(new MvxClosePresentationHint(this));
		}

		protected override Task Load(bool forceCacheInvalidation)
		{
			return Labels.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetLabels(), forceCacheInvalidation);
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
			public ulong Id { get; set; }
			public bool SaveOnSelect { get; set; }
		}
    }
}

