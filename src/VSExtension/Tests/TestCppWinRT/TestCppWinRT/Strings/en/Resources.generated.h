// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#pragma once
#include <winrt/Windows.ApplicationModel.Resources.h>
#include <winrt/Windows.UI.Xaml.Markup.h>

namespace TestCppWinRT
{
    namespace Strings
    {
        class Resources sealed
        {
        private:
            static winrt::Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;
            static winrt::Windows::ApplicationModel::Resources::ResourceLoader GetResourceLoader();
        public:
            Resources() {}

        /* Methods and properties for YouGotEmails */
        public:
            /// <summary>
            ///   Get the pluralized version of the string similar to: Hello %2$ls, you got %1$u email
            /// </summary>
            static winrt::hstring YouGotEmails(double number);
        public:
            /// <summary>
            ///   Format the string similar to: Hello %2$ls, you got %1$u email
            /// </summary>
            static winrt::hstring YouGotEmails_Format(unsigned int numberMessages, winrt::hstring username);

        /* Methods and properties for Hello */
        public:
            /// <summary>
            ///   Looks up a localized string similar to: Hello world
            /// </summary>
            static winrt::hstring GetHello();
        };

        enum class KeyEnum
        {
            __Undefined = 0,
            Hello,
        };

        class ResourcesExtension sealed: public winrt::Windows::UI::Xaml::Markup::MarkupExtension
        {
        private:
            winrt::Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;
        public:
            ResourcesExtension();
            KeyEnum Key;
            winrt::Windows::UI::Xaml::Data::IValueConverter Converter;
            winrt::Windows::Foundation::IInspectable ConverterParameter;
        protected:
            virtual winrt::Windows::Foundation::IInspectable ProvideValue() const override;
    };
} // namespace Strings
} // namespace TestCppWinRT
