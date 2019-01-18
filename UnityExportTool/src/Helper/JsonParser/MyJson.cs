using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Egret3DExportTools
{
    /**
	 * 自定义的节点的属性值类型
	 */
    public interface IJsonNode
    {
        void CovertToStringWithFormat(StringBuilder sb, int spaceSub);
        string ToString();
    }

    /**
	 * 数值类型
	 */
    public class MyJson_Number : IJsonNode
    {
        public double value;
        bool isBool;
        public MyJson_Number(double value)
        {
            this.value = value;
            this.isBool = false;
        }
        public MyJson_Number(bool value)
        {
            this.value = value ? 1 : 0;
            this.isBool = true;
        }
        public MyJson_Number(System.Enum value)
        {
            this.value = (int)System.Enum.Parse(value.GetType(), value.ToString());
        }
        public string HashToString()
        {
            return ResourceManager.instance.ResetHash((int)value).ToString();
        }

        /**
		 * 将数据写成树结构
		 */
        public void CovertToStringWithFormat(StringBuilder sb, int spacesub)
        {
            sb.Append(ToString());
        }

        public override string ToString()
        {
            if (this.isBool)
            {
                return (this.value == 1) ? "true" : "false";
            }
            else
            {
                return value.ToString();
            }
        }
    }

    /**
	 * 字符串类型
	 */
    public class MyJson_String : IJsonNode
    {
        public string value;
        public MyJson_String(string value)
        {
            this.value = value;
        }
        public MyJson_String(System.Enum value)
        {
            this.value = value.ToString();
        }
        /**
		 * 将数据写成树结构
		 */
        public void CovertToStringWithFormat(StringBuilder sb, int spacesub)
        {
            sb.Append(ToString());
        }

        public override string ToString()
        {
            string v = "";
            if (value != null)
            {
                v = value.Replace("\\", "\\\\");
                v = v.Replace("\"", "\\\"");
            }
            v = "\"" + v + "\"";

            return v;
        }
        public string HashToString()
        {
            var uuidStr = ResourceManager.instance.ResetHash((int.Parse(value))).ToString();
            string v = "";
            if (uuidStr != null)
            {
                v = uuidStr.Replace("\\", "\\\\");
                v = v.Replace("\"", "\\\"");
            }
            v = "\"" + v + "\"";

            return v;
        }
    }

    /**
	 * hashCode类型
	 */
    public class MyJson_HashCode : IJsonNode
    {
        public int value;
        public MyJson_HashCode(int value)
        {
            this.value = value;
        }
        public void CovertToStringWithFormat(StringBuilder sb, int spacesub)
        {
        }

        public override string ToString()
        {
            return "{" + "\"" + "uuid" + "\":" + "\"" + ResourceManager.instance.ResetHash((int)value).ToString() + "\"" + "}";
        }
    }

    /**
	 * 数组类型
	 */
    public class MyJson_Array : List<IJsonNode>, IJsonNode
    {
        /**
		 * 将数据写成树结构
		 */
        public void CovertToStringWithFormat(StringBuilder sb, int spaceCount)
        {
            sb.Append("[\n");
            for (int i = 0; i < this.Count; i++)
            {
                for (int spaceIndex = 0; spaceIndex < spaceCount; spaceIndex++)
                {
                    sb.Append(' ');
                }
                this[i].CovertToStringWithFormat(sb, spaceCount + 4);
                if (i != this.Count - 1)
                {
                    sb.Append(',');
                }
                sb.Append('\n');
            }

            for (int i = 0; i < spaceCount - 4; i++)
            {
                sb.Append(' ');
            }
            sb.Append(']');
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < this.Count; i++)
            {
                sb.Append(this[i].ToString());
                if (i != this.Count - 1)
                {
                    sb.Append(",");
                }

            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    /**
	 * 数据树，用于.mat.json和.ingdesc.json文件
	 */
    public class MyJson_Tree : Dictionary<string, IJsonNode>, IJsonNode
    {
        public bool isWithFormat = true;
        public MyJson_Tree()
        {

        }
        public MyJson_Tree(bool isWithFormat)
        {
            this.isWithFormat = isWithFormat;
        }

        public void CovertToString(StringBuilder sb)
        {
            sb.Append("{");
            int i = Count;
            foreach (var item in this)
            {
                sb.Append("\"" + item.Key + "\":" + item.Value.ToString());
                i--;
                if (i != 0)
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");
        }

        /**
		 * 将数据写成树结构
		 */
        public void CovertToStringWithFormat(StringBuilder sb, int spacesub)
        {
            sb.Append("{");
            int i = Count;
            foreach (var item in this)
            {
                for (int _i = 0; _i < spacesub; _i++)
                {
                    sb.Append(' ');
                }

                sb.Append('\"');
                sb.Append(item.Key);
                sb.Append("\":");

                item.Value.CovertToStringWithFormat(sb, spacesub + 4);
                i--;
                if (i != 0)
                {
                    sb.Append(',');
                }
            }
            for (int _i = 0; _i < spacesub - 4; _i++)
            {
                sb.Append(' ');
            }
            sb.Append("}");
        }

        private string FormatJsonString(string str, int spacesub = 4)
        {
            //格式化json字符串
            var result = str;
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = spacesub,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                result = textWriter.ToString();
                textWriter.Close();
                jsonWriter.Close();
            }
            tr.Close();
            jtr.Close();
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.isWithFormat)
            {
                this.CovertToString(sb);
                return this.FormatJsonString(sb.ToString());
            }
            else
            {
                this.CovertToString(sb);
                return sb.ToString();
            }
        }
    }

    /**
	 * 输出列表的每一项,用于.scene.json文件和.prefab.json文件
	 */
    public class MyJson_Object : Dictionary<string, IJsonNode>, IJsonNode
    {
        public void CovertToStringWithFormat(StringBuilder sb, int spacesub)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            int i = Count;
            foreach (var item in this)
            {
                if (item.Key == "uuid")
                {
                    sb.Append("\"" + item.Key + "\":" + (item.Value as MyJson_String).HashToString());
                }
                else
                {
                    sb.Append("\"" + item.Key + "\":" + item.Value.ToString());
                }
                i--;
                if (i != 0)
                {
                    sb.Append(",");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
