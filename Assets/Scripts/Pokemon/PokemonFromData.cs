using UnityEngine;

public class PokemonFromData : MonoBehaviour
{
	[Header("数据引用")]
	public PokemonData data;

	[Header("当前状态")]
	[SerializeField] private int _currentHP;
	public int currentHP => _currentHP;

	public string pokemonName => data != null ? data.displayName : "Unknown";
	public int pokemonHP => data != null ? data.baseHP : 0;
	public int pokemonAttack => data != null ? data.baseAttack : 0;
	public int pokemonDefense => data != null ? data.baseDefense : 0;

	private void Awake()
	{
		if (data == null)
		{
			Debug.LogError($"PokemonFromData on {gameObject.name} has no PokemonData assigned.");
		}
		_currentHP = pokemonHP;
	}
}
