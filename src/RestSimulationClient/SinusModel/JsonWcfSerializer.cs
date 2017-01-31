
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace DymolaModel
{
    public sealed class JsonWcfSerializer:ISerializer, IDeserializer
    {
        public string Serialize(object obj)
        {
            if (null == obj) throw new ArgumentNullException("obj");

            string result;
            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(obj.GetType());

            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractJsonSerializer.WriteObject(memoryStream, obj);
                memoryStream.Seek(0, SeekOrigin.Begin);

                using (StreamReader streamReader = new StreamReader(memoryStream))
                {
                    result = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                memoryStream.Close();
            }

            return result;
        }

        string ISerializer.RootElement
        {
            get;// { return RootElement; }
            set;// { RootElement = value; }
        }

        string IDeserializer.Namespace
        {
            get;// { return Namespace; }
            set;// { Namespace = value; }
        }

        string IDeserializer.DateFormat
        {
            get;// { return DateFormat; }
            set;// { DateFormat = value; }
        }

        string ISerializer.Namespace
        {
            get;// { return Namespace; }
            set;// { Namespace = value; }
        }

        string ISerializer.DateFormat
        {
            get;// { };
            set;// {  }
        }

        public string ContentType
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public T Deserialize<T>(IRestResponse response) // where T : new()
        {
            if (response == null) throw new ArgumentNullException("response");

            DataContractJsonSerializer dataContractJsonSerializer = new DataContractJsonSerializer(typeof (T));
            T result;

            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(response.Content)))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                result = (T) dataContractJsonSerializer.ReadObject(memoryStream);
                memoryStream.Close();
            }

            return result;
        }

        string IDeserializer.RootElement
        {
            get;// { return RootElement; }
            set;//s { RootElement = value; }
        }
    }
}
