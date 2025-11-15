using System.Collections.Generic;
using Category.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraitItem : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public Text traitPopulation;
    public Trait trait;
    public LineUp lineUp;

    public void Initialize(Trait trait, LineUp lineUp = LineUp.Player)
    {
        this.trait = trait;
        this.lineUp = lineUp;
        var traitAttribute = BattleWorldManager.Instance.traitAttributesDict[trait];
        var traitObj = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[trait];
        iconImage.sprite = traitAttribute.icon;

        string populationString = "";
        int level = traitObj.level;
        List<int> thresholds = traitAttribute.levelThresholds;
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (i == thresholds.Count - 1)
                populationString += thresholds[i];
            else
                populationString += thresholds[i] + "/";
        }
        traitPopulation.text = populationString;
        charAlpha(traitPopulation, (level - 1) * 2, 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            BattleWorldManager.Instance.traitDescriptionPanel.InitializeForBattle(this);
        }
    }

    private void charAlpha(Text textComp, int charIndex, float alpha)
    {
        if (textComp == null) return;
        string s = textComp.text ?? "";
        if (charIndex < 0 || charIndex >= s.Length) return;

        textComp.supportRichText = true;

        string before = s.Substring(0, charIndex);
        string target = s[charIndex].ToString();
        string after = s.Substring(charIndex + 1);

        // 获取当前颜色 RGB 并拼接 alpha 的十六进制（RRGGBBAA）
        string rgb = ColorUtility.ToHtmlStringRGB(textComp.color);
        int aByte = Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f);
        string aHex = aByte.ToString("X2");

        string colored = $"<color=#{rgb}{aHex}>{target}</color>";
        textComp.text = before + colored + after;
    }

    
}