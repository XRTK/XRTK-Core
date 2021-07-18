# Magic Leap

The Magic Leap platform provides a wide range of capabilities able to run on Magic leap devices.

## Requirements

To develop for the Windows Mixed Reality platform, you will need:

* A Windows 10 PC
* Visual Studio 2019 community edition or later
* The Magic Leap Lab
  * Magic Leap SDK 0.24.1
* The XRTK.SDK package (installed first!)
* The XRTK.Lumin package

It is advantageous to also have a Magic Leap headset, however the Magic Leap Developer Lab also provides simulation capabilities for testing.

## Platform considerations

* Please check the Magic Leap Developer Lab guidance for correctly setting up your machine for Unity development
* Add `mlsdk` to your system environment variables with the path to your magic leap folder install
* Add the magic leap sdk path to your systems `path` environment variables

|Variable|Value|
|---|---|
|`mlsdk`|`C:\users\<>\MagicLeap\mlsdk`|
|`path`|`C:\Users\<>\MagicLeap\mlsdk\v0.24.1`|

## Capabilities

The following capabilities are currently available for the Windows Mixed Reality platform

1. Headset Tracking
2. Magic Leap controller
3. Spatial Awareness

The following capabilities are not currently available or are in development

1. Magic Leap Hands implementation

## Quickstart

1. Create a new Unity Project
2. Switch to the `Lumin` platform in `Build Settings`
3. Setup the Unity XR settings in "Edit -> Project Settings -> XR Plugin Management" for `Lumin`
4. Add the XRTK UPM registry to Unity by Opening the Unity Package Manager (`Window -> Package manager`), selecting the `Advanced` drop down and clicking on `Advanced Project Settings`, then Adding the following details:
    > Name: XRTK
    >
    > URL: http://upm.xrtk.io:4873
    >
    > Scope(s): com.xrtk
5. Return to the Unity Package manager and select `My Registries` in the Sources) dropdown (next to the `+` symbol)
6. Select the `XRTK.SDK` and click `Install`
7. Install the `XRTK.Lumin` platform
8. When prompted, install the Lumin platform configuration in to your current project
9. Close the Unity Package Manager and return to your scene
10. Select or create the scene you want to create Mixed Reality Content in
11. Select `Mixed Reality Toolkit -> Configure` in the Unity Menu. THis will update your scene and add the MixedRealityToolkit instance.
12. Enable the Magic Leap XR Loader in the XR SDK Manager

---

### [**Raise an Information Request**](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=)

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=).
