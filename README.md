# Egret3DExportTools
* 支持Unity5.6.4版本以上

## 使用DLL方法
1. 复制Out目录下Egret3DExportTools文件夹到Unity项目Assets下。
2. 等待Unity编译，完成后会在控制台出现"Finished updating scripts / assemblies"。
3. Unity编辑器菜单会多出一个Egret3DExportTools选项，点击打开即可。

## 生成DLL方法
1. 使用visual studio 2017打开UnityExportTool目录下Unity3DExportTool.sln文件。
2. 默认引用libs/unity5下的Unity库，可以自行选择想要引用的Unity库。
3. 打开项目设置，在生成标签下选择自己的DLL输出路径。
4. 点击生成解决方案生成插件DLL。

## 注意事项
1. 导出目录请不要带有空格，空格会被插件替换为下划线。
3. 如果希望渲染效果尽量保持一致，可以把Unity的平台选择为WebGL(Lightmap必须选择WebGL，否则效果会有明显的差异)。

## 导出 Unity3D 的场景

* 选择场景导出功能会把当前激活的场景导出为.scene.json文件。
* 将导出的文件放入您的 Egret Pro 的 resource 文件夹中。
    > [!IMPORTANT]
    > 请确认资源导出到了 resource 文件夹的根路径中，目前暂时只支持导出到根路径
* 在项目中添加如下代码加载场景
```typescript
await Application.instance.sceneManager.loadScene("your.scene.json");
```
或者
```typescript
await ResourceManager.instance.loadUri("your.scene.json");
const scene = EntityModelAssetEntity.createInstance("your.scene.json");
```


## 导出 Unity3D 的预制体

* 选择资源导出功能会把当前激活的场景导出为.prefab.json文件。
* 将导出的文件放入您的 Egret Pro 的 resource 文件夹中。
* 使用如下代码在场景中添加预制体
```typescript
const prefab = await EngineFactory.createPrefab("your.prefab.json");
```
或者
```typescript
await ResourceManager.instance.loadUri("your.prefab.json");
const prefab = EntityModelAssetEntity.createInstance("your.prefab.json");
```

## 导出设置

### 路径设置
* 路径名称：指定导出文件路径的根路径名称。
* 导出路径： 选择导出的文件路径。

### 光照设置
* Lambert：导出非光泽表面的材质，没有镜面高光。
* Phong：导出具有镜面高光的光泽表面的材质。
* Standard：导出基于物理的标准材质。

### 场景设置
* 光照贴图：导出Unity烘焙出来的光照贴图以供Egret3D使用。
* 静态合并：导出为StaticBatching组件，Egret3D加载场景后，会尝试把所有在Unity中标记为静态的网格对象合并为若干大的网格对象，以实现减少Drawcall目的。
* 雾：导出为Fog组件以供Egret3D使用。

### 图片设置
* 导出原始图片：勾选后，导出时直接复制原始图片资源到导出目录，否则会尝试使用Unity的EncodeToJPG，EncodeToPNG导出资源。
* JPG导出质量：根据需求选择导出JPG的质量。

### 网格设置
* UV2，Normals，Colors，Bones，Tangents，根据需求可以选择导出网格时是否导出这些属性，以减少网格资源的体积大小。

### 保存配置
* 保存配置：可以把你现在Egret3D导出面板所做的设置保存起来，以方便下次使用。

## 支持组件列表
* Transform
* MeshFilter
* MeshRenderer
* Camera
* BoxCollider
* SphereCollider
* SkinnedMeshRenderer
* ParticleSystem(部分支持)
* ParticleRenderer
* Animation(部分支持)
* Animator(部分支持)
* DirectionalLight
* SpotLight
* Skybox

## 注意事项

* Animation组件暂时只实现了对以下属性的导出：
    * m_LocalPosition
    * m_LocalRotation
    * localEulerAnglesRaw
    * m_LocalScale
    * m_IsActive
* Animator现在会被导出为Egret3D的Animation组件,暂时不支持状态机数据导出.
* 粒子组件暂时只支持以下模块：
    * MainMoudle
    * Emission
    * Shape
    * VelocityOverLifetime
    * ColorOverLifetime
    * SizeOverLifetime
    * RotationOverLifetime
    * TextureSheetAnimation
* 粒子系统的Shader必须是Particles目录下的Shader，如果不是，在Egret3D中可能会造成粒子系统不显示。

## 问题反馈

考虑到 Egret3D Unity导出插件可能无法覆盖您现有的 Unity3D 制作的游戏所需要移植的全部特性，您在使用此软件时有可能会出现导出失败或导出结果不正确的情况。您可以将问题反馈至[这里](https://github.com/egret-labs/egret3d-unityplugin/issues)。为了帮助您快速解决问题，请您在反馈问题时将您的游戏资源进行尽可能的简化，并作为附件提交。