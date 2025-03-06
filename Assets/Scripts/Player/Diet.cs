using System.Collections.Generic;
using UnityEngine;

static class Diet
{
    public interface IFood
    {
        public int Protein { get; }
        public int Carbs { get; }
        public int Nutrients { get; }
    }

    private static List<(float, string)> _recoveryMessages = new List<(float, string)>
    {
        (0f, "your stomach is empty. hunger gnaws."),
        (0.1f, "the scraps you found offer little relief."),
        (0.33f, "it wasn't much to eat. it keeps you going."),
        (0.66f, "your hunger fades. your strength returns."),
        (1f, "you ate well. you feel whole."),
    };

    public static void EatFood(PlayerData playerData, IFood food)
    {
        playerData.TodaysProtein = playerData.TodaysProtein + food.Protein > PlayerData.PROTEIN_REQUIRED_DAILY ? PlayerData.PROTEIN_REQUIRED_DAILY : playerData.TodaysProtein + food.Protein;
        playerData.TodaysCarbs = playerData.TodaysCarbs + food.Carbs > PlayerData.CARBS_REQUIRED_DAILY ? PlayerData.CARBS_REQUIRED_DAILY : playerData.TodaysCarbs + food.Carbs;
        playerData.TodaysNutrients = playerData.TodaysNutrients + food.Nutrients > PlayerData.NUTRIENTS_REQUIRED_DAILY ? PlayerData.NUTRIENTS_REQUIRED_DAILY : playerData.TodaysNutrients + food.Nutrients;
        PrintFoodMessage(food);
    }

    private static void PrintFoodMessage(IFood food)
    {
        if (Narrator.Instance == null) return;

        string _protein = food.Protein == 0 ? "" : $"+{food.Protein}P ";
        string _carbs = food.Carbs == 0 ? "" : $"+{food.Carbs}C";
        string _nutrients = food.Nutrients == 0 ? "" : $"+{food.Nutrients}N";
        Narrator.Instance.PostMessage(_nutrients + _carbs + _protein);
    }

    public static void ResetDailyIntake(PlayerData playerData)
    {
        playerData.TodaysProtein = 0;
        playerData.TodaysCarbs = 0;
        playerData.TodaysNutrients = 0;
    }

    public static float GetRecoveryRatio(PlayerData playerData)
    {
        return
        (
            playerData.TodaysProtein / PlayerData.PROTEIN_REQUIRED_DAILY +
            playerData.TodaysCarbs / PlayerData.CARBS_REQUIRED_DAILY +
            playerData.TodaysNutrients / PlayerData.NUTRIENTS_REQUIRED_DAILY
        ) / 3;
    }

    public static string GetRecoveryMessage(PlayerData playerData)
    {
        float _recoveryRatio = GetRecoveryRatio(playerData);
        Debug.Log($"Food recovery value: {_recoveryRatio}");
        for (int i = _recoveryMessages.Count - 1; i >= 0; i--)
            if (_recoveryRatio >= _recoveryMessages[i].Item1)
                return _recoveryMessages[i].Item2;

        Debug.LogError("Unreachable code reached.");
        return "";
    }
}