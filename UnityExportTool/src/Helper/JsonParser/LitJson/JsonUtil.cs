using UnityEngine;
using System.Collections;
using LitJsonGT;
using System;


/// <summary>
/// Json封装类
/// </summary>
public class JsonUtil
{
    /// <summary>
    /// 根据string解析成object<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T DeserializeObject<T>(string value)
    {
        T result = JsonMapper.ToObject<T>(value);
        return result;
    }

    /// <summary>
    /// 根据object生成string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string SerializeObject(object value)
    {
        string result = JsonMapper.ToJson(value);
        return result;
    }


    /// <summary>
    /// 剥离头部
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
//    public static PkgResponse UnpackageHead(string value)
//    {
//		//value = value.Substring(1,value.Length -2);
//        PkgResponse result = new PkgResponse();
//		int index = value.IndexOf("{");
//		value.Substring(index);
//        JsonData jd = JsonMapper.ToObject(value);
//        //result.head = JsonMapper.ToObject<Head>(JsonMapper.ToJson(jd["head"]));
//        result.value = JsonMapper.ToJson(jd["msg"]);
//        result.tag = JsonMapper.ToJson(jd["tag"]);
//		result.error = JsonMapper.ToJson(jd["error"]);
////        try
////        {
////            //result.value = JsonMapper.ToJson(jd["value"]);
////            result.ret = JsonMapper.ToJson(jd["ret"]);
////            //Debug.Log(result.ret);
////        }
////        catch (Exception)
////        {
////            result.value = null;
////        }
//
//        return result;
//    }
}
