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

hstring local::Resources::YouGotEmails(double number)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", number);
}

hstring local::Resources::YouGotEmails_Format(unsigned int numberMessages, hstring const& username)
{
    size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages)).c_str(), numberMessages, username.c_str());
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
        res = _resourceLoader.GetString(L"TODO");
    }
    return Converter() == nullptr ? box_value(res) : Converter().Convert(box_value(res), xaml_typename<hstring>(), ConverterParameter(), L"");
}
