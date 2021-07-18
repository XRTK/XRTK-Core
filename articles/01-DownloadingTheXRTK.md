# How to download the Mixed Reality Toolkit

The Mixed Reality Toolkit provides many ways for users and developers to get access to the Mixed Reality Toolkit and the various extensions/platforms it supports.  These methods are tuned to use the common patterns most familiar to Unity developers, these include:

* [Unity Package Manager](#register-upm-server-in-project-settings) - Use the Unity Package Manager interface to register and install the XRTK.
* [Automatic UPM installation](#automatic-upm-installation) - Automatically register the Mixed Reality Toolkit's upm server and install selected packages using the package seed found in the [release assets dropdown](https://github.com/XRTK/XRTK-Core/releases).
* [Manual UPM installation](#manual-upm-installation) - Register the Mixed Reality Toolkit upm server in your manifest and manually download modules al la carte.
* [UPM via GitHub](https://docs.unity3d.com/Manual/upm-ui-giturl.html) - The Unity Package Manager can load a package from GitHub using the git url.
  > **Note:** To utilize this method, use the `upm` branch for each respective project. For Example:
  >
  > `https://github.com/XRTK/XRTK-Core.git#upm`
* [GIT Submodules](#git-submodules) - For advanced developers who want to use the Mixed Reality Toolkit in their live project and contribute directly back to the project in real time.

In this article, we will walk through each approach to get you up and running, starting with the simplest first.

## Register UPM Server in Project Settings

Unity has now enabled the ability to register scoped registries through the Unity Editor and is now the recommended way to add custom solutions to your project, giving you visibility of the assets you have registered in your solution and where they come from.

To start, you will need to register the XRTK Scoped registry in the Unity Package Manager, which you will find in the Package Manager Window (Window -> Package Manager) Advanced dropdown, as shown below:

![Unity Package Manager Advanced options](../images/DownloadingTheXRTK/UnityPackageManager-Advanced.jpg)

Once opened, you will see the Scoped Registry configuration (which is also available via the Project Settings window), from here, you can add the XRTK scoped registry using the details below:

![Unity Package Manager Scoped Registries](../images/DownloadingTheXRTK/UnityPackageManager-ScopedRegistries.jpg)

```text
Name: XRTK
URL: http://upm.xrtk.io:4873
Scope(s): com.xrtk
```

Once entered, simply click "Save" and close the window. Upon returning to the Unity Package Manager, you can now select "My Registries" in the package filter on the left hand side drop down, as shown here:

![Unity Package Manager My Registries](../images/DownloadingTheXRTK/UnityPackageManager-MyRegistries.jpg)

Which will list all the current XRTK packages available for install.  If you also wish to check out the latest "Preview packages", then simply check the "Show Preview Packages" option in the "Advanced" dropdown to display them, as shown here:

![Unity Package Manager My Registries](../images/DownloadingTheXRTK/UnityPackageManager-XRTKRegistries.jpg)

Once you are ready, simply install the XRTK and any platforms you wish to use in your project to get started.

> We recommend starting with the **XRTK.SDK** package to begin with, which will also install the Core XRTK package, for the simplest path to begin with.

Once you've installed all the packages you wish to utilize, you can continue to [Configuring your base scene](00-GettingStarted.md#configure-your-base-scene).

## Automatic UPM Installation

Our preferred deployment approach is to fully utilize [Unity's native package manager](https://docs.unity3d.com/Manual/upm-ui.html) to incorporate the Mixed Reality Toolkit in your solution, akin to the other modules Unity provides out of the box.  This is the quickest and safest way to get Mixed Reality Toolkit in your solution and is automatically updated as new releases are published.

Download the [XRTK-Core.unitypackage](https://github.com/XRTK/XRTK-Core/releases) asset, then drag and drop it into your project window.  This adds an Mixed Reality Toolkit seed, that will automatically register the Mixed Reality Toolkit's scoped registry with the Unity package manager, then starts searching for the latest release packages on the upm server.

![Seed Package](../images/DownloadingTheXRTK/XRTKAutoInstallAsset.png)

### Install platform packages

The Mixed Reality Toolkit will pull all of the known packages under `com.xrtk.*` and will dispaly them for you to easily install using the package picker window. The latest release pacakges are installed for you by default.

![Package List](../images/DownloadingTheXRTK/XRTKPackageList.png)

## Manual UPM Installation

If you prefer to do things yourself, you can simply edit the ***manifest.json*** file in your Unity project and add an entry to register the Mixed Reality Toolkit and begin the download and installation into your project.

To locate the folder the manifest is in, simply right-click in your Unity project folder on the "Packages" folder as shown below, then select the "Show in Explorer" option:

![Context Menu](../images/DownloadingTheXRTK/LocatePackagesFolder.png)

In the packages folder, you will see the specific Manifest.json file which you will need to open for editing using your favorite tool ([We prefer VSCode as it's awesome](https://code.visualstudio.com/))

Inside the Manifest file, you will see all the packages Unity provides "out of the box" listed:

To this you simply need to add the extra entry for the Mixed Reality Toolkit as follows:

```json
{
  "scopedRegistries": [
    {
      "name": "XRTK",
      "url": "http://upm.xrtk.io:4873/",
      "scopes": [
        "com.xrtk"
      ]
    }
  ],
  "dependencies": {
    "com.xrtk.core": "XX.XX.XX",
    "com.xrtk.sdk": "XX.XX.XX",
    ...
  },
}
```

Once it's updated and you return to Unity, the packages will be refreshed and all the defined Mixed Reality Toolkit packages will be downloaded and imported.

## GIT Submodules

Advanced developers and those wishing to contribute to the Mixed Reality Toolkit also have the option to directly import and edit the framework in their live projects using GIT Submodules.

> **Note:** The XRTK-Core repository is setup to utilize submodules and is a great example of how to utilize this workflow.

### Prerequisites

* Each contributor to the project will need to have [git installed](https://git-scm.com/) on their machines with the proper environment variable set so the symbolic linker can find and execute git commands via the terminal.
* You'll need to install the core package via one of the [traditional installation](#automatic-upm-installation) paths before you can create symbolic links in your project using the editor context menu in later setup steps.

### Setup Steps

1. Initialize your project's repository with the XRTK-Core submodule.
    > <font color="red">**Warning!**</font> Don't recursively checkout submodules! Each package will need to be added as a submodule and replaced using the same steps listed below.
2. Checkout the `development` branch of the XRTK-Core submodule and make sure it's up to date and synced with any upstream changes.
    > <font color="red">**Warning!**</font> We strongly recommend you use do not use the **upm** branch to avoid breaking the symbolic links when switching between branches.
3. Create Symbolic links for each package you wish to directly modify in your project by using the project window context menu: `Symbolic Links/Create Link`
  ![Symbolic Link Context Menu](../images/DownloadingTheXRTK/SymbolicLinks.png)
4. Using the symbolic link wizard, choose the source folder for the package you wish you replace with the editable version, and the target path for your project, then import the package.
  ![Symbolic Link Wizard](../images/DownloadingTheXRTK/SymbolicLinkWizard.png)
    > <font color="orange">**Important:**</font> It's strongly advised to use the `Packages` folder as your target path for all symbolically linked packages.
5. Repeat step 4 for each additional package you'd like to directly modify.

This workflow works with any project using GIT and the symbolically linked folders can be customized to utilize any path available under source control.

---

### Related Articles

* [Getting Started](00-GettingStarted.md#getting-started-with-the-mixed-reality-toolkit)
  * [Configuring your base scene](00-GettingStarted.md#configure-your-base-scene)

---

### [**Raise an Information Request**](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=)

If there is anything not mentioned in this document or you simply want to know more, raise an [RFI (Request for Information) request here](https://github.com/XRTK/XRTK-Core/issues/new?assignees=&labels=question&template=request_for_information.md&title=).
