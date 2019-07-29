using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReswPlus
{
    public class VSUIIntegration : IErrorLogger
    {
        public static VSUIIntegration Instance { get; } = new VSUIIntegration();

        private readonly IServiceProvider _provider;
        private static ErrorListProvider _errorListProvider;

        public VSUIIntegration()
        {
            _provider = ServiceProvider.GlobalProvider;
            _errorListProvider = new ErrorListProvider(_provider);
        }

        public void ClearErrors()
        {
            _errorListProvider.Tasks.Clear();
        }

        public void LogError(string message, string document = null)
        {
            Log(TaskErrorCategory.Error, message, document);
        }

        public void LogWarning(string message, string document = null)
        {
            Log(TaskErrorCategory.Warning, message, document);
        }

        private void Log(TaskErrorCategory category, string message, string document = null)
        {
            if (_errorListProvider == null)
            {
                return;
            }

            _errorListProvider.Tasks.Add(new ErrorTask()
            {
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Error,
                CanDelete = true,
                Document = document,
                Text = message,
            });
            _errorListProvider.ForceShowErrors();
            _errorListProvider.Show();
        }


        public void DisplayMessageOutput(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var outputWindow = _provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                return;
            }
            var customGuid = new Guid("CD10903C-5DF9-4DD1-A027-FFFC7F88426F");
            string customTitle = "ReswPlus";
            outputWindow.CreatePane(ref customGuid, customTitle, 1, 1);

            outputWindow.GetPane(ref customGuid, out IVsOutputWindowPane pane);
            if (pane != null)
            {
                pane.OutputString(message + "\n");
            }
        }

        public void SetStatusBar(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _provider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
            {
                return;
            }

            statusBar.IsFrozen(out int frozen);
            if (frozen == 0)
            {
                object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;
                statusBar.Animation(1, ref icon);
                statusBar.SetText(message);
            }
        }

        public void CleanStatusBar()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _provider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar == null)
            {
                return;
            }

            statusBar.IsFrozen(out int frozen);
            if (frozen == 0)
            {
                statusBar.Clear();
                object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Build;
                statusBar.Animation(0, icon);
            }
        }
    }
}
