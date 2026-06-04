using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public int score;
    public string date;
    public string mode; // "Levels" or "Infinite"
}

[System.Serializable]
public class ScoreListWrapper
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}

public static class ScoreboardManager
{
    private const string PrefsKey = "ScoreboardData";

    public static List<ScoreEntry> GetScores()
    {
        string json = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new List<ScoreEntry>();
        }

        try
        {
            ScoreListWrapper wrapper = JsonUtility.FromJson<ScoreListWrapper>(json);
            return wrapper.scores ?? new List<ScoreEntry>();
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing scoreboard JSON: " + e.Message);
            return new List<ScoreEntry>();
        }
    }

    public static void SaveScore(string playerName, int score, string mode)
    {
        List<ScoreEntry> currentScores = GetScores();

        ScoreEntry newEntry = new ScoreEntry
        {
            playerName = string.IsNullOrEmpty(playerName) ? "PLAYER" : playerName.ToUpper(),
            score = score,
            date = DateTime.Now.ToString("dd/MM/yyyy"),
            mode = mode
        };

        currentScores.Add(newEntry);

        // Sort descending by score
        currentScores.Sort((a, b) => b.score.CompareTo(a.score));

        // Filter and keep only top 10 per mode
        List<ScoreEntry> levelsScores = currentScores.FindAll(s => s.mode == "Levels");
        List<ScoreEntry> infiniteScores = currentScores.FindAll(s => s.mode == "Infinite");

        if (levelsScores.Count > 10) levelsScores.RemoveRange(10, levelsScores.Count - 10);
        if (infiniteScores.Count > 10) infiniteScores.RemoveRange(10, infiniteScores.Count - 10);

        List<ScoreEntry> finalScores = new List<ScoreEntry>();
        finalScores.AddRange(levelsScores);
        finalScores.AddRange(infiniteScores);

        ScoreListWrapper wrapper = new ScoreListWrapper { scores = finalScores };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
    }

    public static bool IsHighScore(int score, string mode)
    {
        if (score <= 0) return false;

        List<ScoreEntry> currentScores = GetScores();
        List<ScoreEntry> modeScores = currentScores.FindAll(s => s.mode == mode);

        if (modeScores.Count < 10)
        {
            return true;
        }

        modeScores.Sort((a, b) => b.score.CompareTo(a.score));
        return score > modeScores[modeScores.Count - 1].score;
    }
}
