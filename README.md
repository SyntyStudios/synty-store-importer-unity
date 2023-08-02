# Synty Store Importer Tool

The Synty Store Importer tool is a Unity Editor extension designed to streamline the process of importing multiple asset packages from a local directory.
With a clean UI and an intuitive workflow, this tool makes it easy to import synty packs into your Unity project.

## Features

1. Visual Package Preview: Easily view packages with associated `.png` images in the directory.
2. Bulk Import: Import all or selected packages with the click of a button.
3. Link to Synty Store: Quick access to the Synty Store through an embedded header link.
4. Dynamic Resizing with Lists: Efficiently handles varying numbers of packages.
5. Improved Directory Reading: Efficiently reads and caches directory contents for a smooth user experience.

## Prerequisites

Before using the Synty Store Importer tool, you need to prepare the asset packages and associated preview images. Follow these steps:

1. Create a Local Directory: Create a local directory where you want to store the Synty Unity packages.
2. Place Unity Packages: Place all the desired Synty Unity packages (`.unitypackage` files) in the local directory.
3. Add Associated Preview Images: For each Unity package, include the associated PNG preview image available from the Synty Store. Ensure that the image files are in the same directory as the `.unitypackage` files. The Importer tool uses these images to provide visual previews of the packages.

Once you have prepared the directory with the packages and images, you can open the Synty Store Importer tool in Unity and proceed with the importing process.

## Usage

### Opening the Importer

Access the tool by clicking on Synty Tools > Synty Store Importer in the Unity Editor menu.

### Selecting a Directory

* Click the CHOOSE FOLDER button to open a directory selection dialog.
* Navigate to the directory containing your `.unitypackage` files and select it.

### Importing Packages

* Preview asset images and package names in the scrollable list.
* Select individual packages using the "Import Selected" checkboxes.
* Click IMPORT ALL to import all packages in the selected directory.
* Click IMPORT SELECTED to import only the packages with selected checkboxes.

## Technical Details

* `OpenWindow()`: Opens the Importer window.
* `OnGUI()`: Handles the graphical user interface elements and logic.
* `RefreshDirectory()`: Refreshes directory information, including asset packages and preview images.
* `DisplayImage(string filePath)`: Loads an image from a given filePath as a Texture2D object. Used for the header title Image.
* `LoadPNG(string filePath)`: Loads a PNG image from a given file path as a Texture2D object. Used for the package preview images.

### Requirements

Unity Editor 2021.3 LTS+.

### Contributing

Feel free to fork, customize, and submit pull requests if you'd like to contribute to the development of this tool.
