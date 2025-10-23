using UnityEngine;

public class OutfitManager : MonoBehaviour
{
    [Header("Partes do Corpo Base (Sempre Visíveis)")]
    public GameObject[] alwaysVisible;
    
    [Header("Conjunto 1")]
    public GameObject[] outfit1; // Helmets, Armor, etc do Conjunto 1
    
    [Header("Conjunto 2")]
    public GameObject[] outfit2; // Helmets, Armor, etc do Conjunto 2
    
    [Header("Conjunto 3")]
    public GameObject[] outfit3;

    [Header("Conjunto 4")]
    public GameObject[] outfit4;
    
    [Header("Configuração")]
    public int activeOutfit = 1; // Qual conjunto está ativo (1, 2, 3...)
    
    void Start()
    {
        // Ativa o conjunto escolhido ao iniciar
        SetOutfit(activeOutfit);
    }
    
    public void SetOutfit(int outfitNumber)
    {
        // Desativa todos os conjuntos primeiro
        SetActive(outfit1, false);
        SetActive(outfit2, false);
        SetActive(outfit3, false);
        SetActive(outfit4, false);
        
        // Ativa apenas o conjunto escolhido
        switch(outfitNumber)
        {
            case 1:
                SetActive(outfit1, true);
                break;
            case 2:
                SetActive(outfit2, true);
                break;
            case 3:
                SetActive(outfit3, true);
                break;
            case 4:
                SetActive(outfit4, true);
                break;
        }
        
        // Garante que as partes base estão sempre visíveis
        SetActive(alwaysVisible, true);
        
        activeOutfit = outfitNumber;
    }
    
    void SetActive(GameObject[] objects, bool active)
    {
        if (objects == null) return;
        
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }
    
    // Para testar durante o jogo - teclas 1, 2, 3
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetOutfit(1);
            Debug.Log("Conjunto 1 ativado");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetOutfit(2);
            Debug.Log("Conjunto 2 ativado");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetOutfit(3);
            Debug.Log("Conjunto 3 ativado");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetOutfit(4);
            Debug.Log("Conjunto 4 ativado");
        }
    }
}
