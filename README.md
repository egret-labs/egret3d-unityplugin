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
2. 导出随机种子不支持
3. 如果希望渲染效果尽量保持一致，可以把Unity的平台选择为WebGL(Lightmap必须选择WebGL，否则效果会有明显的差异)

## 支持的模块
1. Main
2. Emission
3. Shape
4. Velocity over lifetime
5. Color over lifetime
6. Size over lifetime
7. Rotation over lifetime
8. Texture Sheet Animation

### 2019/01/30 更新
* 【新增】支持导出PBR材质
* 【新增】网格导出增加文件过滤
* 【调整】正确导出Camera.cullingMsk值
* 【修复】导出内置图片报错的问题
* 【修复】修复粒子导出时颜色过渡错误问题

### 2018/09/30 更新
* 【调整】采用标准蒙皮动画结构输出动画数据
* 【调整】默认材质,网格，图片统一导到Library目录下
* 【修复】同一模型文件中不同网格导出的数据会有重复的问题

### 2018/08/30 更新
* 【新增】增加导出资源到最新版本4.0
* 【新增】增加导出非透明光照材质
* 【新增】增加导出日志
* 【修复】文件名带有冒号引起的导出错误