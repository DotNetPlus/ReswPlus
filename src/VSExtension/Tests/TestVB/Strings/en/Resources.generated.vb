' File generated automatically by ReswPlus. https://github.com/rudyhuyn/ReswPlus
' The NuGet package ReswPlusLib is necessary to support Pluralization.
Imports System
Imports Windows.ApplicationModel.Resources
Imports Windows.UI.Xaml.Markup
Imports Windows.UI.Xaml.Data

<System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")>
<System.Diagnostics.DebuggerNonUserCodeAttribute()>
<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>
Public Class Resources
    Private Shared _resourceLoader as ResourceLoader

    Shared Sub New()
        _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources")
    End Sub
    #Region "FileShared"
    ' <summary>
    '   Get the pluralized version of the string similar to: {0} shared {1} photo from {2}
    ' </summary>
    Public Shared Function FileShared(number As Double) As String
        If number = 0 Then
            Return _resourceLoader.GetString("FileShared_None")
        End If
        Return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "FileShared", CDec(number))
    End Function

    ' <summary>
    '   Format the string similar to: {0} shared {1} photo from {2}
    ' </summary>
    Public Shared Function FileShared_Format(ByVal username As String, ByVal pluralCount As Double, ByVal city As String) As String
        Return String.Format(FileShared(pluralCount), username, pluralCount, city)
    End Function
    #End Region

    #Region "MinutesLeft"
    ' <summary>
    '   Get the pluralized version of the string similar to: {0} minute left
    ' </summary>
    Public Shared Function MinutesLeft(number As Double) As String
        If number = 0 Then
            Return _resourceLoader.GetString("MinutesLeft_None")
        End If
        Return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "MinutesLeft", CDec(number))
    End Function

    ' <summary>
    '   Format the string similar to: {0} minute left
    ' </summary>
    Public Shared Function MinutesLeft_Format(ByVal pluralCount As Double) As String
        Return String.Format(MinutesLeft(pluralCount), pluralCount)
    End Function
    #End Region

    #Region "PluralizationTest"
    ' <summary>
    '   Get the pluralized version of the string similar to: This is the singular form
    ' </summary>
    Public Shared Function PluralizationTest(number As Double) As String
        If number = 0 Then
            Return _resourceLoader.GetString("PluralizationTest_None")
        End If
        Return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "PluralizationTest", CDec(number))
    End Function
    #End Region

    #Region "ReceivedMessages"
    ' <summary>
    '   Get the pluralized version of the string similar to: No new messages from {1}
    ' </summary>
    Public Shared Function ReceivedMessages(number As Double) As String
        Return ReswPlusLib.ResourceLoaderExtension.GetPlural(_resourceLoader, "ReceivedMessages", CDec(number))
    End Function

    ' <summary>
    '   Format the string similar to: No new messages from {1}
    ' </summary>
    Public Shared Function ReceivedMessages_Format(ByVal pluralCount As Double, ByVal paramString2 As String) As String
        Return String.Format(ReceivedMessages(pluralCount), pluralCount, paramString2)
    End Function
    #End Region

    #Region "ForecastAnnouncement"
    ' <summary>
    '   Looks up a localized string similar to: The current temperature in {2} is {0}°F ({1}°C)
    ' </summary>
    Public Shared ReadOnly Property ForecastAnnouncement As String
        Get
            Return _resourceLoader.GetString("ForecastAnnouncement")
        End Get
    End Property

    ' <summary>
    '   Format the string similar to: The current temperature in {2} is {0}°F ({1}°C)
    ' </summary>
    Public Shared Function ForecastAnnouncement_Format(ByVal tempFahrenheit As Integer, ByVal tempCelsius As Integer, ByVal city As String) As String
        Return String.Format(ForecastAnnouncement, tempFahrenheit, tempCelsius, city)
    End Function
    #End Region

    #Region "GotMessages"
    ' <summary>
    '   Looks up a localized string similar to: Welcome {0}, you got {1} emails!
    ' </summary>
    Public Shared ReadOnly Property GotMessages As String
        Get
            Return _resourceLoader.GetString("GotMessages")
        End Get
    End Property

    ' <summary>
    '   Format the string similar to: Welcome {0}, you got {1} emails!
    ' </summary>
    Public Shared Function GotMessages_Format(ByVal paramString1 As String, ByVal paramUint2 As UInteger) As String
        Return String.Format(GotMessages, paramString1, paramUint2)
    End Function
    #End Region

    #Region "ThisIsATooltip"
    ' <summary>
    '   Looks up a localized string similar to: this is a tooltip text
    ' </summary>
    Public Shared ReadOnly Property ThisIsATooltip As String
        Get
            Return _resourceLoader.GetString("ThisIsATooltip")
        End Get
    End Property
    #End Region

    #Region "WelcomeTitle"
    ' <summary>
    '   Looks up a localized string similar to: Hello World!
    ' </summary>
    Public Shared ReadOnly Property WelcomeTitle As String
        Get
            Return _resourceLoader.GetString("WelcomeTitle")
        End Get
    End Property
    #End Region

    #Region "YourAgeAndName"
    ' <summary>
    '   Looks up a localized string similar to: Your are {0}yo and named {1}!
    ' </summary>
    Public Shared ReadOnly Property YourAgeAndName As String
        Get
            Return _resourceLoader.GetString("YourAgeAndName")
        End Get
    End Property

    ' <summary>
    '   Format the string similar to: Your are {0}yo and named {1}!
    ' </summary>
    Public Shared Function YourAgeAndName_Format(ByVal paramDouble1 As Double, ByVal paramString2 As String) As String
        Return String.Format(YourAgeAndName, paramDouble1, paramString2)
    End Function
    #End Region
End Class

<System.CodeDom.Compiler.GeneratedCodeAttribute("Huyn.ReswPlus", "0.1.0.0")>
<System.Diagnostics.DebuggerNonUserCodeAttribute()>
<System.Runtime.CompilerServices.CompilerGeneratedAttribute()>
<MarkupExtensionReturnType(ReturnType:=GetType(String))>
Public Class ResourcesExtension
    Inherits MarkupExtension
    Public Enum KeyEnum
        __Undefined = 0
        ForecastAnnouncement
        GotMessages
        ThisIsATooltip
        WelcomeTitle
        YourAgeAndName
    End Enum

    Private Shared _resourceLoader as ResourceLoader
    Shared Sub New()
        _resourceLoader = ResourceLoader.GetForViewIndependentUse("Resources")
    End Sub

    Public Property Key As KeyEnum
    Public Property Converter As IValueConverter
    Public Property ConverterParameter As Object
    Protected Overrides Function ProvideValue() As Object
        Dim res As String
        If Key = KeyEnum.__Undefined Then
            res = ""
        Else
            res = _resourceLoader.GetString(Key.ToString())
        End If
        Return If(Converter Is Nothing, res, Converter.Convert(res, GetType(String), ConverterParameter, Nothing))
    End Function
End Class
