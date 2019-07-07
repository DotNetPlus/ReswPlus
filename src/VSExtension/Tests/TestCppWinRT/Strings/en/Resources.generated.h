// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#pragma once
#include "Strings.Resources.g.h"
#include "Strings.ResourcesExtension.g.h"
#include <winrt/Windows.ApplicationModel.Resources.h>
#include <winrt/Windows.UI.Xaml.Markup.h>
#include <winrt/ReswPlusLib.h>

namespace winrt::TestCppWinRT::Strings::implementation
{
    struct Resources
    {
    private:
        static Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;
        static Windows::ApplicationModel::Resources::ResourceLoader GetResourceLoader();
    public:
        Resources() {}

    /* Methods and properties for YouGotEmails */
    public:
        /// <summary>
        ///   Get the pluralized version of the string similar to: Hello %2$ls, you got %1$u email
        /// </summary>
        static hstring YouGotEmails(double pluralNumber);
    public:
        /// <summary>
        ///   Format the string similar to: Hello %2$ls, you got %1$u email
        /// </summary>
        static hstring YouGotEmails_Format(unsigned int numberMessages, hstring const& username);

    /* Methods and properties for GotMessagesFrom */
    public:
        /// <summary>
        ///   Get the pluralized version of the string similar to: You got %d message from her
        /// </summary>
        static hstring GotMessagesFrom(long variantId, double pluralNumber);
    public:
        /// <summary>
        ///   Format the string similar to: You got %d message from her
        /// </summary>
        static hstring GotMessagesFrom_Format(unsigned int numberMessages, long personalPronoun);

    /* Methods and properties for SendMessage */

    /* Methods and properties for YouGotEmailsDotNet */
    public:
        /// <summary>
        ///   Get the pluralized version of the string similar to: Hello {1}, you got {0:F0} email (DotNet formatting)
        /// </summary>
        static hstring YouGotEmailsDotNet(double pluralNumber);
    public:
        /// <summary>
        ///   Format the string similar to: Hello {1}, you got {0:F0} email (DotNet formatting)
        /// </summary>
        static hstring YouGotEmailsDotNet_Format(unsigned int numberMessages, hstring const& username);

    /* Methods and properties for Hello */
    public:
        /// <summary>
        ///   Looks up a localized string similar to: Hello world
        /// </summary>
        static hstring Hello();

    /* Methods and properties for TestWithObject */
    public:
        /// <summary>
        ///   Looks up a localized string similar to: Test with object %s
        /// </summary>
        static hstring TestWithObject();
    public:
        /// <summary>
        ///   Format the string similar to: Test with object %s
        /// </summary>
        static hstring TestWithObject_Format(Windows::Foundation::IStringable const& obj);
    };

    struct ResourcesExtension: ResourcesExtensionT<ResourcesExtension>
    {
    private:
        Windows::ApplicationModel::Resources::ResourceLoader _resourceLoader;
        TestCppWinRT::Strings::KeyEnum _key;
        Windows::UI::Xaml::Data::IValueConverter _converter;
        Windows::Foundation::IInspectable _converterParameter;
    public:
        ResourcesExtension();
        TestCppWinRT::Strings::KeyEnum Key(){ return _key; }
        void Key(TestCppWinRT::Strings::KeyEnum value){ _key = value; }
        Windows::UI::Xaml::Data::IValueConverter Converter(){{ return _converter; }}
        void Converter(Windows::UI::Xaml::Data::IValueConverter value){{ _converter = value; }}
        Windows::Foundation::IInspectable ConverterParameter(){{ return _converterParameter; }}
        void ConverterParameter(Windows::Foundation::IInspectable value){{ _converterParameter = value; }}
        Windows::Foundation::IInspectable ProvideValue();
    private:
        static hstring KeyEnumToString(KeyEnum key);
    };
} // namespace winrt::TestCppWinRT::Strings::implementation

namespace winrt::TestCppWinRT::Strings::factory_implementation
{
    struct Resources : ResourcesT<Resources, implementation::Resources>
    {
    };

    struct ResourcesExtension : ResourcesExtensionT<ResourcesExtension, implementation::ResourcesExtension>
    {
    };
} // namespace winrt::TestCppWinRT::Strings::factory_implementation
