//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"
#include <stdio.h>
#include <wchar.h>
#include <string>
#include "StringFormat.h"

using namespace TestCppCXUWP;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::ApplicationModel::Resources;
using namespace ReswPlusLib;
using namespace std;

MainPage::MainPage()
{
	InitializeComponent();
	CodeBehindSlider->Value = 4.3;
}

void TestCppCXUWP::MainPage::Slider_ValueChanged(Platform::Object^ sender, Windows::UI::Xaml::Controls::Primitives::RangeBaseValueChangedEventArgs^ e)
{
	if (DistanceCodeBehindTextBlock == nullptr)
		return;
	auto pluralFormat = ResourceLoaderExtension::GetPlural(ResourceLoader::GetForCurrentView(), "RunDistance", e->NewValue);
	wchar_t * str;
	if (vasprintf(&str, pluralFormat->Data(), e->NewValue) >= 0)
	{
		DistanceCodeBehindTextBlock->Text = ref new Platform::String(str);
		delete[] str;
	}
}
