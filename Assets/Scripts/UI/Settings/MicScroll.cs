using System.Linq;
using Adrenak.UniMic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MicScroll : MonoBehaviour
{
    [SerializeField] GameObject button;
    GameObject manager;
    SystemManager sysManager;
    string[] Mics;
    void Update()
    {
        if (Mics != null && !Mics.SequenceEqual(Microphone.devices))
        {
            UpdateMicCollection();        
        }
        Mics = Microphone.devices;
    }
    void Start()
    {
        manager = GameObject.Find("Manager");
        sysManager = manager.GetComponent<SystemManager>();
        UpdateMicCollection();
    }
    void SetMic(string mic)
    {
        Debug.Log(mic);
        sysManager.mic = mic;
    }
    void UpdateMicCollection()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        int i = 0;
        foreach (string mic in Microphone.devices)
        {
            i++;
            Debug.Log(mic);
            GameObject obj = Instantiate(button);
            obj.transform.SetParent(gameObject.transform);

            obj.GetComponentInChildren<TextMeshProUGUI>().text = mic;
            obj.GetComponent<Button>().onClick.AddListener(() => SetMic(mic));
        }
        if (i > 5) i = 5;
        transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(450, (100 * i)+100);
        transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(450, (-50 * i), 0);
    }

}
