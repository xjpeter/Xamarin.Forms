# Xamarin.Forms #

Xamarin.Forms provides a way to quickly build native apps for iOS, Android, and Windows, completely in C#.

Read more about the platform at https://www.xamarin.com/forms.

## Getting Started ##

##### Install Visual Studio 2015 #####
VS 2015 is required for developing Xamarin.Forms. If you do not already have it installed, you can download it [here](https://www.visualstudio.com/downloads/download-visual-studio-vs). VS 2015 Community is completely free. If you are installing VS 2015 for the first time, select the "Custom" installation type and select the following from the features list to install:

- C#/.NET (Xamarin v4.0.3)
- Universal Windows App Development Tools
- Windows 8.1 and Windows Phone 8.0/8.1 Tools

We also recommend installing [Microsoft Visual Studio Emulator for Android](https://www.visualstudio.com/en-us/features/msft-android-emulator-vs.aspx) as well as [Emulators for Windows Phone 8.1](https://www.microsoft.com/en-us/download/details.aspx?id=44574). If you already have VS 2015 installed, you can verify that these features are installed by modifying the VS 2015 installation via the Control Panel.

##### Install Additional Features #####
After installing VS 2015, you will also need to install the following:
  - Bing Maps SDK for Windows 8.1 Store apps -- you can find this in `Tools > Extensions and Updates` and searching for "bing" in the Online pane.
  - Android SDKs -- you can install these via `Tools > Android > Android SDK Manager`.

##### Solution Configuration #####
Upon opening the Xamarin.Forms solution, you will find that there are a number of errors and warnings under the Error List pane; you can resolve this by changing the filter of `Build + IntelliSense` to `Build Only`. At this point, you should be able to successfully build the solution.

By default, the `Xamarin.Forms.Controls` project does not have a configuration for various API keys to access certain features on each platform (e.g. maps). When building the solution the first time, a `controlgallery.config` file will be generated inside that project, which looks like this:

    Win8MapsAuthKey:
    WinPhoneMapsAuthKey:
    UWPMapsAuthKey:
    InsightsApiKey:
    WP8AppId:
    WP8AuthToken:

You will have to obtain your own API keys for each of these services, inserted directly after the identifier (e.g. `Win8MapsAuthKey:abcdefghijklmnopqrstuvwxyz`). You can find out how to obtain each of these as follows:

- `Win8MapsAuthKey`, `WinPhoneMapsAuthKey`, and `UWPMapsAuthKey` at https://www.microsoft.com/maps/create-a-bing-maps-key.aspx
- `InsightsApiKey` at https://insights.xamarin.com/
- `WP8AppId` and `WP8AuthToken` at https://dev.windows.com/.

Due to the way that Android works, the maps API key cannot be injected at runtime. As a result, you will have to add this key to the `MapsKey.cs` file under `Xamarin.Forms.ControlGallery.Android/Properties`:

    [assembly: Android.App.MetaData("com.google.android.maps.v2.API_KEY", Value = "INSERT_KEY_HERE")]

You can find out how to obtain a Google Maps API key [here](https://developer.xamarin.com/guides/android/platform_features/maps_and_location/maps/obtaining_a_google_maps_api_key/).

## Coding Style ##

We follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We use hard tabs over spaces. You can change this setting in VS 2015 via `Tools > Options` and navigating to `Text Editor > C#` and selecting the "Keep tabs" radio option.

## Contributing ##

- [How to Contribute](https://github.com/xamarin/xamarin.forms/wiki/Contributing-Code)

### Mailing Lists

To discuss this project, and participate in the design, we use the [forms-devel@lists.xamarin.com](http://lists.xamarin.com/mailman/listinfo/forms-devel) mailing list.   

### Reporting Bugs

We use [Bugzilla](https://bugzilla.xamarin.com/newbug) to track issues.


