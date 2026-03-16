using UnityEngine;

public static class TimeTrialProgress
{
    private const string BestPrefix = "TimeTrialProgress.Best.";
    private const string LastSceneKey = "TimeTrialProgress.LastScene";
    private const string LastTimeKey = "TimeTrialProgress.LastTime";

    public static void SaveTime(string sceneName, float elapsedSeconds)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        float clampedTime = Mathf.Max(0f, elapsedSeconds);
        float previousBest = GetBestTime(sceneName);
        if (!HasRecordedTime(sceneName) || clampedTime < previousBest)
            PlayerPrefs.SetFloat(GetBestKey(sceneName), clampedTime);

        PlayerPrefs.SetString(LastSceneKey, sceneName);
        PlayerPrefs.SetFloat(LastTimeKey, clampedTime);
        PlayerPrefs.Save();
    }

    public static bool HasRecordedTime(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        return PlayerPrefs.HasKey(GetBestKey(sceneName));
    }

    public static float GetBestTime(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return 0f;

        return Mathf.Max(0f, PlayerPrefs.GetFloat(GetBestKey(sceneName), 0f));
    }

    public static string GetLastCompletedSceneName()
    {
        return PlayerPrefs.GetString(LastSceneKey, string.Empty);
    }

    public static float GetLastCompletedTime()
    {
        return Mathf.Max(0f, PlayerPrefs.GetFloat(LastTimeKey, 0f));
    }

    public static void ClearSavedProgress(params string[] sceneNames)
    {
        PlayerPrefs.DeleteKey(LastSceneKey);
        PlayerPrefs.DeleteKey(LastTimeKey);

        if (sceneNames == null)
            return;

        foreach (string sceneName in sceneNames)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            PlayerPrefs.DeleteKey(GetBestKey(sceneName));
        }
    }

    public static string FormatTime(float elapsedSeconds)
    {
        float clampedTime = Mathf.Max(0f, elapsedSeconds);
        int minutes = Mathf.FloorToInt(clampedTime / 60f);
        float seconds = clampedTime % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }

    private static string GetBestKey(string sceneName)
    {
        return $"{BestPrefix}{sceneName.Trim()}";
    }
}
