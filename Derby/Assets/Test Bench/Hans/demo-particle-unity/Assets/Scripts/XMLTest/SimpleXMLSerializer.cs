//----------------------------------------------------------------------------
//Author: Progieman
//Project: Demo Derby Dinkies
//Purpose: This script is used to extend the capabilities of the built in C#
//         XmlSerializer in order to allow for easier deserializing of custom
//         objects
//----------------------------------------------------------------------------

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

/// <summary>
/// Use instead of the basic XmlSerializer
/// </summary>
public class SimpleXMLSerializer : MonoSingleton<SimpleXMLSerializer>
{
    #region Members
    private Dictionary<Type, XmlSerializer> m_Serializers;
    #endregion Members

    #region Properties    
    private Dictionary<Type, XmlSerializer> Serializers
    {
        get { return this.m_Serializers; }
        set { this.m_Serializers = value; }
    }
    #endregion Properties

    #region Constructors
    private SimpleXMLSerializer()
    {
        this.Serializers = new Dictionary<Type, XmlSerializer>();
    }
    #endregion Constructors

    #region Public Methods
    public object Deserialize(XmlNode p_node)
    {
        Type t = Type.GetType(p_node.Name);
        if (t == null)
        {
            t = Type.GetType("." + p_node.Name);
        }
        return this.Deserialize(p_node, t);
    }

    public object Deserialize(XmlNode p_node, Type p_type)
    {
        XmlSerializer xmls = null;

        if (!this.Serializers.ContainsKey(p_type))
        {
            xmls = new XmlSerializer(p_type);
            this.Serializers.Add(p_type, xmls);
        }
        else
        {
            xmls = this.Serializers[p_type];
        }
        Debug.Log(p_type.ToString());
        return xmls.Deserialize(new XmlNodeReader(p_node));
    }

    public T Deserialize<T>(XmlNode node)
    {
        Debug.Log("HERE: " + node);
        return (T)this.Deserialize(node, typeof(T));
    }


    public void Serialize(Stream stream, object obj)
    {
        XmlSerializer serializer = null;
        if (!this.Serializers.ContainsKey(obj.GetType()))
        {
            serializer = new XmlSerializer(obj.GetType());
            this.Serializers.Add(obj.GetType(), serializer);
        }
        else
        {
            serializer = this.Serializers[obj.GetType()];
        }
        serializer.Serialize(stream, obj);
    }
    public string SerializeToString(object obj)
    {
        MemoryStream ms = new MemoryStream();
        this.Serialize(ms, obj);
        ms.Position = 0;
        TextReader tr = new StreamReader(ms);
        return tr.ReadToEnd();

    }
    #endregion Public Methods
}