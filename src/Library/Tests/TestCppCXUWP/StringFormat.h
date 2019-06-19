#pragma once
#include <stdio.h>
#include <stdarg.h>
namespace TestCppCXUWP
{
	int vasprintf_internal(wchar_t **str, const wchar_t *fmt, va_list ap);
	int vasprintf(wchar_t **str, const wchar_t *format, ...);
}
