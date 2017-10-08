﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Captain.Application {
  /// <summary>
  ///   Hüseyin Tüfekçilerli's XML-serializable Dictionary
  ///   (see http://huseyint.com/2007/12/xml-serializable-generic-dictionary-tipi)
  /// </summary>
  /// <typeparam name="TKey">Dictionary key type</typeparam>
  /// <typeparam name="TValue">Dictionary value type</typeparam>
  public abstract class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable {
    protected abstract string ItemName { get; }
    protected abstract string KeyName { get; }
    protected abstract string ValueName { get; }

    #region IXmlSerializable Members
    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader) {
      var keySerializer = new XmlSerializer(typeof(TKey));
      var valueSerializer = new XmlSerializer(typeof(TValue));

      bool wasEmpty = reader.IsEmptyElement;
      reader.Read();

      if (wasEmpty) {
        return;
      }

      while (reader.NodeType != XmlNodeType.EndElement) {
        reader.ReadStartElement(ItemName);

        reader.ReadStartElement(KeyName);
        var key = (TKey)keySerializer.Deserialize(reader);
        reader.ReadEndElement();

        reader.ReadStartElement(ValueName);
        var value = (TValue)valueSerializer.Deserialize(reader);
        reader.ReadEndElement();

        Add(key, value);

        reader.ReadEndElement();
        reader.MoveToContent();
      }

      reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer) {
      var keySerializer = new XmlSerializer(typeof(TKey));
      var valueSerializer = new XmlSerializer(typeof(TValue));

      foreach (TKey key in Keys) {
        writer.WriteStartElement(ItemName);
        writer.WriteStartElement(KeyName);
        keySerializer.Serialize(writer, key);
        writer.WriteEndElement();

        writer.WriteStartElement(ValueName);
        TValue value = this[key];
        valueSerializer.Serialize(writer, value);
        writer.WriteEndElement();

        writer.WriteEndElement();
      }
    }
    #endregion
  }
}
