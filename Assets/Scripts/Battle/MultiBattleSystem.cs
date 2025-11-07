using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 多人战斗系统 - 管理多个宝可梦同时战斗
/// </summary>
public class MultiBattleSystem : MonoBehaviour
{
    [Header("对话系统")]
    [SerializeField] private TextMeshProUGUI dialogText;
    
    [Header("玩家位置")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private Transform playerMPigPosition;
    [SerializeField] private Transform playerSsnakePosition;
    [SerializeField] private Transform playerGreenHatFishPosition;
    
    [Header("敌人位置")]
    [SerializeField] private Transform enemyMPigPosition;
    [SerializeField] private Transform enemySsnakePosition;
    [SerializeField] private Transform enemyGreenHatFishPosition;
    [SerializeField] private Transform enemyBossPosition;
    
    [Header("玩家方HUD")]
    [SerializeField] private BattleHUD playerHUD;
    [SerializeField] private BattleHUD playerMPigHUD;
    [SerializeField] private BattleHUD playerSsnakeHUD;
    [SerializeField] private BattleHUD playerGreenHatFishHUD;
    
    [Header("敌方HUD")]
    [SerializeField] private BattleHUD enemyMPigHUD;
    [SerializeField] private BattleHUD enemySsnakeHUD;
    [SerializeField] private BattleHUD enemyGreenHatFishHUD;
    [SerializeField] private BattleHUD enemyBossHUD;
    
    [Header("宝可梦Prefab")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject mPigPrefab;
    [SerializeField] private GameObject ssnakePrefab;
    [SerializeField] private GameObject greenHatFishPrefab;
    [SerializeField] private GameObject bossPrefab;
    
    [Header("UI面板")]
    [SerializeField] private CanvasGroup rootPanel;
    [SerializeField] private CanvasGroup skillsPanel;
    [SerializeField] private CanvasGroup itemsPanel;
    [SerializeField] private CanvasGroup giftsPanel;
    [SerializeField] private CanvasGroup endBattlePanel;
    public GameObject endBattlePanelLose;
    [SerializeField] private CanvasGroup targetSelectionPanel; // 目标选择面板
    private CanvasGroup[] allPanels;
    
    [Header("结束面板")]
    [SerializeField] private TextMeshProUGUI endBattleText;
    [SerializeField] private Button endButton;
    
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
    
    [Header("目标选择按钮")]
    [SerializeField] private GameObject targetButton1;
    [SerializeField] private GameObject targetButton2;
    [SerializeField] private GameObject targetButton3;
    [SerializeField] private GameObject targetButton4;
    
    // 战斗实例
    private PokemonFromData playerPokemon;
    private PokemonFromData mPigPokemon;
    private PokemonFromData ssnakePokemon;
    private PokemonFromData greenHatFishPokemon;
    private PokemonFromData bossPokemon;
    
    // 战斗顺序队列
    private List<BattleUnit> battleOrder = new List<BattleUnit>();
    private int currentTurnIndex = 0;
    
    // 战斗状态
    private bool isBattleActive = false;
    
    // 当前轮到的单位
    private BattleUnit currentUnit;
    private bool playerActionCompleted;
    
    private void Awake()
    {
        allPanels = new[] { rootPanel, skillsPanel, itemsPanel, giftsPanel, endBattlePanel, targetSelectionPanel };
        
        // 初始化：隐藏所有面板
        foreach (var panel in allPanels)
        {
            if (panel != null)
            {
                SetPanel(panel, false, 0f);
            }
        }
    }
    
    private void Start()
    {
        endBattlePanelLose.SetActive(false);
        // 检查EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("[MultiBattleSystem] ⚠️ 场景中没有EventSystem！UI无法响应点击！");
        }
        else
        {
            Debug.Log($"[MultiBattleSystem] ✅ EventSystem 存在: {eventSystem.name}");
        }
        
        // 检查是否有来自 Claw 抓取的战斗初始化数据
        if (BattleInitializer.IsMultiBattle && BattleInitializer.PlayerData != null && BattleInitializer.EnemyData != null)
        {
            Debug.Log("[MultiBattleSystem] 使用 BattleInitializer 的数据初始化多人战斗");
            StartCoroutine(SetupMultiBattleFromInitializer());
        }
        else
        {
            Debug.Log("[MultiBattleSystem] 使用默认设置初始化多人战斗");
            StartCoroutine(SetupMultiBattle());
        }
    }
    
    /// <summary>
    /// 从BattleInitializer初始化多人战斗
    /// </summary>
    private IEnumerator SetupMultiBattleFromInitializer()
    {
        yield return StartCoroutine(TypeDialog("Boss战斗开始！"));
        yield return new WaitForSeconds(1f);
        
        // 检测玩家队伍
        List<PlayerPokemonData> playerParty = PlayerInventory.Instance.pokemonParty;
        
        // 标记哪些宝可梦在玩家队伍
        bool hasMPig = playerParty.Any(p => p.pokemonData.displayName.Contains("M猪") || p.pokemonData.displayName.Contains("MPig"));
        bool hasSsnake = playerParty.Any(p => p.pokemonData.displayName.Contains("S蛇") || p.pokemonData.displayName.Contains("SSnake"));
        bool hasGreenHatFish = playerParty.Any(p => p.pokemonData.displayName.Contains("绿帽鱼") || p.pokemonData.displayName.Contains("GreenHatFish"));
        
        // 使用BattleInitializer的玩家数据生成玩家宝可梦
        yield return StartCoroutine(SpawnPlayerPokemonFromInitializer());
        
        // 生成M猪
        if (hasMPig)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("MPig", true, playerMPigPosition, mPigPrefab, playerMPigHUD, (p) => mPigPokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("MPig", false, enemyMPigPosition, mPigPrefab, enemyMPigHUD, (p) => mPigPokemon = p));
        }
        
        // 生成S蛇
        if (hasSsnake)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("Ssnake", true, playerSsnakePosition, ssnakePrefab, playerSsnakeHUD, (p) => ssnakePokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("Ssnake", false, enemySsnakePosition, ssnakePrefab, enemySsnakeHUD, (p) => ssnakePokemon = p));
        }
        
        // 生成绿帽鱼
        if (hasGreenHatFish)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("GreenHatFish", true, playerGreenHatFishPosition, greenHatFishPrefab, playerGreenHatFishHUD, (p) => greenHatFishPokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("GreenHatFish", false, enemyGreenHatFishPosition, greenHatFishPrefab, enemyGreenHatFishHUD, (p) => greenHatFishPokemon = p));
        }
        
        // 生成Boss（使用BattleInitializer的Boss数据）
        yield return StartCoroutine(SpawnBossFromInitializer());
        
        // 清除BattleInitializer数据
        BattleInitializer.Clear();
        
        // 设置战斗顺序
        SetupBattleOrder(hasMPig, hasSsnake, hasGreenHatFish);
        
        yield return new WaitForSeconds(1f);
        
        // 开始战斗循环
        isBattleActive = true;
        yield return StartCoroutine(BattleLoop());
    }
    
    /// <summary>
    /// 初始化多人战斗（默认方式）
    /// </summary>
    private IEnumerator SetupMultiBattle()
    {
        yield return StartCoroutine(TypeDialog("战斗开始！"));
        yield return new WaitForSeconds(1f);
        
        // 检测玩家队伍
        List<PlayerPokemonData> playerParty = PlayerInventory.Instance.pokemonParty;
        
        // 标记哪些宝可梦在玩家队伍
        bool hasMPig = playerParty.Any(p => p.pokemonData.displayName.Contains("M猪") || p.pokemonData.displayName.Contains("MPig"));
        bool hasSsnake = playerParty.Any(p => p.pokemonData.displayName.Contains("S蛇") || p.pokemonData.displayName.Contains("SSnake"));
        bool hasGreenHatFish = playerParty.Any(p => p.pokemonData.displayName.Contains("绿帽鱼") || p.pokemonData.displayName.Contains("GreenHatFish"));
        
        // 生成玩家宝可梦
        yield return StartCoroutine(SpawnPlayerPokemon());
        
        // 生成M猪
        if (hasMPig)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("MPig", true, playerMPigPosition, mPigPrefab, playerMPigHUD, (p) => mPigPokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("MPig", false, enemyMPigPosition, mPigPrefab, enemyMPigHUD, (p) => mPigPokemon = p));
        }
        
        // 生成S蛇
        if (hasSsnake)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("Ssnake", true, playerSsnakePosition, ssnakePrefab, playerSsnakeHUD, (p) => ssnakePokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("Ssnake", false, enemySsnakePosition, ssnakePrefab, enemySsnakeHUD, (p) => ssnakePokemon = p));
        }
        
        // 生成绿帽鱼
        if (hasGreenHatFish)
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("GreenHatFish", true, playerGreenHatFishPosition, greenHatFishPrefab, playerGreenHatFishHUD, (p) => greenHatFishPokemon = p));
        }
        else
        {
            yield return StartCoroutine(SpawnPokemonWithDialog("GreenHatFish", false, enemyGreenHatFishPosition, greenHatFishPrefab, enemyGreenHatFishHUD, (p) => greenHatFishPokemon = p));
        }
        
        // 生成Boss（总是在敌方）
        yield return StartCoroutine(SpawnPokemonWithDialog("Boss", false, enemyBossPosition, bossPrefab, enemyBossHUD, (p) => bossPokemon = p));
        
        // 设置战斗顺序
        SetupBattleOrder(hasMPig, hasSsnake, hasGreenHatFish);
        
        yield return new WaitForSeconds(1f);
        
        // 开始战斗循环
        isBattleActive = true;
        yield return StartCoroutine(BattleLoop());
    }
    
    /// <summary>
    /// 从BattleInitializer生成玩家宝可梦
    /// </summary>
    private IEnumerator SpawnPlayerPokemonFromInitializer()
    {
        if (BattleInitializer.PlayerData == null)
        {
            Debug.LogError("[MultiBattleSystem] BattleInitializer.PlayerData 为空！");
            yield break;
        }
        
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        
        // 设置大小为0.3
        player.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        
        playerPokemon = player.GetComponent<PokemonFromData>();
        playerPokemon.InitializeAsPlayer(BattleInitializer.PlayerData);
        SetPokemonFacing(player, true);
        
        playerHUD.SetHUD(playerPokemon);
        
        battleOrder.Add(new BattleUnit
        {
            pokemon = playerPokemon,
            hud = playerHUD,
            isPlayerControlled = true,
            name = "Player"
        });
        
        yield return StartCoroutine(TypeDialog($"{playerPokemon.pokemonName} 加入战斗！"));
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 从BattleInitializer生成Boss宝可梦
    /// </summary>
    private IEnumerator SpawnBossFromInitializer()
    {
        if (BattleInitializer.EnemyData == null)
        {
            Debug.LogError("[MultiBattleSystem] BattleInitializer.EnemyData 为空！");
            yield break;
        }
        
        GameObject boss = Instantiate(bossPrefab, enemyBossPosition.position, Quaternion.identity);
        
        // 设置大小为0.3
        boss.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        
        bossPokemon = boss.GetComponent<PokemonFromData>();
        bossPokemon.data = BattleInitializer.EnemyData;
        bossPokemon.InitializeAsWild();
        SetPokemonFacing(boss, false);
        
        if (enemyBossHUD != null)
        {
            enemyBossHUD.SetHUD(bossPokemon);
        }
        
        yield return StartCoroutine(TypeDialog($"敌方 {bossPokemon.pokemonName} 加入战斗！"));
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 生成玩家宝可梦（默认方式）
    /// </summary>
    private IEnumerator SpawnPlayerPokemon()
    {
        PlayerPokemonData playerData = PlayerInventory.Instance.GetFirstAlivePokemon();
        if (playerData == null)
        {
            Debug.LogError("玩家没有存活的宝可梦！");
            yield break;
        }
        
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        
        // 设置大小为0.3
        player.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        
        playerPokemon = player.GetComponent<PokemonFromData>();
        playerPokemon.InitializeAsPlayer(playerData);
        SetPokemonFacing(player, true);
        
        playerHUD.SetHUD(playerPokemon);
        
        battleOrder.Add(new BattleUnit
        {
            pokemon = playerPokemon,
            hud = playerHUD,
            isPlayerControlled = true,
            name = "Player"
        });
        
        yield return StartCoroutine(TypeDialog($"{playerPokemon.pokemonName} 加入战斗！"));
        yield return new WaitForSeconds(0.5f);
    }
    
    /// <summary>
    /// 生成宝可梦并返回实例
    /// </summary>
    private PokemonFromData SpawnPokemonInstance(string pokemonType, bool isPlayerSide, Transform position, GameObject prefab, BattleHUD hud)
    {
        if (prefab == null || position == null)
        {
            Debug.LogError($"无法生成{pokemonType}：Prefab或位置为空");
            return null;
        }
        
        GameObject instance = Instantiate(prefab, position.position, Quaternion.identity);
        
        // 设置大小为0.3
        instance.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
        
        PokemonFromData pokemon = instance.GetComponent<PokemonFromData>();
        pokemon.InitializeAsWild();
        
        SetPokemonFacing(instance, isPlayerSide);
        
        if (hud != null)
        {
            hud.SetHUD(pokemon);
        }
        
        return pokemon;
    }
    
    /// <summary>
    /// 生成宝可梦（带对话）
    /// </summary>
    private IEnumerator SpawnPokemonWithDialog(string pokemonType, bool isPlayerSide, Transform position, GameObject prefab, BattleHUD hud, System.Action<PokemonFromData> callback)
    {
        PokemonFromData pokemon = SpawnPokemonInstance(pokemonType, isPlayerSide, position, prefab, hud);
        
        if (pokemon != null)
        {
            string side = isPlayerSide ? "己方" : "敌方";
            yield return StartCoroutine(TypeDialog($"{side}{pokemon.pokemonName} 加入战斗！"));
            yield return new WaitForSeconds(0.5f);
        }
        
        callback?.Invoke(pokemon);
    }
    
    /// <summary>
    /// 设置战斗顺序
    /// </summary>
    private void SetupBattleOrder(bool hasMPig, bool hasSsnake, bool hasGreenHatFish)
    {
        // Player已经在battleOrder中了
        
        // M猪
        if (mPigPokemon != null)
        {
            battleOrder.Add(new BattleUnit
            {
                pokemon = mPigPokemon,
                hud = hasMPig ? playerMPigHUD : enemyMPigHUD,
                isPlayerControlled = hasMPig,
                name = "MPig"
            });
        }
        
        // S蛇
        if (ssnakePokemon != null)
        {
            battleOrder.Add(new BattleUnit
            {
                pokemon = ssnakePokemon,
                hud = hasSsnake ? playerSsnakeHUD : enemySsnakeHUD,
                isPlayerControlled = hasSsnake,
                name = "Ssnake"
            });
        }
        
        // 绿帽鱼
        if (greenHatFishPokemon != null)
        {
            battleOrder.Add(new BattleUnit
            {
                pokemon = greenHatFishPokemon,
                hud = hasGreenHatFish ? playerGreenHatFishHUD : enemyGreenHatFishHUD,
                isPlayerControlled = hasGreenHatFish,
                name = "GreenHatFish"
            });
        }
        
        // Boss
        if (bossPokemon != null)
        {
            battleOrder.Add(new BattleUnit
            {
                pokemon = bossPokemon,
                hud = enemyBossHUD,
                isPlayerControlled = false,
                name = "Boss"
            });
        }
        
        Debug.Log($"战斗顺序设置完成，共{battleOrder.Count}个单位");
    }
    
    /// <summary>
    /// 战斗循环
    /// </summary>
    private IEnumerator BattleLoop()
    {
        while (isBattleActive)
        {
            // 获取当前回合单位
            currentUnit = battleOrder[currentTurnIndex];
            
            // 检查当前单位是否存活
            if (currentUnit.pokemon.currentHP <= 0)
            {
                // 跳过已倒下的宝可梦
                NextTurn();
                yield return new WaitForSeconds(0.3f);
                continue;
            }
            
            yield return StartCoroutine(TypeDialog($"轮到 {currentUnit.pokemon.pokemonName}！"));
            yield return new WaitForSeconds(0.5f);
            
            // 判断是玩家控制还是AI
            if (currentUnit.isPlayerControlled)
            {
                // 玩家回合
                yield return StartCoroutine(PlayerControlledTurn(currentUnit));
            }
            else
            {
                // AI回合
                yield return StartCoroutine(AITurn(currentUnit));
            }
            
            // 检查战斗是否结束
            if (CheckBattleEnd())
            {
                break;
            }
            
            // 下一回合
            NextTurn();
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    /// <summary>
    /// 玩家控制的回合
    /// </summary>
    private IEnumerator PlayerControlledTurn(BattleUnit unit)
    {
        yield return StartCoroutine(TypeDialog($"{unit.pokemon.pokemonName} 做什么？"));
        
        // 显示根面板，等待玩家选择
        Debug.Log($"[MultiBattleSystem] 显示rootPanel，等待玩家操作");
        SwitchPanel(rootPanel);
        
        // 调试：检查所有按钮状态
        yield return new WaitForSeconds(0.1f); // 等待面板完全显示
        DebugCheckButtons();
        
        // 等待玩家选择技能和目标，或通过礼物/道具结束回合
        playerActionCompleted = false;
        yield return new WaitUntil(() => !isBattleActive || playerActionCompleted || (currentUnit.selectedSkill != null && currentUnit.selectedTarget != null));

        if (!playerActionCompleted && currentUnit.selectedSkill != null)
        {
            yield return StartCoroutine(ExecuteSkill(currentUnit, currentUnit.selectedSkill));
            currentUnit.selectedSkill = null;
        }

        if (playerActionCompleted)
        {
            currentUnit.selectedSkill = null;
            currentUnit.selectedTarget = null;
            playerActionCompleted = false;
        }
    }
    
    /// <summary>
    /// 调试：检查所有按钮状态
    /// </summary>
    private void DebugCheckButtons()
    {
        Debug.Log("========== 按钮状态检查 ==========");
        
        // 检查rootPanel
        if (rootPanel != null)
        {
            Debug.Log($"✅ RootPanel: " +
                $"active={rootPanel.gameObject.activeInHierarchy}, " +
                $"alpha={rootPanel.alpha}, " +
                $"interactable={rootPanel.interactable}, " +
                $"blocksRaycasts={rootPanel.blocksRaycasts}");
            
            // 获取所有按钮
            UnityEngine.UI.Button[] buttons = rootPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            Debug.Log($"🔍 找到 {buttons.Length} 个按钮:");
            
            foreach (var btn in buttons)
            {
                Debug.Log($"  - {btn.name}: " +
                    $"active={btn.gameObject.activeInHierarchy}, " +
                    $"enabled={btn.enabled}, " +
                    $"interactable={btn.interactable}, " +
                    $"onClick事件数={btn.onClick.GetPersistentEventCount()}");
                
                // 检查是否有监听器
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    Debug.LogWarning($"⚠️ {btn.name} 没有绑定OnClick事件！");
                }
            }
        }
        else
        {
            Debug.LogError("❌ rootPanel 为 null！");
        }
        
        // 检查dialogText所在的面板是否挡住了按钮
        if (dialogText != null)
        {
            Transform dialogParent = dialogText.transform.parent;
            while (dialogParent != null)
            {
                CanvasGroup cg = dialogParent.GetComponent<CanvasGroup>();
                if (cg != null && cg.blocksRaycasts)
                {
                    Debug.LogWarning($"⚠️ DialogPanel层级 {dialogParent.name} 的blocksRaycasts=true，可能挡住按钮！");
                }
                dialogParent = dialogParent.parent;
            }
        }
        
        Debug.Log("==================================");
    }
    
    /// <summary>
    /// AI回合
    /// </summary>
    private IEnumerator AITurn(BattleUnit unit)
    {
        // AI随机选择一个技能
        if (unit.pokemon.data.skills.Count == 0)
        {
            yield return StartCoroutine(TypeDialog($"{unit.pokemon.pokemonName} 没有可用技能！"));
            yield break;
        }
        
        SkillData selectedSkill = unit.pokemon.data.skills[Random.Range(0, unit.pokemon.data.skills.Count)];
        
        yield return StartCoroutine(ExecuteSkill(unit, selectedSkill));
    }
    
    /// <summary>
    /// 执行技能
    /// </summary>
    private IEnumerator ExecuteSkill(BattleUnit attacker, SkillData skill)
    {
        if (attacker == null || attacker.pokemon == null)
        {
            Debug.LogError("[MultiBattleSystem] ExecuteSkill 收到空的攻击者，终止此次技能执行");
            yield break;
        }

        if (skill == null)
        {
            Debug.LogWarning($"[MultiBattleSystem] {attacker.pokemon.pokemonName} 没有可用技能或技能数据为空，跳过回合");
            yield return StartCoroutine(TypeDialog($"{attacker.pokemon.pokemonName} 犹豫了，没有使用任何技能。"));
            yield break;
        }

        yield return StartCoroutine(TypeDialog($"{attacker.pokemon.pokemonName} 使用了 {skill.skillName}！"));
        yield return new WaitForSeconds(1f);
        
        // 确定目标
        BattleUnit target = null;
        
        // 如果攻击者已经选择了目标（玩家控制），使用选定的目标
        if (attacker.selectedTarget != null)
        {
            target = attacker.selectedTarget;
            Debug.Log($"[MultiBattleSystem] 使用玩家选择的目标：{target.pokemon.pokemonName}");
        }
        else
        {
            // 否则随机选择一个敌对阵营的存活目标（AI）
        List<BattleUnit> possibleTargets = GetPossibleTargets(attacker);
            
            if (possibleTargets.Count == 0)
            {
                yield return StartCoroutine(TypeDialog("没有可攻击的目标！"));
                yield break;
            }
            
            target = possibleTargets[Random.Range(0, possibleTargets.Count)];
            Debug.Log($"[MultiBattleSystem] AI随机选择目标：{target.pokemon.pokemonName}");
        }
        
        // 清除已选择的目标
        attacker.selectedTarget = null;
        if (target == null || target.pokemon == null)
        {
            Debug.LogWarning("[MultiBattleSystem] 目标为空或没有宝可梦数据，终止此次技能执行");
            yield return StartCoroutine(TypeDialog("技能没有命中任何目标！"));
            yield break;
        }

        // 计算伤害
        int damage = CalculateDamage(attacker.pokemon, target.pokemon, skill);
        target.pokemon.TakeDamage(damage);
        
        yield return StartCoroutine(TypeDialog($"{target.pokemon.pokemonName} 受到了 {damage} 点伤害！"));
        
        // 更新HUD
        if (target.hud != null)
        {
            target.hud.SetHP(target.pokemon.currentHP);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 检查目标是否倒下
        if (target.pokemon.currentHP <= 0)
        {
            yield return StartCoroutine(TypeDialog($"{target.pokemon.pokemonName} 倒下了！"));
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// 获取可能的目标
    /// </summary>
    private List<BattleUnit> GetPossibleTargets(BattleUnit attacker)
    {
        List<BattleUnit> targets = new List<BattleUnit>();
        
        foreach (var unit in battleOrder)
        {
            // 跳过自己
            if (unit == attacker) continue;
            
            // 跳过已倒下的
            if (unit.pokemon.currentHP <= 0) continue;
            
            // 玩家控制的攻击敌方，敌方攻击玩家方
            if (attacker.isPlayerControlled != unit.isPlayerControlled)
            {
                targets.Add(unit);
            }
        }
        
        return targets;
    }
    
    /// <summary>
    /// 计算伤害
    /// </summary>
    private int CalculateDamage(PokemonFromData attacker, PokemonFromData defender, SkillData skill)
    {
        // 简化的伤害计算
        int baseDamage = skill.power;
        int attackStat = attacker.data.baseAttack;
        int defenseStat = defender.data.baseDefense;
        
        float damageMultiplier = (float)attackStat / (defenseStat + 1);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * damageMultiplier));
        
        return finalDamage;
    }
    
    /// <summary>
    /// 检查战斗是否结束
    /// </summary>
    private bool CheckBattleEnd()
    {
        // 检查玩家方是否全灭
        bool playerSideAlive = false;
        // 检查敌方是否全灭
        bool enemySideAlive = false;
        
        foreach (var unit in battleOrder)
        {
            if (unit.pokemon.currentHP > 0)
            {
                if (unit.isPlayerControlled)
                {
                    playerSideAlive = true;
                }
                else
                {
                    enemySideAlive = true;
                }
            }
        }
        
        if (!playerSideAlive)
        {
            isBattleActive = false;
            endBattlePanelLose.SetActive(true);
            //ShowEndBattlePanel("战斗失败...");
            return true;
        }
        
        if (!enemySideAlive)
        {
            isBattleActive = false;
            
            ShowEndBattlePanel("战斗胜利！");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 下一回合
    /// </summary>
    private void NextTurn()
    {
        currentTurnIndex = (currentTurnIndex + 1) % battleOrder.Count;
    }
    
    /// <summary>
    /// 设置宝可梦朝向
    /// </summary>
    private void SetPokemonFacing(GameObject pokemon, bool faceRight)
    {
        if (pokemon == null) return;
        
        // 检查 Prefab 的默认朝向
        PokemonFromData pokemonData = pokemon.GetComponent<PokemonFromData>();
        if (pokemonData == null || pokemonData.data == null) return;
        
        Vector3 scale = pokemon.transform.localScale;
        
        // 判断是否是玩家Prefab（只有"玩家"这个特殊Prefab默认朝右）
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
            }
            else
            {
                // 需要朝左 → 翻转
                scale.x = Mathf.Abs(scale.x) * -1f;
            }
        }
        else
        {
            // 其他宝可梦 Prefab 默认朝左
            if (faceRight)
            {
                // 需要朝右（玩家方） → 翻转
                scale.x = Mathf.Abs(scale.x) * -1f;
            }
            else
            {
                // 需要朝左（敌方） → 保持原样
                scale.x = Mathf.Abs(scale.x);
            }
        }
        
        pokemon.transform.localScale = scale;
    }
    
    /// <summary>
    /// 打字机效果显示对话
    /// </summary>
    private IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / 30);
        }
    }
    
    /// <summary>
    /// 切换面板
    /// </summary>
    private void SwitchPanel(CanvasGroup target)
    {
        Debug.Log($"[MultiBattleSystem] SwitchPanel called, target: {(target != null ? target.name : "null")}");
        
        foreach (var cg in allPanels)
        {
            if (cg != null)
            {
                SetPanel(cg, false, 0f);
            }
        }
        
        if (target != null)
        {
            SetPanel(target, true, 1f);
            Debug.Log($"[MultiBattleSystem] Panel {target.name} enabled: alpha={target.alpha}, interactable={target.interactable}, blocksRaycasts={target.blocksRaycasts}");
        }
    }
    
    private void SetPanel(CanvasGroup cg, bool enable, float alpha)
    {
        if (cg == null) return;
        
        cg.alpha = alpha;
        cg.interactable = enable;
        cg.blocksRaycasts = enable;
    }
    
    // ========== 技能按钮回调 ==========
    
    /// <summary>
    /// 点击技能1
    /// </summary>
    public void OnSkill1Clicked()
    {
        if (currentUnit != null && currentUnit.pokemon.data.skills.Count > 0)
        {
            currentUnit.selectedSkill = currentUnit.pokemon.data.skills[0];
            // 显示目标选择面板
            SetupTargetSelection();
            SwitchPanel(targetSelectionPanel);
        }
    }
    
    /// <summary>
    /// 点击技能2
    /// </summary>
    public void OnSkill2Clicked()
    {
        if (currentUnit != null && currentUnit.pokemon.data.skills.Count > 1)
        {
            currentUnit.selectedSkill = currentUnit.pokemon.data.skills[1];
            // 显示目标选择面板
            SetupTargetSelection();
            SwitchPanel(targetSelectionPanel);
        }
    }
    
    /// <summary>
    /// 点击技能3
    /// </summary>
    public void OnSkill3Clicked()
    {
        if (currentUnit != null && currentUnit.pokemon.data.skills.Count > 2)
        {
            currentUnit.selectedSkill = currentUnit.pokemon.data.skills[2];
            // 显示目标选择面板
            SetupTargetSelection();
            SwitchPanel(targetSelectionPanel);
        }
    }
    
    /// <summary>
    /// 点击战斗按钮（显示技能面板）
    /// </summary>
    public void OnBattleButtonClicked()
    {
        Debug.Log($"[MultiBattleSystem] OnBattleButtonClicked called! currentUnit: {(currentUnit != null ? currentUnit.name : "null")}, isPlayerControlled: {(currentUnit != null ? currentUnit.isPlayerControlled.ToString() : "null")}");
        
        if (currentUnit != null && currentUnit.isPlayerControlled)
        {
            SetupSkillButtons(currentUnit.pokemon);
            SwitchPanel(skillsPanel);
        }
        else
        {
            Debug.LogWarning($"[MultiBattleSystem] OnBattleButtonClicked - 条件不满足！currentUnit is null: {currentUnit == null}, isPlayerControlled: {currentUnit?.isPlayerControlled}");
        }
    }
    
    /// <summary>
    /// 点击道具按钮（显示道具面板）
    /// </summary>
    public void OnItemButtonClicked()
    {
        Debug.Log($"[MultiBattleSystem] OnItemButtonClicked called!");
        
        if (currentUnit != null && currentUnit.isPlayerControlled)
        {
            SetupItemButtons();
            SwitchPanel(itemsPanel);
        }
        else
        {
            Debug.LogWarning($"[MultiBattleSystem] OnItemButtonClicked - 条件不满足！");
        }
    }
    
    /// <summary>
    /// 点击礼物按钮（显示礼物面板）
    /// </summary>
    public void OnGiftButtonClicked()
    {
        Debug.Log($"[MultiBattleSystem] OnGiftButtonClicked called!");
        
        if (currentUnit != null && currentUnit.isPlayerControlled)
        {
            SetupGiftButtons();
            SwitchPanel(giftsPanel);
        }
        else
        {
            Debug.LogWarning($"[MultiBattleSystem] OnGiftButtonClicked - 条件不满足！");
        }
    }
    
    /// <summary>
    /// 点击逃跑按钮
    /// </summary>
    public void OnEscapeButtonClicked()
    {
        Debug.Log($"[MultiBattleSystem] OnEscapeButtonClicked called!");
        StartCoroutine(TryEscape());
    }
    
    /// <summary>
    /// 尝试逃跑
    /// </summary>
    private IEnumerator TryEscape()
    {
        yield return StartCoroutine(TypeDialog("你逃跑了！"));
        yield return new WaitForSeconds(1f);
        
        isBattleActive = false;
        ShowEndBattlePanel("逃跑成功");
    }
    
    /// <summary>
    /// 返回按钮（返回主面板）
    /// </summary>
    public void OnBackButtonClicked()
    {
        SwitchPanel(rootPanel);
    }
    
    /// <summary>
    /// 设置技能按钮（参考BattleSystem.UpdateSkillButtons）
    /// </summary>
    private void SetupSkillButtons(PokemonFromData pokemon)
    {
        if (pokemon == null)
        {
            Debug.LogWarning("[MultiBattleSystem] 宝可梦为空，无法更新技能按钮");
            return;
        }
        
        // 获取技能列表
        var skills = pokemon.GetSkills();
        
        // 创建技能按钮数组
        GameObject[] skillButtons = new GameObject[] { skillButton1, skillButton2, skillButton3 };
        
        // 遍历三个技能按钮
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] == null)
            {
                Debug.LogWarning($"[MultiBattleSystem] 技能按钮{i + 1}未赋值！");
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
                        Debug.Log($"[MultiBattleSystem] 技能按钮{i + 1}: {skills[i].skillName}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MultiBattleSystem] 技能按钮{i + 1}的Text_Battle没有TextMeshProUGUI组件");
                    }
                }
                else
                {
                    Debug.LogWarning($"[MultiBattleSystem] 技能按钮{i + 1}找不到Text_Battle子物体");
                }
            }
            else
            {
                // 技能不足，隐藏按钮
                skillButtons[i].SetActive(false);
            }
        }
    }
    
    // ========== 道具按钮功能 ==========
    
    /// <summary>
    /// 设置道具按钮（参考BattleSystem.UpdateItemButtons）
    /// </summary>
    private void SetupItemButtons()
    {
        Debug.Log("[MultiBattleSystem] 开始更新道具按钮...");
        
        // 检查背包系统是否存在
        if (ItemInventory.Instance == null)
        {
            Debug.LogError("[MultiBattleSystem] ItemInventory.Instance 为 null！");
            return;
        }
        
        Debug.Log($"[MultiBattleSystem] ItemInventory 找到，背包物品数量: {ItemInventory.Instance.bagItems.Count}");
        
        // 获取背包中所有道具类型的物品
        var toolItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsTool())
            {
                toolItems.Add(slotData);
                Debug.Log($"[MultiBattleSystem] 找到道具: {slotData.itemData.itemName} x{slotData.count}");
            }
        }
        
        Debug.Log($"[MultiBattleSystem] 背包中共有 {toolItems.Count} 个道具类型物品");
        
        // 创建道具按钮数组
        GameObject[] itemButtons = new GameObject[] { itemButton1, itemButton2, itemButton3 };
        
        // 遍历三个道具按钮
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i] == null)
            {
                Debug.LogWarning($"[MultiBattleSystem] 道具按钮{i + 1}未赋值！");
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
                        // 设置道具名称和数量
                        textComponent.text = $"{toolItems[i].itemData.itemName} x{toolItems[i].count}";
                        Debug.Log($"[MultiBattleSystem] 道具按钮{i + 1}: {toolItems[i].itemData.itemName} x{toolItems[i].count}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MultiBattleSystem] 道具按钮{i + 1}的Text_Battle没有TextMeshProUGUI组件");
                    }
                }
                else
                {
                    Debug.LogWarning($"[MultiBattleSystem] 道具按钮{i + 1}找不到Text_Battle子物体");
                }
            }
            else
            {
                // 道具不足，隐藏按钮
                itemButtons[i].SetActive(false);
                Debug.Log($"[MultiBattleSystem] 道具按钮{i + 1} 已隐藏（无道具）");
            }
        }
        
        Debug.Log($"[MultiBattleSystem] 道具按钮更新完成，显示 {Mathf.Min(toolItems.Count, 3)} 个道具");
    }
    
    /// <summary>
    /// 使用道具1
    /// </summary>
    public void OnItem1Clicked()
    {
        UseItem(0);
    }
    
    /// <summary>
    /// 使用道具2
    /// </summary>
    public void OnItem2Clicked()
    {
        UseItem(1);
    }
    
    /// <summary>
    /// 使用道具3
    /// </summary>
    public void OnItem3Clicked()
    {
        UseItem(2);
    }
    
    /// <summary>
    /// 使用道具
    /// </summary>
    private void UseItem(int itemIndex)
    {
        if (ItemInventory.Instance == null || currentUnit == null) return;
        
        // 获取背包中所有道具类型的物品
        var toolItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsTool())
            {
                toolItems.Add(slotData);
            }
        }
        
        if (itemIndex >= 0 && itemIndex < toolItems.Count)
        {
            var itemSlot = toolItems[itemIndex];
            if (itemSlot != null && itemSlot.itemData != null && itemSlot.count > 0)
            {
                // 使用道具（例如回血）
                if (itemSlot.itemData.IsTool() && itemSlot.itemData.healAmount > 0)
                {
                    int healAmount = itemSlot.itemData.healAmount;
                    currentUnit.pokemon.Heal(healAmount);
                    
                    if (currentUnit.hud != null)
                    {
                        currentUnit.hud.SetHP(currentUnit.pokemon.currentHP);
                    }
                    
                    // 减少道具数量
                    ItemInventory.Instance.RemoveItemByID(itemSlot.itemData.itemID, 1);
                    
                    StartCoroutine(TypeDialog($"使用了{itemSlot.itemData.itemName}，恢复了{healAmount}HP！"));
                    
                    Debug.Log($"[MultiBattleSystem] 使用道具：{itemSlot.itemData.itemName}，恢复HP：{healAmount}");
                    
                    // 结束回合
                    currentUnit.selectedSkill = null;
                    currentUnit.selectedTarget = null;
                    playerActionCompleted = true;
                    SwitchPanel(null);
                }
            }
        }
    }
    
    // ========== 礼物按钮功能 ==========
    
    /// <summary>
    /// 设置礼物按钮（参考BattleSystem.UpdateGiftButtons）
    /// </summary>
    private void SetupGiftButtons()
    {
        Debug.Log("[MultiBattleSystem] 开始更新礼物按钮...");
        
        // 检查背包系统是否存在
        if (ItemInventory.Instance == null)
        {
            Debug.LogError("[MultiBattleSystem] ItemInventory.Instance 为 null！");
            return;
        }
        
        // 获取背包中所有礼物类型的物品
        var giftItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsGift())
            {
                giftItems.Add(slotData);
                Debug.Log($"[MultiBattleSystem] 找到礼物: {slotData.itemData.itemName} x{slotData.count}");
            }
        }
        
        Debug.Log($"[MultiBattleSystem] 背包中共有 {giftItems.Count} 个礼物类型物品");
        
        // 创建礼物按钮数组
        GameObject[] giftButtons = new GameObject[] { giftButton1, giftButton2, giftButton3 };
        
        // 遍历三个礼物按钮
        for (int i = 0; i < giftButtons.Length; i++)
        {
            if (giftButtons[i] == null)
            {
                Debug.LogWarning($"[MultiBattleSystem] 礼物按钮{i + 1}未赋值！");
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
                        // 设置礼物名称和数量
                        textComponent.text = $"{giftItems[i].itemData.itemName} x{giftItems[i].count}";
                        Debug.Log($"[MultiBattleSystem] 礼物按钮{i + 1}: {giftItems[i].itemData.itemName} x{giftItems[i].count}");
                    }
                    else
                    {
                        Debug.LogWarning($"[MultiBattleSystem] 礼物按钮{i + 1}的Text_Battle没有TextMeshProUGUI组件");
                    }
                }
                else
                {
                    Debug.LogWarning($"[MultiBattleSystem] 礼物按钮{i + 1}找不到Text_Battle子物体");
                }
            }
            else
            {
                // 礼物不足，隐藏按钮
                giftButtons[i].SetActive(false);
                Debug.Log($"[MultiBattleSystem] 礼物按钮{i + 1} 已隐藏（无礼物）");
            }
        }
        
        Debug.Log($"[MultiBattleSystem] 礼物按钮更新完成，显示 {Mathf.Min(giftItems.Count, 3)} 个礼物");
    }
    
    /// <summary>
    /// 赠送礼物1
    /// </summary>
    public void OnGift1Clicked()
    {
        GiveGift(0);
    }
    
    /// <summary>
    /// 赠送礼物2
    /// </summary>
    public void OnGift2Clicked()
    {
        GiveGift(1);
    }
    
    /// <summary>
    /// 赠送礼物3
    /// </summary>
    public void OnGift3Clicked()
    {
        GiveGift(2);
    }
    
    /// <summary>
    /// 赠送礼物
    /// </summary>
    private void GiveGift(int giftIndex)
    {
        if (ItemInventory.Instance == null || currentUnit == null) return;
        
        // 获取背包中所有礼物类型的物品
        var giftItems = new System.Collections.Generic.List<ItemSlotData>();
        foreach (var slotData in ItemInventory.Instance.bagItems)
        {
            if (slotData.itemData != null && slotData.count > 0 && slotData.itemData.IsGift())
            {
                giftItems.Add(slotData);
            }
        }
        
        if (giftIndex >= 0 && giftIndex < giftItems.Count)
        {
            var giftSlot = giftItems[giftIndex];
            if (giftSlot != null && giftSlot.itemData != null && giftSlot.count > 0)
            {
                // 选择一个敌方目标
                List<BattleUnit> enemies = GetPossibleTargets(currentUnit);
                if (enemies.Count > 0)
                {
                    BattleUnit target = enemies[0]; // 简化：选择第一个敌人
                    
                    // 固定好感度增加值（根据礼物名称）
                    int affinityIncrease = 10; // 默认增加10点好感度
                    if (giftSlot.itemData.itemName.Contains("花") || giftSlot.itemData.itemName.Contains("Flower"))
                    {
                        affinityIncrease = 10;
                    }
                    else if (giftSlot.itemData.itemName.Contains("巧克力") || giftSlot.itemData.itemName.Contains("Chocolate"))
                    {
                        affinityIncrease = 15;
                    }
                    else if (giftSlot.itemData.itemName.Contains("宝石") || giftSlot.itemData.itemName.Contains("Gem"))
                    {
                        affinityIncrease = 20;
                    }
                    
                    if (target.pokemon != null)
                    {
                        // 增加好感度
                        target.pokemon.AddAffinity(affinityIncrease);
                        
                        StartCoroutine(TypeDialog($"赠送了{giftSlot.itemData.itemName}，{target.pokemon.pokemonName}好感度+{affinityIncrease}！"));
                        Debug.Log($"[MultiBattleSystem] 赠送礼物：{giftSlot.itemData.itemName}，好感度增加：{affinityIncrease}");
                    }
                    
                    // 减少礼物数量
                    ItemInventory.Instance.RemoveItemByID(giftSlot.itemData.itemID, 1);
                    
                    // 结束回合
                    currentUnit.selectedSkill = null;
                    currentUnit.selectedTarget = null;
                    playerActionCompleted = true;
                    SwitchPanel(null);
                }
            }
        }
    }
    
    // ========== 目标选择功能 ==========
    
    /// <summary>
    /// 设置目标选择按钮（固定位置：Button1=Boss, Button2=M猪, Button3=S蛇, Button4=绿帽鱼）
    /// </summary>
    private void SetupTargetSelection()
    {
        Debug.Log("[MultiBattleSystem] 设置目标选择按钮...");
        
        if (currentUnit == null)
        {
            Debug.LogWarning("[MultiBattleSystem] 当前单位为空！");
            return;
        }
        
        // 获取所有敌方目标
        List<BattleUnit> possibleTargets = GetPossibleTargets(currentUnit);
        
        // 查找各个宝可梦（如果存在且在敌方阵营）
        BattleUnit bossTarget = possibleTargets.Find(t => t.name == "Boss");
        BattleUnit mpigTarget = possibleTargets.Find(t => t.name == "MPig");
        BattleUnit ssnakeTarget = possibleTargets.Find(t => t.name == "Ssnake");
        BattleUnit greenHatTarget = possibleTargets.Find(t => t.name == "GreenHatFish");
        
        // 按照固定位置设置按钮
        // TargetButton1 = Boss
        SetupSingleTargetButton(targetButton1, bossTarget, "Boss");
        
        // TargetButton2 = M猪
        SetupSingleTargetButton(targetButton2, mpigTarget, "M猪");
        
        // TargetButton3 = S蛇
        SetupSingleTargetButton(targetButton3, ssnakeTarget, "S蛇");
        
        // TargetButton4 = 绿帽鱼
        SetupSingleTargetButton(targetButton4, greenHatTarget, "绿帽鱼");
        
        int activeCount = (bossTarget != null ? 1 : 0) + 
                          (mpigTarget != null ? 1 : 0) + 
                          (ssnakeTarget != null ? 1 : 0) + 
                          (greenHatTarget != null ? 1 : 0);
        
        Debug.Log($"[MultiBattleSystem] 目标选择按钮更新完成，显示 {activeCount} 个目标");
    }
    
    /// <summary>
    /// 设置单个目标按钮
    /// </summary>
    private void SetupSingleTargetButton(GameObject button, BattleUnit target, string pokemonName)
    {
        if (button == null)
        {
            Debug.LogWarning($"[MultiBattleSystem] {pokemonName} 按钮未赋值！");
            return;
        }
        
        // 如果目标存在，显示按钮
        if (target != null)
        {
            // 显示按钮
            button.SetActive(true);
            
            // 查找Text_Battle子物体
            Transform textTransform = button.transform.Find("Text_Battle");
            if (textTransform != null)
            {
                TextMeshProUGUI textComponent = textTransform.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    // 设置目标名称和HP信息
                    textComponent.text = $"{target.pokemon.pokemonName} (HP:{target.pokemon.currentHP}/{target.pokemon.pokemonHP})";
                    Debug.Log($"[MultiBattleSystem] {pokemonName} 按钮: {target.pokemon.pokemonName}");
                }
            }
            
            // 绑定点击事件（移除旧的监听器，添加新的）
            Button buttonComponent = button.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                BattleUnit capturedTarget = target; // 捕获目标引用
                buttonComponent.onClick.AddListener(() => OnTargetSelectedDirect(capturedTarget));
            }
        }
        else
        {
            // 目标不存在，隐藏按钮
            button.SetActive(false);
            Debug.Log($"[MultiBattleSystem] {pokemonName} 按钮已隐藏（不在敌方阵营或已倒下）");
        }
    }
    
    /// <summary>
    /// 选择目标1
    /// </summary>
    public void OnTarget1Clicked()
    {
        OnTargetSelected(0);
    }
    
    /// <summary>
    /// 选择目标2
    /// </summary>
    public void OnTarget2Clicked()
    {
        OnTargetSelected(1);
    }
    
    /// <summary>
    /// 选择目标3
    /// </summary>
    public void OnTarget3Clicked()
    {
        OnTargetSelected(2);
    }
    
    /// <summary>
    /// 选择目标4
    /// </summary>
    public void OnTarget4Clicked()
    {
        OnTargetSelected(3);
    }
    
    /// <summary>
    /// 选择目标（通过索引，保留用于手动绑定）
    /// </summary>
    private void OnTargetSelected(int targetIndex)
    {
        if (currentUnit == null)
        {
            Debug.LogWarning("[MultiBattleSystem] 当前单位为空！");
            return;
        }
        
        // 获取所有可能的目标
        List<BattleUnit> possibleTargets = GetPossibleTargets(currentUnit);
        
        // 按照固定顺序排列目标：Boss -> M猪 -> S蛇 -> 绿帽鱼
        List<BattleUnit> orderedTargets = new List<BattleUnit>();
        BattleUnit bossTarget = possibleTargets.Find(t => t.name == "Boss");
        if (bossTarget != null) orderedTargets.Add(bossTarget);
        BattleUnit mpigTarget = possibleTargets.Find(t => t.name == "MPig");
        if (mpigTarget != null) orderedTargets.Add(mpigTarget);
        BattleUnit ssnakeTarget = possibleTargets.Find(t => t.name == "Ssnake");
        if (ssnakeTarget != null) orderedTargets.Add(ssnakeTarget);
        BattleUnit greenHatTarget = possibleTargets.Find(t => t.name == "GreenHatFish");
        if (greenHatTarget != null) orderedTargets.Add(greenHatTarget);
        
        if (targetIndex >= 0 && targetIndex < orderedTargets.Count)
        {
            currentUnit.selectedTarget = orderedTargets[targetIndex];
            Debug.Log($"[MultiBattleSystem] 选择了目标：{currentUnit.selectedTarget.pokemon.pokemonName}");
            
            // 隐藏所有面板（触发技能执行）
            SwitchPanel(null);
        }
        else
        {
            Debug.LogWarning($"[MultiBattleSystem] 无效的目标索引：{targetIndex}");
        }
    }
    
    /// <summary>
    /// 选择目标（直接传入目标引用）
    /// </summary>
    private void OnTargetSelectedDirect(BattleUnit target)
    {
        if (currentUnit == null)
        {
            Debug.LogWarning("[MultiBattleSystem] 当前单位为空！");
            return;
        }
        
        if (target != null)
        {
            currentUnit.selectedTarget = target;
            Debug.Log($"[MultiBattleSystem] 选择了目标：{currentUnit.selectedTarget.pokemon.pokemonName}");
            
            // 隐藏所有面板（触发技能执行）
            SwitchPanel(null);
        }
        else
        {
            Debug.LogWarning($"[MultiBattleSystem] 目标为空！");
        }
    }
    
    // ========== 结束面板功能 ==========
    
    /// <summary>
    /// 显示战斗结束面板
    /// </summary>
    private void ShowEndBattlePanel(string message)
    {
        Debug.Log($"[MultiBattleSystem] 显示战斗结束面板：{message}");
        
        // 隐藏所有其他面板
        foreach (var cg in allPanels)
        {
            if (cg != endBattlePanel)
            {
                SetPanel(cg, false, 0f);
            }
        }
        
        // 设置结束文本
        if (endBattleText != null)
        {
            endBattleText.text = message;
        }
        else
        {
            Debug.LogWarning("[MultiBattleSystem] endBattleText 未赋值！");
        }
        
        // 显示结束面板
        if (endBattlePanel != null)
        {
            SetPanel(endBattlePanel, true, 1f);
        }
        else
        {
            Debug.LogError("[MultiBattleSystem] endBattlePanel 未赋值！");
        }
    }
    
    /// <summary>
    /// 点击结束按钮（返回场景）
    /// </summary>
    public void OnEndButtonClicked()
    {
        Debug.Log("[MultiBattleSystem] 返回 UI_Test 场景");
        UnityEngine.SceneManagement.SceneManager.LoadScene("UI_Test");
    }
}

/// <summary>
/// 战斗单位数据结构
/// </summary>
[System.Serializable]
public class BattleUnit
{
    public PokemonFromData pokemon;
    public BattleHUD hud;
    public bool isPlayerControlled;
    public string name;
    public SkillData selectedSkill; // 玩家选择的技能
    public BattleUnit selectedTarget; // 玩家选择的目标
}

