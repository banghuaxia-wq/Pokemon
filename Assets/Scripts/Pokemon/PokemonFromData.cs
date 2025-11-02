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

	[Header("动画控制")]
	[Tooltip("Animator组件引用")]
	public Animator animator;

	// 属性访问器
	public string pokemonName => GetPokemonName();
	public int pokemonHP => data != null ? data.baseHP : 0;
	public int pokemonAttack => data != null ? data.baseAttack : 0;
	public int pokemonDefense => data != null ? data.baseDefense : 0;
	
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
		
		// 初始化HP
		_currentHP = pokemonHP;
		
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
		
		// 从玩家数据读取HP
		_currentHP = playerPokemonData.currentHP;
		
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
	/// 治疗
	/// </summary>
	public void Heal(int amount)
	{
		_currentHP = Mathf.Min(pokemonHP, _currentHP + amount);
	}
	
	/// <summary>
	/// 是否存活
	/// </summary>
	public bool IsAlive() => _currentHP > 0;
	
	/// <summary>
	/// 播放攻击动画（根据技能类型）
	/// </summary>
	public void PlayAttackAnimation(SkillType skillType)
	{
		if (animator == null) return;
		
		// 根据技能类型触发不同的动画
		switch (skillType)
		{
			case SkillType.Physical:
				animator.SetTrigger("PhysicalAttack");
				break;
			case SkillType.Special:
				animator.SetTrigger("SpecialAttack");
				break;
			case SkillType.Status:
				animator.SetTrigger("StatusSkill");
				break;
			case SkillType.Heal:
				animator.SetTrigger("Heal");
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
}
