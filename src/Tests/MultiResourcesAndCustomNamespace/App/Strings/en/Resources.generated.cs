// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package PluralNet is necessary to support Pluralization.
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Data;

namespace MultiResources.Strings {
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        private static ResourceLoader  _resourceLoader;
        static Resources()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }

        /// <summary>
        ///   Looks up a localized string similar to: Hello
        /// </summary>
        public static string HelloMessage => _resourceLoader.GetString("HelloMessage");

    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public class ResourcesExtension: MarkupExtension
    {
        public enum KeyEnum
        {
            __Undefined = 0,
            HelloMessage,
        }

        private static ResourceLoader  _resourceLoader;
        static ResourcesExtension()
        {
            _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources");
        }
        public KeyEnum Key { get; set;}
        public IValueConverter Converter { get; set;}
        public object ConverterParameter { get; set;}
        protected override object ProvideValue()
        {
            string res;
            if(Key == KeyEnum.__Undefined)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader.GetString(Key.ToString());
            }
            return Converter == null ? res : Converter.Convert(res, typeof(String), ConverterParameter, null);
        }
    }

}

