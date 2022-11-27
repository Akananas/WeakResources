using System;
using System.Runtime.CompilerServices;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace WeakResources{
    [System.Serializable]
    public class WeakResources<T>
        where T : UnityEngine.Object
    {
        private WeakReference reference = new WeakReference(null);
        public string AssetPath;
        private Action onCompletedCallback;
    
        WeakResources()
        {
            reference = new WeakReference(null);
        }

        WeakResources(string assetPath)
        {
            reference = new WeakReference(null);
            AssetPath = assetPath;
        }

        public T Get()
        {
            if(reference.IsAlive)
            {
                return reference.Target as T;  
            }

            return LoadBlocking();
        }
    
        public T LoadBlocking()
        {
            if(reference.IsAlive)
            {
                return reference.Target as T;  
            }
        
            if(AssetPath.Length > 0)
            {
                var newObj = Resources.Load(AssetPath, typeof(T)) as T;
                Assert.IsNotNull(newObj, "[WeakResources::LoadBlocking] Path to asset in resources folder is not valid");
                reference = new WeakReference(newObj);
                return reference.Target as T;
            }
            return default(T);
        }

        public void LoadAsync(Action callback)
        {
            Assert.IsNotNull(callback, "[WeakResources::LoadAsync] callback is null");
    
            if(reference.IsAlive)
            {
                callback?.Invoke();  
            }
        
            var newObj = Resources.LoadAsync(AssetPath, typeof(T));
            newObj.completed += OnCompleted;
            onCompletedCallback = callback;
        }

        private void OnCompleted(UnityEngine.AsyncOperation asyncOperation)
        {
            var resource = (ResourceRequest)asyncOperation;
            Assert.IsNotNull(resource, "[WeakResources::OnCompleted] resource is not a ResourceRequest");

            Assert.IsNotNull(resource.asset, "[WeakResources::OnCompleted] No asset loaded (Check asset path)");
            reference = new WeakReference(resource.asset);
        
            onCompletedCallback?.Invoke();
            onCompletedCallback = null;
        }
    }
}
