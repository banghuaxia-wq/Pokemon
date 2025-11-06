using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 队伍面板UI - 显示所有宝可梦的图标，根据是否拥有来调整透明度
/// </summary>
public class TeamPanelUI : MonoBehaviour
{
	[Header("宝可梦图标设置")]
	[Tooltip("宝可梦图标列表（名称需要与PokemonData中的pokemonName匹配）")]
	public List<PokemonIconSlot> pokemonIcons = new List<PokemonIconSlot>();
	
	[Header("透明度设置")]
	[Tooltip("已拥有的宝可梦透明度（0-1）")]
	[Range(0f, 1f)]
	public float ownedAlpha = 1f; // 255 -> 1.0
	
	[Tooltip("未拥有的宝可梦透明度（0-1）")]
	[Range(0f, 1f)]
	public float unownedAlpha = 0.117f; // 30 -> 0.117 (30/255)
	
	private void Start()
	{
		// 初始刷新
		RefreshTeamDisplay();
		
		Debug.Log($"[TeamPanelUI] 队伍面板已初始化，监控 {pokemonIcons.Count} 个宝可梦图标");
	}
	
	private void OnEnable()
	{
		// 每次激活时刷新
		RefreshTeamDisplay();
	}
	
	/// <summary>
	/// 刷新队伍显示
	/// </summary>
	public void RefreshTeamDisplay()
	{
		if (PlayerInventory.Instance == null)
		{
			Debug.LogWarning("[TeamPanelUI] PlayerInventory未找到，无法刷新队伍显示");
			return;
		}
		
		// 遍历所有图标槽位
		foreach (var iconSlot in pokemonIcons)
		{
			if (iconSlot.iconImage == null)
			{
				Debug.LogWarning($"[TeamPanelUI] 图标槽位 '{iconSlot.pokemonName}' 的 Image 组件未设置");
				continue;
			}
			
			// 检查这个宝可梦是否在背包中
			bool isOwned = IsPokemonOwned(iconSlot.pokemonName);
			
			// 设置透明度
			Color color = iconSlot.iconImage.color;
			color.a = isOwned ? ownedAlpha : unownedAlpha;
			iconSlot.iconImage.color = color;
			
			Debug.Log($"[TeamPanelUI] 宝可梦 '{iconSlot.pokemonName}' -> {(isOwned ? "已拥有" : "未拥有")}，透明度: {color.a}");
		}
	}
	
	/// <summary>
	/// 检查指定名称的宝可梦是否在背包中
	/// </summary>
	private bool IsPokemonOwned(string pokemonName)
	{
		if (PlayerInventory.Instance == null || string.IsNullOrEmpty(pokemonName))
			return false;
		
		// 遍历玩家背包中的所有宝可梦
		foreach (var pokemon in PlayerInventory.Instance.pokemonParty)
		{
			if (pokemon != null && pokemon.pokemonData != null)
			{
				// 对比名称（不区分大小写）
				if (pokemon.pokemonData.displayName.Equals(pokemonName, System.StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// 从外部调用此方法来刷新显示（例如捕捉宝可梦后）
	/// </summary>
	public static void RefreshAllTeamPanels()
	{
		TeamPanelUI[] allPanels = FindObjectsOfType<TeamPanelUI>();
		foreach (var panel in allPanels)
		{
			panel.RefreshTeamDisplay();
		}
		
		Debug.Log($"[TeamPanelUI] 已刷新 {allPanels.Length} 个队伍面板");
	}
}

/// <summary>
/// 宝可梦图标槽位
/// </summary>
[System.Serializable]
public class PokemonIconSlot
{
	[Tooltip("宝可梦名称（需要与PokemonData中的displayName完全匹配）")]
	public string pokemonName;
	
	[Tooltip("图标Image组件")]
	public Image iconImage;
}

