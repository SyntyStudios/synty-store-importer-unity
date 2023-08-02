// ************************************************************************
// Copyright (c) Synty Studios, All Rights Reserved.
// ************************************************************************

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// This class represents a Synty Store Importer tool that allows the importing of asset packages from a local directory.
/// </summary>
namespace SyntyStudios.SyntyStore
{
    public class SyntyStoreImporter : EditorWindow
    {
        // Fields and properties
        private bool _importTrue;
        private string _filterText = "";
        private string _folderName;
        private string _path;
        private DirectoryInfo _packageDir;
        private List<bool> _checkList = new List<bool>();
        private List<FileInfo> _info = new List<FileInfo>();
        private List<Texture2D> _infoImg = new List<Texture2D>();
        private Texture2D _placeholderImage;
        private Vector2 _scrollPos;

        private static readonly string _HEADER_TEXTURE = "Assets/Plugins/SyntyStudios/Editor/SyntyStoreImporter/synty_packages_header.png";
        /// <value>A temporary cache for loaded textures, referenced via file location.</value>
        private static Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Opens the Synty Store Importer window.
        /// </summary>
        [MenuItem("Synty Tools/Synty Store Importer")]
        public static void OpenWindow()
        {
            SyntyStoreImporter window = GetWindow<SyntyStoreImporter>();
            window.titleContent = new GUIContent("Synty Store Importer");
            window.minSize = new Vector2(650, 500);

            window._folderName = EditorPrefs.GetString("SyntyStoreImporter_FolderName", "");
            if (!string.IsNullOrEmpty(window._folderName))
            {
                window._path = window._folderName;
                window._importTrue = true;
                window.RefreshDirectory();
            }
        }

        /// <inheritdoc />
        public void OnInspectorUpdate()
        {
            Repaint();
        }

        /// <inheritdoc />
        public void OnGUI()
        {
            RenderHeaderSection();
            RenderFolderSelection();
            RenderImportButtons();
            RenderSearchFilter();
            RenderPackagesList();
        }

        /// <inheritdoc />
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
        }

        /// <inheritdoc />
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
        }

        /// <summary>
        /// Refresh the package and image lists when user exits play mode.
        /// Also reload the image textures from the cache.
        /// </summary>
        private void HandlePlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                RefreshDirectory();
            }
        }

        /// <summary>
        /// Renders the header section of the Synty Store Importer, including the clickable logo that leads to the Synty Store website.
        /// </summary>
        private void RenderHeaderSection()
        {
            GUILayout.BeginHorizontal();
            Texture button_tex = DisplayImage(_HEADER_TEXTURE);
            GUIContent button_tex_con = new GUIContent(button_tex);
            Rect buttonRect = GUILayoutUtility.GetRect(button_tex_con, GUIStyle.none, GUILayout.Width(650), GUILayout.Height(171));
            if (buttonRect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
            }
            if (GUI.Button(buttonRect, button_tex_con, GUIStyle.none))
            {
                Application.OpenURL("https://syntystore.com/");
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders the folder selection GUI, allowing the user to choose the directory for Unity package import.
        /// </summary>
        private void RenderFolderSelection()
        {
            GUIStyle folderLabelStyle = new GUIStyle { fontSize = 15 };
            folderLabelStyle.normal.textColor = Color.yellow;
            GUILayout.Label(_folderName, folderLabelStyle);

            if (GUILayout.Button("CHOOSE FOLDER", GUILayout.Height(30)))
            {
                _path = EditorUtility.OpenFolderPanel("Choose Import Directory", "", "");
                _folderName = _path;
                _importTrue = true;
                EditorPrefs.SetString("SyntyStoreImporter_FolderName", _folderName);
                RefreshDirectory();
            }
        }

        /// <summary>
        /// Renders the import buttons for all and selected Unity package import actions.
        /// </summary>
        private void RenderImportButtons()
        {
            if (_importTrue)
            {
                if (GUILayout.Button("IMPORT ALL", GUILayout.Height(30)))
                {
                    ImportAll();
                }
                if (GUILayout.Button("IMPORT SELECTED", GUILayout.Height(30)))
                {
                    ImportSelected();
                }
            }
        }

        /// <summary>
        /// Imports all Unity packages located in the specified folder and shows a warning dialog.
        /// </summary>
        private void ImportAll()
        {
            bool dialogResponse = EditorUtility.DisplayDialog(
                "WARNING!",
                "This will import ALL packages including any existing packages in your project.\nThis may be time consuming.",
                "Yes import all",
                "Cancel"
            );
            if (!dialogResponse)
            {
                return;
            }

            FileInfo[] infoAll = _packageDir.GetFiles("*.unitypackage*");
            foreach (FileInfo f in infoAll)
            {
                AssetDatabase.ImportPackage(Path.Combine(_folderName, f.Name), false);
            }
            RefreshDirectory();
            RenderPackagesList();
        }

        /// <summary>
        /// Imports selected Unity packages located in the specified folder.
        /// </summary>
        private void ImportSelected()
        {
            // Check if any packages are selected
            if (!_checkList.Any(selected => selected))
            {
                EditorUtility.DisplayDialog(
                    "No Packages Selected",
                    "Please select at least one package to import.",
                    "OK"
                );
                return;
            }

            bool dialogResponse = EditorUtility.DisplayDialog(
                "WARNING!",
                "This will import SELECTED packages including any existing packages in your project.\nThis may be time consuming.",
                "Yes import selected",
                "Cancel"
            );
            if (!dialogResponse)
            {
                return;
            }

            FileInfo[] infoSelect = _packageDir.GetFiles("*.unitypackage*");
            for (int i = 0; i < _checkList.Count; i++)
            {
                if (_checkList[i])
                {
                    if (i < infoSelect.Length)
                    {
                        AssetDatabase.ImportPackage(Path.Combine(_folderName, infoSelect[i].Name), false);
                    }
                    else
                    {
                        // Handle the error case where the file is missing
                        Debug.LogError("File for selected package is missing: " + i);
                    }
                }
            }
            RefreshDirectory();
            RenderPackagesList();
        }

        /// <summary>
        /// Renders the search filter text field in the editor window.
        /// </summary>
        private void RenderSearchFilter()
        {
            if (!string.IsNullOrEmpty(_folderName) && _info != null && _info.Count > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                _filterText = EditorGUILayout.TextField("Filter:", _filterText, GUILayout.Width(400));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }
        }

        /// <summary>
        /// Gets the indices of the packages that should be rendered based on the current filter text and folder name.
        /// </summary>
        /// <returns>
        /// An enumerable containing the indices of the packages that match the filter criteria.
        /// </returns>
        private IEnumerable<int> GetPackagesToRender()
        {
            if (_folderName == null)
            {
                return Enumerable.Empty<int>();
            }

            List<int> packages = new List<int>();
            int loopLength = Mathf.Min(_info.Count, _checkList.Count, _infoImg.Count);
            for (int i = 0; i < loopLength; i++)
            {
                if (_info[i].Name.ToLower().Contains(_filterText.ToLower()))
                {
                    packages.Add(i);
                }
            }
            return packages;
        }

        /// <summary>
        /// Renders the entire list of packages within a scrollable view in the editor window.
        /// </summary>
        private void RenderPackagesList()
        {
            int startPosition = 0;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));

            IEnumerable<int> packagesToRender = GetPackagesToRender();
            foreach (int index in packagesToRender)
            {
                RenderPackageItem(index, ref startPosition);
            }

            GUILayout.Space(150);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders a single package item in the list of packages within the editor window.
        /// </summary>
        private void RenderPackageItem(int index, ref int startPosition)
        {
            GUI.Label(new Rect(140, startPosition - 90, 400, 200), _info[index].Name, EditorStyles.boldLabel);
            _checkList[index] = GUI.Toggle(new Rect(140, startPosition + 60, 120, 30), _checkList[index], "Import Selected");

            if (GUI.Button(new Rect(140, startPosition + 30, 200, 30), "IMPORT"))
            {
                AssetDatabase.ImportPackage(Path.Combine(_folderName, _info[index].Name), false);
            }

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(new Rect(0, startPosition + 140, Screen.width, 1), new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUI.DrawPreviewTexture(new Rect(10, startPosition, 120, 120), _infoImg[index]);

            startPosition += 160;
            GUILayout.Space(180);
        }

        /// <summary>
        /// Refreshes the directory by re-loading Unity package files and corresponding images.
        /// </summary>
        private void RefreshDirectory()
        {
            if (string.IsNullOrEmpty(_folderName))
            {
                return;
            }
            _packageDir = new DirectoryInfo(_folderName);
            FileInfo[] unityPackageFiles = _packageDir.GetFiles("*.unitypackage*");
            _info = new List<FileInfo>(unityPackageFiles);
            _checkList = Enumerable.Repeat(false, unityPackageFiles.Length).ToList();

            FileInfo[] imageFiles = _packageDir.GetFiles("*.png");
            _textureCache.Clear();
            _infoImg.Clear();

            _placeholderImage = LoadPNG("Assets/Plugins/SyntyStudios/Editor/SyntyStoreImporter/synty_logo_white.png");

            foreach (FileInfo packageFile in unityPackageFiles)
            {
                Texture2D matchingImage = FindMatchingImage(packageFile, imageFiles);
                _infoImg.Add(matchingImage);
            }
        }

        /// <summary>
        /// Finds the matching image for a given Unity package file.
        /// </summary>
        /// <param name="packageFile">The Unity package file for which the matching image is to be found.</param>
        /// <param name="imageFiles">An array of possible image files that might match the package file.</param>
        /// <returns>The matching image as a Texture2D object. If no matching image is found a placeholder texture.</returns>
        private Texture2D FindMatchingImage(FileInfo packageFile, FileInfo[] imageFiles)
        {
            string packageName = packageFile.Name;
            string commonPrefix;

            int unityIndex = packageName.IndexOf("_Unity_");
            if (unityIndex > -1)
            {
                commonPrefix = packageName.Substring(0, unityIndex).ToUpper();
            }
            else
            {
                commonPrefix = packageName.Substring(0, packageName.LastIndexOf('.')).Replace("_Unity_Package", "").ToUpper();
                commonPrefix = commonPrefix.Split('_')[0] + (commonPrefix.Split('_').Length > 1 ? "_" + commonPrefix.Split('_')[1] : "");
            }

            FileInfo matchingImageFile = imageFiles.FirstOrDefault(imageFile => imageFile.Name.ToUpper().StartsWith(commonPrefix + "_ICON"));

            if (matchingImageFile == null)
            {
                return _placeholderImage;
            }
            else
            {
                return LoadPNG(Path.Combine(_folderName, matchingImageFile.Name));
            }
        }

        /// <summary>
        /// Loads an image from a given file path and returns it as a Texture2D object.
        /// </summary>
        /// <param name="filePath">The path of the image file on the local system.</param>
        /// <returns>A Texture2D object representing the image.</returns>
        private static Texture2D DisplayImage(string filePath)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(filePath));
            return texture;
        }

        /// <summary>
        /// Loads a PNG image from a given file path.
        /// </summary>
        /// <param name="filePath">The path to the PNG file.</param>
        /// <returns>A Texture2D object representing the image if found, null otherwise.</returns>
        private static Texture2D LoadPNG(string filePath)
        {
            // Check if the texture has already been loaded and cached
            if (_textureCache.TryGetValue(filePath, out Texture2D cachedTexture))
            {
                return cachedTexture;
            }

            // If not, load the texture from the file
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);

                // Cache the loaded texture for future use
                _textureCache[filePath] = texture;

                return texture;
            }
            else
            {
                Debug.LogWarning($"File not found at path: {filePath}");
                return null;
            }
        }
    }
}
