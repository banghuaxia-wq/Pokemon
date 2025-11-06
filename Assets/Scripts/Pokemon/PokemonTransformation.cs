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
        
        Debug.Log($"[PokemonTransformation] Awake - SpriteRenderer: {(spriteRenderer != null ? "找到" : "未找到")}");
        Debug.Log($"[PokemonTransformation] Awake - PokemonFromData: {(pokemonData != null ? "找到" : "未找到")}");
        
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            Debug.Log($"[PokemonTransformation] 保存原始贴图: {(originalSprite != null ? originalSprite.name : "null")}");
        }
        else
        {
            // 尝试在子对象中查找 SpriteRenderer
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalSprite = spriteRenderer.sprite;
                Debug.Log($"[PokemonTransformation] 在子对象中找到 SpriteRenderer，保存原始贴图: {(originalSprite != null ? originalSprite.name : "null")}");
            }
            else
            {
                Debug.LogError($"[PokemonTransformation] 找不到 SpriteRenderer 组件！请确保 Prefab 中有 SpriteRenderer！");
            }
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
            Debug.LogWarning($"[PokemonTransformation] {pokemonData?.pokemonName ?? "Unknown"} 已经处于变化形态！");
            return;
        }
        
        Debug.Log($"[PokemonTransformation] 开始变形：{pokemonData?.pokemonName ?? "Unknown"}");
        Debug.Log($"[PokemonTransformation] transformedSprite = {(transformedSprite != null ? transformedSprite.name : "null")}");
        Debug.Log($"[PokemonTransformation] spriteRenderer = {(spriteRenderer != null ? "存在" : "null")}");
        Debug.Log($"[PokemonTransformation] enableForcedSkill = {enableForcedSkill}");
        Debug.Log($"[PokemonTransformation] forcedSkillType = {forcedSkillType}");
        
        isTransformed = true;
        
        // 更换贴图
        if (transformedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = transformedSprite;
            Debug.Log($"[PokemonTransformation] 贴图已更换为：{transformedSprite.name}");
        }
        else
        {
            if (transformedSprite == null)
                Debug.LogWarning($"[PokemonTransformation] transformedSprite 未设置！");
            if (spriteRenderer == null)
                Debug.LogWarning($"[PokemonTransformation] spriteRenderer 未找到！");
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
            Debug.Log($"[PokemonTransformation] 准备提升防御力，defenseBonus = {defenseBonus}");
            
            // 使用反射获取私有字段（因为_currentDefense是私有的）
            var field = typeof(PokemonFromData).GetField("_currentDefense", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                int currentDefense = (int)field.GetValue(pokemonData);
                int newDefense = currentDefense + defenseBonus;
                field.SetValue(pokemonData, newDefense);
                Debug.Log($"[PokemonTransformation] {pokemonData.pokemonName} 防御力提升：{currentDefense} → {newDefense} (+{defenseBonus})");
            }
            else
            {
                Debug.LogError($"[PokemonTransformation] 无法找到 _currentDefense 字段！");
            }
        }
        else
        {
            Debug.Log($"[PokemonTransformation] defenseBonus = {defenseBonus}，不提升防御力");
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
        Debug.Log($"[PokemonTransformation] GetForcedSkill 被调用");
        Debug.Log($"[PokemonTransformation] isTransformed = {isTransformed}");
        Debug.Log($"[PokemonTransformation] enableForcedSkill = {enableForcedSkill}");
        Debug.Log($"[PokemonTransformation] forcedSkillType = {forcedSkillType}");
        Debug.Log($"[PokemonTransformation] 技能数量 = {(skills != null ? skills.Count : 0)}");
        
        if (!isTransformed)
        {
            Debug.LogWarning($"[PokemonTransformation] 未变形，不使用强制技能");
            return null;
        }
        
        if (!enableForcedSkill)
        {
            Debug.LogWarning($"[PokemonTransformation] 强制技能未启用");
            return null;
        }
        
        if (skills == null || skills.Count == 0)
        {
            Debug.LogWarning($"[PokemonTransformation] 技能列表为空");
            return null;
        }
        
        // 在技能列表中查找匹配的技能类型
        Debug.Log($"[PokemonTransformation] 开始查找强制技能类型：{forcedSkillType}");
        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];
            Debug.Log($"[PokemonTransformation] 技能 {i}: {skill.skillName}, 类型: {skill.skillType}");
            
            if (skill.skillType == forcedSkillType)
            {
                Debug.Log($"[PokemonTransformation] ✓ 找到强制技能：{skill.skillName}（类型：{forcedSkillType}）");
                return skill;
            }
        }
        
        Debug.LogWarning($"[PokemonTransformation] ✗ 未找到强制技能类型 {forcedSkillType}，当前有 {skills.Count} 个技能");
        return null;
    }
}

