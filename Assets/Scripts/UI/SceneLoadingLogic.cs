using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SceneLoadingLogic : MonoBehaviour
{
    public Slider progressBarSlider;
    public GameObject visualPart;
    public TextMeshProUGUI loadingMessages;
    public float fakeLoadTime = 1f;
    public string[] text;
    private GameObject hidePart;

    private void Awake()
    {
        DontDestroyOnLoad(transform.parent);
        hidePart = GameObject.FindGameObjectWithTag("HideUIOnLoad");
     
        visualPart.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadGameSceneCor(SceneManager.GetSceneByName(sceneName).buildIndex));
    }

    public void LoadScene(int sceneNo)
    {
        StartCoroutine(LoadGameSceneCor(sceneNo));
    }
    private IEnumerator LoadGameSceneCor(int sceneNo)
    {
        
        visualPart.SetActive(true);
        hidePart.SetActive(false);
        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneNo);
        asyncLoading.allowSceneActivation = false;
        loadingMessages.text = text[0];

        float timer = 0;
        float startTime = Time.realtimeSinceStartup; 
        int i = 1;

        while (timer < fakeLoadTime || asyncLoading.progress < 0.9f)
        {
            if ((100 / text.Length )*i < (timer / fakeLoadTime)*100)
            {
                print((timer / fakeLoadTime)*100);
                print((100 / text.Length) * i);
                if (i<text.Length) {
                    loadingMessages.text = text[i];
                    i++;
                }
            }

            timer += (Time.realtimeSinceStartup - startTime);
            SetProgressBarProgress(timer / fakeLoadTime);

            yield return null;
        }

        asyncLoading.allowSceneActivation = true;
        
        while (!asyncLoading.isDone)
            yield return null;
        visualPart.SetActive(false);
        hidePart = GameObject.FindGameObjectWithTag("HideUIOnLoad");
        Time.timeScale = 1;
        if (sceneNo == 0)
            Destroy(transform.parent.gameObject);

    }

    private void SetProgressBarProgress(float progress)
    {
        progressBarSlider.value = progress;
    }

    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
