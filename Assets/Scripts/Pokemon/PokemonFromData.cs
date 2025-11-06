using UnityEngine;
using System.Collections.Generic;

public class PokemonFromData : MonoBehaviour
{
	[Header("数据引用")]
	[Tooltip("种族数据（野生宝可梦必填）")]
	public PokemonData data;

	[Header("运行时数据")]
	[Tooltip("运行时技能实例")]
	private List<Skill> skills = new List<Skill>();
	
	[Tooltip("是否为玩家宝可梦")]
	private bool isPlayerPokemon = false;
	
	[Tooltip("玩家宝可梦数据引用")]
	private PlayerPokemonData playerData;

	[Header("当前状态")]
	[SerializeField] private int _currentHP;
	public int currentHP => _currentHP;
	
	[Tooltip("运行时防御力（战斗中可变）")]
	[SerializeField] private int _currentDefense;
	
	[Tooltip("是否被嘲讽（多人战斗用）")]
	private PokemonFromData tauntedBy = null;
	
	[Header("好感度系统")]
	[Tooltip("当前好感度（0-100）")]
	[SerializeField] private int _currentAffinity = 0;
	
	[Tooltip("好感度条UI引用（战斗时自动设置）")]
	private UnityEngine.UI.Slider affinitySlider;

	[Header("动画控制")]
	[Tooltip("Animator组件引用")]
	public Animator animator;
	
	[Header("技能动画生成点")]
	[Tooltip("鞭打攻击特效生成位置")]
	public Transform whipAttackAnimPosition;
	
	[Tooltip("防御姿态特效生成位置")]
	public Transform shieldAnimPosition;
	
	[Tooltip("治疗特效生成位置")]
	public Transform healAnimPosition;

	// 属性访问器
	public string pokemonName => GetPokemonName();
	public int pokemonHP => data != null ? data.baseHP : 0;
	public int pokemonAttack => data != null ? data.baseAttack : 0;
	
	/// <summary>
	/// 当前防御力（返回运行时防御力）
	/// </summary>
	public int pokemonDefense => _currentDefense;
	
	/// <summary>
	/// 获取技能列表
	/// </summary>
	public List<Skill> GetSkills() => skills;
	
	/// <summary>
	/// 是否为玩家的宝可梦
	/// </summary>
	public bool IsPlayerPokemon() => isPlayerPokemon;

	private void Awake()
	{
		// 自动获取Animator
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
	}
	
	/// <summary>
	/// 初始化为野生宝可梦（从PokemonData）
	/// </summary>
	public void InitializeAsWild()
	{
		if (data == null)
		{
			Debug.LogError($"PokemonFromData on {gameObject.name} has no PokemonData assigned.");
			return;
		}
		
		isPlayerPokemon = false;
		playerData = null;
		
		// 初始化HP和防御力
		_currentHP = pokemonHP;
		_currentDefense = data != null ? data.baseDefense : 0;
		
		// 初始化好感度（野生宝可梦从0开始）
		_currentAffinity = 0;
		
		// 从PokemonData读取技能
		InitializeSkillsFromPokemonData();
	}
	
	/// <summary>
	/// 初始化为玩家宝可梦（从PlayerPokemonData）
	/// </summary>
	public void InitializeAsPlayer(PlayerPokemonData playerPokemonData)
	{
		if (playerPokemonData == null || playerPokemonData.pokemonData == null)
		{
			Debug.LogError("PlayerPokemonData is null!");
			return;
		}
		
		isPlayerPokemon = true;
		playerData = playerPokemonData;
		data = playerPokemonData.pokemonData;
		
		// 从玩家数据读取HP和防御力
		_currentHP = playerPokemonData.currentHP;
		_currentDefense = data != null ? data.baseDefense : 0;
		
		// 初始化好感度（玩家宝可梦从0开始）
		_currentAffinity = 0;
		
		// 从玩家数据读取技能
		InitializeSkillsFromPlayerData();
	}
	
	/// <summary>
	/// 从PokemonData初始化技能（野生宝可梦）
	/// </summary>
	private void InitializeSkillsFromPokemonData()
	{
		skills.Clear();
		
		if (data == null || data.skills == null) return;
		
		foreach (var skillData in data.skills)
		{
			if (skillData != null)
			{
				skills.Add(new Skill(skillData));
			}
		}
	}
	
	/// <summary>
	/// 从PlayerPokemonData初始化技能（玩家宝可梦）
	/// </summary>
	private void InitializeSkillsFromPlayerData()
	{
		skills.Clear();
		
		if (playerData == null || playerData.learnedSkills == null) return;
		
		for (int i = 0; i < playerData.learnedSkills.Count; i++)
		{
			var skillData = playerData.learnedSkills[i];
			if (skillData != null)
			{
				Skill skill = new Skill(skillData);
				
				// 恢复玩家存档中的PP值
				if (i < playerData.skillCurrentPP.Count)
				{
					skill.SetCurrentPP(playerData.skillCurrentPP[i]);
				}
				
				skills.Add(skill);
			}
		}
	}
	
	/// <summary>
	/// 获取宝可梦名称
	/// </summary>
	private string GetPokemonName()
	{
		if (isPlayerPokemon && playerData != null)
		{
			return playerData.GetDisplayName();
		}
		
		return data != null ? data.displayName : "Unknown";
	}
	
	/// <summary>
	/// 受到伤害
	/// </summary>
	public void TakeDamage(int damage)
	{
		_currentHP = Mathf.Max(0, _currentHP - damage);
	}
	
	/// <summary>
	/// 是否存活
	/// </summary>
	public bool IsAlive() => _currentHP > 0;
	
	/// <summary>
	/// 播放攻击动画（根据技能类型）- 此方法已被 BattleAnimationManager 取代
	/// 保留用于宝可梦自身的动画触发（如果Animator有对应参数）
	/// </summary>
	public void PlayAttackAnimation(SkillType skillType)
	{
		if (animator == null) return;
		
		// 根据技能类型触发不同的动画
		switch (skillType)
		{
			case SkillType.TackleAttack:
				animator.SetTrigger("Attack");
				break;
			case SkillType.WhipAttack:
				animator.SetTrigger("Attack");
				break;
			case SkillType.DefenseStance:
				animator.SetTrigger("Defend");
				break;
			case SkillType.Heal:
				animator.SetTrigger("Heal");
				break;
			case SkillType.Taunt:
				// 嘲讽动画由 BattleAnimationManager 处理（跳动）
				// 这里可以触发宝可梦自身的动画（如果有的话）
				animator.SetTrigger("Attack");
				break;
		}
	}
	
	/// <summary>
	/// 战斗结束后同步数据到PlayerPokemonData（仅玩家宝可梦）
	/// </summary>
	public void SyncToPlayerData()
	{
		if (!isPlayerPokemon || playerData == null) return;
		
		// 同步HP
		playerData.currentHP = _currentHP;
		
		// 同步PP值
		playerData.skillCurrentPP.Clear();
		foreach (var skill in skills)
		{
			playerData.skillCurrentPP.Add(skill.currentPP);
		}
	}
	
	/// <summary>
	/// 获取玩家宝可梦数据引用（用于存档）
	/// </summary>
	public PlayerPokemonData GetPlayerData()
	{
		return playerData;
	}
	
	/// <summary>
	/// 根据技能类型获取对应的动画生成点
	/// </summary>
	/// <param name="skillType">技能类型</param>
	/// <returns>生成点 Transform，如果没有设置则返回自身 Transform</returns>
	public Transform GetAnimationSpawnPoint(SkillType skillType)
	{
		switch (skillType)
		{
			case SkillType.WhipAttack:
				return whipAttackAnimPosition != null ? whipAttackAnimPosition : transform;
			
			case SkillType.DefenseStance:
				return shieldAnimPosition != null ? shieldAnimPosition : transform;
			
			case SkillType.Heal:
				return healAnimPosition != null ? healAnimPosition : transform;
			
			case SkillType.TackleAttack:
			case SkillType.Taunt:
			default:
				return transform; // 撞击攻击和嘲讽直接使用自身位置
		}
	}
	
	/// <summary>
	/// 提升防御力（防御技能使用）
	/// 公式：提升 (100 - 当前防御力) / 2，最高不超过 95
	/// </summary>
	public int BoostDefense()
	{
		int oldDefense = _currentDefense;
		
		// 计算提升量：(100 - 当前防御力) / 2
		int boost = (100 - _currentDefense) / 2;
		
		// 提升防御力
		_currentDefense = Mathf.Min(_currentDefense + boost, 95);
		
		int actualBoost = _currentDefense - oldDefense;
		Debug.Log($"[{pokemonName}] 防御力提升：{oldDefense} → {_currentDefense} (+{actualBoost})");
		
		return actualBoost;
	}
	
	/// <summary>
	/// 治疗HP
	/// </summary>
	/// <param name="amount">治疗量</param>
	/// <returns>实际治疗量</returns>
	public int Heal(int amount)
	{
		int oldHP = _currentHP;
		_currentHP = Mathf.Min(_currentHP + amount, pokemonHP);
		int actualHeal = _currentHP - oldHP;
		
		Debug.Log($"[{pokemonName}] HP恢复：{oldHP} → {_currentHP} (+{actualHeal})");
		
		return actualHeal;
	}
	
	/// <summary>
	/// 设置嘲讽状态（被某个宝可梦嘲讽）
	/// </summary>
	/// <param name="taunter">嘲讽者</param>
	public void SetTaunted(PokemonFromData taunter)
	{
		tauntedBy = taunter;
		Debug.Log($"[{pokemonName}] 被 {taunter.pokemonName} 嘲讽了！");
	}
	
	/// <summary>
	/// 清除嘲讽状态
	/// </summary>
	public void ClearTaunt()
	{
		if (tauntedBy != null)
		{
			Debug.Log($"[{pokemonName}] 嘲讽状态解除");
		}
		tauntedBy = null;
	}
	
	/// <summary>
	/// 检查是否被嘲讽
	/// </summary>
	/// <returns>嘲讽者，如果没有被嘲讽则返回 null</returns>
	public PokemonFromData GetTauntedBy()
	{
		// 如果嘲讽者已经倒下，清除嘲讽状态
		if (tauntedBy != null && !tauntedBy.IsAlive())
		{
			ClearTaunt();
		}
		
		return tauntedBy;
	}
	
	// ==================== 好感度系统 ====================
	
	/// <summary>
	/// 获取当前好感度
	/// </summary>
	public int GetAffinity()
	{
		// 如果是绿帽鱼，返回全局好感度
		if (IsGreenHatFish())
		{
			return AffinityManager.Instance.GetGreenHatFishAffinity();
		}
		return _currentAffinity;
	}
	
	/// <summary>
	/// 增加好感度
	/// </summary>
	public void AddAffinity(int amount)
	{
		// 如果是绿帽鱼，修改全局好感度
		if (IsGreenHatFish())
		{
			AffinityManager.Instance.AddGreenHatFishAffinity(amount);
			UpdateAffinityUI();
			return;
		}
		
		int oldAffinity = _currentAffinity;
		_currentAffinity = Mathf.Clamp(_currentAffinity + amount, 0, 100);
		Debug.Log($"[{pokemonName}] 好感度: {oldAffinity} → {_currentAffinity} (+{amount})");
		UpdateAffinityUI();
	}
	
	/// <summary>
	/// 设置好感度
	/// </summary>
	public void SetAffinity(int value)
	{
		// 绿帽鱼不允许直接设置好感度（只能通过 AffinityManager）
		if (IsGreenHatFish())
		{
			Debug.LogWarning("绿帽鱼的好感度由 AffinityManager 管理，不能直接设置！");
			return;
		}
		
		_currentAffinity = Mathf.Clamp(value, 0, 100);
		Debug.Log($"[{pokemonName}] 好感度设置为: {_currentAffinity}");
		UpdateAffinityUI();
	}
	
	/// <summary>
	/// 重置好感度（战斗结束时调用，绿帽鱼除外）
	/// </summary>
	public void ResetAffinity()
	{
		// 绿帽鱼的好感度不重置
		if (IsGreenHatFish())
		{
			return;
		}
		
		_currentAffinity = 0;
		UpdateAffinityUI();
		Debug.Log($"[{pokemonName}] 好感度已重置");
	}
	
	/// <summary>
	/// 更新好感度UI
	/// </summary>
	private void UpdateAffinityUI()
	{
		if (affinitySlider != null)
		{
			affinitySlider.value = GetAffinity() / 100f;
		}
	}
	
	/// <summary>
	/// 检查是否是绿帽鱼
	/// </summary>
	public bool IsGreenHatFish()
	{
		if (data == null) return false;
		string name = data.displayName.ToLower();
		return name.Contains("绿帽鱼") || name.Contains("greenhatfish") || name.Contains("green hat fish");
	}
	
	/// <summary>
	/// 设置好感度条引用（由 BattleSystem 调用）
	/// </summary>
	public void SetAffinitySlider(UnityEngine.UI.Slider slider)
	{
		affinitySlider = slider;
		InitializeAffinityUI();
	}
	
	/// <summary>
	/// 初始化好感度UI
	/// </summary>
	private void InitializeAffinityUI()
	{
		if (affinitySlider != null)
		{
			affinitySlider.maxValue = 1f;
			affinitySlider.minValue = 0f;
			affinitySlider.gameObject.SetActive(true); // 显示好感度条
			UpdateAffinityUI();
		}
	}
}
