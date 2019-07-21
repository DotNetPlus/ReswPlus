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

        /* Methods and properties for GotMessagesFrom */
        public:
            /// <summary>
            ///   Get the pluralized version of the string similar to: You got %d message from her
            /// </summary>
            static Platform::String^ GotMessagesFrom(long long variantId, double pluralNumber);
        public:
            /// <summary>
            ///   Format the string similar to: You got %d message from her
            /// </summary>
            static Platform::String^ GotMessagesFrom_Format(unsigned int numberMessages, long long personalPronoun);

        /* Methods and properties for SendMessage */

        /* Methods and properties for YouGotEmailsDotNet */
        public:
            /// <summary>
            ///   Get the pluralized version of the string similar to: Hello {1}, you got {0:F0} email (DotNet formatting)
            /// </summary>
            static Platform::String^ YouGotEmailsDotNet(double pluralNumber);
        public:
            /// <summary>
            ///   Format the string similar to: Hello {1}, you got {0:F0} email (DotNet formatting)
            /// </summary>
            static Platform::String^ YouGotEmailsDotNet_Format(unsigned int numberMessages, Platform::String^ username);

        /* Methods and properties for YouGotEmails */
        public:
            /// <summary>
            ///   Get the pluralized version of the string similar to: Hello %2$ls, you got %1$u email
            /// </summary>
            static Platform::String^ YouGotEmails(double pluralNumber);
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

        /* Methods and properties for TestFormatWithLiteralString */
        public:
            /// <summary>
            ///   Looks up a localized string similar to: This '%ls' is a literal string
            /// </summary>
            static property Platform::String^ TestFormatWithLiteralString
            {
                Platform::String^ get();
            }
        public:
            /// <summary>
            ///   Format the string similar to: This '%ls' is a literal string
            /// </summary>
            static Platform::String^ TestFormatWithLiteralString_Format();

        /* Methods and properties for TestWithObject */
        public:
            /// <summary>
            ///   Looks up a localized string similar to: Test with object %s
            /// </summary>
            static property Platform::String^ TestWithObject
            {
                Platform::String^ get();
            }
        public:
            /// <summary>
            ///   Format the string similar to: Test with object %s
            /// </summary>
            static Platform::String^ TestWithObject_Format(Platform::Object^ obj);
        };

        public enum class KeyEnum
        {
            __Undefined = 0,
            Hello,
            TestFormatWithLiteralString,
            TestWithObject,
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
