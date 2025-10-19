using UnityEngine;
using UnityEngine.UI;
public class SettlementManager : MonoBehaviour
{
    public GameObject FoodPanel;
    public GameObject CreaturePanel;
    public GameObject BattleCardPanel;
    void OnEnable()
    {
        DealWithSettlementData();
    }
    public void DealWithSettlementData()
    {
        // Implement settlement logic here
        Debug.Log("Dealing with settlement...");
        var data = SceneManager.Instance.productionToSettlementData;
        data.PrintData();
    }
}