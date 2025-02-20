using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class XcodePostProcess
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Xcodeプロジェクトを編集するための準備
            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // UnityFrameworkターゲットのGUIDを取得
            string unityFrameworkTarget = proj.GetUnityFrameworkTargetGuid();
            
            // ヘッダーファイルへのパスを追加
            proj.AddBuildProperty(unityFrameworkTarget, "HEADER_SEARCH_PATHS", "$(SRCROOT)/Libraries/Mapbox/Core/Plugins/iOS/MapboxMobileEvents/Include");

            // 変更を保存
            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
