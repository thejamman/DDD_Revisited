using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using System.Text;
using System.Xml.Serialization;

[Serializable()]
public class BasicXMLClass : Object
{
    #region Private Members
    private int m_speed;
    private int m_armor;
    private List<string> m_types = new List<string>();
    #endregion Private Members

    [XmlAttributeAttribute("speed")]
    public int Speed
    {
        get { return this.m_speed; }
        set { this.m_speed = value; }
    }

    [XmlAttributeAttribute("armor")]
    public int Armor
    {
        get { return this.m_armor; }
        set { this.m_armor = value; }
    }

    [XmlArrayAttribute("Types")]
    public List<string> Types
    {
        get { return this.m_types; }
        set
        {
            foreach(string s in value)
            {
                this.m_types.Add(s);
            }
        }
    }

    public override string ToString()
    {
        string s = "";
        s += "-----------Car----------";
        s += "\n\tSpeed: " + this.m_speed;
        s += "\n\tArmor: " + this.m_armor;
        s += "\n\tTypes: " + this.m_types.Count;
        return s;
    }
}
