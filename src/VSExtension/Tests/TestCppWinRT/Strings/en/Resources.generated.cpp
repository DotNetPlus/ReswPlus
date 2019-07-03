// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#include <pch.h>
#include "Resources.generated.h"
#include "Strings.Resources.g.cpp"
#include "Strings.ResourcesExtension.g.cpp"
#include <stdio.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.UI.Xaml.Interop.h>
using namespace winrt;
using namespace std;
using namespace winrt::TestCppWinRT::Strings;
using namespace winrt::Windows::Foundation;
using namespace winrt::Windows::ApplicationModel::Resources;
using namespace winrt::Windows::UI::Xaml::Interop;
namespace local = winrt::TestCppWinRT::Strings::implementation;

ResourceLoader local::Resources::_resourceLoader = nullptr;
ResourceLoader local::Resources::GetResourceLoader()
{
    if (_resourceLoader == nullptr)
    {
        _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
    }
    return _resourceLoader;
}

hstring local::Resources::YouGotEmails(double pluralNumber)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", pluralNumber);
}

hstring local::Resources::YouGotEmails_Format(unsigned int numberMessages, hstring const& username)
{
    size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
    return hstring(buffer);
}

hstring local::Resources::GotMessagesFrom(double pluralNumber, int variantId)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), hstring(L"GotMessagesFrom_Variant") + to_wstring(variantId), pluralNumber);
}

hstring local::Resources::GotMessagesFrom_Format(unsigned int numberMessages, int personalPronoun)
{
    size_t needed = _swprintf_p(nullptr, 0, GotMessagesFrom(static_cast<double>(numberMessages), personalPronoun).c_str(), numberMessages, personalPronoun);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GotMessagesFrom(static_cast<double>(numberMessages), personalPronoun).c_str(), numberMessages, personalPronoun);
    return hstring(buffer);
}


hstring local::Resources::Hello()
{
    return GetResourceLoader().GetString(L"Hello");
}

hstring local::Resources::TestWithObject()
{
    return GetResourceLoader().GetString(L"TestWithObject");
}

hstring local::Resources::TestWithObject_Format(IInspectable const& obj)
{
    auto _obj_istringable = obj.try_as<IStringable>();
    auto _obj_string = _obj_istringable == nullptr ? L"" : _obj_istringable.ToString().c_str();
    size_t needed = _swprintf_p(nullptr, 0, TestWithObject().c_str(), _obj_string);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, TestWithObject().c_str(), _obj_string);
    return hstring(buffer);
}

local::ResourcesExtension::ResourcesExtension()
{
    _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
}

IInspectable local::ResourcesExtension::ProvideValue()
{
    hstring res;
    if(Key() == KeyEnum::__Undefined)
    {
        res = L"";
    }
    else
    {
        auto keyStr = KeyEnumToString(Key());
        if(keyStr == L"")
        {
            return box_value(L"");
        }
        res = _resourceLoader.GetString(keyStr);
    }
    return Converter() == nullptr ? box_value(res) : Converter().Convert(box_value(res), xaml_typename<hstring>(), ConverterParameter(), L"");
}

hstring local::ResourcesExtension::KeyEnumToString(KeyEnum key)
{
    switch(key)
    {
        case KeyEnum::Hello:
            return hstring(L"Hello");
        case KeyEnum::TestWithObject:
            return hstring(L"TestWithObject");
        default:
            return hstring(L"");
    }
}
