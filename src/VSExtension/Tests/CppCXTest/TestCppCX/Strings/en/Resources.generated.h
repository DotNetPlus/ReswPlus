// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#pragma once
#include <stdio.h>

namespace TestCppCX{namespace Strings{
    public ref class Resources sealed{
    private:
        static Windows::ApplicationModel::Resources::ResourceLoader^ GetResourceLoader() {
            static Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader(nullptr);
            if (_resourceLoader == nullptr)
            {
                _resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L"Resources");
            }
            return _resourceLoader;
        }
    public:
        Resources() {}

    #pragma region YouGotEmails
    public:
        /// <summary>
        ///   Get the pluralized version of the string similar to: Hello %2$ls, you got %1$u email
        /// </summary>
        static Platform::String^ YouGotEmails(double number)
        {
            return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), "YouGotEmails", number);
        }

    /// <summary>
    ///   Format the string similar to: Hello %2$ls, you got %1$u email
    /// </summary>
    public:
        static Platform::String^ YouGotEmails_Format(unsigned int numberMessages, Platform::String^ username)
        {
            size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
            wchar_t *buffer = new wchar_t[needed + 1];
            _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
            return ref new Platform::String(buffer);
        }
    #pragma endregion

    public:
        /// <summary>
        ///   Looks up a localized string similar to: Hello world
        /// </summary>
        static property Platform::String^ Hello
        {
            Platform::String^ get() { return GetResourceLoader()->GetString("Hello"); }
        }

    };

    public enum class KeyEnum
    {
        __Undefined = 0,
        Hello,
    };

    public ref class ResourcesExtension sealed: public Windows::UI::Xaml::Markup::MarkupExtension
    {
    private:
        Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;
    public:
        ResourcesExtension()
        {
            _resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L"Resources");
        }
        property KeyEnum Key;
        property Windows::UI::Xaml::Data::IValueConverter^ Converter;
        property Platform::Object^ ConverterParameter;
    protected:
        virtual Platform::Object^ ProvideValue() override
        {
            Platform::String^ res;
            if(Key == KeyEnum::__Undefined)
            {
                res = "";
            }
            else
            {
                res = _resourceLoader->GetString(Key.ToString());
            }
            return Converter == nullptr ? res : Converter->Convert(res, Windows::UI::Xaml::Interop::TypeName(Platform::String::typeid), ConverterParameter, nullptr);
        }
    };

}} //TestCppCX::Strings
