## ReswPlus 0.4

### New Features

- Support of **C++/WinRT**
- Support of string variants, including genders
- .Net String formatting support ("Hello {0}") added to C++/CX and C++/WinRT

### Changes
- C++/CX code generator creates now .h+.cpp
- Menu items renamed

## ReswPlus 0.3

### New Features

- Support of **C++/CX**
- Support .Net Standard (1.5+) and .Net Core projects (2.0+)
- Right click menu on resw files will now display a **submenu ReswPlus** allowing you to generate strongly typed class without using CustomTool
  - In addition to make ReswPlus easier to use, it allows us to support C++ projects (Visual Studio doesn't support Custom Tools with C++ projects).
  - <img src="https://user-images.githubusercontent.com/1226538/59745769-57278400-922a-11e9-8395-f87f8faeb4bd.png" height="120" />
- PluralNet is now part of ReswPlus, a new nuget package named `ReswPlusLib` replaces it. (this new package will in the future manages more than pluralization).
- ReswPlus **automatically adds** the nuget package to your project if you use pluralizations.
- The type of the parameter used as a quantifier for pluralization can be customized
  - previously: only double was supported
  - new: You can explicitly select the type: 
    - `Q` or `Qd`: double 
    - `Qi`: int
    - `Qu`: uint
    - `Ql`: long
    - `Qul`: unsigned long  
    - `Qb`: byte  
    
### Breaking change
- PluralNet nuget package has been replaced by `ReswPlusLib`
  - The change was necessary for many reasons:
     - PluralNet used the type `Decimal`, specific to .Net apps and not supported by C++ projects.
     - The library will support more than pluralization in the future.
- The Custom Tool `ReswPlusPluralizationGenerator` has been renamed `ReswPlusAdvancedGenerator` to support more than pluralization in an upcoming update.
