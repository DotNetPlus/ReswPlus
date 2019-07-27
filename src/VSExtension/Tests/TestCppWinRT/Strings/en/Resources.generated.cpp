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


hstring local::Resources::YouGotEmails(unsigned int numberMessages, hstring const& username)
{
    size_t needed = _swprintf_p(nullptr, 0, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", static_cast<double>(numberMessages), false).c_str(), numberMessages, username.c_str());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", static_cast<double>(numberMessages), false).c_str(), numberMessages, username.c_str());
    return hstring(buffer);
}


hstring local::Resources::GotMessagesFrom(unsigned int numberMessages, long personalPronoun)
{
    size_t needed = _swprintf_p(nullptr, 0, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), hstring(L"GotMessagesFrom_Variant") + to_wstring(personalPronoun), static_cast<double>(numberMessages), false).c_str(), numberMessages, personalPronoun);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), hstring(L"GotMessagesFrom_Variant") + to_wstring(personalPronoun), static_cast<double>(numberMessages), false).c_str(), numberMessages, personalPronoun);
    return hstring(buffer);
}


hstring local::Resources::SendMessage(int variantId)
{
    return GetResourceLoader().GetString(hstring(L"SendMessage_Variant") + to_wstring(variantId));
}


hstring local::Resources::YouGotEmailsDotNet(unsigned int numberMessages, hstring const& username)
{
    array<IInspectable const, 2> _string_parameters = {box_value(numberMessages), box_value(username)};
    return ReswPlusLib::StringFormatting::FormatDotNet(ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmailsDotNet", static_cast<double>(numberMessages), false), _string_parameters);
}


hstring local::Resources::Hello()
{
    return GetResourceLoader().GetString(L"Hello");
}


hstring local::Resources::TestWithObject(IStringable const& obj)
{
    auto _obj_string = obj == nullptr ? L"" : obj.ToString().c_str();
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader().GetString(L"TestWithObject").c_str(), _obj_string);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader().GetString(L"TestWithObject").c_str(), _obj_string);
    return hstring(buffer);
}


hstring local::Resources::TestFormatWithLiteralString()
{
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader().GetString(L"TestFormatWithLiteralString").c_str(), L"Hello world");
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader().GetString(L"TestFormatWithLiteralString").c_str(), L"Hello world");
    return hstring(buffer);
}


hstring local::Resources::TestFormatWithLocalizationRef()
{
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader().GetString(L"TestFormatWithLocalizationRef").c_str(), Hello().c_str());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader().GetString(L"TestFormatWithLocalizationRef").c_str(), Hello().c_str());
    return hstring(buffer);
}


hstring local::Resources::TestFormatWithMacro()
{
    array<IInspectable const, 3> _string_parameters = {box_value(ReswPlusLib::Macros::AppVersionFull()), box_value(ReswPlusLib::Macros::LocaleName()), box_value(ReswPlusLib::Macros::ShortDate())};
    return ReswPlusLib::StringFormatting::FormatDotNet(GetResourceLoader().GetString(L"TestFormatWithMacro"), _string_parameters);
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
        case KeyEnum::YouGotEmails:
            return hstring(L"YouGotEmails");
        case KeyEnum::GotMessagesFrom:
            return hstring(L"GotMessagesFrom");
        case KeyEnum::SendMessage:
            return hstring(L"SendMessage");
        case KeyEnum::YouGotEmailsDotNet:
            return hstring(L"YouGotEmailsDotNet");
        case KeyEnum::Hello:
            return hstring(L"Hello");
        case KeyEnum::TestWithObject:
            return hstring(L"TestWithObject");
        case KeyEnum::TestFormatWithLiteralString:
            return hstring(L"TestFormatWithLiteralString");
        case KeyEnum::TestFormatWithLocalizationRef:
            return hstring(L"TestFormatWithLocalizationRef");
        case KeyEnum::TestFormatWithMacro:
            return hstring(L"TestFormatWithMacro");
        default:
            return hstring(L"");
    }
}
