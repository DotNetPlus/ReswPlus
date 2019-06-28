// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#include <pch.h>
#include "Resources.generated.h"
#include <stdio.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.UI.Xaml.Interop.h>
using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::ApplicationModel::Resources;
using namespace winrt::Windows::UI::Xaml::Interop;

ResourceLoader TestCppWinRT::Strings::Resources::_resourceLoader = nullptr;
ResourceLoader TestCppWinRT::Strings::Resources::GetResourceLoader()
{
    if (_resourceLoader == nullptr)
    {
        _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
    }
    return _resourceLoader;
}

winrt::hstring TestCppWinRT::Strings::Resources::YouGotEmails(double number)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", number);
}

winrt::hstring TestCppWinRT::Strings::Resources::YouGotEmails_Format(unsigned int numberMessages, winrt::hstring username)
{
    size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
    return winrt::box_value(buffer);
}

winrt::hstring TestCppWinRT::Strings::Resources::GetHello()
{
    return GetResourceLoader().GetString(L"Hello");
}

TestCppWinRT::Strings::ResourcesExtension::ResourcesExtension()
{
    _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
}

IInspectable TestCppWinRT::Strings::ResourcesExtension::ProvideValue()
{
    winrt::hstring res;
    if(Key == KeyEnum::__Undefined)
    {
        res = L"";
    }
    else
    {
        res = _resourceLoader.GetString(Key.ToString());
    }
    return Converter == nullptr ? res : Converter.Convert(res, TypeName(winrt::hstring::typeid), ConverterParameter, nullptr);
}
