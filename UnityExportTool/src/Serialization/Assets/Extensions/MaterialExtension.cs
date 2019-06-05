using System.Collections.Generic;
using Egret3DExportTools;
using GLTF.Schema;
using Newtonsoft.Json.Linq;


public class Functions : IExtension
{
    //colorMask
    //depthRange
    //lineWidth
    //polygonOffset
    // public int[] blendColor;
    public BlendEquation[] blendEquationSeparate;
    public BlendFactor[] blendFuncSeparate;
    public CullFace[] cullFace;
    public DepthFunc[] depthFunc;
    public bool[] depthMask;
    public FrontFace[] frontFace;

    public void Clear()
    {
        this.blendEquationSeparate = null;
        this.blendFuncSeparate = null;
        this.cullFace = null;
        this.depthFunc = null;
        this.depthMask = null;
        this.frontFace = null;
    }

    public IExtension Clone(GLTFRoot root)
    {
        return new Functions
        {
        };
    }

    public JProperty Serialize()
    {
        JObject ext = new JObject();

        var b = false;
        if (this.blendEquationSeparate != null && this.blendEquationSeparate.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.blendEquationSeparate)
            {
                arr.Add(System.Enum.Parse(s.GetType(), s.ToString()));
            }

            ext.Add("blendEquationSeparate", arr);
        }

        if (this.blendFuncSeparate != null && this.blendFuncSeparate.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.blendFuncSeparate)
            {
                arr.Add(System.Enum.Parse(s.GetType(), s.ToString()));
            }

            ext.Add("blendFuncSeparate", arr);
        }

        if (this.cullFace != null && this.cullFace.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.cullFace)
            {
                arr.Add(System.Enum.Parse(s.GetType(), s.ToString()));
            }

            ext.Add("cullFace", arr);
        }

        if (this.depthFunc != null && this.depthFunc.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.depthFunc)
            {
                arr.Add(System.Enum.Parse(s.GetType(), s.ToString()));
            }

            ext.Add("depthFunc", arr);
        }

        if (this.depthMask != null && this.depthMask.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.depthMask)
            {
                arr.Add(s);
            }

            ext.Add("depthMask", arr);
        }

        if (this.frontFace != null && this.frontFace.Length > 0)
        {
            b = true;
            var arr = new JArray();
            foreach (var s in this.frontFace)
            {
                arr.Add(System.Enum.Parse(s.GetType(), s.ToString()));
            }

            ext.Add("frontFace", arr);
        }

        return b ? new JProperty("functions", ext) : null;
    }
}

public class States : IExtension
{
    public EnableState[] enable;
    public Functions functions;

    public IExtension Clone(GLTFRoot root)
    {
        return new States
        {
            enable = enable,
            functions = functions,
        };
    }

    public JProperty Serialize()
    {
        var b = false;
        JObject ext = new JObject();
        if (this.enable != null && this.enable.Length > 0)
        {
            b = true;
            var es = new JArray();
            foreach (var e in this.enable)
            {
                es.Add(System.Enum.Parse(e.GetType(), e.ToString()));
            }
            ext.Add("enable", es);
        }

        if (this.functions != null)
        {
            var fun = this.functions.Serialize();
            if (fun != null)
            {
                b = true;
                ext.Add(this.functions.Serialize());
            }
        }
        return b ? new JProperty("states", ext) : null;
    }
}

public class Techniques : IExtension
{
    // public int program = 0;
    // public Dictionary<string, System.Object> attributes = new Dictionary<string, object>();
    public States states;
    public Dictionary<string, string> uniforms = new Dictionary<string, string>();

    public IExtension Clone(GLTFRoot root)
    {
        return new Techniques
        {
            states = states,
            uniforms = uniforms,
        };
    }

    public JProperty Serialize()
    {
        JObject ext = new JObject();

        if (this.states != null)
        {
            var json = this.states.Serialize();
            if (json != null)
            {
                ext.Add(json);
            }
        }

        ext.Add(new JProperty("uniforms", new JObject()));
        return new JProperty("", ext);
    }
}

public class KhrTechniqueWebglGlTfExtension : IExtension
{
    public List<Techniques> techniques = new List<Techniques>();

    public IExtension Clone(GLTFRoot root)
    {
        return new KhrTechniqueWebglGlTfExtension
        {
            techniques = techniques,
        };
    }

    public JProperty Serialize()
    {
        JObject ext = new JObject();
        if (this.techniques != null && this.techniques.Count > 0)
        {
            JArray ts = new JArray();
            foreach (var technique in this.techniques)
            {
                JObject tj = new JObject();
                if (technique.states != null)
                {
                    var json = technique.states.Serialize();
                    if(json != null)
                    {
                        tj.Add(json);
                    }                    
                }
                tj.Add(new JProperty("uniforms", new JObject()));
                ts.Add(tj);

                // ts.Add(technique.Serialize());
            }
            ext.Add("techniques", ts);
        }


        return new JProperty("KHR_techniques_webgl", ext);
    }
}

public class KhrTechniquesWebglMaterialExtension : IExtension
{
    public int technique = 0;
    public List<JProperty> values = new List<JProperty>();

    public IExtension Clone(GLTFRoot root)
    {
        return new KhrTechniquesWebglMaterialExtension
        {
            technique = technique,
            values = values,
        };
    }

    public JProperty Serialize()
    {
        JObject ext = new JObject();

        ext.Add(new JProperty(
            "technique",
            technique
        ));

        var valueProps = new JObject();
        foreach (var item in this.values)
        {
            valueProps.Add(item);
        }

        ext.Add("values", valueProps);

        return new JProperty("KHR_techniques_webgl", ext);
    }
}

public class MaterialAssetExtension : AssetVersionExtension
{
    public string asset = "";
    public List<Define> defines = new List<Define>();
    public override JProperty Serialize()
    {
        var hasDefine = this.defines.Count > 0;
        var res = base.Serialize();

        JObject ext = res.First as JObject;

        var assets = new JArray();
        assets.Add(this.asset.Replace("Assets", ExportConfig.instance.rootDir));//TODO
        ext.Add("assets", assets);

        var entities = new JArray();
        var assetEntity = new JObject();
        assetEntity.Add(new JProperty(SerizileData.KEY_UUID, "0"));
        assetEntity.Add(new JProperty(SerizileData.KEY_CLASS, SerializeClass.AssetEntity));
        var entityComps = new JArray();
        entityComps.Add(new JObject(new JProperty(SerizileData.KEY_UUID, "1")));
        if (hasDefine)
        {
            entityComps.Add(new JObject(new JProperty(SerizileData.KEY_UUID, "2")));
        }
        assetEntity.Add(new JProperty("components", entityComps));
        entities.Add(assetEntity);
        ext.Add("entities", entities);

        var components = new JArray();
        if (hasDefine)
        {
            var define = new JObject();
            define.Add(new JProperty(SerizileData.KEY_UUID, "1"));
            define.Add(new JProperty(SerizileData.KEY_CLASS, SerializeClass.Defines));

            var defineData = new JArray();
            foreach (var d in this.defines)
            {
                defineData.Add(d.name + d.content);
            }

            define.Add(new JProperty("defines", defineData));

            components.Add(define);
        }

        var material = new JObject();
        material.Add(new JProperty(SerizileData.KEY_UUID, hasDefine ? "2" : "1"));
        material.Add(new JProperty(SerizileData.KEY_CLASS, SerializeClass.Material));
        material.Add(new JProperty("glTF", 0));

        var shader = new JObject();
        shader.Add(new JProperty(SerizileData.KEY_ASSET, 0));
        material.Add(new JProperty("shader", shader));
        components.Add(material);

        ext.Add("components", components);

        return res;
    }
}