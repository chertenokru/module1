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
    public GameObject hidePart;
    public TextMeshProUGUI loadingMessages;
    public float fakeLoadTime = 1f;
    public string[] text;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        progressBarSlider.gameObject.SetActive(false);
        hidePart.SetActive(true);
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
        
        progressBarSlider.gameObject.SetActive(true);
        hidePart.SetActive(false);
        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneNo);
        asyncLoading.allowSceneActivation = false;
        loadingMessages.text = text[0];

        float timer = 0;
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

            timer += Time.deltaTime;
            SetProgressBarProgress(timer / fakeLoadTime);

            yield return null;
        }

        asyncLoading.allowSceneActivation = true;

        while (!asyncLoading.isDone)
            yield return null;
        visualPart.SetActive(false);
    }

    private void SetProgressBarProgress(float progress)
    {
        progressBarSlider.value = progress;
    }
}
