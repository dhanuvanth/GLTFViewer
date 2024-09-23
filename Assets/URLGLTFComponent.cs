using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // Add this for UI components

public class URLGLTFComponent : MonoBehaviour
{
    public string GLTFUrl = null;
    public bool UseCache = true;
    public bool UseRuntimeLoading = false; // New toggle for runtime loading feature

    public TMP_InputField urlInputField; // Reference to the InputField
    public Button loadButton; // Reference to the Button

    private UnityGLTF.GLTFComponent gltfComponent;
    private GameObject loadedModel;

    private void Start()
    {
        if (UseRuntimeLoading)
        {
            SetupRuntimeUI();
        }
        else
        {
            LoadModelFromUrl(GLTFUrl);
        }
    }

    private void SetupRuntimeUI()
    {
        if (urlInputField != null && loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClick);
        }
        else
        {
            Debug.LogError("InputField or Button not assigned in the inspector.");
        }
    }

    private void OnLoadButtonClick()
    {
        string url = urlInputField.text;
        if (!string.IsNullOrEmpty(url))
        {
            LoadModelFromUrl(url);
        }
        else
        {
            Debug.LogWarning("Please enter a valid URL.");
        }
    }

    private async void LoadModelFromUrl(string url)
    {
        try
        {
            // Destroy the previous model if it exists
            if (loadedModel != null)
            {
                Destroy(loadedModel);
            }

            string localPath = await DownloadAndSaveModel(url);
            loadedModel = new GameObject("LoadedModel");
            AddGLTFComponentAndSetUri(loadedModel, localPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to download and load model: {e.Message}");
        }
    }

    private void AddGLTFComponentAndSetUri(GameObject targetObject, string localPath)
    {
        gltfComponent = targetObject.AddComponent<UnityGLTF.GLTFComponent>();
        gltfComponent.GLTFUri = localPath;
        gltfComponent.AppendStreamingAssets = false;

        Debug.Log($"GLTFComponent added to {targetObject.name} and URI set to: {localPath}");
    }

    private async Task<string> DownloadAndSaveModel(string url)
    {
        string fileName = Path.GetFileName(new Uri(url).LocalPath);
        string localPath = Path.Combine(Application.persistentDataPath, fileName);

        if (UseCache && File.Exists(localPath))
        {
            Debug.Log("Loading model from cache");
            return localPath;
        }

        Debug.Log("Downloading model");
        await DownloadFile(url, localPath);
        return localPath;
    }

    private async Task DownloadFile(string url, string localPath)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Failed to download file: {www.error}");
            }

            File.WriteAllBytes(localPath, www.downloadHandler.data);
        }
    }
}