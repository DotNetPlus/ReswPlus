// File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
// The NuGet package ReswPlusLib is necessary to support Pluralization.
#pragma once

namespace TestCppCX
{
    namespace Strings
    {
        public ref class Resources sealed
        {
        private:
            static Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;
            static Windows::ApplicationModel::Resources::ResourceLoader^ GetResourceLoader();
        public:
            Resources() {}

        /* Methods and properties for YouGotEmails */
        public:
            /// <summary>
            ///   Get the pluralized version of the string similar to: Hello %2$ls, you got %1$u email
            /// </summary>
            static Platform::String^ YouGotEmails(double number);
        public:
            /// <summary>
            ///   Format the string similar to: Hello %2$ls, you got %1$u email
            /// </summary>
            static Platform::String^ YouGotEmails_Format(unsigned int numberMessages, Platform::String^ username);

        /* Methods and properties for Hello */
        public:
            /// <summary>
            ///   Looks up a localized string similar to: Hello world
            /// </summary>
            static property Platform::String^ Hello
            {
                Platform::String^ get();
            }
        };

        public enum class KeyEnum
        {
            __Undefined = 0,
            Hello,
        };

        public ref class ResourcesExtension sealed: public Windows::UI::Xaml::Markup::MarkupExtension
        {
        private:
            Windows::ApplicationModel::Resources::ResourceLoader^ _resourceLoader;
        public:
            ResourcesExtension();
            property KeyEnum Key;
            property Windows::UI::Xaml::Data::IValueConverter^ Converter;
            property Platform::Object^ ConverterParameter;
        protected:
            virtual Platform::Object^ ProvideValue() override;
    };
} // namespace Strings
} // namespace TestCppCX
