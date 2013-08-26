using System;
using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class RepositoriesWatchedController : RepositoriesController
    {
        public RepositoriesWatchedController(IListView<RepositoryModel> view)
            : base(view, string.Empty)
        {
        }

        public override void Update(bool force)
        {
            var response = Application.Client.AuthenticatedUser.Repositories.GetWatching();
            Model = new ListModel<RepositoryModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}

