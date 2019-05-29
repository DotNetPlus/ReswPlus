#pragma once


namespace TestCppCXUWP {
	namespace Converters {
		public ref class PluralConverter sealed : Windows::UI::Xaml::Data::IValueConverter
		{
		public:
			PluralConverter();
			virtual ~PluralConverter();
			virtual Object^ Convert(Object^ value, Windows::UI::Xaml::Interop::TypeName targetType, Object^ parameter, Platform::String^ language);
			virtual Object^ ConvertBack(Object^ value, Windows::UI::Xaml::Interop::TypeName targetType, Object^ parameter, Platform::String^ language);
		};
	}
}