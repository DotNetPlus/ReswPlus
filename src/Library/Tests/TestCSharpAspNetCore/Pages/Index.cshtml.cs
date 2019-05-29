using ReswPlusLib;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace TestCSharpAspNetCore.Pages
{
    public class IndexModel : PageModel
    {
        public List<string> ResourceManagerStrings { get; set; }
        public List<string> StringLocalizerStrings { get; set; }

        private readonly IStringLocalizer<SharedResources> _localizer;
        public IndexModel(IStringLocalizer<SharedResources> localizer)
        {
            _localizer = localizer;
        }

        public void OnGet()
        {
            {
                var resourceManager = TestCSharpAspNetCore.Resources.SharedResources.ResourceManager;
                ResourceManagerStrings = new List<string>();
                for (var i = 0; i < 5; ++i)
                {
                    ResourceManagerStrings.Add(string.Format(resourceManager.GetPlural("YouGotMail", i), i));
                }

                for (double i = 0; i < 5; i += 0.5)
                {
                    ResourceManagerStrings.Add(string.Format(resourceManager.GetPlural("RunDistance", i), i));
                }
            }

            {
                StringLocalizerStrings = new List<string>();
                for (var i = 0; i < 5; ++i)
                {
                    StringLocalizerStrings.Add(string.Format(_localizer.GetPlural("YouGotMail", i), i));
                }

                for (double i = 0; i < 5; i += 0.5)
                {
                    StringLocalizerStrings.Add(string.Format(_localizer.GetPlural("RunDistance", i), i));
                }
            }
        }
    }
}
