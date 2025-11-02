using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Slider hpSlider;

    public void SetHUD(PokemonFromData pokemon)
    {
        nameText.text = pokemon.pokemonName;
        hpSlider.maxValue = pokemon.pokemonHP;
        hpSlider.value = pokemon.currentHP;
    }

    public void SetHP(int hp)
    {
        hpSlider.value = hp;

    }
}
