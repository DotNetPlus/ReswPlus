using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.SingleFileGenerators;
using ReswPlus.Utils;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using Task = System.Threading.Tasks.Task;

namespace ReswPlus
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ContextMenu
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int GenerateCommandId = 0x0200;
        public const int GeneratePluralizationCommandId = 0x0201;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6e52371a-5161-41cf-bd14-72203edf374d");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenu"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ContextMenu(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, GenerateCommandId);
            var menuItem = new OleMenuCommand(GenerateExecute, menuCommandID);
            commandService.AddCommand(menuItem);

            menuCommandID = new CommandID(CommandSet, GeneratePluralizationCommandId);
            menuItem = new OleMenuCommand(GeneratePluralizationExecute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ProjectItem GetCurrentProjectItem()
        {
            object selectedObject = null;

            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out var hierarchyPointer,
                out var projectItemId,
                out var multiItemSelect,
                out var selectionContainerPointer);

            var selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                hierarchyPointer,
                typeof(IVsHierarchy)) as IVsHierarchy;

            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                    projectItemId,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out selectedObject));

            }
            return selectedObject as ProjectItem;
        }


        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ContextMenu Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ContextMenu's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ContextMenu(package, commandService);
        }

        private void GeneratePluralizationExecute(object sener, EventArgs e)
        {
            GenerateResourceFile(true);
        }

        private void GenerateExecute(object sender, EventArgs e)
        {
            GenerateResourceFile(false);
        }
        private void GenerateResourceFile(bool usePluralization)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectItem = GetCurrentProjectItem();
            if (!projectItem.Name.EndsWith(".resw"))
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "File not compatible with ReswPlus",
                    "ReswPlus",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }
            var language = projectItem.GetLanguage();
            if (language == Utils.Language.CSHARP || language == Utils.Language.VB)
            {
                projectItem.Properties.Item("CustomTool").Value = usePluralization ? "ReswPlusAdvancedGenerator" : "ReswPlusGenerator";
            }
            else if (language == Utils.Language.CPP)
            {
                // CPP projects doesn't support custom tools, we need to create the file ourselves.

                var filepath = (string)projectItem.Properties.Item("FullPath").Value;
                var fileNamespace = (string)projectItem.ContainingProject.Properties.Item("RootNamespace").Value;

                var relativeDirectoryPath = Path.GetDirectoryName((string)projectItem.Properties.Item("RelativePath").Value);
                var reswNamespace = relativeDirectoryPath;
                if (!string.IsNullOrEmpty(reswNamespace))
                {
                    fileNamespace += "." + reswNamespace.Replace("\\", ".");
                }
                var generator = new ReswPlusGeneratorBase(usePluralization);
                generator.Generate(projectItem, File.ReadAllText(filepath), fileNamespace, out var output, null);

                var extension = ".h";
                var generatedFilePath = Path.Combine(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath)) + ".generated" + extension;
                using (var streamWriter = File.Create(generatedFilePath))
                {
                    streamWriter.Write(output, 0, output.Length);
                }
                try
                {
                    projectItem.ProjectItems.AddFromFile(generatedFilePath);
                }
                catch { }
            }
            else
            {
                VsShellUtilities.ShowMessageBox(
                           package,
                           "Project language not compatible with ReswPlus",
                           "ReswPlus",
                           OLEMSGICON.OLEMSGICON_INFO,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }
    }
}
