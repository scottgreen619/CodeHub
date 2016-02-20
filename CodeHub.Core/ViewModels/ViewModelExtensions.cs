using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.Services;
using System.ComponentModel;
using System.Collections.Specialized;
using MvvmCross.Platform;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels
{
    public static class ViewModelExtensions
    {
        public static async Task RequestModel<TRequest>(this MvxViewModel viewModel, GitHubRequest<TRequest> request, bool forceDataRefresh, Action<GitHubResponse<TRequest>> update) where TRequest : new()
        {
            if (forceDataRefresh)
            {
                request.CheckIfModified = false;
                request.RequestFromCache = false;
            }

            var application = Mvx.Resolve<IApplicationService>();
            var uiThrad = Mvx.Resolve<IUIThreadService>();

			var result = await application.Client.ExecuteAsync(request).ConfigureAwait(false);
            uiThrad.MarshalOnUIThread(() => update(result));

            if (result.WasCached)
            {
                request.RequestFromCache = false;
                var uncachedTask = application.Client.ExecuteAsync(request);
                uncachedTask.FireAndForget();
                uncachedTask.ContinueWith(t => uiThrad.MarshalOnUIThread(() => update(t.Result)), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
		}

        public static void CreateMore<T>(this MvxViewModel viewModel, GitHubResponse<T> response, 
										 Action<Action> assignMore, Action<T> newDataAction) where T : new()
        {
            if (response.More == null)
            {
                assignMore(null);
                return;
            }

			Action task = () =>
            {
                response.More.UseCache = false;
				var moreResponse = Mvx.Resolve<IApplicationService>().Client.ExecuteAsync(response.More).Result;
                viewModel.CreateMore(moreResponse, assignMore, newDataAction);
                newDataAction(moreResponse.Data);
            };

            assignMore(task);
        }

        public static Task SimpleCollectionLoad<T>(this CollectionViewModel<T> viewModel, GitHubRequest<List<T>> request, bool forceDataRefresh) where T : new()
        {
            var fuckYou = new WeakReference<CollectionViewModel<T>>(viewModel);
            return viewModel.RequestModel(request, forceDataRefresh, response =>
            {
                fuckYou.Get()?.CreateMore(response, m => {
                    var weak = fuckYou.Get();
                    if (weak != null)
                        weak.MoreItems = m;
                }, viewModel.Items.AddRange);
                fuckYou.Get()?.Items.Reset(response.Data);
            });
        }
    }
}

public static class BindExtensions
{
    public static IObservable<TR> Bind<T, TR>(this T viewModel, System.Linq.Expressions.Expression<Func<T, TR>> outExpr, bool activate = false) where T : INotifyPropertyChanged
    {
        var expr = (System.Linq.Expressions.MemberExpression) outExpr.Body;
        var prop = (System.Reflection.PropertyInfo) expr.Member;
        var name = prop.Name;
        var comp = outExpr.Compile();

        var ret = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(t => viewModel.PropertyChanged += t, t => viewModel.PropertyChanged -= t)
            .Where(x => string.Equals(x.EventArgs.PropertyName, name))
            .Select(x => comp(viewModel));
        return activate ? ret.StartWith(comp(viewModel)) : ret;
    }

    public static void BindCollection<T>(this T viewModel, System.Linq.Expressions.Expression<Func<T, INotifyCollectionChanged>> outExpr, Action<NotifyCollectionChangedEventArgs> b, bool activateNow = false) where T : INotifyPropertyChanged
    {
        var exp = outExpr.Compile();
        var m = exp(viewModel);
        m.CollectionChanged += (sender, e) =>
        {
            try
            {
                b(e);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        };

        if (activateNow)
        {
            try
            {
                b(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}