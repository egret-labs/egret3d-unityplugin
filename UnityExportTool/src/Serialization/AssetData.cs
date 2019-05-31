namespace Egret3DExportTools
{
    using System.Collections.Generic;

    public interface ISerizileData
    {
        string id { get; set; }
        string className { get; set; }
    }

    public abstract class SerizileData : ISerizileData
    {
        protected string _id;
        protected string _className;
        public string id { get { return this._id; } set { this._id = value; } }
        public string className { get { return this._className; } set { this._className = value; } }
    }
    public class MyEntity : SerizileData
    {
        public UnityEngine.GameObject unityEntity;
        protected List<MyComponent> _components = new List<MyComponent>();

        public void AddComponent(MyComponent comp)
        {
            if(!this._components.Contains(comp))
            {
                this._components.Add(comp);
            }
        }
    }

    public class MyComponent : SerizileData
    {
        public Dictionary<string, IJsonNode> props = new Dictionary<string, IJsonNode>();
    }

    public class AssetData
    {
        public List<MyEntity> entities = new List<MyEntity>();
    }
}