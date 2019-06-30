#pragma once
#include "TestStringable.g.h"

namespace winrt::TestCppWinRT::implementation
{
    struct TestStringable : TestStringableT<TestStringable>
    {
        TestStringable() = default;

        hstring ToString();
    };
}

namespace winrt::TestCppWinRT::factory_implementation
{
    struct TestStringable : TestStringableT<TestStringable, implementation::TestStringable>
    {
    };
}
