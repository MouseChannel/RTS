using System.Collections;
using System.Collections.Generic;
using Pb;
using UnityEngine;

public interface IWindow  
{
    public void Enter();
    public void Update(PbMessage message);
    public MainWindow GetWindowType();
    public void Exit();
}
