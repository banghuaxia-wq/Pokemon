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
    private void Awake()
    {
        _all = new[] { rootPanel, skillsPanel, itemsPanel, giftsPanel };
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
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        playerPokemon = player.GetComponent<PokemonFromData>();
        GameObject enemy = Instantiate(enemyPrefab, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        //dialogText.text = "野生的" + enemyPokemon.pokemonName + "跳出来了";
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

    // public void OnBattleButton()
    // {
    //     if (state != BattleState.PlayerTurn) return;
    //     StartCoroutine(Skill_1());
    // }

    private IEnumerator Skill_1()
    {
        //造成伤害
        yield return new WaitForSeconds(1f);
        //检查敌人是否被击败
        //根据结果切换阶段
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

    public void OnBattleButton() { SwitchPanel(skillsPanel); }
    public void OnItemButton()   { SwitchPanel(itemsPanel); }
    public void OnGiftButton()   { SwitchPanel(giftsPanel); }
    public void OnBackButton()   { SwitchPanel(rootPanel); }
}
