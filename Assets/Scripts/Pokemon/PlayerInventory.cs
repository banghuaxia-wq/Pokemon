using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家背包管理器 - 管理玩家的宝可梦队伍
/// </summary>
public class PlayerInventory : MonoBehaviour
{
	[Header("宝可梦队伍")]
	[Tooltip("玩家的宝可梦队伍（最多6只）")]
	public List<PlayerPokemonData> pokemonParty = new List<PlayerPokemonData>();
	
	[Tooltip("宝可梦仓库")]
	public List<PlayerPokemonData> pokemonStorage = new List<PlayerPokemonData>();
	
	[Header("初始化设置")]
	[Tooltip("初始默认宝可梦（游戏开始时自动添加）")]
	public PokemonData initialPokemon;
	
	private static PlayerInventory instance;
	public static PlayerInventory Instance => instance;
	
	private void Awake()
	{
		// 单例模式（由父物体 PermanentManager 统一管理持久化）
		if (instance == null)
		{
			instance = this;
			
			// 初始化默认宝可梦（只在第一次创建时执行）
			InitializeDefaultPokemon();
			
			Debug.Log("[PlayerInventory] 单例已初始化");
		}
		else if (instance != this)
		{
			Debug.LogWarning("[PlayerInventory] 检测到重复实例，销毁当前物体");
			Destroy(gameObject);
		}
	}
	
	/// <summary>
	/// 初始化默认宝可梦（游戏开始时）
	/// </summary>
	private void InitializeDefaultPokemon()
	{
		// 如果背包为空且有初始宝可梦数据，添加一只初始宝可梦
		if (pokemonParty.Count == 0 && initialPokemon != null)
		{
			PlayerPokemonData startPokemon = new PlayerPokemonData(initialPokemon);
			startPokemon.nickname = ""; // 使用种族名
			AddToParty(startPokemon);
			Debug.Log($"[PlayerInventory] 已添加初始宝可梦: {initialPokemon.displayName}");
		}
	}
	
	/// <summary>
	/// 添加宝可梦到队伍
	/// </summary>
	public bool AddToParty(PlayerPokemonData pokemon)
	{
		if (pokemonParty.Count >= 6)
		{
			Debug.LogWarning("队伍已满！");
			return false;
		}
		
		pokemonParty.Add(pokemon);
		pokemon.isInParty = true;
		return true;
	}
	
	/// <summary>
	/// 获取第一只存活的宝可梦
	/// </summary>
	public PlayerPokemonData GetFirstAlivePokemon()
	{
		foreach (var pokemon in pokemonParty)
		{
			if (pokemon.currentHP > 0)
			{
				return pokemon;
			}
		}
		return null;
	}
	
	/// <summary>
	/// 捕获野生宝可梦
	/// </summary>
	public void CatchWildPokemon(PokemonData wildPokemonData, int level = 5)
	{
		PlayerPokemonData newPokemon = new PlayerPokemonData(wildPokemonData, level);
		
		if (!AddToParty(newPokemon))
		{
			// 队伍满了，放入仓库
			pokemonStorage.Add(newPokemon);
			Debug.Log($"{wildPokemonData.displayName} 已放入仓库");
		}
		else
		{
			Debug.Log($"捕获了 {wildPokemonData.displayName}！");
		}
		
		// 刷新队伍面板UI（如果存在）
		TeamPanelUI.RefreshAllTeamPanels();
	}
	
	/// <summary>
	/// 治疗所有宝可梦
	/// </summary>
	public void HealAllPokemon()
	{
		foreach (var pokemon in pokemonParty)
		{
			pokemon.FullRestore();
		}
		Debug.Log("所有宝可梦已恢复！");
	}
	
	/// <summary>
	/// 保存到存档（简化版）
	/// </summary>
	public void SaveToPlayerPrefs()
	{
		// TODO: 实现完整的存档系统
		Debug.Log("存档功能待实现");
	}
	
	/// <summary>
	/// 从存档加载（简化版）
	/// </summary>
	public void LoadFromPlayerPrefs()
	{
		// TODO: 实现完整的存档系统
		Debug.Log("读档功能待实现");
	}
}

