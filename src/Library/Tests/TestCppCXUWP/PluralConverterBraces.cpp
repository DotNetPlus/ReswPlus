#include "pch.h"
#include "PluralConverterBraces.h"
#include "StringFormat.h"

using namespace Windows::ApplicationModel::Resources;
using namespace ReswPlusLib;
using namespace Platform;
using namespace Windows::UI::Xaml::Interop;

namespace TestCppCXUWP {
    namespace Converters {
        PluralConverterBraces::PluralConverterBraces()
        {
        }

        PluralConverterBraces::~PluralConverterBraces()
        {
        }

        Object^ PluralConverterBraces::Convert(Object^ value, TypeName targetType, Object^ parameter, String^ language)
        {
            auto key = static_cast<Platform::String^>(parameter);
            if (key == nullptr || key->IsEmpty())
            {
                return L"";
            }

            auto number = safe_cast<double>(value);

            auto resource = ResourceLoader::GetForCurrentView();
            auto pluralFormat = ResourceLoaderExtension::GetPlural(ResourceLoader::GetForCurrentView(), key, number);
            return ReswPlusLib::StringFormatting::FormatDotNet(pluralFormat, ref new Array<Object^>(1) { number });
        }

        Object^ PluralConverterBraces::ConvertBack(Object^ value, TypeName targetType, Object^ parameter, String^ language)
        {
            return nullptr;
        }
    }
}
