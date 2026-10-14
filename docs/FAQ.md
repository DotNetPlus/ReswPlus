## What's the minimum version of Windows 10 supported for applications using Resw? 

Because ReswPlus's markup extension (deprecated — see below) uses [MarkupExtension](https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.markup.markupextension), an app that still uses it needs Windows 10 Fall Creators Update (1709). An app that uses only the generated class and `x:Bind` has no such requirement.

## Can I use the markup extension in an app compiled with Native AOT?

No, and it is deprecated for that reason. A markup extension is created by the XAML parser while it reads a page, and a UWP app compiled with Native AOT cannot create it, so a page that uses one fails to load with `Markup extension could not provide value`. Preserving the generated types with `rd.xml` or a trimmer root does not change it.

It is still generated, so an app that does not use Native AOT keeps building, but it is marked `[Obsolete]` and the build tells you what to write instead. A page that uses it reports `WMC1500` on the line of the markup itself, and the XAML compiler's own generated type table reports `CS0618` alongside it. A build with `TreatWarningsAsErrors` will therefore fail until the markup is rewritten or the warning is suppressed.

Use `x:Bind`, which reads the same generated members, is resolved while the app is compiled, and works whichever way the app is built:

| Instead of | Write |
| --- | --- |
| `{strings:Resources Key=Foo}` | `{x:Bind strings:Resources.Foo}` |
| `{strings:Resources Key=Foo, Converter={StaticResource C}}` | `{x:Bind strings:Resources.Foo, Converter={StaticResource C}}` |

`x:Bind` is not available everywhere the markup extension was. A `Setter` in a `Style`, and a standalone `ResourceDictionary` with no code-behind — a shared `Styles.xaml`, say — are the two common cases: `x:Bind` needs a compiled-binding host, which those do not have. Set those from code-behind using the same generated members:

```csharp
myTextBlock.Text = Resources.Foo;
```

Everything else ReswPlus generates works with Native AOT.

## Does it support VB or C++?

VB support is currently under development. C++/CX and C++/WinRT support are planned for a future update.

## Does the source generator support Uno WinUI?

Yes. ReswPlus detects `Microsoft.UI.Xaml.DependencyObject` from the `Uno.UI` assembly or the `Uno.WinUI` assembly used by Uno 7.0 and later, regardless of the NuGet package path or whether the type is a class or interface. It generates `Microsoft.UI.Xaml` helpers and uses `Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse`.

Generated macro helpers are still subject to Uno's Windows API support. Uno may report `Uno0001` warnings for unsupported APIs in those helpers, even when the resource uses a different macro.

If you previously worked around `RESWP0005` by setting `UseUwp=true` and adding `Windows.UI.Xaml` versions of `MarkupExtension`, `MarkupExtensionReturnTypeAttribute`, and `IValueConverter`, remove that override and those compatibility types after updating. A separate `WinRT.DynamicWindowsRuntimeCastAttribute` polyfill, if required by your app, is unrelated to ReswPlus and should be kept.

## Is it free?

Yes and it won't change.

## What's the license of this product?

This code is under MIT License, you can find the license here: https://github.com/rudyhuyn/ReswPlus/blob/master/LICENSE

## How can I ask ReswPlus to ignore a resource item?

Add the hashtag **#ReswPlusIgnore** in the comment field to ask ReswPlus to ignore the current item.

## I have an idea/suggestion, where can I share it?

[Open a ticket](https://github.com/rudyhuyn/ReswPlus/issues/new), we will study your idea or suggestion and will include it in a next update if approved!

## Can I contribute?

Of course! First [open a ticket](https://github.com/rudyhuyn/ReswPlus/issues/new) and describe your suggestion or the feature you would like to work on, once approved, send a Pull request with your change.
