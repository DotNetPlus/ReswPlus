#pragma once


namespace TestCppCXUWP {
    namespace Converters {
        public ref class PluralConverterBraces sealed : Windows::UI::Xaml::Data::IValueConverter
        {
        public:
            PluralConverterBraces();
            virtual ~PluralConverterBraces();
            virtual Object^ Convert(Object^ value, Windows::UI::Xaml::Interop::TypeName targetType, Object^ parameter, Platform::String^ language);
            virtual Object^ ConvertBack(Object^ value, Windows::UI::Xaml::Interop::TypeName targetType, Object^ parameter, Platform::String^ language);
        };
    }
}
