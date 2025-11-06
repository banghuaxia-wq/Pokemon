using UnityEngine;

/// <summary>
/// 战斗初始化数据存储器
/// 用于在场景切换时传递战斗初始化信息
/// </summary>
public static class BattleInitializer
{
    /// <summary>
    /// 玩家宝可梦数据
    /// </summary>
    public static PlayerPokemonData PlayerData { get; set; }
    
    /// <summary>
    /// 敌方宝可梦数据
    /// </summary>
    public static PokemonData EnemyData { get; set; }
    
    /// <summary>
    /// 是否是多人战斗（Boss战）
    /// </summary>
    public static bool IsMultiBattle { get; set; }
    
    /// <summary>
    /// 目标战斗场景名称
    /// </summary>
    public static string BattleSceneName { get; set; }
    
    /// <summary>
    /// 设置单人战斗数据
    /// </summary>
    public static void SetupSingleBattle(PlayerPokemonData playerData, PokemonData enemyData)
    {
        PlayerData = playerData;
        EnemyData = enemyData;
        IsMultiBattle = false;
        BattleSceneName = "SingleBattleScene";
        
        Debug.Log($"[BattleInitializer] 单人战斗初始化 - 玩家: {playerData?.pokemonData?.displayName}, 敌方: {enemyData?.displayName}");
    }
    
    /// <summary>
    /// 设置多人战斗数据（Boss战）
    /// </summary>
    public static void SetupMultiBattle(PlayerPokemonData playerData, PokemonData bossData)
    {
        PlayerData = playerData;
        EnemyData = bossData;
        IsMultiBattle = true;
        BattleSceneName = "MultiBattleScene";
        
        Debug.Log($"[BattleInitializer] 多人战斗初始化 - 玩家: {playerData?.pokemonData?.displayName}, Boss: {bossData?.displayName}");
    }
    
    /// <summary>
    /// 清除战斗数据
    /// </summary>
    public static void Clear()
    {
        PlayerData = null;
        EnemyData = null;
        IsMultiBattle = false;
        BattleSceneName = null;
    }
}

