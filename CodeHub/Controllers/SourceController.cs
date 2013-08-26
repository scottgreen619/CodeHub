using MonoTouch.Dialog;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Filters.Models;
using CodeHub.ViewControllers;
using GitHubSharp.Models;

namespace CodeHub.Controlleres
{
    public class SourceController : ListController<ContentModel, SourceFilterModel>
    {
        private readonly string _username;
        private readonly string _slug;
        private readonly string _branch;
        private readonly string _path;

        public SourceController(IListView<ContentModel> view, string username, string slug, string branch = "master", string path = "")
            : base(view)
        {
            _username = username;
            _slug = slug;
            _branch = branch;
            _path = path;

            Filter = Application.Account.GetFilter<SourceFilterModel>(this);
        }

        protected override void SaveFilterAsDefault(SourceFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
        }

        protected override List<ContentModel> FilterModel(List<ContentModel> model, SourceFilterModel filter)
        {
            IEnumerable<ContentModel> ret = model;
            var order = (SourceFilterModel.Order)filter.OrderBy;
            if (order == SourceFilterModel.Order.Alphabetical)
                 ret = model.OrderBy(x => x.Name);
            return filter.Ascending ? ret.ToList() : ret.Reverse().ToList();
        }
  
        public override void Update(bool force)
        {
            var response = Application.Client.Users[_username].Repositories[_slug].GetContent(_path, _branch);
            Model = new ListModel<ContentModel> { Data = response.Data };
            Model.More = this.CreateMore(response);
        }
    }
}

