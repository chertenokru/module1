using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class SceneLoadingLogic : MonoBehaviour
{
    public enum SceneNums
    {
        Menu,
        Level1,
        Level2
    }

    [Serializable]
    public class DescriptionAutoCreatedObject
    {
        public GameObject gameobject;
        public bool isCreateInMenu = false;
        public bool isCreateInLevel = true;
    }

    public const string TAG_UI_SCENE_TO_HIDE = "HideUIOnLoad";

    // индикатор загрузки
    public Slider progressBarSlider;

    // визуальная часть лоадера
    public GameObject visualPart;

    // куда выводить текст
    public TextMeshProUGUI loadingMessages;

    // фикс время загрузки
    public float fakeLoadTime = 1f;

    // рекламные тексты во время загрузки
    public string[] text;

    public DescriptionAutoCreatedObject[] ObjectToAutoCreate;

    // скрываемая часть UI на сцене, чтоб не мешала при загрузке
    // определяется по тэгу при загрузке
    private GameObject hidePart;


    private void Awake()
    {
        DontDestroyOnLoad(transform.parent);
        hidePart = GameObject.FindGameObjectWithTag(TAG_UI_SCENE_TO_HIDE);
        visualPart.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        LoadScene(SceneManager.GetSceneByName(sceneName).buildIndex);
    }

    public void LoadScene(SceneNums sceneEnumName)
    {
        LoadScene((int)sceneEnumName);
    }

    public void LoadScene(int sceneNo)
    {
        // останавливаем все возможные процессы
        StopAllCoroutines();
        
        
        // загружаем
        StartCoroutine(LoadGameSceneCor(sceneNo));
    }

    private IEnumerator LoadGameSceneCor(int sceneNo)
    {
        foreach (GameObject tempGameObject in GameObject.FindGameObjectsWithTag(TAG_UI_SCENE_TO_HIDE))
        {
            tempGameObject.gameObject.SetActive(false);
        }

        visualPart.SetActive(true);

        
        //GameObject.FindGameObjectsWithTag(TAG_UI_SCENE_TO_HIDE);
        //hidePart.SetActive(false);

        AsyncOperation asyncLoading = SceneManager.LoadSceneAsync(sceneNo);
        asyncLoading.allowSceneActivation = false;
        loadingMessages.text = text[0];

        float timer = 0;
        float startTime = Time.realtimeSinceStartup;
        int textToShowIndex = 1;

        while (timer < fakeLoadTime || asyncLoading.progress < 0.9f)
        {
            // залипуха с показом текста при загрузке
            if ((100 / text.Length) * textToShowIndex < (timer / fakeLoadTime) * 100)
            {
                if (textToShowIndex < text.Length)
                {
                    loadingMessages.text = text[textToShowIndex];
                    textToShowIndex++;
                }
            }

            timer += (Time.realtimeSinceStartup - startTime);
            startTime = Time.realtimeSinceStartup;
            SetProgressBarProgress(timer / fakeLoadTime);
            yield return null;
        }

        asyncLoading.allowSceneActivation = true;
        while (!asyncLoading.isDone)
            yield return null;
        visualPart.SetActive(false);
        // ищем новый UI на загруженной сцене
        //hidePart = GameObject.FindGameObjectWithTag(TAG_UI_SCENE_TO_HIDE);

        // в главной сцене уже есть компонент - грохаем его
        if (sceneNo == (int) SceneNums.Menu)
            Destroy(transform.parent.gameObject);
        // смотрим что надо создать
        foreach (DescriptionAutoCreatedObject obj in ObjectToAutoCreate)
        {
            if ((obj.isCreateInLevel && (sceneNo != (int) SceneNums.Menu)) ||
                (obj.isCreateInMenu && (sceneNo == (int) SceneNums.Menu)))
            {
                Instantiate(obj.gameobject);
                obj.gameobject.tag = TAG_UI_SCENE_TO_HIDE;
            }
        }

        // включаем время
        Time.timeScale = 1;
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