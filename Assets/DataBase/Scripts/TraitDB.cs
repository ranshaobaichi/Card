using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Trait", menuName = "ScriptableObjects/Trait", order = 1)]
public class TraitDB : ScriptableObject
{
    [Serializable]
    public class TraitAttribute
    {
        public Category.Battle.Trait trait;
        public int maxLevel;
        public List<int> levelThresholds;
        public Sprite icon;
        [TextArea] public string description;
        [TextArea] public List<string> levelDescriptions;
    }

    public List<TraitAttribute> traitAttributes;
    private Dictionary<Category.Battle.Trait, TraitAttribute> traitAttributesDict = null;
    public void InitializeTraitDict()
    {
        traitAttributesDict = new Dictionary<Category.Battle.Trait, TraitAttribute>();
        foreach (var attribute in traitAttributes)
        {
            traitAttributesDict[attribute.trait] = attribute;
        }
    }
    public TraitAttribute GetTraitAttribute(Category.Battle.Trait trait)
        => traitAttributesDict[trait];
    public ReadOnlyDictionary<Category.Battle.Trait, TraitAttribute> GetAllTraitAttributes()
        => new ReadOnlyDictionary<Category.Battle.Trait, TraitAttribute>(traitAttributesDict);
}
