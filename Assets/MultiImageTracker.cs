using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class MultiImageTracker : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs; // Массив префабов для каждого изображения
    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            if (spawnedObjects.TryGetValue(trackedImage.referenceImage.name, out GameObject obj))
            {
                Destroy(obj);
                spawnedObjects.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    void UpdateImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        // Если изображение не распознано (например, качество низкое), скрываем объект
        if (trackedImage.trackingState != TrackingState.Tracking)
        {
            if (spawnedObjects.TryGetValue(imageName, out GameObject obj))
            {
                obj.SetActive(false);
            }
            return;
        }

        // Если объект ещё не создан — создаём
        if (!spawnedObjects.ContainsKey(imageName))
        {
            GameObject prefabToSpawn = GetPrefabForImage(imageName);
            if (prefabToSpawn != null)
            {
                GameObject obj = Instantiate(prefabToSpawn, trackedImage.transform);
                obj.SetActive(true);
                spawnedObjects[imageName] = obj;
            }
        }
        else
        {
            spawnedObjects[imageName].SetActive(true);
        }

        // Позиция и поворот уже управляются через trackedImage.transform
    }

    GameObject GetPrefabForImage(string imageName)
    {
        // Простой способ: сопоставить по имени
        foreach (var prefab in prefabs)
        {
            if (prefab != null && prefab.name == imageName)
                return prefab;
        }
        return null;
    }
}