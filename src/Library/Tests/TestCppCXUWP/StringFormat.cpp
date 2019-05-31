#include "pch.h"
#include "StringFormat.h"
namespace TestCppCXUWP
{
	int vasprintf_internal(wchar_t **str, const wchar_t *format, va_list ap)
	{
		int len = _vscwprintf(format, ap);
		if (len == -1) {
			return -1;
		}
		auto size = (size_t)len + 1;
		auto buffer = new wchar_t[size];
		if (!buffer) {
			return -1;
		}

		int r = vswprintf_s(buffer, len + 1, format, ap);
		if (r == -1) {
			delete[] str;
			return -1;
		}
		*str = buffer;
		return r;
	}
	int vasprintf(wchar_t **str, const wchar_t *format, ...)
	{
		va_list args;
		va_start(args, format);
		auto res = vasprintf_internal(str, format, args);
		va_end(args);
		return res;
	}
}
