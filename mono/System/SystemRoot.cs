using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class SysRoot : MonoBehaviour{
    protected GameRoot root;
    private NetService _netService;
    private ResourceService _resourceService;
    private AudioService _audioService;
    public void Init() {
        root = GameRoot.Instance;
        _netService = NetService.Instance;
        _resourceService = ResourceService.Instance;
        _audioService = AudioService.Instance;
    }
}
