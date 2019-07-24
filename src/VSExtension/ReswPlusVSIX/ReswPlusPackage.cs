// Copyright (c) Rudy Huyn. All rights reserved.
// Licensed under the MIT License.
// Source: https://github.com/rudyhuyn/ReswPlus

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace ReswPlus
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideUIContextRule(_uiContextSupportedFiles,
        name: "Supported Files",
        expression: "ReswFile",
        termNames: new[] { "ReswFile" },
        termValues: new[] { "HierSingleSelectionName:.resw$" })]

    public sealed class ReswPlusPackage : AsyncPackage
    {
        /// <summary>
        /// ReswPlusPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "8c9d543c-0e65-4319-8395-f7b38088a080";
        public const string _uiContextSupportedFiles = "34551deb-f034-43e9-a279-0e541241687e"; // Must match guid in VsCommandTable.vsct
        private static ErrorListProvider _errorListProvider;
        private static ReswPlusPackage _instance;
        public ReswPlusPackage()
        {
            _errorListProvider = new ErrorListProvider(this);
            _instance = this;
        }

        public static void ClearErrors()
        {
            _errorListProvider.Tasks.Clear();
        }

        public static void LogWarning(string message)
        {
            if (_errorListProvider == null)
            {
                return;
            }

            _errorListProvider.Tasks.Add(new ErrorTask()
            {
                Category = TaskCategory.Misc,
                CanDelete = true,
                ErrorCategory = TaskErrorCategory.Warning,
                Text = message
            });
            _errorListProvider.Show();
        }

        public static void LogError(string message, string document = null)
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

        public static void DisplayMessageOutput(string message)
        {
            if (_instance == null)
            {
                return;
            }
            ThreadHelper.ThrowIfNotOnUIThread();
            var outputWindow = _instance.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null)
            {
                return;
            }
            Guid customGuid = new Guid("CD10903C-5DF9-4DD1-A027-FFFC7F88426F");
            string customTitle = "ReswPlus";
            outputWindow.CreatePane(ref customGuid, customTitle, 1, 1);

            outputWindow.GetPane(ref customGuid, out IVsOutputWindowPane pane);
            if (pane != null)
            {
                pane.OutputString(message + "\n");
            }
        }

        public static void SetStatusBar(string message)
        {
            if (_instance == null)
            {
                return;
            }
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _instance.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
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

        public static void CleanStatusBar()
        {
            if (_instance == null)
            {
                return;
            }
            ThreadHelper.ThrowIfNotOnUIThread();
            var statusBar = _instance.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
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


        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ContextMenu.InitializeAsync(this);
        }

        #endregion


    }
}
