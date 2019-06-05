using System;
using UnityEngine;
namespace Egret3DExportTools
{
    /**
     * 组件管理器接口
     */
    public interface IComponentSerializer
    {
        Type compType { set; get; }
        string className { set; get; }
        bool Serialize(Component component, ComponentData compData);
    }

    public abstract class ComponentSerializer : IComponentSerializer
    {
        protected Type _compType;
        protected string _className;

        public virtual bool Serialize(Component component, ComponentData compData)
        {
            return true;
        }

        public Type compType { get => _compType; set => _compType = value; }
        public string className { get => _className; set => _className = value; }
    }
}