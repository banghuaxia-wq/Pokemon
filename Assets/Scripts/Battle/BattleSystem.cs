using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public enum BattleState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    Won,
    Lost,
    Escape
}

public class BattleSystem : MonoBehaviour
{
    public BattleState state;
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public Transform playerPosition;
    public Transform enemyPosition;
    public TextMeshProUGUI dialogText;
    private PokemonFromData playerPokemon;
    private PokemonFromData enemyPokemon;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;
    [SerializeField] private CanvasGroup rootPanel;
    [SerializeField] private CanvasGroup skillsPanel;
    [SerializeField] private CanvasGroup itemsPanel;
    [SerializeField] private CanvasGroup giftsPanel;
    private CanvasGroup[] _all;
    
    [Header("技能按钮")]
    [SerializeField] private GameObject skillButton1;
    [SerializeField] private GameObject skillButton2;
    [SerializeField] private GameObject skillButton3;
    
    [Header("道具按钮")]
    [SerializeField] private GameObject itemButton1;
    [SerializeField] private GameObject itemButton2;
    [SerializeField] private GameObject itemButton3;
    
    [Header("礼物按钮")]
    [SerializeField] private GameObject giftButton1;
    [SerializeField] private GameObject giftButton2;
    [SerializeField] private GameObject giftButton3;
    
    [Header("动画管理")]
    [Tooltip("战斗动画管理器")]
    private BattleAnimationManager animationManager;
    
    [Header("礼物系统")]
    [Tooltip("礼物赠送管理器")]
    private GiftSystem giftSystem;
    
    [Header("好感度系统")]
    [Tooltip("敌人宝可梦好感度条")]
    public UnityEngine.UI.Slider enemyAffinitySlider;
    
    [Tooltip("绿帽鱼好感度条")]
    public UnityEngine.UI.Slider greenHatAffinitySlider;
    
    [Header("绿帽鱼偷看系统")]
    [Tooltip("绿帽鱼站位（场景中的空物体）")]
    public Transform greenHatFishStation;
    
    [Tooltip("绿帽鱼的 PokemonData")]
    public PokemonData greenHatFishData;
    
    private GameObject greenHatFishInstance; // 绿帽鱼实例
    
    private void Awake()
    {
        _all = new[] { rootPanel, skillsPanel, itemsPanel, giftsPanel };
        
        // 检查所有面板是否都已赋值
        if (rootPanel == null) Debug.LogError("BattleSystem: rootPanel 未赋值！");
        if (skillsPanel == null) Debug.LogError("BattleSystem: skillsPanel 未赋值！");
        if (itemsPanel == null) Debug.LogError("BattleSystem: itemsPanel 未赋值！");
        if (giftsPanel == null) Debug.LogError("BattleSystem: giftsPanel 未赋值！");
        
        // 获取或添加动画管理器
        animationManager = GetComponent<BattleAnimationManager>();
        if (animationManager == null)
        {
            animationManager = gameObject.AddComponent<BattleAnimationManager>();
            Debug.LogWarning("[BattleSystem] 自动添加了 BattleAnimationManager 组件");
        }
        
        // 获取或添加礼物系统
        giftSystem = GetComponent<GiftSystem>();
        if (giftSystem == null)
        {
            giftSystem = gameObject.AddComponent<GiftSystem>();
            Debug.Log("[BattleSystem] 自动添加了 GiftSystem 组件");
        }
    }
    public void Start()
    {
        state = BattleState.Start;
        
        // 检查是否有来自 Claw 抓取的战斗初始化数据
        if (BattleInitializer.PlayerData != null && BattleInitializer.EnemyData != null)
        {
            Debug.Log("[BattleSystem] 使用 BattleInitializer 的数据初始化战斗");
            StartCoroutine(SetupBattleFromInitializer());
        }
        else
        {
            Debug.Log("[BattleSystem] 使用默认 Prefab 初始化战斗");
        StartCoroutine(SetupBattle());
        }
    }
    public void Update()
    {

    }
    private IEnumerator SetupBattle()
    {
        // 实例化玩家宝可梦
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        playerPokemon = player.GetComponent<PokemonFromData>();
        
        // TODO: 从背包数据初始化玩家宝可梦
        // 临时：使用野生初始化
        playerPokemon.InitializeAsWild();
        
        // 设置玩家宝可梦朝向（朝右）
        SetPokemonFacing(player, true);
        
        // 实例化敌方宝可梦（野生）
        GameObject enemy = Instantiate(enemyPrefab, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        enemyPokemon.InitializeAsWild();
        
        // 设置敌方宝可梦朝向（朝左）
        SetPokemonFacing(enemy, false);
        
        yield return StartCoroutine(TypeDialog("野生的" + enemyPokemon.pokemonName + "跳出来了"));
        playerHUD.SetHUD(playerPokemon);
        enemyHUD.SetHUD(enemyPokemon);
        
        // 设置好感度条
        SetupAffinitySliders();

        yield return new WaitForSeconds(1f);
        
        // 检查绿帽鱼逻辑
        yield return StartCoroutine(CheckGreenHatFishLogic());
        
        // 如果绿帽鱼逃跑，状态会被设置为 Escape
        if (state == BattleState.Escape)
        {
            yield break;
        }
        
        state = BattleState.PlayerTurn;
        yield return StartCoroutine(PlayerTurn());
    }
    
    /// <summary>
    /// 从 BattleInitializer 初始化战斗
    /// </summary>
    private IEnumerator SetupBattleFromInitializer()
    {
        // 实例化玩家宝可梦
        GameObject playerPrefabToUse = GetPlayerPrefab(BattleInitializer.PlayerData);
        GameObject player = Instantiate(playerPrefabToUse, playerPosition.position, Quaternion.identity);
        playerPokemon = player.GetComponent<PokemonFromData>();
        playerPokemon.InitializeAsPlayer(BattleInitializer.PlayerData);
        
        // 设置玩家宝可梦朝向（朝右）
        SetPokemonFacing(player, true);
        
        // 实例化敌方宝可梦（根据 EnemyData 动态选择 Prefab）
        GameObject enemyPrefabToUse = GetEnemyPrefab(BattleInitializer.EnemyData);
        
        GameObject enemy = Instantiate(enemyPrefabToUse, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        
        // 设置敌方宝可梦的数据
        enemyPokemon.data = BattleInitializer.EnemyData;
        enemyPokemon.InitializeAsWild();
        
        // 设置敌方宝可梦朝向（朝左）
        SetPokemonFacing(enemy, false);
        
        Debug.Log($"[BattleSystem] 已生成敌方宝可梦: {BattleInitializer.EnemyData.displayName}，使用 Prefab: {enemyPrefabToUse.name}");
        
        // 清除 BattleInitializer 数据
        BattleInitializer.Clear();
        
        // 显示战斗UI
        yield return StartCoroutine(TypeDialog("野生的" + enemyPokemon.pokemonName + "跳出来了"));
        playerHUD.SetHUD(playerPokemon);
        enemyHUD.SetHUD(enemyPokemon);
        
        // 设置好感度条
        SetupAffinitySliders();

        yield return new WaitForSeconds(1f);
        
        // 检查绿帽鱼逻辑
        yield return StartCoroutine(CheckGreenHatFishLogic());
        
        // 如果绿帽鱼逃跑，状态会被设置为 Escape
        if (state == BattleState.Escape)
        {
            yield break;
        }
        
        state = BattleState.PlayerTurn;
        yield return StartCoroutine(PlayerTurn());
    }
    
    /// <summary>
    /// 获取玩家宝可梦的 Prefab
    /// </summary>
    private GameObject GetPlayerPrefab(PlayerPokemonData playerData)
    {
        // 如果玩家宝可梦的 PokemonData 中设置了专用的 battlePrefab，使用它
        if (playerData != null && playerData.pokemonData != null && playerData.pokemonData.battlePrefab != null)
        {
            Debug.Log($"[BattleSystem] 使用 {playerData.pokemonData.displayName} 的专用 battlePrefab");
            return playerData.pokemonData.battlePrefab;
        }
        
        // 否则使用默认的 playerPrefab
        Debug.LogWarning($"[BattleSystem] {playerData?.pokemonData?.displayName ?? "未知宝可梦"} 没有设置 battlePrefab，使用默认 playerPrefab");
        return playerPrefab;
    }
    
    /// <summary>
    /// 设置宝可梦朝向
    /// </summary>
    /// <param name="pokemon">宝可梦 GameObject</param>
    /// <param name="faceRight">true=朝右（玩家方），false=朝左（敌方）</param>
    private void SetPokemonFacing(GameObject pokemon, bool faceRight)
    {
        if (pokemon == null) return;
        
        // 检查 Prefab 的默认朝向
        PokemonFromData pokemonData = pokemon.GetComponent<PokemonFromData>();
        if (pokemonData == null || pokemonData.data == null) return;
        
        Vector3 scale = pokemon.transform.localScale;
        
        // 玩家宝可梦（Player）默认朝右，其他宝可梦默认朝左
        bool isPlayerPrefab = pokemonData.data.displayName == "玩家" || 
                              pokemonData.data.displayName == "Player" ||
                              pokemonData.data.displayName == "player";
        
        if (isPlayerPrefab)
        {
            // 玩家 Prefab 默认朝右
            if (faceRight)
            {
                // 需要朝右 → 保持原样
                scale.x = Mathf.Abs(scale.x);
                Debug.Log($"[BattleSystem] {pokemon.name} 保持朝右（玩家方），Scale.x = {scale.x}");
            }
            else
            {
                // 需要朝左 → 翻转
                scale.x = Mathf.Abs(scale.x) * -1f;
                Debug.Log($"[BattleSystem] {pokemon.name} 翻转朝左，Scale.x = {scale.x}");
            }
        }
        else
        {
            // 其他宝可梦 Prefab 默认朝左
            if (faceRight)
            {
                // 需要朝右 → 翻转
                scale.x = Mathf.Abs(scale.x) * -1f;
                Debug.Log($"[BattleSystem] {pokemon.name} 翻转朝右，Scale.x = {scale.x}");
            }
            else
            {
                // 需要朝左 → 保持原样
                scale.x = Mathf.Abs(scale.x);
                Debug.Log($"[BattleSystem] {pokemon.name} 保持朝左（敌方），Scale.x = {scale.x}");
            }
        }
        
        pokemon.transform.localScale = scale;
    }
    
    /// <summary>
    /// 获取敌方宝可梦的 Prefab
    /// </summary>
    private GameObject GetEnemyPrefab(PokemonData enemyData)
    {
        // 如果 PokemonData 中设置了专用的 battlePrefab，使用它
        if (enemyData != null && enemyData.battlePrefab != null)
        {
            Debug.Log($"[BattleSystem] 使用 {enemyData.displayName} 的专用 battlePrefab");
            return enemyData.battlePrefab;
        }
        
        // 否则使用默认的 enemyPrefab
        Debug.LogWarning($"[BattleSystem] {enemyData?.displayName ?? "未知宝可梦"} 没有设置 battlePrefab，使用默认 enemyPrefab");
        return enemyPrefab;
    }
    
    /// <summary>
    /// 使用玩家数据初始化战斗（旧方法，保留兼容性）
    /// </summary>
    public void InitializeBattleWithPlayerData(PlayerPokemonData playerData, GameObject wildEnemyPrefab)
    {
        // 实例化玩家宝可梦
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        playerPokemon = player.GetComponent<PokemonFromData>();
        playerPokemon.InitializeAsPlayer(playerData);
        
        // 实例化野生宝可梦
        GameObject enemy = Instantiate(wildEnemyPrefab, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        enemyPokemon.InitializeAsWild();
        
        // 开始战斗
        StartCoroutine(SetupBattleUI());
    }
    
    private IEnumerator SetupBattleUI()
    {
        yield return StartCoroutine(TypeDialog("野生的" + enemyPokemon.pokemonName + "跳出来了"));
        playerHUD.SetHUD(playerPokemon);
        enemyHUD.SetHUD(enemyPokemon);

        yield return new WaitForSeconds(1f);
        state = BattleState.PlayerTurn;
        yield return StartCoroutine(PlayerTurn());
    }
    private IEnumerator PlayerTurn()
    {
        //dialogText.text = playerPokemon.pokemonName + "做什么？";
        yield return StartCoroutine(TypeDialog(playerPokemon.pokemonName + "做什么？"));
    }
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / 30);
        }
    }
    public void SwitchPanel(CanvasGroup target)
    {
        foreach (var cg in _all) SetPanel(cg, false, 0f);
        SetPanel(target, true, 1f);
    }
    private void SetPanel(CanvasGroup cg, bool enable, float alpha)
    {
        cg.alpha = alpha;
        cg.interactable = enable;
        cg.blocksRaycasts = enable;
    }
    private IEnumerator FadeIn(CanvasGroup cg, float duration = 0.2f)
    {
        cg.interactable = cg.blocksRaycasts = true;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    /// <summary>
    /// 点击战斗按钮，显示技能面板
    /// </summary>
    public void OnBattleButton()
    {
        UpdateSkillButtons();
        StartCoroutine(ShowPanelWithFade(skillsPanel));
    }
    
    /// <summary>
    /// 点击道具按钮，显示道具面板
    /// </summary>
    public void OnItemButton()
    {
        UpdateItemButtons();
        StartCoroutine(ShowPanelWithFade(itemsPanel));
    }
    
    /// <summary>
    /// 点击礼物按钮，显示礼物面板
    /// </summary>
    public void OnGiftButton()
    {
        UpdateGiftButtons();
        StartCoroutine(ShowPanelWithFade(giftsPanel));
    }
    
    /// <summary>
    /// 点击返回按钮，回到根面板
    /// </summary>
    public void OnBackButton()
    {
        StartCoroutine(ShowPanelWithFade(rootPanel));
    }
    
    /// <summary>
    /// 显示面板并带淡入效果
    /// </summary>
    private IEnumerator ShowPanelWithFade(CanvasGroup targetPanel)
    {
        // 先隐藏所有面板
        foreach (var cg in _all)
        {
            SetPanel(cg, false, 0f);
        }
        
        // 淡入目标面板
        yield return StartCoroutine(FadeIn(targetPanel, 0.2f));
    }
    
    /// <summary>
    /// 战斗结束（胜利、失败或逃跑后调用）
    /// </summary>
    public void EndBattle()
    {
        // 如果玩家宝可梦是玩家的，同步数据
        if (playerPokemon != null && playerPokemon.IsPlayerPokemon())
        {
            playerPokemon.SyncToPlayerData();
            Debug.Log("玩家宝可梦数据已同步到背包");
        }
        
        // TODO: 保存背包数据到存档
        // TODO: 返回地图场景
    }
    
    /// <summary>
    /// 更新技能按钮显示
    /// </summary>
    private void UpdateSkillButtons()
    {
        // 检查玩家宝可梦是否存在
        if (playerPokemon == null)
        {
            Debug.LogWarning("玩家宝可梦为空，无法更新技能按钮");
            return;
        }
        
        // 获取技能列表
        var skills = playerPokemon.GetSkills();
        
        // 创建技能按钮数组
        GameObject[] skillButtons = new GameObject[] { skillButton1, skillButton2, skillButton3 };
        
        // 遍历三个技能按钮
        for (int i = 0; i < skillButtons.Length; i++)
        {
            // 检查按钮是否存在
            if (skillButtons[i] == null)
            {
                Debug.LogWarning($"技能按钮{i + 1}未赋值！");
                continue;
            }
            
            // 如果技能数量大于当前索引，显示技能
            if (i < skills.Count && skills[i] != null)
            {
                // 显示按钮
                skillButtons[i].SetActive(true);
                
                // 查找Text_Battle子物体
                Transform textTransform = skillButtons[i].transform.Find("Text_Battle");
                if (textTransform != null)
                {
                    TextMeshProUGUI textComponent = textTransform.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        // 设置技能名称
                        textComponent.text = skills[i].skillName;
                    }
                    else
                    {
                        Debug.LogWarning($"技能按钮{i + 1}的Text_Battle没有TextMeshProUGUI组件");
                    }
                }
                else
                {
                    Debug.LogWarning($"技能按钮{i + 1}找不到Text_Battle子物体");
                }
            }
            else
            {
                // 技能不足，隐藏按钮
                skillButtons[i].SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 更新道具按钮显示
    /// </summary>
    private void UpdateItemButtons()
    {
        Debug.Log("[BattleSystem] 开始更新道具按钮...");
        
        // 检查背包系统是否存在
        if (ItemInventory.Instance == null)
        {
            Debug.LogError("[BattleSystem] ItemInventory.Instance 为 null！PermanentManager 可能在场景切换时丢失。");
            Debug.LogError("[BattleSystem] 请确保场景中有 PermanentManager 并挂载了 PermanentManager 脚本！");
            
            // 尝试在场景中查找
            ItemInventory foundInventory = FindObjectOfType<ItemInventory>();
            if (foundInventory != null)
            {
                Debug.LogWarning("[BattleSystem] 在场景中找到了 ItemInventory，但单例未设置。可能是 Awake 顺序问题。");
            }
            return;
        }
        
        Debug.Log($"[BattleSystem] ItemInventory 找到，背包物品数量: {ItemInventory.Instance.bagItems.Count}");
        
        // 获取背包中所有道具类型的物品
        var toolItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsTool())
            {
                toolItems.Add(slotData);
                Debug.Log($"[BattleSystem] 找到道具: {slotData.itemData.itemName} x{slotData.count}");
            }
        }
        
        Debug.Log($"[BattleSystem] 背包中共有 {toolItems.Count} 个道具类型物品");
        
        // 创建道具按钮数组
        GameObject[] itemButtons = new GameObject[] { itemButton1, itemButton2, itemButton3 };
        
        // 遍历三个道具按钮
        for (int i = 0; i < itemButtons.Length; i++)
        {
            // 检查按钮是否存在
            if (itemButtons[i] == null)
            {
                Debug.LogWarning($"[BattleSystem] 道具按钮{i + 1}未赋值！");
                continue;
            }
            
            // 如果道具数量大于当前索引，显示道具
            if (i < toolItems.Count)
            {
                // 显示按钮
                itemButtons[i].SetActive(true);
                
                // 查找Text_Battle子物体
                Transform textTransform = itemButtons[i].transform.Find("Text_Battle");
                if (textTransform != null)
                {
                    TextMeshProUGUI textComponent = textTransform.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        // 设置道具名称
                        textComponent.text = toolItems[i].itemData.itemName;
                        Debug.Log($"[BattleSystem] 道具按钮{i + 1} 设置为: {toolItems[i].itemData.itemName}");
                    }
                }
            }
            else
            {
                // 道具不足，隐藏按钮
                itemButtons[i].SetActive(false);
                Debug.Log($"[BattleSystem] 道具按钮{i + 1} 已隐藏（无道具）");
            }
        }
        
        Debug.Log($"[BattleSystem] 道具按钮更新完成，显示 {Mathf.Min(toolItems.Count, 3)} 个道具");
    }
    
    /// <summary>
    /// 更新礼物按钮显示
    /// </summary>
    private void UpdateGiftButtons()
    {
        Debug.Log("[BattleSystem] 开始更新礼物按钮...");
        
        // 检查背包系统是否存在
        if (ItemInventory.Instance == null)
        {
            Debug.LogError("[BattleSystem] ItemInventory.Instance 为 null！PermanentManager 可能在场景切换时丢失。");
            return;
        }
        
        // 获取背包中所有礼物类型的物品
        var giftItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsGift())
            {
                giftItems.Add(slotData);
                Debug.Log($"[BattleSystem] 找到礼物: {slotData.itemData.itemName} x{slotData.count}");
            }
        }
        
        Debug.Log($"[BattleSystem] 背包中共有 {giftItems.Count} 个礼物类型物品");
        
        // 创建礼物按钮数组
        GameObject[] giftButtons = new GameObject[] { giftButton1, giftButton2, giftButton3 };
        
        // 遍历三个礼物按钮
        for (int i = 0; i < giftButtons.Length; i++)
        {
            // 检查按钮是否存在
            if (giftButtons[i] == null)
            {
                Debug.LogWarning($"[BattleSystem] 礼物按钮{i + 1}未赋值！");
                continue;
            }
            
            // 如果礼物数量大于当前索引，显示礼物
            if (i < giftItems.Count)
            {
                // 显示按钮
                giftButtons[i].SetActive(true);
                
                // 查找Text_Battle子物体
                Transform textTransform = giftButtons[i].transform.Find("Text_Battle");
                if (textTransform != null)
                {
                    TextMeshProUGUI textComponent = textTransform.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        // 设置礼物名称
                        textComponent.text = giftItems[i].itemData.itemName;
                        Debug.Log($"[BattleSystem] 礼物按钮{i + 1} 设置为: {giftItems[i].itemData.itemName}");
                    }
                }
                
                // 添加点击事件（如果按钮有 Button 组件）
                UnityEngine.UI.Button buttonComponent = giftButtons[i].GetComponent<UnityEngine.UI.Button>();
                if (buttonComponent != null)
                {
                    // 移除旧的监听器
                    buttonComponent.onClick.RemoveAllListeners();
                    
                    // 捕获当前的 itemData
                    ItemData itemData = giftItems[i].itemData;
                    
                    // 添加新的监听器
                    buttonComponent.onClick.AddListener(() => UseGift(itemData));
                }
            }
            else
            {
                // 礼物不足，隐藏按钮
                giftButtons[i].SetActive(false);
                Debug.Log($"[BattleSystem] 礼物按钮{i + 1} 已隐藏（无礼物）");
            }
        }
        
        Debug.Log($"[BattleSystem] 礼物按钮更新完成，显示 {Mathf.Min(giftItems.Count, 3)} 个礼物");
    }
    
    // ========== 礼物使用系统 ==========
    
    /// <summary>
    /// 使用礼物（赠送给敌方宝可梦）
    /// </summary>
    /// <param name="item">要使用的礼物</param>
    private void UseGift(ItemData item)
    {
        if (state != BattleState.PlayerTurn)
        {
            Debug.LogWarning("[BattleSystem] 当前不是玩家回合，无法使用礼物");
            return;
        }
        
        if (item == null)
        {
            Debug.LogError("[BattleSystem] 礼物数据为空！");
            return;
        }
        
        if (enemyPokemon == null)
        {
            Debug.LogError("[BattleSystem] 敌方宝可梦为空！");
            return;
        }
        
        Debug.Log($"[BattleSystem] 使用礼物：{item.itemName}");
        
        // 开始礼物赠送流程
        StartCoroutine(UseGiftCoroutine(item));
    }
    
    /// <summary>
    /// 使用礼物协程
    /// </summary>
    private IEnumerator UseGiftCoroutine(ItemData item)
    {
        // 隐藏礼物面板
        yield return StartCoroutine(ShowPanelWithFade(rootPanel));
        
        // 调用礼物系统处理赠送
        if (giftSystem != null)
        {
            yield return StartCoroutine(giftSystem.GiveGiftToPokemon(item, enemyPokemon));
        }
        else
        {
            Debug.LogError("[BattleSystem] 礼物系统未初始化！");
        }
        
        // 刷新礼物按钮（数量可能已改变）
        UpdateGiftButtons();
        
        // 切换到敌方回合
        state = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurn());
    }
    
    // ========== 技能使用系统 ==========
    
    /// <summary>
    /// 使用技能1
    /// </summary>
    public void OnSkillButton1()
    {
        UseSkill(0);
    }
    
    /// <summary>
    /// 使用技能2
    /// </summary>
    public void OnSkillButton2()
    {
        UseSkill(1);
    }
    
    /// <summary>
    /// 使用技能3
    /// </summary>
    public void OnSkillButton3()
    {
        UseSkill(2);
    }
    
    /// <summary>
    /// 使用技能（核心方法）
    /// </summary>
    /// <param name="skillIndex">技能索引（0-2）</param>
    private void UseSkill(int skillIndex)
    {
        // 检查游戏状态
        if (state != BattleState.PlayerTurn)
        {
            Debug.LogWarning($"[BattleSystem] 当前不是玩家回合，无法使用技能");
            return;
        }
        
        // 检查宝可梦
        if (playerPokemon == null || enemyPokemon == null)
        {
            Debug.LogError("[BattleSystem] 宝可梦为空，无法使用技能");
            return;
        }
        
        // 获取技能列表
        var skills = playerPokemon.GetSkills();
        if (skillIndex < 0 || skillIndex >= skills.Count)
        {
            Debug.LogError($"[BattleSystem] 技能索引越界: {skillIndex}");
            return;
        }
        
        // 获取技能
        Skill skill = skills[skillIndex];
        
        // 检查PP
        if (!skill.HasPP)
        {
            StartCoroutine(TypeDialog($"{skill.skillName} 的PP不足！"));
            return;
        }
        
        // 使用技能（消耗PP）
        if (!skill.Use())
        {
            StartCoroutine(TypeDialog("技能使用失败！"));
            return;
        }
        
        Debug.Log($"[BattleSystem] {playerPokemon.pokemonName} 使用了 {skill.skillName}（类型：{skill.skillType}）");
        
        // 开始技能使用流程（包含动画）
        StartCoroutine(PlayerUseSkillCoroutine(skill));
    }
    
    /// <summary>
    /// 玩家使用技能协程（包含动画和伤害结算）
    /// </summary>
    private IEnumerator PlayerUseSkillCoroutine(Skill skill)
    {
        // 隐藏技能面板
        yield return StartCoroutine(ShowPanelWithFade(rootPanel));
        
        // 显示技能使用文本
        yield return StartCoroutine(TypeDialog($"{playerPokemon.pokemonName} 使用了 {skill.skillName}！"));
        yield return new WaitForSeconds(0.5f);
        
        // 播放技能动画
        if (animationManager != null)
        {
            yield return StartCoroutine(animationManager.PlaySkillAnimation(
                skill.skillType, 
                playerPokemon, 
                enemyPokemon
            ));
        }
        else
        {
            Debug.LogWarning("[BattleSystem] 动画管理器未初始化");
            yield return new WaitForSeconds(1f);
        }
        
        // 根据技能类型处理效果
        switch (skill.skillType)
        {
            case SkillType.TackleAttack:
            case SkillType.WhipAttack:
                // 攻击技能：计算并造成伤害
                int damage = CalculateDamage(playerPokemon, enemyPokemon, skill);
                
                if (damage > 0)
                {
                    enemyPokemon.TakeDamage(damage);
                    yield return StartCoroutine(TypeDialog($"造成了 {damage} 点伤害！"));
                    enemyHUD.SetHP(enemyPokemon.currentHP);
                    yield return new WaitForSeconds(1f);
                    
                    // 检查敌方是否死亡
                    if (enemyPokemon.currentHP <= 0)
                    {
                        state = BattleState.Won;
                        yield return StartCoroutine(TypeDialog($"野生的 {enemyPokemon.pokemonName} 倒下了！"));
                        yield return new WaitForSeconds(2f);
                        EndBattle();
                        yield break;
                    }
                }
                break;
            
            case SkillType.Heal:
                // 治疗技能：恢复HP（治疗自己）
                int healAmount = skill.power;
                if (healAmount > 0)
                {
                    int actualHeal = playerPokemon.Heal(healAmount);
                    yield return StartCoroutine(TypeDialog($"{playerPokemon.pokemonName} 恢复了 {actualHeal} HP！"));
                    playerHUD.SetHP(playerPokemon.currentHP);
                    yield return new WaitForSeconds(1f);
                }
                break;
            
            case SkillType.DefenseStance:
                // 防御技能：提升防御力
                int defenseBoost = playerPokemon.BoostDefense();
                yield return StartCoroutine(TypeDialog(
                    $"{playerPokemon.pokemonName} 进入了防御姿态！\n防御力提升了 {defenseBoost} 点！"));
                yield return new WaitForSeconds(1f);
                break;
            
            case SkillType.Taunt:
                // 嘲讽技能：强制敌人攻击自己（多人战斗用）
                enemyPokemon.SetTaunted(playerPokemon);
                yield return StartCoroutine(TypeDialog(
                    $"{playerPokemon.pokemonName} 嘲讽了 {enemyPokemon.pokemonName}！"));
                yield return new WaitForSeconds(1f);
                break;
        }
        
        // 切换到敌方回合
        state = BattleState.EnemyTurn;
        StartCoroutine(EnemyTurn());
    }
    
    /// <summary>
    /// 敌方回合
    /// </summary>
    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(TypeDialog($"{enemyPokemon.pokemonName} 的回合"));
        yield return new WaitForSeconds(1f);
        
        // 敌方AI选择技能
        var enemySkills = enemyPokemon.GetSkills();
        if (enemySkills.Count > 0)
        {
            Skill enemySkill = null;
            
            // 检查是否有形态变化组件，且设置了强制技能（如被捆绑的M猪只能用嘲讽）
            PokemonTransformation transformation = enemyPokemon.GetComponent<PokemonTransformation>();
            if (transformation != null)
            {
                enemySkill = transformation.GetForcedSkill(enemySkills);
                if (enemySkill != null)
                {
                    Debug.Log($"[BattleSystem] {enemyPokemon.pokemonName} 被强制使用技能：{enemySkill.skillName}");
                }
            }
            
            // 如果没有强制技能，随机选择
            if (enemySkill == null)
            {
                enemySkill = enemySkills[Random.Range(0, enemySkills.Count)];
            }
            
            if (enemySkill.Use())
            {
                yield return StartCoroutine(TypeDialog($"{enemyPokemon.pokemonName} 使用了 {enemySkill.skillName}！"));
                yield return new WaitForSeconds(0.5f);
                
                // 播放敌方动画
                if (animationManager != null)
                {
                    yield return StartCoroutine(animationManager.PlaySkillAnimation(
                        enemySkill.skillType,
                        enemyPokemon,
                        playerPokemon
                    ));
                }
                
                // 结算伤害
                if (enemySkill.skillType == SkillType.TackleAttack || enemySkill.skillType == SkillType.WhipAttack)
                {
                    int damage = CalculateDamage(enemyPokemon, playerPokemon, enemySkill);
                    playerPokemon.TakeDamage(damage);
                    
                    yield return StartCoroutine(TypeDialog($"你的宝可梦受到了 {damage} 点伤害！"));
                    playerHUD.SetHP(playerPokemon.currentHP);
                    yield return new WaitForSeconds(1f);
                    
                    // 检查玩家是否死亡
                    if (playerPokemon.currentHP <= 0)
                    {
                        state = BattleState.Lost;
                        yield return StartCoroutine(TypeDialog($"{playerPokemon.pokemonName} 倒下了！"));
                        yield return new WaitForSeconds(2f);
                        EndBattle();
                        yield break;
                    }
                }
            }
        }
        
        // 回到玩家回合
        state = BattleState.PlayerTurn;
        yield return StartCoroutine(PlayerTurn());
    }
    
    /// <summary>
    /// 计算伤害（新公式）
    /// 基础伤害 = 技能威力 * (攻击者攻击力/100) * [(100 - 防御者防御力)/100]
    /// </summary>
    private int CalculateDamage(PokemonFromData attacker, PokemonFromData defender, Skill skill)
    {
        // 威力为0的技能不造成伤害
        if (skill.power <= 0)
        {
            return 0;
        }
        
        // 新公式：技能威力 * (攻击力/100) * [(100 - 防御力)/100]
        float attackMultiplier = attacker.pokemonAttack / 100f;
        float defenseMultiplier = (100f - defender.pokemonDefense) / 100f;
        
        // 防御力不能超过100，如果超过则视为100（完全防御）
        if (defender.pokemonDefense >= 100)
        {
            defenseMultiplier = 0f;
        }
        
        float baseDamage = skill.power * attackMultiplier * defenseMultiplier;
        
        // 添加随机波动（±10%）
        float randomFactor = Random.Range(0.9f, 1.1f);
        baseDamage *= randomFactor;
        
        // 四舍五入取整，至少造成1点伤害
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage));
        
        Debug.Log($"[伤害计算] {attacker.pokemonName} → {defender.pokemonName}：" +
                  $"威力{skill.power} × 攻击{attacker.pokemonAttack}/100 × " +
                  $"(100-防御{defender.pokemonDefense})/100 = {damage}点伤害");
        
        return damage;
    }
    
    // ==================== 绿帽鱼系统 ====================
    
    /// <summary>
    /// 检查绿帽鱼逻辑（逃跑和偷看）
    /// </summary>
    private IEnumerator CheckGreenHatFishLogic()
    {
        bool isEnemyGreenHatFish = enemyPokemon != null && enemyPokemon.IsGreenHatFish();
        bool isGreenHatFishCaptured = AffinityManager.Instance.IsGreenHatFishCaptured();
        
        // 情况1：敌人是绿帽鱼，且未被捕捉，好感度不足100
        if (isEnemyGreenHatFish && !isGreenHatFishCaptured && AffinityManager.Instance.WillGreenHatFishEscape())
        {
            yield return StartCoroutine(TypeDialog("绿帽鱼不屑一顾并逃走了"));
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(ShowEndBattlePanel());
            state = BattleState.Escape; // 设置状态为逃跑
            yield break;
        }
        
        // 情况2：敌人不是绿帽鱼，且绿帽鱼未被捕捉
        if (!isEnemyGreenHatFish && !isGreenHatFishCaptured)
        {
            SpawnGreenHatFish(); // 生成绿帽鱼
            yield return StartCoroutine(TypeDialog("绿帽鱼正在偷看"));
            yield return new WaitForSeconds(1.5f);
        }
    }
    
    /// <summary>
    /// 在 GreenHatFishStation 位置生成绿帽鱼
    /// </summary>
    private void SpawnGreenHatFish()
    {
        if (greenHatFishInstance != null)
        {
            Debug.LogWarning("[BattleSystem] 绿帽鱼已经存在");
            return;
        }
        
        if (greenHatFishStation == null)
        {
            Debug.LogWarning("[BattleSystem] 未设置 GreenHatFishStation");
            return;
        }
        
        if (greenHatFishData == null || greenHatFishData.battlePrefab == null)
        {
            Debug.LogWarning("[BattleSystem] 未设置绿帽鱼的 PokemonData 或 battlePrefab");
            return;
        }
        
        // 生成绿帽鱼
        greenHatFishInstance = Instantiate(greenHatFishData.battlePrefab, greenHatFishStation.position, Quaternion.identity);
        PokemonFromData greenHatFishPokemon = greenHatFishInstance.GetComponent<PokemonFromData>();
        if (greenHatFishPokemon != null)
        {
            greenHatFishPokemon.data = greenHatFishData;
            greenHatFishPokemon.InitializeAsWild();
            
            // 绑定绿帽鱼好感度条
            if (greenHatAffinitySlider != null)
            {
                greenHatFishPokemon.SetAffinitySlider(greenHatAffinitySlider);
                greenHatAffinitySlider.gameObject.SetActive(true);
                Debug.Log("[BattleSystem] 偷看的绿帽鱼好感度条已绑定");
            }
        }
        
        // 设置大小为 0.5
        Vector3 scale = greenHatFishInstance.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * 0.5f;
        scale.y = Mathf.Abs(scale.y) * 0.5f;
        greenHatFishInstance.transform.localScale = scale;
        
        // 设置朝向（根据站位判断）
        SetPokemonFacing(greenHatFishInstance, greenHatFishStation.position.x > 0);
        
        Debug.Log("[BattleSystem] 绿帽鱼已生成在 GreenHatFishStation（缩放0.5）");
    }
    
    /// <summary>
    /// 移除绿帽鱼
    /// </summary>
    private void RemoveGreenHatFish()
    {
        if (greenHatFishInstance != null)
        {
            Destroy(greenHatFishInstance);
            greenHatFishInstance = null;
            Debug.Log("[BattleSystem] 绿帽鱼已移除");
        }
    }
    
    /// <summary>
    /// 显示结束战斗面板
    /// </summary>
    private IEnumerator ShowEndBattlePanel()
    {
        // 隐藏其他面板
        foreach (var cg in _all)
        {
            SetPanel(cg, false, 0f);
        }
        
        // 显示 EndBattlePanel
        CanvasGroup endBattlePanel = GameObject.Find("EndBattlePanel")?.GetComponent<CanvasGroup>();
        if (endBattlePanel != null)
        {
            endBattlePanel.alpha = 1;
            endBattlePanel.blocksRaycasts = true;
            endBattlePanel.interactable = true;
        }
        else
        {
            Debug.LogWarning("[BattleSystem] 未找到 EndBattlePanel！");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 返回 UI_Test 场景
    /// </summary>
    public void ReturnToUITest()
    {
        Debug.Log("[BattleSystem] 返回 UI_Test 场景");
        UnityEngine.SceneManagement.SceneManager.LoadScene("UI_Test");
    }
    
    /// <summary>
    /// 战斗结束时重置好感度（绿帽鱼除外）
    /// </summary>
    private void ResetAffinityOnBattleEnd()
    {
        if (playerPokemon != null)
        {
            playerPokemon.ResetAffinity();
        }
        
        if (enemyPokemon != null)
        {
            enemyPokemon.ResetAffinity();
        }
        
        // 移除偷看的绿帽鱼
        RemoveGreenHatFish();
        
        Debug.Log("[BattleSystem] 战斗结束，好感度已重置（绿帽鱼除外）");
    }
    
    /// <summary>
    /// 设置好感度条（战斗开始时调用）
    /// </summary>
    private void SetupAffinitySliders()
    {
        if (enemyPokemon == null)
        {
            Debug.LogWarning("[BattleSystem] enemyPokemon 为空，无法设置好感度条");
            return;
        }
        
        // 判断敌人是否是绿帽鱼
        bool isEnemyGreenHatFish = enemyPokemon.IsGreenHatFish();
        
        if (isEnemyGreenHatFish)
        {
            // 敌人是绿帽鱼 → 显示绿帽鱼好感度条，隐藏普通敌人好感度条
            if (greenHatAffinitySlider != null)
            {
                enemyPokemon.SetAffinitySlider(greenHatAffinitySlider);
                greenHatAffinitySlider.gameObject.SetActive(true);
                Debug.Log("[BattleSystem] 绿帽鱼好感度条已启用");
            }
            else
            {
                Debug.LogWarning("[BattleSystem] greenHatAffinitySlider 未设置！");
            }
            
            if (enemyAffinitySlider != null)
            {
                enemyAffinitySlider.gameObject.SetActive(false);
            }
        }
        else
        {
            // 敌人是普通宝可梦 → 显示普通敌人好感度条，隐藏绿帽鱼好感度条
            if (enemyAffinitySlider != null)
            {
                enemyPokemon.SetAffinitySlider(enemyAffinitySlider);
                enemyAffinitySlider.gameObject.SetActive(true);
                Debug.Log($"[BattleSystem] 敌人（{enemyPokemon.pokemonName}）好感度条已启用");
            }
            else
            {
                Debug.LogWarning("[BattleSystem] enemyAffinitySlider 未设置！");
            }
            
            if (greenHatAffinitySlider != null)
            {
                greenHatAffinitySlider.gameObject.SetActive(false);
            }
        }
    }
}
