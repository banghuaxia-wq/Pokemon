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
    private void Awake()
    {
        _all = new[] { rootPanel, skillsPanel, itemsPanel, giftsPanel };
        
        // 检查所有面板是否都已赋值
        if (rootPanel == null) Debug.LogError("BattleSystem: rootPanel 未赋值！");
        if (skillsPanel == null) Debug.LogError("BattleSystem: skillsPanel 未赋值！");
        if (itemsPanel == null) Debug.LogError("BattleSystem: itemsPanel 未赋值！");
        if (giftsPanel == null) Debug.LogError("BattleSystem: giftsPanel 未赋值！");
    }
    public void Start()
    {
        state = BattleState.Start;
        StartCoroutine(SetupBattle());
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
        
        // 实例化敌方宝可梦（野生）
        GameObject enemy = Instantiate(enemyPrefab, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        enemyPokemon.InitializeAsWild();
        
        yield return StartCoroutine(TypeDialog("野生的" + enemyPokemon.pokemonName + "跳出来了"));
        playerHUD.SetHUD(playerPokemon);
        enemyHUD.SetHUD(enemyPokemon);

        yield return new WaitForSeconds(1f);
        state = BattleState.PlayerTurn;
        yield return StartCoroutine(PlayerTurn());
    }
    
    /// <summary>
    /// 使用玩家数据初始化战斗
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
    private IEnumerator EnemyTurn()
    {
        yield return StartCoroutine(TypeDialog(enemyPokemon.pokemonName + "做什么？"));
    }
    private IEnumerator TypeDialog(string dialog)
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
        StartCoroutine(ShowPanelWithFade(itemsPanel));
    }
    
    /// <summary>
    /// 点击礼物按钮，显示礼物面板
    /// </summary>
    public void OnGiftButton()
    {
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
}
