using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadingLogic : MonoBehaviour
{
    public Slider ProgressBarSlider;
    public GameObject VisualPart;
    public float FakeLoadTime = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
      //  VisualPart.SetActive(false);
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
        VisualPart.SetActive(true);
        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneNo);
        asyncLoading.allowSceneActivation = false;

        float timer = 0;

        while (timer < FakeLoadTime || asyncLoading.progress < 0.9f)
        {
            timer += Time.deltaTime;
            SetProgressBarProgress(timer / FakeLoadTime);

            yield return null;
        }

        asyncLoading.allowSceneActivation = true;

        while (!asyncLoading.isDone)
            yield return null;
        VisualPart.SetActive(false);
    }

    private void SetProgressBarProgress(float progress)
    {
        ProgressBarSlider.value = progress;
    }
}
