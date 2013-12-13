using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;


public class XMLReaderTest : MonoBehaviour 
{
    public TextAsset m_XMLTest;

	// Use this for initialization
	void Start ()
    {
        this.StartCoroutine(this.GetCar());               
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    private IEnumerator GetCar()
    {
        BasicXMLClass test;
        TextAsset xmlText = m_XMLTest;
        if (xmlText != null)
        {
            Debug.Log(xmlText.text);
        }
        else
        {
            throw new Exception("Failed to load XML.");
        }

        MemoryStream ms = new MemoryStream(xmlText.bytes);
        StreamReader reader = new StreamReader(ms, Encoding.GetEncoding("iso-8859-1"));
        XmlDocument document = new XmlDocument();
        document.Load(reader);

        XmlNodeList carnl = document.GetElementsByTagName("BasicXMLClass");

        //Get the languagepack XmlElement;
        XmlNode carnode = carnl[0] as XmlNode;

        yield return test = SimpleXMLSerializer.instance.Deserialize<BasicXMLClass>(carnode);
        Debug.Log(test.ToString());
    }
}
