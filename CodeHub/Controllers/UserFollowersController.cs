using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class UserFollowersController : ListController<BasicUserModel>
    {
        private readonly string _name;
        
        public UserFollowersController(IView<ListModel<BasicUserModel>> view, string name)
            : base(view)
        {
            _name = name;
        }

        public override void Update(bool force)
        {
            var response = Application.Client.Users[_name].GetFollowers(force);
            Model = new ListModel<BasicUserModel> {Data = response.Data, More = this.CreateMore(response)};
        }
    }
}

