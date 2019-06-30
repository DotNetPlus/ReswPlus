#pragma once

#include "MainPage.g.h"
#include "Strings/en/Resources.generated.h"

namespace winrt::TestCppWinRT::implementation
{
    struct MainPage : MainPageT<MainPage>
    {
        MainPage();

        int32_t MyProperty();
        void MyProperty(int32_t value);

        void ClickHandler(Windows::Foundation::IInspectable const& sender, Windows::UI::Xaml::RoutedEventArgs const& args);
        TestCppWinRT::TestStringable Str() {
            return _str;
        }

    private:
        TestCppWinRT::TestStringable _str;
    };
}

namespace winrt::TestCppWinRT::factory_implementation
{
    struct MainPage : MainPageT<MainPage, implementation::MainPage>
    {
    };
}
