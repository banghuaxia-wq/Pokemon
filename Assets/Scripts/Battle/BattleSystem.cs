using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
    public void Start()
    {
        state = BattleState.Start;
        SetupBattle();
    }
    public void Update()
    {

    }
    private void SetupBattle()
    {
        state = BattleState.PlayerTurn;
        GameObject player = Instantiate(playerPrefab, playerPosition.position, Quaternion.identity);
        playerPokemon = player.GetComponent<PokemonFromData>();
        GameObject enemy = Instantiate(enemyPrefab, enemyPosition.position, Quaternion.identity);
        enemyPokemon = enemy.GetComponent<PokemonFromData>();
        dialogText.text = "野生的" + enemyPokemon.pokemonName + "跳出来了";
        playerHUD.SetHUD(playerPokemon);
        enemyHUD.SetHUD(enemyPokemon);

    }
    private void PlayerTurn()
    {
        state = BattleState.EnemyTurn;
    }
    private void EnemyTurn()
    {
        state = BattleState.PlayerTurn;
    }
}
