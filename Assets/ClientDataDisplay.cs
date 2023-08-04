using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using static UnityEditor.Progress;

public class ClientDataDisplay : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public GameObject popup;
    public GameObject popitem;
    public GameObject itemPrefab;
    public Transform content;
    public string url = "https://qa2.sunbasedata.com/sunbase/portal/api/assignment.jsp?cmd=client_data";

    private List<ClientInfo> allClientsData;

    private void Start()
    {
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        allClientsData = new List<ClientInfo>();
        FetchDataFromURL();
    }

    private void OnDropdownValueChanged(int value)
    {
        ClearContent();

        switch (value)
        {
            case 0: 
                PopulateClientData(allClientsData);
                break;
            case 1: 
                PopulateManagersData(allClientsData);
                break;
            case 2: 
                PopulateNonManagersData(allClientsData);
                break;
            default:
                break;
        }
    }

    private void FetchDataFromURL()
    {
        StartCoroutine(FetchDataCoroutine());
    }

    public void closepopup()
    {
        popup.SetActive(false);
    }
    private IEnumerator FetchDataCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonData = webRequest.downloadHandler.text;
                Debug.Log(jsonData);
                allClientsData = ParseJSONData(jsonData);
                PopulateClientData(allClientsData);
            }
            else
            {
                Debug.LogError("Failed to fetch data: " + webRequest.error);
            }
        }
    }

    private List<ClientInfo> ParseJSONData(string jsonData)
    {
        var json = JSON.Parse(jsonData);
        JSONArray clientsArray = json["clients"].AsArray;

        List<ClientInfo> clients = new List<ClientInfo>();
        foreach (JSONNode clientNode in clientsArray)
        {
            ClientInfo client = new ClientInfo();
            client.isManager = clientNode["isManager"].AsBool;
            client.id = clientNode["id"].AsInt;
            Debug.Log(client.id);
            client.label = clientNode["label"];
           
            Debug.Log(client.name);



            
            JSONNode data = json["data"][client.id.ToString()];
            if (data != null)
            {
                client.name = data["name"];
                client.address = data["address"];
                client.points = data["points"].AsInt;
            }
            else
            {
               
                Debug.Log("Data not found for client ID: " + client.id);
              
                client.name = "unknown";
                client.address = "N/A";
                client.points = 0;
            }

            clients.Add(client);
        }

        return clients;
    }

    private void ClearContent()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateClientData(List<ClientInfo> clients)
    {
        foreach (ClientInfo client in clients)
        {
            SpawnItem(client.name, client.label, client.points,client.address);
            
        }
    }

    private void PopulateManagersData(List<ClientInfo> clients)
    {
        foreach (ClientInfo client in clients)
        {
            if (client.isManager)
            {
                SpawnItem(client.name, client.label, client.points, client.address);
            }
        }
    }

    private void PopulateNonManagersData(List<ClientInfo> clients)
    {
        foreach (ClientInfo client in clients)
        {
            if (!client.isManager)
            {
                SpawnItem(client.name, client.label, client.points, client.address);
            }
        }
    }

    private void SpawnItem(string name, string label, int points,string address)
    {
        Debug.Log(name);
        GameObject item = Instantiate(itemPrefab, content);
        TMP_Text[] itemTexts = item.GetComponentsInChildren<TMP_Text>();

        itemTexts[0].text = name;
        itemTexts[1].text = label;
        itemTexts[2].text = points.ToString();

        item.GetComponent<Button>().onClick.AddListener(() => showpopup( name,  label,  points,  address));
    }

   public void showpopup(string name, string label, int points, string address)
    {
        popup.SetActive(true);
        TMP_Text[] itemTexts = popitem.GetComponentsInChildren<TMP_Text>();

        itemTexts[0].text = name;
      
        itemTexts[1].text = points.ToString();
        itemTexts[2].text = address;
    }
}