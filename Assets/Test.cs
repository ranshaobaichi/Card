using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button button;
    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            var manager = GameObject.FindAnyObjectByType<BattleWorldManager>();
            manager.AddObj(Category.Battle.LineUp.Player, Category.CreatureCardType.´óµØ¾«³âºò);
        });
    }
}
