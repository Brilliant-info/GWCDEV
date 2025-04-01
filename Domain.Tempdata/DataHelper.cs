using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Domain.Tempdata
{
    public class DataHelper
    {
        public string SerializeEntity<T>(T tObject) where T : class
        {
            string xml = null;
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(memoryStream, tObject);
                memoryStream.Position = 0;
                xml = reader.ReadToEnd();
            }
            return xml;
        }

        public T DeserializeEntity<T>(string xml) where T : class
        {
            T tObjectRestored = default(T);

            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(T));
                tObjectRestored = (T)deserializer.ReadObject(stream);
            }

            return tObjectRestored;
        }

        /*User DeserializeEntity1 Deserialize array into List<T>*/
        public List<T> DeserializeEntity1<T>(string xml) where T : class
        {
            //T tObjectRestored = default(T);

            List<T> tObjectRestored1 = default(List<T>);

            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(List<T>));
                tObjectRestored1 = (List<T>)deserializer.ReadObject(stream);
            }

            return tObjectRestored1;
        }

    }
}
