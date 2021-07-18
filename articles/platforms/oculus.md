# Oculus

The Oculus platform provides a wide range of capabilities able to run under the Oculus client for windows standalone and on Android with the Oculus Quest.

## Requirements

To develop for the Oculus platform, you will need:

* A Windows PC, Windows 7 or greater or a Mac with 10.1 or greater (Quest/Android only)
* The Oculus client, installed and running
* An Oculus Device, Rift or Quest 1/2
* The XRTK.SDK package installed (first)
* The XRTK.Oculus package (once a Mixed Reality Scene has been setup)

## Platform considerations

* Building for Oculus Quest is rather unique, if you have your Oculus Quest connected to your machine via a high speed USB3 cable and have the Oculus Client running, you can run Quest project in Windows Standalone mode directly on the Quest via Oculus Link, [please see Oculus Support for further details](https://support.oculus.com/444256562873335/).  This will enable both controller and hands support for quick development.
* For oculus Rift, you simply need to ensure that `Oculus` is one of the configured platforms in the Unity legacy XR settings for the `Standalone` platform
* For Oculus Quest (on device), you will need the Android SDK/NDK installed with Unity and the `Oculus` platform configured in the Unity legacy XR settings for the `Android` platform.  You also need to include an **`AndroidManifest.xml`** included as a Plugin in your Projects Asset folder (the XRTK provides one via the `MixedRealityToolkit` menu)
* For Oculus Quest (via Oculus Link), you will need the `Oculus` platform configured in the Unity legacy XR settings for the Standalone platform

## Capabilities

The following capabilities are currently available for the Oculus platform

1. Headset Tracking
2. Oculus Touch controllers
3. Oculus Hands implementation (Editor with Oculus link and On Device only)
4. Oculus Native API, available via the `XRTK.Oculus.Plugins` namespace

The following capabilities are not currently available or are in development

1. Oculus Boundary (System boundary enabled by default)

## Quickstart

1. Create a new Unity Project
2. Setup the Legacy XR settings in "Edit -> Project Settings -> Player -> XR Settings" for `Oculus` to the Windows Standalone and Android platforms
3. Add the XRTK UPM registry to Unity by Opening the Unity Package Manager (`Window -> Package manager`), selecting the `Advanced` drop down and clicking on `Advanced Project Settings`, then Adding the following details:
    > Name: XRTK
    >
    > URL: http://upm.xrtk.io:4873
    >
    > Scope(s): com.xrtk
4. Return to the Unity Package manager and select `My Registries` in the Sources) dropdown (next to the `+` symbol)
5. Select the `XRTK.SDK` and click `Install`
6. Install the `XRTK.Oculus` platform
7. When prompted, install the Oculus platform configuration in to your current project
8. Close the Unity Package Manager and return to your scene
9. Select or create the scene you want to create Mixed Reality Content in
10. Select `Mixed Reality Toolkit -> Configure` in the Unity Menu. THis will update your scene and add the MixedRealityToolkit instance.
11. Install an Android Manifest in to your project using `Mixed Reality Toolkit -> Tools -> Oculus -> Create Oculus Quest compatible AndroidManifest.xml` in the Editor menu
12. Enable the `Mock HMD` XR Loader in the XR SDK Manager

For Oculus Rift / Quest

* Ensure the Oculus client is running before playing
* If using Oculus Quest via Oculus Link, ensure your cable is connected and you have started Oculus Link on the device. (We recommend disabling the Oculus home in the Oculus Client settings for rapid development, and saving memory on your dev machine)
* If using Oculus Quest for android, ensure your cable is connected and your device is setup for `Developer mode` from the Oculus mobile application used to configure the device.  ALso ensure Oculus Link is NOT running when building and deploying for Android

---

### [**Raise an Information Request**](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=)

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=).
