using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

//=================================================================================================================

namespace jUtility
{

	#region serialization methods

	public static class Serialization
	{
		
		//-- String Serialization -----------------------------------------------------------------------
		
		public static string SerializeToString<T>( T toSerialize )	where T : class
		{	
			MemoryStream stream = new MemoryStream();
			
			BinaryFormatter bformatter = new BinaryFormatter();
			bformatter.Serialize(stream, toSerialize);
			
			string s = Convert.ToBase64String(stream.ToArray());
			stream.Close();
			
			return s;
		}
		
		public static T DeserializeFromString<T>( string serialized ) where T : class
		{
			object returnObject;
			byte[] bytes = Convert.FromBase64String(serialized);
			
			MemoryStream stream = new MemoryStream(bytes);
			
			BinaryFormatter bformatter = new BinaryFormatter();
			returnObject = bformatter.Deserialize(stream);
			
			if(returnObject is T)
				return returnObject as T;
			else
				return null;
		}

		//-- XML Serialization --------------------------------------------------------------------------
		
		//	serializes given object of type T as a node into given document
		//	the attr and attrValue parameters are an optional possibility 
		//	to give the resulting serialized node an attribute
		
		//	doc 		- the XML document to be written to
		//	root 		- the root node of the document
		//	obj			- the object of type T to be serialized
		//	attr		- name of an attribute the resulting node will have (optional)
		//	attrValue  	- Value of the attribute (optional)
		//	parent		- an alternative parent node to append to; make sure the node is in the same document
		
		public static void SerializeObjectToXmlNode<T>(XmlDocument doc, XmlNode root, T obj, string attr = "", string attrValue = "", XmlNode parent= null)
		{
			if(obj == null || doc == null || root == null)
				return;
			
			XmlElement resultNode = null;
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (MemoryStream memoryStream = new MemoryStream())
			{
				
				xmlSerializer.Serialize(memoryStream, obj);
				memoryStream.Position = 0;
				doc.Load(memoryStream);
				resultNode = doc.DocumentElement as XmlElement;
				if(attr.Length > 0 && attrValue.Length > 0)
				{
					resultNode.SetAttribute(attr, attrValue);
				}
				
			}
			if(parent == null)
				root.AppendChild(resultNode);
			else
				parent.AppendChild(resultNode);
			doc.AppendChild(root);
		}
		
		//	deserializes given node from given document and returns 
		//	the contents as type T
		
		public static T DeserializeXmlNode<T>(XmlNode node)
			where T : class
		{
			if (node == null)
				return null;
			
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using (MemoryStream memoryStream = new MemoryStream())
			{
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.ImportNode(node, true));
				doc.Save(memoryStream);
				memoryStream.Position = 0;
				XmlReader reader = XmlReader.Create(memoryStream);
				try
				{
					return (T) xmlSerializer.Deserialize(reader);
				}
				catch
				{
					return null;
				}
			}
		}
		
		//-- XML Helper ---------------------------------------------------------------------------------
		
		//	returns the absolute xpath expressio for a single given node
		//	you can index nodes by storing their path to have a fast lookup
		public static string GetXPathToNode(XmlNode node)
		{        
			if (node.NodeType == XmlNodeType.Attribute)
			{            
				// attributes have an OwnerElement, not a ParentNode; also they have            
				// to be matched by name, not found by position            
				return string.Format("{0}/@{1}", GetXPathToNode(((XmlAttribute)node).OwnerElement), node.Name);
			}
			if (node.ParentNode == null)
			{            
				// the only node with no parent is the root node, which has no path
				return "";
			}
			
			//get the index
			int iIndex = 1;
			XmlNode xnIndex = node;
			while (xnIndex.PreviousSibling != null && xnIndex.PreviousSibling.Name == xnIndex.Name)
			{
				iIndex++;
				xnIndex = xnIndex.PreviousSibling;
			}
			
			// the path to a node is the path to its parent, plus "/node()[n]", where
			// n is its position among its siblings.        
			return string.Format("{0}/{1}[{2}]", GetXPathToNode(node.ParentNode), node.Name, iIndex);
		}
		
	}


	#endregion

	//=================================================================================================================

	#region helper classes

	[System.Serializable]
	public struct SerializableVector2
	{
		public Vector2 _vec 
		{	 
			get {
				return new Vector2(x,y);
			}
			set {
				x = value.x;
				y = value.y;
			}
		}
		
		[SerializeField] float x;
		[SerializeField] float y;
		
		public SerializableVector2( Vector2 v )
        { 
            x = v.x;
            y = v.y;
        }
	}

	[System.Serializable]
	public struct SerializableVector3
	{
		public Vector3 _vec 
		{	 
			get {
				return new Vector3(x,y,z);
			}
			set {
				x = value.x;
				y = value.y;
				z = value.z;
			}
		}
		
		[SerializeField] float x;
		[SerializeField] float y;
		[SerializeField] float z;
		
		public SerializableVector3( Vector3 v )
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
	}

	[System.Serializable]
	public struct SerializableQuaternion
	{
		public Quaternion _q 
		{	 
			get {
				return new Quaternion(x,y,z,w);
			}
			set {
				w = value.w;
				x = value.x;
				y = value.y;
				z = value.z;
			}
		}
		
		[SerializeField] float w;
		[SerializeField] float x;
		[SerializeField] float y;
		[SerializeField] float z;
		
		public SerializableQuaternion( Quaternion q ) {
            w = q.w;
            x = q.x;
            y = q.y;
            z = q.z;
        }
	}

	#endregion

}

//=================================================================================================================
