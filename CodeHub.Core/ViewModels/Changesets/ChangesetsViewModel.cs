using GitHubSharp;
using System.Collections.Generic;
using GitHubSharp.Models;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Changesets
{
	public class ChangesetsViewModel : CommitsViewModel
    {
        public string Branch { get; private set; }

        public ChangesetsViewModel(IFeaturesService featuresService)
            : base(featuresService)
        {
        }

        public void Init(NavObject navObject)
        {
			base.Init(navObject);
            Branch = navObject.Branch ?? "master";
        }

		protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
			return this.GetApplication().Client.Users[Username].Repositories[Repository].Commits.GetAll(Branch);
        }

		public new class NavObject : CommitsViewModel.NavObject
        {
            public string Branch { get; set; }
        }
    }
}

