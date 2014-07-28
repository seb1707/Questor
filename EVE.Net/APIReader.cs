//
// The MIT License (MIT)
//
// Copyright (C) 2012 Gary McNickle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace EVE.Net
{
    public class APIReader
    {
        private string cacheFile;

        public APIReader() { }

        private string ConvertFormatArgumentsToCacheString(string fmt, params object[] args)
        {
            string parsed = String.Format(CultureInfo.InvariantCulture, fmt, args);

            string[] input = parsed.Split('&');
            List<string> output = new List<string>();

            foreach (string key in input)
            {
                if (String.IsNullOrEmpty(key))
                    continue;

                string[] data = key.Split('=');

                output.Add(data[1].Replace(',', '.'));
            }

            return string.Join(".", output.ToArray());
        }

        private string ComputeHash(string inputStr)
        {
            try
            {
                byte[] result = System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputStr));
                return new Guid(result).ToString("D");
            }

            catch (ArgumentNullException)
            {
                System.Diagnostics.Debug.WriteLine("Hash has not been generated.");
                throw;
            }
        }

        private string BuildCacheFileName(object api_obj, string uri, string fmt, params object[] args)
        {
            if (!Directory.Exists(Settings.CacheFolder))
                Directory.CreateDirectory(Settings.CacheFolder);

            string baseName = Path.GetFileNameWithoutExtension(uri);

            if (baseName.Contains(".xml"))
                baseName = baseName.Substring(0, baseName.IndexOf('.'));

            string argumentGuid = ComputeHash(ConvertFormatArgumentsToCacheString(fmt, args)).ToUpper();

            baseName = string.Format("{0}.{1}.{2}.xml",
               Path.Combine(Settings.CacheFolder, Settings.APIUri.Contains(EVEConstants.singularityAPIUri) ? "s" : "t"),
               baseName,
               argumentGuid);

            if (api_obj is IAPIReader)
                (api_obj as IAPIReader).CacheFile = baseName;

            return baseName;
        }

        private string BuildUri(object api_obj, string uri, string fmt, params object[] args)
        {
            DateTime cachedUntilTime = CachedTime();

            if (api_obj is IAPIReader)
                (api_obj as IAPIReader).CachedTime = cachedUntilTime;

            if (cachedUntilTime > DateTime.Now)
                return cacheFile;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}", Settings.APIUri, uri);

            if (!String.IsNullOrEmpty(fmt))
            {
                sb.Append("?");
                sb.Append(string.Format(CultureInfo.InvariantCulture, fmt, args));
            }

            return sb.ToString();
        }

        private void SaveCacheFile(XPathNavigator nav)
        {
            if (String.IsNullOrEmpty(cacheFile))
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.None;

            try
            {
                using (XmlWriter writer = XmlWriter.Create(cacheFile, settings))
                {
                    nav.WriteSubtree(writer);
                }
            }
            catch (Exception)
            {
                // derp!
            }
        }

        private List<string> PropertyIdentifiers(PropertyInfo property)
        {
            List<string> ids = new List<string>();

            XmlIdentifier[] customAttributes = property.GetCustomAttributes(typeof(XmlIdentifier), false) as XmlIdentifier[];

            if (customAttributes == null || customAttributes.Length == 0)
                ids.Add(property.Name);
            else
            {
                ids = new List<string>(customAttributes[0].Identifier.Split(','));
            }

            return ids;
        }

        private void ParsePrimitive(object parent, PropertyInfo property, XPathNodeIterator xml)
        {
            string innerXml = xml.Current.InnerXml;

            if (property.PropertyType == typeof(string))
            {
                property.SetValue(parent, innerXml, null);
            }
            else if (!string.IsNullOrEmpty(innerXml))
            {
                // handle 'bool' types of 'true/false' and '0/1'
                if (property.PropertyType == typeof(bool))
                {
                    if (innerXml == "1")
                        innerXml = "true";
                    else if (innerXml == "0")
                        innerXml = "false";

                    bool result = false;

                    if (bool.TryParse(innerXml, out result))
                        property.SetValue(parent, result, null);
                }
                else
                    if (property.PropertyType == typeof(DateTime))
                    {
                        object result = property.PropertyType.InvokeMember(
                           "ParseExact",
                           BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                           null,
                           property.PropertyType.TypeHandle,
                           new object[] { innerXml, EVEConstants.dateStringFormat, CultureInfo.InvariantCulture },
                           CultureInfo.InvariantCulture);

                        property.SetValue(parent, result, null);
                    }
                    else
                    {
                        object result = property.PropertyType.InvokeMember(
                           "Parse",
                           BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public,
                           null,
                           property.PropertyType.TypeHandle,
                           new object[] { innerXml, CultureInfo.InvariantCulture },
                           CultureInfo.InvariantCulture);

                        property.SetValue(parent, result, null);
                    }
            }
        }

        private void ParseRowset(object parent, PropertyInfo property, XPathNodeIterator xml)
        {
            Type containerOfTypeT = null;

            if (property.PropertyType.IsGenericType)
                containerOfTypeT = property.PropertyType.GetGenericArguments()[0];
            else
                containerOfTypeT = property.PropertyType.GetElementType();

            bool isPrimitive = (containerOfTypeT.IsPrimitive || containerOfTypeT == typeof(string));

            IList listObject = null;

            if (property.PropertyType.IsArray)
                listObject = new List<object>();
            else
            {
                listObject = property.GetValue(parent, null) as IList;
                if (listObject == null)
                {
                    ConstructorInfo ctor = property.PropertyType.GetConstructor(new Type[] { });
                    listObject = ctor.Invoke(new object[0]) as IList;
                    property.SetValue(parent, listObject, null);
                }
            }

            listObject.Clear();

            XPathNodeIterator row = xml.Current.Select("row");
            while (row.MoveNext())
            {
                object rowObject = Activator.CreateInstance(containerOfTypeT);

                foreach (PropertyInfo objectProperty in rowObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    foreach (string identifier in PropertyIdentifiers(objectProperty))
                    {
                        XPathNodeIterator propertyXml = null;

                        if (string.Compare(identifier, "CDATA", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            propertyXml = row.Current.Select("text()");
                            if (propertyXml.Count > 0)
                            {
                                objectProperty.SetValue(rowObject, propertyXml.Current.InnerXml, null);
                            }
                            continue;
                        }

                        propertyXml = row.Current.Select("@" + identifier);

                        // I am assuming here that all properties stored as xml attributes are primitive types.
                        if (propertyXml != null && propertyXml.Count > 0)
                        {
                            propertyXml.MoveNext();
                            ParsePrimitive(rowObject, objectProperty, propertyXml);
                            continue;
                        }
                        else
                        {
                            // Can't just call ParseXml here, because unlike in other places, complex objects stored as a class within a rowset 
                            // are not stored with xml elements, but instead with attributes - so we have to 'Select' the xml node differently
                            if (!(objectProperty.PropertyType == typeof(string)) && typeof(IEnumerable).IsAssignableFrom(objectProperty.PropertyType))
                            {
                                propertyXml = row.Current.Select("rowset[@name=\"" + identifier + "\"]");
                            }
                            else if (objectProperty.PropertyType.IsClass)
                            {
                                propertyXml = row.Current.Select(identifier);
                            }

                            if (propertyXml.Count > 0)
                            {
                                ParseXml(rowObject, objectProperty, propertyXml);
                                continue;
                            }
                        }
                    }
                }

                listObject.Add(rowObject);
            }

            if (property.PropertyType.IsArray)
            {
                Array ar = Array.CreateInstance(containerOfTypeT, listObject.Count);

                for (int x = 0; x < listObject.Count; x++)
                    ar.SetValue(listObject[x], x);

                property.SetValue(parent, ar, null);
            }
        }

        private void ParseObject(object api_obj, XPathNodeIterator xml)
        {
            XPathNavigator result = xml.Current;
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (PropertyInfo objectProperty in api_obj.GetType().GetProperties(flags))
            {
                XPathNodeIterator propertyXml = null;

                foreach (string identifier in PropertyIdentifiers(objectProperty))
                {
                    if (!(objectProperty.PropertyType == typeof(string)) && typeof(IEnumerable).IsAssignableFrom(objectProperty.PropertyType))
                    {
                        propertyXml = result.Select("rowset[@name=\"" + identifier + "\"]");
                    }
                    else
                    {
                        propertyXml = result.Select(identifier);

                        if (propertyXml == null || propertyXml.Count == 0)
                            propertyXml = result.Select(objectProperty.Name);

                        if (propertyXml == null || propertyXml.Count == 0) // object within a row ?
                        {
                            XPathNavigator marker = result.Clone();

                            bool moved = false;

                            propertyXml = result.Select("@" + identifier);

                            if (propertyXml.Count == 0)
                            {
                                moved = true;
                                result.MoveToFirstChild();
                                propertyXml = result.Select("@" + identifier);
                            }

                            if (propertyXml.Count == 0 && moved)
                                result = marker.Clone();
                        }
                    }

                    if (propertyXml.Count > 0)
                    {
                        ParseXml(api_obj, objectProperty, propertyXml);

                        continue;
                    }
                }
            }
        }

        private void ParseXml(object parent, PropertyInfo property, XPathNodeIterator xml)
        {
            if (!xml.MoveNext())
                throw new Exception("Unexpected xml Error");

            bool isEnumerable = property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType);

            if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string) || property.PropertyType == typeof(decimal) || property.PropertyType == typeof(DateTime))
            {
                ParsePrimitive(parent, property, xml);
            }
            else if (isEnumerable)
            {
                ParseRowset(parent, property, xml);
            }
            else if (property.PropertyType.IsClass)
            {
                object newO = Activator.CreateInstance(property.PropertyType);

                ParseObject(newO, xml);

                property.SetValue(parent, newO, null);
            }
        }

        public DateTime CachedTime()
        {
            if (String.IsNullOrEmpty(cacheFile) || !File.Exists(cacheFile))
                return DateTime.MinValue;

            XPathDocument doc = new XPathDocument(cacheFile);
            XPathNavigator nav = doc.CreateNavigator();

            XPathNodeIterator cachedUntil = nav.Select("//cachedUntil");
            if (cachedUntil == null || cachedUntil.Count == 0)
                return DateTime.MinValue;

            cachedUntil.MoveNext();
            DateTime releaseTime = DateTime.ParseExact(cachedUntil.Current.Value.ToString(), EVEConstants.dateStringFormat, CultureInfo.InvariantCulture);

            return releaseTime;
        }

        public string CacheFile { get { return cacheFile; } }

        public bool Query(string uri, object api_obj, string fmt, params object[] args)
        {
            try
            {
                cacheFile = BuildCacheFileName(api_obj, uri, fmt, args);

                string address = BuildUri(api_obj, uri, fmt, args);

                XPathDocument doc = new XPathDocument(address);
                XPathNavigator nav = doc.CreateNavigator();

                XmlNamespaceManager ns = new XmlNamespaceManager(nav.NameTable);
                XPathNodeIterator nodes = nav.Select("//result", ns);

                try
                {
                    while (nodes.MoveNext())
                    {
                        ParseObject(api_obj, nodes);
                    }
                }
                catch (Exception e)
                {
                    (api_obj as APIObject).error = new APIError(0, "An unexpected error occurred. Recorded error: " + e.ToString());
                    if (!Settings.FailGracefully)
                    {
                        throw e;
                    }
                    return false;
                }

                XPathNodeIterator errornode = nav.Select("//error", ns);
                if (errornode != null && errornode.Count > 0)
                {
                    errornode.MoveNext();
                    (api_obj as APIObject).error = new APIError(Convert.ToInt32(errornode.Current.GetAttribute("code", errornode.Current.NamespaceURI)),
                                                                errornode.Current.Value);
                    if (!Settings.FailGracefully)
                    {
                        throw new Exception("Error fetching API: " + errornode.Current.Value);
                    }
                    return false;
                }

                if (!string.Equals(cacheFile, address))
                    SaveCacheFile(nav);
            }
            catch (System.Net.WebException e)
            {
                (api_obj as APIObject).error = new APIError(0, "Could not connect to the API server. Recorded error: " + e.Message);
                if (!Settings.FailGracefully)
                {
                    throw e;
                }
                return false;
            }
            catch (Exception e)
            {
                (api_obj as APIObject).error = new APIError(0, "Unexpected error: " + e.Message);
                if (!Settings.FailGracefully)
                {
                    throw e;
                }
                return false;
            }
            // finally, success!
            return true;
        }
    }
}