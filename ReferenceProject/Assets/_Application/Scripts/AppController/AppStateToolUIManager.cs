using System;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.Tools;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AppStateToolUIManager : ToolUIManager
    {
        [Inject]
        void Setup(IAppStateController appStateController, IToolUIManager toolUIManager)
        {
            appStateController.StateExit += _ => { toolUIManager.CloseAllTools(); };
        }
    }
}
