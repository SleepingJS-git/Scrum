using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndRoom : MonoBehaviour
{
    public Podium[] podiums;
    public Material[] playerMats;
    public Ragdoll ragdollPrefab;
    void Start()
    {
        GameManager.Instance.OnGameEnd += GetGameResults;
    }

    private void GetGameResults()
    {
        PlayerResults[] results = GameManager.Instance.BuildPlayerResults(out int numOfPlayers);

        for (int i = 0; i < results.Length; i++)
        {
            PlayerResults result = results[i];
            Podium podium = podiums[i];
            if (result == null)
            {
                podium.playerModel.gameObject.SetActive(false);
                podium.username.text = "";
                podium.statsText.text = "";
                podium.light.gameObject.SetActive(false);
                return;
            }

            // if (i == 0)
            // {
            //     podium.playerModel.Play("Dance");
            // }

            podium.renderer.material = playerMats[result.playerID];
            //podium.username.SetText(result.playerName);
            podium.statsText.SetText($"Wins: {result.wins}" + 
            $"\nKills: {result.kills}");

            StartCoroutine(SpawnDeadBodies(result.kills, playerMats[result.playerID], numOfPlayers));
        }
    }

    private IEnumerator SpawnDeadBodies(int kills, Material culledMaterial, int numOfPlayers)
    {
        for (int i = 0; i < kills; i++)
        {
            Material randMat = playerMats[Random.Range(0, numOfPlayers)];

            if (randMat == culledMaterial)
            {
                i--; continue;
            }
            else
            {
                Debug.Log("Color spawned: " + randMat.name);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}


[System.Serializable]
public class Podium
{
    public Animator playerModel;
    public SkinnedMeshRenderer renderer;
    public TextMeshPro username;
    public TextMeshPro statsText;
    public Light light;
    public Transform bodySpawner;
}

