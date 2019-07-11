<img src="https://user-images.githubusercontent.com/1226538/56482508-6fbd2d00-6479-11e9-8fc0-b20d5f3171ad.png" height="80" />

# ReswPlus - Advanced File Code Generator for Resw files.
[![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/rudyhuyn.ReswPlus.svg?color=green)](https://marketplace.visualstudio.com/items?itemName=rudyhuyn.ReswPlus)

ReswPlus is a Visual Studio extension enriching your existing .resw files with many high valuable features:
- Access to strings via strongly typed static properties.
- Generates methods to format your strings with named and strongly typed parameters.
- Adds pluralization support (support plural rules for 196 languages!).
- In addition to pluralization, also supports empty states when the number of items is zero.
- Generate a Markup extension to access to your strings with compile-time verification.

Currently supported: 
- Visual Studio 2017 and 2019 (all versions).
- C#, VB.Net, C++/CX apps and C++/WinRT.

![reswplus](https://user-images.githubusercontent.com/1226538/56525314-a76eb800-64ff-11e9-9e39-1bb4cd2dd012.gif)



|                                                 | Resw | Resw + ReswPlus | Resx | Android XML (for reference) |
|-------------------------------------------------|------|-----------------|------|-------------|
| Modify UI properties via resource files (x:uid) | ✅    | ✅               |      |             |
| Generate strongly typed accessors               |      | ✅               | ✅    | ✅           |
| Generate String Formatting methods              |      | ✅               |     |            |
| Support Plural forms                            |      | ✅               |      | ✅           |
| Support 'None' state                            |      | ✅               |      |             |
| Strongly typed string formatting                |      | ✅               |      |             |
| Support Resources in libraries                  |      | ✅               | ✅    |             |
| Support Genders                                 |      | ✅               |     |             |
| Support String variants                         |      | ✅               |     |             |

## How to install

ReswPlus supports Visual Studio 2017 and 2019.

In Visual Studio, select `Tools > Extensions and Updates...`, then select `Online` on the left, search `ReswPlus` and click on Install. 

Alternatively, you can directly download the extension here: https://marketplace.visualstudio.com/items?itemName=rudyhuyn.ReswPlus

## How to activate ReswPlus in my project

In your project, right-click on the resw file of the default language of your application (commonly `/Strings/en/Resources.resw`) and select the menu `ReswPlus`.

In the submenu, select:
- `Generate only accessors`: generate stronged typed generation accessors + custom markup
- `Generate advanced class`: all the above + pluralization and empty state support + variants + string formatting (the nuget package ReswPlusLib will be automatically added to your project)

<img src="https://user-images.githubusercontent.com/1226538/61084395-bc623580-a3e2-11e9-9836-eee8f0ea57c6.png" height="120" />
It will automatically generate a class associated to your Resource file:

<img src="https://user-images.githubusercontent.com/1226538/56481455-f111c100-6473-11e9-8c04-f512a6136fd2.png" height="120" />
<img src="https://user-images.githubusercontent.com/1226538/61084551-077c4880-a3e3-11e9-8e50-18ac0685d665.png" height="120" style="margin-left:10px" />

The generated code file will be automatically updated when the .resw file is modified and saved.

## Features
### Strongly typed static properties
ReswPlus generates a class exposing all strings from your .resw files as strongly typed static properties, providing a compile-time-safe way to access those strings XAML-side or code-side. 

Contrary to `ResourceLoader.GetString("IdString"),` the compiler will verify how your XAML and C# access your strings and will fail the compilation if a resource doesn't exist.

This feature will allow you to localize your applications using bindings (including native bindings) and code-behind (similar to .resx files in WPF/Silverlight applications) and will allow you to use converter, functions, etc...

The privilegied way to access strings XAML-side is using native bindings. An alternative is normal binding, but these don't provide verification at compilation time. To fix this issue, ReswPlus also generates a custom MarkupExtension (verified at compilation time), also supporting Converters.

**Code generated:**
```csharp
public class Resources {
    private static ResourceLoader _resourceLoader;
    static Resources()
    {
        _resourceLoader = ResourceLoader.GetForViewIndependentUse();
    }
    public static string WelcomeTitle => _resourceLoader.GetString("WelcomeTitle");
}
```

**How to use it:**

XAML - native binding:
```xaml
<TextBlock Text="{x:Bind strings:Resources.WelcomeTitle}" />
```
XAML - special markup (generated by Resw):
```xaml
<TextBlock Text="{strings:Resources Key=WelcomeTitle}" />
```
Code behind:
```csharp
titlebar.Title = Resources.WelcomeTitle;
```
These 3 ways are compile-time verified.

### String formatting

ReswPlus can generate strongly typed methods to format your strings. Simply add the tag `#Format[...]` in the comment column and ReswPlus will automatically generate a method YourResourceName_Format(..) with strongly typed parameters.

Types currently supported for parameters:

| identifier | C# Type | VB Type  | C++/CX Type       | C++/WinRT Type    |
|------------|---------|----------|-------------------|-------------------|
| b          | byte    | Byte     | char              | char              |
| i          | int     | Integer  | int               | int               |
| u          | uint    | UInteger | unsigned int      | unsigned int      |
| l          | long    | Long     | long              | long              |
| s          | string  | String   | Platform::String^ | hstring           |
| f          | double  | Double   | double            | double            |
| c          | char    | Char     | wchar_t           | wchar_t           |
| ul         | ulong   | ULong    | unsigned long     | unsigned long     |
| m          | decimal | Decimal  | long double       | long double       |
| o          | Object  | object   | Platform::Object^ | IStringable       |

Resw also allows you to name the parameters to make the code easy to read.

**Example:**

The resource:

| Key                  | Value                                           | Comment                                                    |
|----------------------|-------------------------------------------------|------------------------------------------------------------|
| ForecastAnnouncement | The temperature in {2} is {0}°F ({1}°C)         | #Format[d(fahrenheit), d(celsius), s(city)]         |

will generate the following code, with strong type and named parameters based on the hashtag in the comment section):

```csharp
#region ForecastAnnouncement
/// <summary>
///   Looks up a localized string similar to: The current temperature in {2} is {0}°F ({1}°C)
/// </summary>
public static string ForecastAnnouncement => _resourceLoader.GetString("ForecastAnnouncement");

/// <summary>
///   Format the string similar to: The current temperature in {2} is {0}°F ({1}°C)
/// </summary>
public static string ForecastAnnouncement_Format(int tempFahrenheit, int tempCelsius, string city)
{
	return string.Format(ForecastAnnouncement, tempFahrenheit, tempCelsius, city);
}
#endregion
```

C++ developers: You can replace the tag `#Format` by `#FormatNet` if you want to format your strings the .Net way (using `{0}`, `{1:F0}` instead of `%d`, `%s`...`. 

### Pluralization

ReswPlus can generate methods to easily access your pluralized strings. Simply right-click on your resw file, select `ReswPlus` > `Generate strongly typed class with pluralization`, the nuget package `ReswPlusLib` will be automatically added to your project and generate all the functions necessary to manage your localization.

**Example:**

The resources:

| Key               | Value            | Comment           |
|-------------------|------------------|-------------------|
| MinutesLeft_One   | {0} minute left  | #Format[Q] |
| MinutesLeft_Other | {0} minutes left |                   |

Will automatically generate the following code:

```csharp
#region MinutesLeft
/// <summary>
///   Get the pluralized version of the string similar to: {0} minute left
/// </summary>
public static string MinutesLeft(double number)
{
	return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "MinutesLeft", number);
}
/// <summary>
///   Format the string similar to: {0} minute left
/// </summary>
public static string MinutesLeft_Format(double pluralCount)
{
	return string.Format(MinutesLeft(pluralCount), pluralCount);
}
#endregion
```

ReswPlus will then automatically select one of the string based on the number passed as a parameter. While English has only 2 plural forms, some languages have up to 5 different forms, 196 different languages are supported by this library.

Pluralization can be used in combination with string formatting.

**Example:**

| Key              | Value                          | Comment                                 |
|------------------|--------------------------------|-----------------------------------------|
| FileShared_One   | {0} shared {1} photo from {2}  | #Format[s(username), Q, s(city)] |
| FileShared_Other | {0} shared {1} photos from {2} |                                         |

Will generate:

```csharp
#region FileShared
/// <summary>
///   Get the pluralized version of the string similar to: {0} shared {1} photos from {2}
/// </summary>
public static string FileShared(double number)
{
	return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "FileShared", number);
}
/// <summary>
///   Format the string similar to: {0} shared {1} photos from {2}
/// </summary>
public static string FileShared_Format(string username, double paramDouble2, string city)
{
	return string.Format(FileShared(paramDouble2), username, paramDouble2, city);
}
#endregion
```

### Empty States

In addition to plural forms, one common task normally delegated to the ViewModel is to display a special message when a quantity is zero, some examples: *'no search results', 'history empty', 'no new messages'* etc... 

ReswPlus provides a way to automate this task and automatically provide the empty state string when necessary, simply add a `_None` state to your pluralized strings:

Example:

Resources:

| Key                    | Value                | Comment                           |
|------------------------|----------------------|-----------------------------------|
| ReceivedMessages_None  | No new messages      |                                   |
| ReceivedMessages_One   | You got {0} message  | #Format[Q(numberMessages)] |
| ReceivedMessages_Other | You got {0} messages |                                   |

The following code:

```xaml
<TextBlock Text="{x:Bind strings:Resources.ReceivedMessages_Format(ViewModel.NumberMessage), Mode=OneWay}" />
```

will automatically display `No new messages`, `You got 1 message`, `You got 1 messages` based on the value of `ViewModel.NumberMessage`.

## Variants

It's sometimes necessary to include many variants of the same string in your application to support the different personal pronouns:   (for example: `Send her/his/them/zir a gift`). To support this type of scenario, generally, you must write code in your viewmodel to select the correct text corresponding to the pronoun you want to use. ReswPlus can simplify this task and fully manage the support of string variants for you, a single resource identifier would return different strings depend of your needs. 

Variants can not only be used to represent genders/personal pronouns but also for groups, categories...

Some examples:

### Support genders/personal pronouns 

**Resw:**

| Key                   | Value               |
|-----------------------|---------------------|
| SendAMessage_Variant1 | Send her a message  |
| SendAMessage_Variant2 | Send him a message  |
| SendAMessage_Variant3 | Send them a message |
| SendAMessage_Variant4 | Send zir a message  |

**Usage:**

```csharp
enum PersonalPronoun{ HER = 1, HIM, THEM, ZIR };
class Person
{
   public PersonalPronoun PronounId{get;set;}
}

Resources.SendAMessage(Person.PronounId);
```

### Personalize your strings based on the context

Display a welcome message depending of the current time:

**Resw:**

| Key                | Value                                    |
|--------------------|------------------------------------------|
| Greeting_Variant0  | Good morning, enjoy your day             |
| Greeting_Variant1  | Good afternoon, have a good time         |
| Greeting_Variant2  | Good evening, hope your day was fine     |
| Greeting_Variant4  | Good night, sleep well, see you tomorrow |

**Usage:**
```csharp
enum PartOfTheDayEnum { MORNING = 0, AFTERNOON = 1, EVENING = 2, NIGHT = 4 };

Resources.Greeting(PartOfTheDay);
```
