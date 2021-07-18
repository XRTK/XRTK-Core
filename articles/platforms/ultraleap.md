# UltraLeap Hands tracking

The UltraLeap hand tracking support.

## Requirements

To develop using UltraLeap hand tracking, you will need:

* A Windows PC, Windows 7 or greater.
* The XRTK.SDK package installed (first)
* The XRTK.UltraLeap package (once a Mixed Reality Scene has been setup)
* A UltraLeap device or simulator

## Capabilities

1. Hand Tracking

## Quickstart

1. Create a new Unity Project
2. Add the XRTK UPM registry to Unity by Opening the Unity Package Manager (`Window -> Package manager`), selecting the `Advanced` drop down and clicking on `Advanced Project Settings`, then Adding the following details:
    > Name: XRTK
    >
    > URL: http://upm.xrtk.io:4873
    >
    > Scope(s): com.xrtk
3. Return to the Unity Package manager and select `My Registries` in the Sources) dropdown (next to the `+` symbol)
4. Select the `XRTK.SDK` and click `Install`
5. Install the `XRTK.UltraLeap` platform
6. When prompted, install the SteamVR platform configuration in to your current project
7. Close the Unity Package Manager and return to your scene
8. Select or create the scene you want to create Mixed Reality Content in
9. Select `Mixed Reality Toolkit -> Configure` in the Unity Menu. This will update your scene and add the MixedRealityToolkit instance.

---

### [**Raise an Information Request**](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=)

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=).
