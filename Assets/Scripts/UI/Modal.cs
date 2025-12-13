using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


public class Modal : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Image infoImage;

    [SerializeField] private TextMeshProUGUI textHeader;
    [SerializeField] private TextMeshProUGUI textDescription;

    [SerializeField] private InputActionReference closeModalAction;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(false);
        closeModalAction.action.performed += ctx => gameObject.SetActive(false);
    }


    public void ShowItemModal()
    {
        gameObject.SetActive(true);
    }
}
