using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;
using UnityEditor.iOS.Xcode;

public class PostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            // Info.plistを編集
            UpdatePlist(pathToBuiltProject);

            // UnityAppController.mmを編集
            UpdateUnityAppController(pathToBuiltProject);
        }
    }

    private static void UpdatePlist(string pathToBuiltProject)
    {
        string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // UIBackgroundModesに"location"を追加
        var array = plist.root.CreateArray("UIBackgroundModes");
        array.AddString("location");

        // 権限説明の追加
        plist.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", "位置情報表示のため");
        plist.root.SetString("NSLocationWhenInUseUsageDescription", "位置情報表示のため");

        // 保存
        plist.WriteToFile(plistPath);

        Debug.Log("Info.plist updated with location permissions.");
    }

    private static void UpdateUnityAppController(string pathToBuiltProject)
    {
        string appControllerPath = Path.Combine(pathToBuiltProject, "Classes", "UnityAppController.mm");

        if (File.Exists(appControllerPath))
        {
            Debug.Log("Modifying UnityAppController.mm for background location updates...");

            string[] lines = File.ReadAllLines(appControllerPath);
            using (StreamWriter writer = new StreamWriter(appControllerPath))
            {
                bool addedLocationManager = false;

                foreach (var line in lines)
                {
                    writer.WriteLine(line);

                    // `didFinishLaunchingWithOptions`内にコードを追加
                    if (line.Contains("[super application:application didFinishLaunchingWithOptions:launchOptions];") && !addedLocationManager)
                    {
                        writer.WriteLine(@"
    // Location Manager initialization
    locationManager = [[CLLocationManager alloc] init];
    locationManager.delegate = self;

    // Request location permissions
    [locationManager requestAlwaysAuthorization];

    // Enable background location updates
    locationManager.allowsBackgroundLocationUpdates = YES;
    locationManager.pausesLocationUpdatesAutomatically = NO;
                        ");
                        addedLocationManager = true;
                    }
                }
            }

            Debug.Log("UnityAppController.mm updated successfully.");
        }
        else
        {
            Debug.LogError("UnityAppController.mm not found in the build.");
        }
    }
}
