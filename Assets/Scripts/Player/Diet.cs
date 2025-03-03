static class Diet
{
    public interface IFood
    {
        public int Protein { get; }
        public int Carbs { get; }
        public int Nutrients { get; }
    }

    public static void EatFood(PlayerData playerData, IFood food)
    {
        playerData.TodaysProtein = playerData.TodaysProtein + food.Protein > PlayerData.PROTEIN_REQUIRED_DAILY ? PlayerData.PROTEIN_REQUIRED_DAILY : playerData.TodaysProtein + food.Protein;
        playerData.TodaysCarbs = playerData.TodaysCarbs + food.Carbs > PlayerData.CARBS_REQUIRED_DAILY ? PlayerData.CARBS_REQUIRED_DAILY : playerData.TodaysCarbs + food.Carbs;
        playerData.TodaysNutrients = playerData.TodaysNutrients + food.Nutrients > PlayerData.NURTRIENTS_REQUIRD_DAILY ? PlayerData.NURTRIENTS_REQUIRD_DAILY : playerData.TodaysNutrients + food.Nutrients;
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
            playerData.TodaysNutrients / PlayerData.NURTRIENTS_REQUIRD_DAILY
        ) / 3;
    }
}