using System;
using UnityEngine;

namespace Unity.ReferenceProject.Common
{
    public interface ICameraProvider
    {
        Camera Camera { get; }
        event Action<Camera> CameraChanged;
    }
    
    public class CameraProvider : ICameraProvider
    {
        public Camera Camera { get; private set; }
        
        public CameraProvider(Camera camera)
        {
            Camera = camera;
        }
        
        public event Action<Camera> CameraChanged;
        
        public void SetCamera(Camera cam)
        {
            if (cam == Camera)
                return;
            
            Camera = cam;
            CameraChanged?.Invoke(Camera);
        }
    }
}
