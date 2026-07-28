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

    public static void EatFood(PlayerEnergyManager energyManager, IFood food)
    {
        energyManager.TodaysProtein = energyManager.TodaysProtein + food.Protein > PlayerEnergyManager.PROTEIN_REQUIRED_DAILY ? PlayerEnergyManager.PROTEIN_REQUIRED_DAILY : energyManager.TodaysProtein + food.Protein;
        energyManager.TodaysCarbs = energyManager.TodaysCarbs + food.Carbs > PlayerEnergyManager.CARBS_REQUIRED_DAILY ? PlayerEnergyManager.CARBS_REQUIRED_DAILY : energyManager.TodaysCarbs + food.Carbs;
        energyManager.TodaysNutrients = energyManager.TodaysNutrients + food.Nutrients > PlayerEnergyManager.NUTRIENTS_REQUIRED_DAILY ? PlayerEnergyManager.NUTRIENTS_REQUIRED_DAILY : energyManager.TodaysNutrients + food.Nutrients;
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

    public static void ResetDailyIntake(PlayerEnergyManager energyManager)
    {
        energyManager.TodaysProtein = 0;
        energyManager.TodaysCarbs = 0;
        energyManager.TodaysNutrients = 0;
    }

    public static float GetRecoveryRatio(PlayerEnergyManager energyManager)
    {
        return
        (
            energyManager.TodaysProtein / PlayerEnergyManager.PROTEIN_REQUIRED_DAILY +
            energyManager.TodaysCarbs / PlayerEnergyManager.CARBS_REQUIRED_DAILY +
            energyManager.TodaysNutrients / PlayerEnergyManager.NUTRIENTS_REQUIRED_DAILY
        ) / 3;
    }

    public static string GetRecoveryMessage(PlayerEnergyManager energyManager)
    {
        float _recoveryRatio = GetRecoveryRatio(energyManager);
        Debug.Log($"Food recovery value: {_recoveryRatio}");
        for (int i = _recoveryMessages.Count - 1; i >= 0; i--)
            if (_recoveryRatio >= _recoveryMessages[i].Item1)
                return _recoveryMessages[i].Item2;

        Debug.LogError("Unreachable code reached.");
        return "";
    }
}
