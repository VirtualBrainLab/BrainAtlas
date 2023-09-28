using System;
using System.Collections.Generic;
using UnityEngine;

public class BrainAtlasManager : MonoBehaviour
{
    private AtlasMetaData _atlasMetaData;

    public List<string> AtlasNames { get { return _atlasMetaData.AtlasNames; } }

    private void Awake()
    {
    }

    private async void Start()
    {
        var metaDataTask = AddressablesRemoteLoader.LoadAtlasMetaData();
        await metaDataTask;
        _atlasMetaData = metaDataTask.Result;

        LoadAtlas(_atlasMetaData.AtlasNames[0]);
    }

    public void LoadAtlas(string atlasName)
    {
#if UNITY_EDITOR
        Debug.Log($"(BAM) Loading {atlasName}");
#endif
    }
}