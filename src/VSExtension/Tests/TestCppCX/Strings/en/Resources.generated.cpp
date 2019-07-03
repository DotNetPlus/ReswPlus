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

String^ local::Resources::GotMessagesFrom(double pluralNumber, int variantId)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), ref new String(L"GotMessagesFrom_Variant") + variantId, pluralNumber);
}

String^ local::Resources::GotMessagesFrom_Format(unsigned int numberMessages, int personalPronoun)
{
    size_t needed = _swprintf_p(nullptr, 0, GotMessagesFrom(static_cast<double>(numberMessages), personalPronoun)->Data(), numberMessages, personalPronoun);
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, GotMessagesFrom(static_cast<double>(numberMessages), personalPronoun)->Data(), numberMessages, personalPronoun);
    return ref new String(buffer);
}


String^ local::Resources::YouGotEmails(double pluralNumber)
{
    return ReswPlusLib::ResourceLoaderExtension::GetPlural(GetResourceLoader(), L"YouGotEmails", pluralNumber);
}

String^ local::Resources::YouGotEmails_Format(unsigned int numberMessages, String^ username)
{
    size_t needed = _swprintf_p(nullptr, 0, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, YouGotEmails(static_cast<double>(numberMessages))->Data(), numberMessages, username->Data());
    return ref new String(buffer);
}

String^ local::Resources::Hello::get()
{
    return GetResourceLoader()->GetString(L"Hello");
}

String^ local::Resources::TestWithObject::get()
{
    return GetResourceLoader()->GetString(L"TestWithObject");
}

String^ local::Resources::TestWithObject_Format(Object^ obj)
{
    size_t needed = _swprintf_p(nullptr, 0, TestWithObject->Data(), obj->ToString()->Data());
    wchar_t *buffer = new wchar_t[needed + 1];
    _swprintf_p(buffer, needed + 1, TestWithObject->Data(), obj->ToString()->Data());
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
