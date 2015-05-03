﻿using System;
using ReactiveUI;
using Octokit;
using System.Reactive;
using CodeHub.Core.Factories;
using System.Reactive.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestDiffViewModel : BaseViewModel
    {
        private readonly ISubject<PullRequestReviewComment> _commentCreatedObservable = new Subject<PullRequestReviewComment>();

        public IObservable<PullRequestReviewComment> CommentCreated
        {
            get { return _commentCreatedObservable.AsObservable(); }
        }

        private readonly ObservableAsPropertyHelper<string> _filename;
        public string Filename
        {
            get { return _filename.Value; }
        }

        private readonly ObservableAsPropertyHelper<string> _patch;
        public string Patch
        {
            get { return _patch.Value; }
        }

        private ObservableAsPropertyHelper<IReadOnlyList<PullRequestReviewComment>> _comments;
        public IReadOnlyList<PullRequestReviewComment> Comments
        {
            get { return _comments.Value; }
        }

        private PullRequestFile _pullRequestFile;
        public PullRequestFile PullRequestFile
        {
            get { return _pullRequestFile; }
            private set { this.RaiseAndSetIfChanged(ref _pullRequestFile, value); }
        }

        private PullRequestFilesViewModel _parentViewModel;
        private PullRequestFilesViewModel ParentViewModel
        {
            get { return _parentViewModel; }
            set { this.RaiseAndSetIfChanged(ref _parentViewModel, value); }
        }

        private int? _selectedPatchLine;
        public int? SelectedPatchLine
        {
            get { return _selectedPatchLine; }
            set { this.RaiseAndSetIfChanged(ref _selectedPatchLine, value); }
        }

        public IReactiveCommand<Unit> GoToCommentCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public PullRequestDiffViewModel(IActionMenuFactory actionMenuFactory)
        {
            var gotoCreateCommentCommand = ReactiveCommand.Create()
                .WithSubscription(_ => {
                    var vm = this.CreateViewModel<PullRequestCommentViewModel>();
                    vm.SaveCommand.Subscribe(_commentCreatedObservable);
                    vm.Init(_parentViewModel.RepositoryOwner, _parentViewModel.RepositoryName, _parentViewModel.PullRequestId,
                        _parentViewModel.HeadSha, _pullRequestFile.FileName, SelectedPatchLine.Value);
                    NavigateTo(vm);
            });

            GoToCommentCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedPatchLine).Select(x => x != null),
                sender => {
                    var sheet = actionMenuFactory.Create();
                    sheet.AddButton(string.Format("Add Comment on Line {0}", SelectedPatchLine), gotoCreateCommentCommand);
                    return sheet.Show(sender);
                });

            this.WhenAnyValue(x => x.PullRequestFile.Patch)
                .IsNotNull()
                .ToProperty(this, x => x.Patch, out _patch);

            this.WhenAnyValue(x => x.ParentViewModel.Comments)
                .Merge(this.WhenAnyObservable(x => x.ParentViewModel.Comments.Changed).Select(_ => ParentViewModel.Comments))
                .Select(x => x.Where(y => string.Equals(y.Path, Filename, StringComparison.OrdinalIgnoreCase)).ToList())
                .ToProperty(this, x => x.Comments, out _comments);

            this.WhenAnyValue(x => x.PullRequestFile.FileName)
                .ToProperty(this, x => x.Filename, out _filename);

            this.WhenAnyValue(x => x.Filename)
                .Subscribe(x => {
                    if (string.IsNullOrEmpty(x))
                        Title = "Diff";
                    else
                        Title = Path.GetFileName(Filename) ?? Filename.Substring(Filename.LastIndexOf('/') + 1);
                });
        }

        public PullRequestDiffViewModel Init(PullRequestFilesViewModel parentViewModel, PullRequestFile pullRequestFile)
        {
            PullRequestFile = pullRequestFile;
            ParentViewModel = parentViewModel;
            return this;
        }
    }
}

