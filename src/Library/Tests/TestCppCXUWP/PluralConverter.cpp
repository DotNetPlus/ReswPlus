#include "pch.h"
#include "PluralConverter.h"
#include "StringFormat.h"

using namespace Windows::ApplicationModel::Resources;
using namespace Huyn::ReswPlusLib;
using namespace Platform;
using namespace Windows::UI::Xaml::Interop;

namespace TestCppCXUWP {
	namespace Converters {
		PluralConverter::PluralConverter()
		{
		}

		PluralConverter::~PluralConverter()
		{
		}

		Object^ PluralConverter::Convert(Object^ value, TypeName targetType, Object^ parameter, String^ language)
		{
			auto key = static_cast<Platform::String^>(parameter);
			if (key == nullptr || key->IsEmpty())
			{
				return L"";
			}

			auto number = safe_cast<double>(value);

			auto resource = ResourceLoader::GetForCurrentView();
			auto pluralFormat = ResourceLoaderExtension::GetPlural(ResourceLoader::GetForCurrentView(), "RunDistance", number);
			wchar_t * str;
			if (vasprintf(&str, pluralFormat->Data(), number) >= 0)
			{
				auto res = ref new Platform::String(str);
				delete[] str;
				return res;
			}
			return L"";
		}

		Object^ PluralConverter::ConvertBack(Object^ value, TypeName targetType, Object^ parameter, String^ language)
		{
			return nullptr;
		}
	}
}