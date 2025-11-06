using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 宝可梦形态变化系统 - 用于处理道具触发的形态变化（如M猪使用绳子后变为被捆绑形态）
/// </summary>
public class PokemonTransformation : MonoBehaviour
{
    [Header("形态变化配置")]
    [Tooltip("变化后的贴图")]
    public Sprite transformedSprite;
    
    [Tooltip("是否已经变化")]
    [SerializeField] private bool isTransformed = false;
    
    [Tooltip("变化后解锁的技能")]
    public List<SkillData> unlockedSkills = new List<SkillData>();
    
    [Tooltip("变化后的防御力加成")]
    public int defenseBonus = 0;
    
    [Tooltip("变化后强制使用的技能类型（AI专用，留空表示随机）")]
    public SkillType forcedSkillType = SkillType.TackleAttack;
    
    [Tooltip("是否启用强制技能（野生宝可梦启用，玩家宝可梦不启用）")]
    public bool enableForcedSkill = false;
    
    private SpriteRenderer spriteRenderer;
    private PokemonFromData pokemonData;
    private Sprite originalSprite;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pokemonData = GetComponent<PokemonFromData>();
        
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
    }
    
    /// <summary>
    /// 触发形态变化
    /// </summary>
    /// <param name="permanent">是否永久变化（存档保存）</param>
    public void Transform(bool permanent = false)
    {
        if (isTransformed)
        {
            Debug.LogWarning($"[{pokemonData.pokemonName}] 已经处于变化形态！");
            return;
        }
        
        isTransformed = true;
        
        // 更换贴图
        if (transformedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = transformedSprite;
            Debug.Log($"[{pokemonData.pokemonName}] 贴图已更换");
        }
        
        // 解锁新技能
        if (unlockedSkills.Count > 0 && pokemonData != null)
        {
            var currentSkills = pokemonData.GetSkills();
            foreach (var skillData in unlockedSkills)
            {
                // 检查是否已有该技能
                bool alreadyHas = false;
                foreach (var skill in currentSkills)
                {
                    if (skill.data == skillData)
                    {
                        alreadyHas = true;
                        break;
                    }
                }
                
                if (!alreadyHas)
                {
                    currentSkills.Add(new Skill(skillData));
                    Debug.Log($"[{pokemonData.pokemonName}] 解锁了新技能：{skillData.skillName}");
                }
            }
        }
        
        // 提升防御力（直接修改运行时防御力字段）
        if (defenseBonus > 0)
        {
            // 使用反射获取私有字段（因为_currentDefense是私有的）
            var field = typeof(PokemonFromData).GetField("_currentDefense", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                int currentDefense = (int)field.GetValue(pokemonData);
                field.SetValue(pokemonData, currentDefense + defenseBonus);
                Debug.Log($"[{pokemonData.pokemonName}] 防御力提升了 {defenseBonus} 点！");
            }
        }
        
        Debug.Log($"[{pokemonData.pokemonName}] 形态变化完成！");
    }
    
    /// <summary>
    /// 恢复原始形态
    /// </summary>
    public void RevertTransformation()
    {
        if (!isTransformed) return;
        
        isTransformed = false;
        
        // 恢复原始贴图
        if (originalSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
        
        // 移除解锁的技能
        if (unlockedSkills.Count > 0 && pokemonData != null)
        {
            var currentSkills = pokemonData.GetSkills();
            foreach (var skillData in unlockedSkills)
            {
                for (int i = currentSkills.Count - 1; i >= 0; i--)
                {
                    if (currentSkills[i].data == skillData)
                    {
                        currentSkills.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        
        // 恢复防御力
        if (defenseBonus > 0)
        {
            var field = typeof(PokemonFromData).GetField("_currentDefense", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                int currentDefense = (int)field.GetValue(pokemonData);
                field.SetValue(pokemonData, currentDefense - defenseBonus);
            }
        }
        
        Debug.Log($"[{pokemonData.pokemonName}] 恢复原始形态");
    }
    
    /// <summary>
    /// 检查是否已变化
    /// </summary>
    public bool IsTransformed() => isTransformed;
    
    /// <summary>
    /// 获取强制使用的技能（AI专用）
    /// </summary>
    /// <param name="skills">宝可梦的技能列表</param>
    /// <returns>强制使用的技能，如果没有则返回 null</returns>
    public Skill GetForcedSkill(List<Skill> skills)
    {
        if (!isTransformed || !enableForcedSkill || skills == null || skills.Count == 0)
        {
            return null;
        }
        
        // 在技能列表中查找匹配的技能类型
        foreach (var skill in skills)
        {
            if (skill.skillType == forcedSkillType)
            {
                Debug.Log($"[PokemonTransformation] 找到强制技能：{skill.skillName}（类型：{forcedSkillType}）");
                return skill;
            }
        }
        
        Debug.LogWarning($"[PokemonTransformation] 未找到强制技能类型 {forcedSkillType}");
        return null;
    }
}

