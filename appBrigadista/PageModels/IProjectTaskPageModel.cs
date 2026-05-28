using appBrigadista.Models;
using CommunityToolkit.Mvvm.Input;

namespace appBrigadista.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}