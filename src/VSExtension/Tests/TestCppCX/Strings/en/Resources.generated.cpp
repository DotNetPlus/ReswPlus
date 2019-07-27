// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#include <pch.h>
#include "Resources.generated.h"
#include <stdio.h>

using namespace Platform;
using namespace Windows::ApplicationModel::Resources;
using namespace Windows::UI::Xaml::Interop;
namespace local = TestCppCX::Strings;

ResourceLoader^ local::Resources::_resourceLoader = nullptr;
ResourceLoader^ local::Resources::GetResourceLoader()
{
    if (_resourceLoader == nullptr)
    {
        _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
    }
    return _resourceLoader;
}


String^ local::Resources::GotMessagesFrom(unsigned int numberMessages, long long personalPronoun)
{
    size_t needed = _swprintf_p(nullptr, 0, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), ref new String(L"GotMessagesFrom_Variant") + personalPronoun, static_cast<double>(numberMessages), false)->Data(), numberMessages, personalPronoun);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), ref new String(L"GotMessagesFrom_Variant") + personalPronoun, static_cast<double>(numberMessages), false)->Data(), numberMessages, personalPronoun);
    return ref new String(buffer);
}


String^ local::Resources::SendMessage(int variantId)
{
    return GetResourceLoader()->GetString(ref new String(L"SendMessage_Variant") + variantId);
}


String^ local::Resources::YouGotEmailsDotNet(unsigned int numberMessages, String^ username)
{
    return ReswPlusLib::StringFormatting::FormatDotNet(ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmailsDotNet", static_cast<double>(numberMessages), false), ref new Array<Object^>(2){numberMessages, username});
}


String^ local::Resources::YouGotEmails(unsigned int numberMessages, String^ username)
{
    size_t needed = _swprintf_p(nullptr, 0, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", static_cast<double>(numberMessages), false)->Data(), numberMessages, username->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", static_cast<double>(numberMessages), false)->Data(), numberMessages, username->Data());
    return ref new String(buffer);
}


String^ local::Resources::Hello::get()
{
    return GetResourceLoader()->GetString(L"Hello");
}


String^ local::Resources::TestFormatWithLiteralString::get()
{
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader()->GetString(L"TestFormatWithLiteralString")->Data(), L"Hello world");
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader()->GetString(L"TestFormatWithLiteralString")->Data(), L"Hello world");
    return ref new String(buffer);
}


String^ local::Resources::TestFormatWithLocalizationRef::get()
{
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader()->GetString(L"TestFormatWithLocalizationRef")->Data(), Hello->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader()->GetString(L"TestFormatWithLocalizationRef")->Data(), Hello->Data());
    return ref new String(buffer);
}


String^ local::Resources::TestFormatWithMacro::get()
{
    return ReswPlusLib::StringFormatting::FormatDotNet(GetResourceLoader()->GetString(L"TestFormatWithMacro"), ref new Array<Object^>(3){ReswPlusLib::Macros::AppVersionFull, ReswPlusLib::Macros::LocaleName, ReswPlusLib::Macros::ShortDate});
}


String^ local::Resources::TestWithObject(Object^ obj)
{
    size_t needed = _swprintf_p(nullptr, 0, GetResourceLoader()->GetString(L"TestWithObject")->Data(), obj->ToString()->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GetResourceLoader()->GetString(L"TestWithObject")->Data(), obj->ToString()->Data());
    return ref new String(buffer);
}

local::ResourcesExtension::ResourcesExtension()
{
    _resourceLoader = ResourceLoader::GetForViewIndependentUse(L"Resources");
}

Object^ local::ResourcesExtension::ProvideValue()
{
    String^ res;
    if(Key == KeyEnum::__Undefined)
    {
        res = L"";
    }
    else
    {
        res = _resourceLoader->GetString(Key.ToString());
    }
    return Converter == nullptr ? res : Converter->Convert(res, TypeName(String::typeid), ConverterParameter, nullptr);
}
