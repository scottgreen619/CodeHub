using System;
using System.Linq;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeHub.ViewControllers;
using GitHubSharp;

namespace CodeHub.Controllers
{
    public class RepositoryEventsController : EventsController
    {
        public string Slug { get; private set; }

        public RepositoryEventsController(IListView<EventModel> view, string username, string slug)
            : base(view, username)
        {
            Slug = slug;
        }

        protected override GitHubResponse<List<EventModel>> GetData(int start = 0, int limit = DataLimit)
        {
            return Application.Client.Users[Username].Repositories[Slug].GetEvents(start, limit);
        }
    }
}