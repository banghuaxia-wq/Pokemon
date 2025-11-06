using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 宝可梦队伍菜单管理器
/// </summary>
public class PokemonMenuManager : MonoBehaviour
{
	[Header("左侧队伍区域")]
	[Tooltip("容量显示文本")]
	public TextMeshProUGUI capacityText;
	
	[Tooltip("所有宝可梦槽位")]
	public List<PokemonUISlot> pokemonSlots = new List<PokemonUISlot>();
	
	[Header("右侧详情面板")]
	[Tooltip("详情面板整体（背景框）")]
	public GameObject detailPanel;
	
	[Tooltip("宝可梦信息面板")]
	public GameObject pokemonInfoPanel;
	
	[Tooltip("技能列表面板")]
	public GameObject skillListPanel;
	
	[Tooltip("操作按钮区域")]
	public GameObject playerControlArea;
	
	[Tooltip("宝可梦图标")]
	public Image pokemonIcon;
	
	[Tooltip("宝可梦名称")]
	public TextMeshProUGUI pokemonNameText;
	
	[Tooltip("HP进度条")]
	public Slider hpBar;
	
	[Tooltip("HP文本")]
	public TextMeshProUGUI hpText;
	
	[Header("技能显示")]
	[Tooltip("技能槽位列表")]
	public List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();
	
	[Header("操作按钮")]
	[Tooltip("恢复HP按钮")]
	public Button recoverHPButton;
	
	[Tooltip("恢复PP按钮")]
	public Button recoverPPButton;
	
	[Tooltip("放生按钮")]
	public Button releaseButton;
	
	[Tooltip("关闭按钮")]
	public Button closeBagButton;
	
	// 当前选中的槽位
	private int currentSelectedIndex = -1;
	private PokemonUISlot currentSelectedSlot = null;
	
	// CanvasGroup用于控制面板显示/隐藏
	private CanvasGroup canvasGroup;
	
	// 单例，方便其他脚本打开菜单
	private static PokemonMenuManager instance;
	public static PokemonMenuManager Instance => instance;
	
	private void Awake()
	{
		// 设置单例
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Debug.LogWarning("[PokemonMenuManager] 场景中存在多个 PokemonMenuManager，这可能导致问题");
		}
		
		// 获取或添加 CanvasGroup 组件
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}
		
		// 初始隐藏面板（使用 CanvasGroup）
		HidePanel();
	}
	
	private void Start()
	{
		// 绑定按钮事件
		if (recoverHPButton != null)
			recoverHPButton.onClick.AddListener(OnRecoverHP);
		
		if (recoverPPButton != null)
			recoverPPButton.onClick.AddListener(OnRecoverPP);
		
		if (releaseButton != null)
			releaseButton.onClick.AddListener(OnReleasePokemon);
		
		if (closeBagButton != null)
			closeBagButton.onClick.AddListener(OnCloseBag);
		
		// 自动查找所有槽位（如果列表为空）
		if (pokemonSlots.Count == 0)
		{
			GameObject scrollView = GameObject.Find("PokemonMenuPanel/PartyGrid/Scroll View");
			if (scrollView != null)
			{
				pokemonSlots.AddRange(scrollView.GetComponentsInChildren<PokemonUISlot>(true));
				Debug.Log($"[PokemonMenuManager] 自动找到 {pokemonSlots.Count} 个宝可梦槽位");
			}
		}
		
		// 初始化详情面板状态
		InitializeDetailPanel();
		
		// 刷新显示
		RefreshPartyDisplay();
	}
	
	/// <summary>
	/// 初始化详情面板状态
	/// </summary>
	private void InitializeDetailPanel()
	{
		// DetailPanel（背景框）始终显示
		if (detailPanel != null)
		{
			detailPanel.SetActive(true);
		}
		
		// 子面板默认隐藏（未选中宝可梦时）
		if (pokemonInfoPanel != null)
			pokemonInfoPanel.SetActive(false);
		
		if (skillListPanel != null)
			skillListPanel.SetActive(false);
		
		if (playerControlArea != null)
			playerControlArea.SetActive(false);
	}
	
	/// <summary>
	/// 刷新队伍显示
	/// </summary>
	public void RefreshPartyDisplay()
	{
		if (PlayerInventory.Instance == null)
		{
			Debug.LogError("[PokemonMenuManager] PlayerInventory未找到！");
			return;
		}
		
		List<PlayerPokemonData> party = PlayerInventory.Instance.pokemonParty;
		
		// 更新容量文本
		if (capacityText != null)
		{
			capacityText.text = $"容量：{party.Count}/4";
		}
		
		// 更新每个槽位
		for (int i = 0; i < pokemonSlots.Count; i++)
		{
			if (i < party.Count)
			{
				// 有宝可梦，显示
				pokemonSlots[i].UpdateSlot(party[i], i);
			}
			else
			{
				// 空槽位，隐藏
				pokemonSlots[i].UpdateSlot(null, i);
			}
		}
		
		Debug.Log($"[PokemonMenuManager] 队伍刷新完成，当前 {party.Count} 只宝可梦");
	}
	
	/// <summary>
	/// 槽位被点击
	/// </summary>
	public void OnPokemonSlotClicked(int index, PokemonUISlot slot)
	{
		// 取消上一个槽位的选中状态
		if (currentSelectedSlot != null)
		{
			currentSelectedSlot.SetSelected(false);
		}
		
		// 设置新的选中槽位
		currentSelectedIndex = index;
		currentSelectedSlot = slot;
		slot.SetSelected(true);
		
		// 显示详情面板
		ShowPokemonDetail(slot.GetPokemonData());
		
		Debug.Log($"[PokemonMenuManager] 选中宝可梦 #{index}: {slot.GetPokemonData().GetDisplayName()}");
	}
	
	/// <summary>
	/// 显示宝可梦详情
	/// </summary>
	private void ShowPokemonDetail(PlayerPokemonData pokemon)
	{
		if (pokemon == null || pokemon.pokemonData == null)
		{
			// 隐藏详情内容，但保持背景框显示
			if (pokemonInfoPanel != null)
				pokemonInfoPanel.SetActive(false);
			
			if (skillListPanel != null)
				skillListPanel.SetActive(false);
			
			if (playerControlArea != null)
				playerControlArea.SetActive(false);
			
			return;
		}
		
		// 显示详情面板（背景框始终显示）
		if (detailPanel != null)
			detailPanel.SetActive(true);
		
		// 显示详情内容
		if (pokemonInfoPanel != null)
			pokemonInfoPanel.SetActive(true);
		
		if (skillListPanel != null)
			skillListPanel.SetActive(true);
		
		if (playerControlArea != null)
			playerControlArea.SetActive(true);
		
		// 更新基本信息
		if (pokemonNameText != null)
		{
			pokemonNameText.text = pokemon.GetDisplayName();
		}
		
		// 更新图标
		if (pokemonIcon != null && pokemon.pokemonData.icon != null)
		{
			pokemonIcon.sprite = pokemon.pokemonData.icon;
			pokemonIcon.enabled = true;
		}
		else if (pokemonIcon != null)
		{
			pokemonIcon.enabled = false;
		}
		
		// 更新HP条
		UpdateHPDisplay(pokemon);
		
		// 更新技能显示
		UpdateSkillDisplay(pokemon);
	}
	
	/// <summary>
	/// 更新HP显示
	/// </summary>
	private void UpdateHPDisplay(PlayerPokemonData pokemon)
	{
		int maxHP = pokemon.pokemonData.baseHP;
		int currentHP = pokemon.currentHP;
		
		// 更新进度条
		if (hpBar != null)
		{
			hpBar.maxValue = maxHP;
			hpBar.value = currentHP;
		}
		
		// 更新文本
		if (hpText != null)
		{
			hpText.text = $"生命值：{currentHP}/{maxHP}";
		}
	}
	
	/// <summary>
	/// 更新技能显示
	/// </summary>
	private void UpdateSkillDisplay(PlayerPokemonData pokemon)
	{
		for (int i = 0; i < skillSlots.Count; i++)
		{
			if (i < pokemon.learnedSkills.Count && pokemon.learnedSkills[i] != null)
			{
				// 有技能，显示
				SkillData skill = pokemon.learnedSkills[i];
				int currentPP = pokemon.skillCurrentPP[i];
				skillSlots[i].UpdateSkill(skill, currentPP);
			}
			else
			{
				// 没有技能，隐藏
				skillSlots[i].ClearSkill();
			}
		}
	}
	
	/// <summary>
	/// 恢复HP按钮
	/// </summary>
	private void OnRecoverHP()
	{
		if (currentSelectedIndex < 0 || currentSelectedIndex >= PlayerInventory.Instance.pokemonParty.Count)
		{
			Debug.LogWarning("[PokemonMenuManager] 请先选择一只宝可梦");
			return;
		}
		
		PlayerPokemonData pokemon = PlayerInventory.Instance.pokemonParty[currentSelectedIndex];
		pokemon.currentHP = pokemon.pokemonData.baseHP;
		
		Debug.Log($"[PokemonMenuManager] {pokemon.GetDisplayName()} 的HP已恢复！");
		
		// 刷新显示
		ShowPokemonDetail(pokemon);
	}
	
	/// <summary>
	/// 恢复PP按钮
	/// </summary>
	private void OnRecoverPP()
	{
		if (currentSelectedIndex < 0 || currentSelectedIndex >= PlayerInventory.Instance.pokemonParty.Count)
		{
			Debug.LogWarning("[PokemonMenuManager] 请先选择一只宝可梦");
			return;
		}
		
		PlayerPokemonData pokemon = PlayerInventory.Instance.pokemonParty[currentSelectedIndex];
		pokemon.RestoreAllPP();
		
		Debug.Log($"[PokemonMenuManager] {pokemon.GetDisplayName()} 的所有技能PP已恢复！");
		
		// 刷新显示
		ShowPokemonDetail(pokemon);
	}
	
	/// <summary>
	/// 放生宝可梦按钮
	/// </summary>
	private void OnReleasePokemon()
	{
		if (currentSelectedIndex < 0 || currentSelectedIndex >= PlayerInventory.Instance.pokemonParty.Count)
		{
			Debug.LogWarning("[PokemonMenuManager] 请先选择一只宝可梦");
			return;
		}
		
		// 检查是否是最后一只宝可梦
		if (PlayerInventory.Instance.pokemonParty.Count <= 1)
		{
			Debug.LogWarning("[PokemonMenuManager] 不能放生最后一只宝可梦！");
			return;
		}
		
		PlayerPokemonData pokemon = PlayerInventory.Instance.pokemonParty[currentSelectedIndex];
		string name = pokemon.GetDisplayName();
		
		// 从队伍中移除
		PlayerInventory.Instance.pokemonParty.RemoveAt(currentSelectedIndex);
		
		Debug.Log($"[PokemonMenuManager] 已放生 {name}");
		
		// 重置选中状态
		currentSelectedIndex = -1;
		currentSelectedSlot = null;
		
		// 隐藏详情面板的内容
		InitializeDetailPanel();
		
		// 刷新显示
		RefreshPartyDisplay();
		
		// 刷新队伍面板UI（如果存在）
		TeamPanelUI.RefreshAllTeamPanels();
	}
	
	/// <summary>
	/// 关闭背包按钮
	/// </summary>
	private void OnCloseBag()
	{
		HidePanel();
		Debug.Log("[PokemonMenuManager] 背包已关闭");
	}
	
	/// <summary>
	/// 打开背包（从外部调用）
	/// </summary>
	public void OpenMenu()
	{
		ShowPanel();
		RefreshPartyDisplay();
		
		// 重置详情面板状态：背景框显示，内容隐藏
		InitializeDetailPanel();
		
		currentSelectedIndex = -1;
		currentSelectedSlot = null;
		
		// 刷新队伍面板UI（如果存在）
		TeamPanelUI.RefreshAllTeamPanels();
	}
	
	/// <summary>
	/// 显示面板
	/// </summary>
	private void ShowPanel()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}
	
	/// <summary>
	/// 隐藏面板
	/// </summary>
	private void HidePanel()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}
}

/// <summary>
/// 技能槽位UI控制
/// </summary>
[System.Serializable]
public class SkillSlotUI
{
	[Tooltip("技能名称文本")]
	public TextMeshProUGUI skillNameText;
	
	[Tooltip("技能威力文本")]
	public TextMeshProUGUI attackPowerText;
	
	[Tooltip("PP值文本")]
	public TextMeshProUGUI ppText;
	
	[Tooltip("槽位根物体")]
	public GameObject slotRoot;
	
	/// <summary>
	/// 更新技能显示
	/// </summary>
	public void UpdateSkill(SkillData skill, int currentPP)
	{
		if (skillNameText != null)
			skillNameText.gameObject.SetActive(true);
		
		if (attackPowerText != null)
			attackPowerText.gameObject.SetActive(true);
		
		if (ppText != null)
			ppText.gameObject.SetActive(true);
		
		if (skillNameText != null)
			skillNameText.text = skill.skillName;
		
		if (attackPowerText != null)
			attackPowerText.text = $"伤害：{skill.power}";
		
		if (ppText != null)
			ppText.text = $"PP：{currentPP}/{skill.maxPP}";
	}
	
	/// <summary>
	/// 清空技能槽（保持SkillSlot激活但隐藏文本）
	/// </summary>
	public void ClearSkill()
	{
		if (skillNameText != null)
			skillNameText.gameObject.SetActive(false);
		
		if (attackPowerText != null)
			attackPowerText.gameObject.SetActive(false);
		
		if (ppText != null)
			ppText.gameObject.SetActive(false);
	}
}

