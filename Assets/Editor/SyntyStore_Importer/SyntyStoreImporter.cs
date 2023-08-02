using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;
public class SyntyStoreImporter : EditorWindow
{
    static FileInfo[] info;
    static FileInfo[] infoImg;
    static string folderName;
    static string path;
    Vector2 scrollPos;
    string t = "This is a string inside a Scroll view!";
    int imageHeight = 0;
    int labelHeight = 0;
    int buttonHeight = 0;
    static int checkCount;
    static List<bool> checkList = new List<bool>();
    static int howMuchIntegers;
    static bool imporTrue;
    static string headerTexture = "Assets/Editor/SyntyStore_Importer/synty_packages_header.png";
    [MenuItem("Synty Tools/Synty Store Importer")]
    private static void OpenWindow()
    {
        SyntyStoreImporter window = GetWindow<SyntyStoreImporter>();
        window.titleContent = new GUIContent("Synty Store Importer");
        window.minSize = new Vector2(650, 500);
        System.GC.Collect();
        howMuchIntegers = 0;
        checkCount = 0;
    }
    //-----------------------------------------------------------------------------------------------------------------------
    void OnInspectorUpdate()
    {
        Repaint();
    }
    //=======================================================================================================================
    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        Texture button_tex;
        GUIContent button_tex_con;
        button_tex = DisplayImage(headerTexture);
        button_tex_con = new GUIContent(button_tex);
        if (GUILayout.Button(button_tex_con, GUILayout.Width(650), GUILayout.Height(171)))
        {
            Application.OpenURL("https://syntystore.com/");
        }
        GUILayout.EndHorizontal();
        GUIStyle folderLableStyle = new GUIStyle();
        folderLableStyle.fontSize = 15;
        folderLableStyle.normal.textColor = Color.yellow;

        GUILayout.Label(folderName, folderLableStyle);
        labelHeight = 50;
        if (GUILayout.Button("CHOOSE FOLDER", GUILayout.Height(30)))
        {
            path = EditorUtility.OpenFolderPanel("Choose Import Directory", "", "");
            string[] files = Directory.GetFiles(path);
            folderName = path;
            imporTrue = true;
        }
        //----------------------------------------------------------------------------------------------------------------------------      
        if (imporTrue)
        {
            if (GUILayout.Button("IMPORT ALL", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("WARNING!", "This will import ALL packages including any existing packages in your project.\nThis may be time consuming.", "Yes import all", "Cancel"))
                {
                    DirectoryInfo dir2 = new DirectoryInfo(folderName + "/");
                    info = dir2.GetFiles("*.unitypackage*");
                    foreach (FileInfo f in info)
                    {
                        AssetDatabase.ImportPackage(folderName + "/" + f.Name.ToString(), false);
                    }
                }
            }
            //----------------------------------------------------------------------------------------------------------------------------
            if (GUILayout.Button("IMPORT SELECTED", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("WARNING!", "This will import SELECTED packages including any existing packages in your project.\nThis may be time consuming.", "Yes import selected", "Cancel"))
                {
                    howMuchIntegers = 0;
                    DirectoryInfo dir3 = new DirectoryInfo(folderName + "/");
                    FileInfo[] infoSelect = dir3.GetFiles("*.unitypackage*");
                    howMuchIntegers = infoSelect.Length;
                    for (int j = 0; j < howMuchIntegers; j++)
                    {
                        if (checkList[j])
                        {
                            AssetDatabase.ImportPackage(folderName + "/" + infoSelect[j].Name.ToString(), false);
                        }
                    }
                }
            }
        }
        //============================================================SHOW BUTTONS AND THUMBS========================================
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
        DirectoryInfo dir = new DirectoryInfo(folderName + "/");
        info = dir.GetFiles("*.unitypackage*");
        DirectoryInfo dirImg = new DirectoryInfo(folderName + "/");
        infoImg = dirImg.GetFiles("*.png");
        buttonHeight = 0;
        imageHeight = 0;
        labelHeight = 50;
        //---------------------------------------------------------------PACKAGE LABELS AND BUTTONS----------------------------------
        if (folderName != null)
        {
            for (int i = 0; i < info.Length; i++)
            {
                checkCount += 1;
                GUI.Label(new Rect(140, imageHeight - 90, 400, 200), info[i].Name, EditorStyles.boldLabel);
                checkList.Add(false);
                checkList[i] = GUI.Toggle(new Rect(140, imageHeight + 60, 120, 30), checkList[i], "Import Selected");
                if (GUI.Button(new Rect(140, (imageHeight + 30), 200, 30), "IMPORT"))
                {
                    AssetDatabase.ImportPackage(folderName + "/" + info[i].Name.ToString(), false);
                }
                //------------------------------------------------------------------------------------------------
                Rect rect = EditorGUILayout.GetControlRect(false, 1);
                rect.height = 1;
                EditorGUI.DrawRect(new Rect(0, imageHeight + 140, Screen.width, 1), new Color(0.5f, 0.5f, 0.5f, 1));
                //--------------------------------------------------------------------------------------------------
                imageHeight += 160;
                GUILayout.Space(160);
            }
            //----------------------------------------------------------------PACKAGE IMAGES----------------------------------------------

            for (int j = 0; j < infoImg.Length; j++)
            {
                //---------------------------------------LABEL---------------------------------------------------
                Texture2D inputTexture2 = LoadPNG(folderName + "/" + infoImg[j].Name.ToString());
                EditorGUI.DrawPreviewTexture(new Rect(10, buttonHeight, 120, 120), inputTexture2);
                buttonHeight += 160;
                GUILayout.Space(160);
            }

        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    //=======================================================TOGGLE CHECKLIST=========================================================
    public static Texture2D DisplayImage(string url)
    {
        Texture2D tex = new Texture2D(2, 2);
        using (WebClient client = new WebClient())
        {
            byte[] data = client.DownloadData(url);
            tex.LoadImage(data);
        }
        return tex;
    }
    //=======================================================GET .PNG THUMBNAIL=======================================================
    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        return tex;
    }
}