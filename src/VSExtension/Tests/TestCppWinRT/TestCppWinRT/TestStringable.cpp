#include "pch.h"
#include "TestStringable.h"
#include "TestStringable.g.cpp"

namespace winrt::TestCppWinRT::implementation
{
    hstring TestStringable::ToString()
    {
        return L"TEXT FROM OBJECT";
    }
}
