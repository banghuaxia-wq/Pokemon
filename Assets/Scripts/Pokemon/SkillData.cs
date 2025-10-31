using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Pokemon/Create Skill Data", order = 1)]
public class SkillData : ScriptableObject
{
	[Header("基础信息")]
	public string skillName;
	[TextArea]
	public string description;

	[Header("数值")]
	public int power = 10;
	public float cooldown = 1.0f;
}
