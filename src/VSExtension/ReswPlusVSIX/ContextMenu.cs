using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ReswPlus.Core.Converters;
using ReswPlus.Core.ClassGenerator;
using ReswPlus.Core.ResourceParser;
using ReswPlus.ResourceInfo;
using ReswPlus.Utils;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Language = ReswPlus.Core.ResourceInfo.Language;
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
        public const int ExportAndroidCommandId = 0x0202;

        /// <summary>
        /// Command menu variant (command set GUID).
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

            menuCommandID = new CommandID(CommandSet, ExportAndroidCommandId);
            menuItem = new OleMenuCommand(ExportAndroidCommandExecute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ProjectItem GetCurrentProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            object selectedObject = null;

            var monitorSelection = (IVsMonitorSelection)Package.GetGlobalService(typeof(SVsShellMonitorSelection));
            monitorSelection.GetCurrentSelection(out var hierarchyPointer,
                out var projectItemId,
                out _,
                out _);

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

        private void ExportAndroidCommandExecute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var projectItem = GetCurrentProjectItem();
                if (projectItem.Name.EndsWith(".resw") &&
                    projectItem.Properties.Item("FullPath").Value is string filepath)
                {
                    var reswContent = File.ReadAllText(filepath);
                    var androidFile = AndroidXMLConverter.ReswToAndroidXML(ReswParser.Parse(reswContent), true);
                    using (var saveFileDialog = new SaveFileDialog()
                    {
                        FileName = Path.GetFileNameWithoutExtension(projectItem.Name) + ".xml"
                    })
                    {
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            androidFile.Save(saveFileDialog.FileName);
                        }
                    }
                    return;
                }
            }
            catch
            { }
            VsShellUtilities.ShowMessageBox(
                 package,
                 "Can't convert this resw file",
                 "ReswPlus",
                 OLEMSGICON.OLEMSGICON_INFO,
                 OLEMSGBUTTON.OLEMSGBUTTON_OK,
                 OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private void GeneratePluralizationExecute(object sender, EventArgs e)
        {
            GenerateResourceFile(true);
        }

        private void GenerateExecute(object sender, EventArgs e)
        {
            GenerateResourceFile(false);
        }
        private int GenerateResourceFile(bool isAdvanced)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            VSUIIntegration.Instance?.ClearErrors();

            try
            {
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
                    return VSConstants.E_FAIL;
                }
                VSUIIntegration.Instance?.SetStatusBar($"Generating class for {Path.GetFileName(projectItem.Name)}...");

                var language = projectItem.ContainingProject.GetLanguage();
                if (language == Language.CSHARP || language == Language.VB)
                {
                    // Reset CustomTool to force the file generation.
                    projectItem.Properties.Item("CustomTool").Value = "";
                    projectItem.Properties.Item("CustomTool").Value = isAdvanced ? "ReswPlusAdvancedGenerator" : "ReswPlusGenerator";
                    return VSConstants.S_OK;
                }
                else if (language == Language.CPPCX || language == Language.CPPWINRT)
                {
                    // CPP projects doesn't support custom tools, we need to create the file ourselves.
                    var filepath = (string)projectItem.Properties.Item("FullPath").Value;
                    var fileNamespace = (string)projectItem.ContainingProject.Properties.Item("RootNamespace").Value;

                    var relativeDirectoryPath = Path.GetDirectoryName((string)projectItem.Properties.Item("RelativePath").Value);
                    var reswNamespace = relativeDirectoryPath.StartsWith("..") ? "" : relativeDirectoryPath;
                    if (!string.IsNullOrEmpty(reswNamespace))
                    {
                        fileNamespace += "." + reswNamespace.Replace("\\", ".");
                    }

                    var resourceFileInfo = ResourceFileInfoBuilder.Create(projectItem);
                    if (resourceFileInfo == null)
                    {
                        return VSConstants.E_FAIL;
                    }
                    var reswCodeGenerator = ReswClassGenerator.CreateGenerator(resourceFileInfo, VSUIIntegration.Instance);
                    if (reswCodeGenerator == null)
                    {
                        return VSConstants.E_FAIL;
                    }

                    var inputFilepath = projectItem.Properties.Item("FullPath").Value as string;
                    var baseFilename = Path.GetFileNameWithoutExtension(filepath) + ".generated";
                    var generationResult = reswCodeGenerator.GenerateCode(baseFilename, File.ReadAllText(inputFilepath), fileNamespace, isAdvanced);
                    foreach (var file in generationResult.Files)
                    {
                        var generatedFilePath = Path.Combine(Path.GetDirectoryName(filepath), file.Filename);
                        using (var streamWriter = File.Create(generatedFilePath))
                        {
                            var contentBytes = System.Text.Encoding.UTF8.GetBytes(file.Content);
                            streamWriter.Write(contentBytes, 0, contentBytes.Length);
                        }
                        try
                        {
                            projectItem.ProjectItems.AddFromFile(generatedFilePath);
                        }
                        catch { }
                    }

                    if (generationResult.MustInstallReswPlusLib)
                    {
                        projectItem.ContainingProject.InstallNuGetPackage("ReswPlusLib", true);
                    }
                    return VSConstants.S_OK;
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
                    return VSConstants.E_UNEXPECTED;
                }
            }
            finally
            {
                VSUIIntegration.Instance?.CleanStatusBar();
            }
        }
    }
}
