using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单个宝可梦槽位UI控制
/// </summary>
public class PokemonUISlot : MonoBehaviour
{
	[Header("UI引用")]
	[Tooltip("默认背景")]
	public GameObject defaultBg;
	
	[Tooltip("选中背景")]
	public GameObject selectedBg;
	
	[Tooltip("宝可梦图标")]
	public Image icon;
	
	[Header("数据")]
	private PlayerPokemonData pokemonData;
	private int slotIndex;
	private Button button;
	
	private void Awake()
	{
		// 获取Button组件
		button = GetComponent<Button>();
		if (button == null)
		{
			button = gameObject.AddComponent<Button>();
		}
		
		// 绑定点击事件
		button.onClick.AddListener(OnSlotClicked);
	}
	
	/// <summary>
	/// 更新槽位显示
	/// </summary>
	public void UpdateSlot(PlayerPokemonData data, int index)
	{
		pokemonData = data;
		slotIndex = index;
		
		if (data == null || data.pokemonData == null)
		{
			// 空槽位，隐藏
			gameObject.SetActive(false);
		}
		else
		{
			// 有宝可梦，显示
			gameObject.SetActive(true);
			
			// 更新图标
			if (icon != null && data.pokemonData.icon != null)
			{
				icon.sprite = data.pokemonData.icon;
				icon.enabled = true;
			}
			else if (icon != null)
			{
				icon.enabled = false;
			}
			
			// 默认显示Default背景
			SetSelected(false);
		}
	}
	
	/// <summary>
	/// 设置选中状态
	/// </summary>
	public void SetSelected(bool isSelected)
	{
		if (defaultBg != null)
			defaultBg.SetActive(!isSelected);
		
		if (selectedBg != null)
			selectedBg.SetActive(isSelected);
	}
	
	/// <summary>
	/// 点击槽位
	/// </summary>
	private void OnSlotClicked()
	{
		if (pokemonData == null) return;
		
		// 通知管理器
		PokemonMenuManager manager = GetComponentInParent<PokemonMenuManager>();
		if (manager != null)
		{
			manager.OnPokemonSlotClicked(slotIndex, this);
		}
	}
	
	/// <summary>
	/// 获取该槽位的宝可梦数据
	/// </summary>
	public PlayerPokemonData GetPokemonData()
	{
		return pokemonData;
	}
}

