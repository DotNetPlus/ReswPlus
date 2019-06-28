// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#include <pch.h>
#include "Resources.generated.h"
#include <stdio.h>

Windows::ApplicationModel::Resources::ResourceLoader^ TestCppCX::Strings::Resources::_resourceLoader = nullptr;
Windows::ApplicationModel::Resources::ResourceLoader^ TestCppCX::Strings::Resources::GetResourceLoader()
{
    if (_resourceLoader == nullptr)
    {
        _resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L"Resources");
    }
    return _resourceLoader;
}

Platform::String^ TestCppCX::Strings::Resources::YouGotEmails(double number)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", number);
}

Platform::String^ TestCppCX::Strings::Resources::YouGotEmails_Format(unsigned int numberMessages, Platform::String^ username)
{
    size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
    return ref new Platform::String(buffer);
}

Platform::String^ TestCppCX::Strings::Resources::Hello::get()
{
    return GetResourceLoader()->GetString(L"Hello");
}

TestCppCX::Strings::ResourcesExtension::ResourcesExtension()
{
    _resourceLoader = Windows::ApplicationModel::Resources::ResourceLoader::GetForViewIndependentUse(L"Resources");
}

Platform::Object^ TestCppCX::Strings::ResourcesExtension::ProvideValue()
{
    Platform::String^ res;
    if(Key == KeyEnum::__Undefined)
    {
        res = L"";
    }
    else
    {
        res = _resourceLoader->GetString(Key.ToString());
    }
    return Converter == nullptr ? res : Converter->Convert(res, Windows::UI::Xaml::Interop::TypeName(Platform::String::typeid), ConverterParameter, nullptr);
}
