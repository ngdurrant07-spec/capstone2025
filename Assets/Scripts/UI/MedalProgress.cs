using UnityEngine;

public static class MedalProgress
{
    private const string LevelPrefix = "MedalProgress.Level.";
    private const string TotalKey = "MedalProgress.Total";
    private const string LastSceneKey = "MedalProgress.LastScene";
    private const string LastCountKey = "MedalProgress.LastCount";

    public static void SaveLevelResult(string sceneName, int medalCount)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        medalCount = Mathf.Max(0, medalCount);

        int previousBest = GetBestForLevel(sceneName);
        if (medalCount > previousBest)
        {
            PlayerPrefs.SetInt(GetLevelKey(sceneName), medalCount);
            PlayerPrefs.SetInt(TotalKey, GetSavedTotalMedals() + (medalCount - previousBest));
        }

        PlayerPrefs.SetString(LastSceneKey, sceneName);
        PlayerPrefs.SetInt(LastCountKey, medalCount);
        PlayerPrefs.Save();
    }

    public static int GetBestForLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return 0;

        return PlayerPrefs.GetInt(GetLevelKey(sceneName), 0);
    }

    public static int GetSavedTotalMedals()
    {
        return PlayerPrefs.GetInt(TotalKey, 0);
    }

    public static string GetLastCompletedSceneName()
    {
        return PlayerPrefs.GetString(LastSceneKey, string.Empty);
    }

    public static int GetLastCompletedMedalCount()
    {
        return PlayerPrefs.GetInt(LastCountKey, 0);
    }

    public static void ClearSavedProgress(params string[] sceneNames)
    {
        PlayerPrefs.DeleteKey(TotalKey);
        PlayerPrefs.DeleteKey(LastSceneKey);
        PlayerPrefs.DeleteKey(LastCountKey);

        if (sceneNames == null)
            return;

        foreach (string sceneName in sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            PlayerPrefs.DeleteKey(GetLevelKey(sceneName));
        }
    }

    private static string GetLevelKey(string sceneName)
    {
        return $"{LevelPrefix}{sceneName.Trim()}";
    }
}
