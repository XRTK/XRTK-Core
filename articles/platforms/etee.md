# etee controllers (Advance information)

The etee platform is currently in development and targeting the XRTK 0.3 release.

## Requirements

To develop for the etee controllers platform, you will need:

* A Windows PC, Windows 7 or greater.
* The XRTK.SDK package installed (first)
* The XRTK.etee package (once a Mixed Reality Scene has been setup)

## Platform considerations

The Platform considerations will be updated once the platform is released

## Capabilities

The following capabilities are in development and likely to be available for the SteamVR platform

1. etee hands controller Tracking

## Quickstart (preview, subject to change)

1. Create a new Unity Project
2. Add the XRTK UPM registry to Unity by Opening the Unity Package Manager (`Window -> Package manager`), selecting the `Advanced` drop down and clicking on `Advanced Project Settings`, then Adding the following details:
    > Name: XRTK
    >
    > URL: http://upm.xrtk.io:4873
    >
    > Scope(s): com.xrtk
3. Return to the Unity Package manager and select `My Registries` in the Sources) dropdown (next to the `+` symbol)
4. Select the `XRTK.SDK` and click `Install`
5. Install the `XRTK.etee` platform
6. When prompted, install the etee platform configuration in to your current project
7. Close the Unity Package Manager and return to your scene
8. Select or create the scene you want to create Mixed Reality Content in
9. Select `Mixed Reality Toolkit -> Configure` in the Unity Menu. This will update your scene and add the MixedRealityToolkit instance.

---

### [**Raise an Information Request**](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=)

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=).
