using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;
using VanillaCloudStorageClient;

namespace VanillaCloudStorageClientTest
{
    [TestFixture]
    public class SerializeableCloudStorageCredentialsTest
    {
        [Test]
        public void EncryptBeforeSerializationProtectsAllNecessaryProperties()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            // The Serialization* are set and are not plaintext
            Assert.IsNotNull(credentials.SerializeableAccessToken);
            Assert.IsNotNull(credentials.SerializeableExpiryDate);
            Assert.IsNotNull(credentials.SerializeableRefreshToken);
            Assert.IsNotNull(credentials.SerializeableUsername);
            Assert.IsNotNull(credentials.SerializeablePassword);
            Assert.IsNotNull(credentials.SerializeableUrl);
            Assert.IsNotNull(credentials.SerializeableSecure);
            Assert.AreNotEqual(credentials.Token.AccessToken, credentials.SerializeableAccessToken);
            Assert.AreNotEqual(credentials.Token.RefreshToken, credentials.SerializeableRefreshToken);
            Assert.AreNotEqual(credentials.Username, credentials.SerializeableUsername);
            Assert.AreNotEqual(credentials.UnprotectedPassword, credentials.SerializeablePassword);
        }

        [Test]
        public void EncryptBeforeSerializationRespectsNullProperties()
        {
            var credentials = new SerializeableCloudStorageCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            // The Serialization* are set and are not plaintext
            Assert.IsNull(credentials.SerializeableAccessToken);
            Assert.IsNull(credentials.SerializeableExpiryDate);
            Assert.IsNull(credentials.SerializeableRefreshToken);
            Assert.IsNull(credentials.SerializeableUsername);
            Assert.IsNull(credentials.SerializeablePassword);
            Assert.IsNull(credentials.SerializeableUrl);
        }

        [Test]
        public void DecryptAfterDesrializationCanReadAllPropertiesBack()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            credentials.Token = null;
            credentials.Username = null;
            credentials.Password = null;
            credentials.Url = null;
            credentials.Secure = false;

            credentials.DecryptAfterDeserialization(PseudoDecrypt);

            Assert.AreEqual("atk", credentials.Token.AccessToken);
            Assert.AreEqual(new DateTime(1999, 12, 24, 0, 0, 0, DateTimeKind.Utc), credentials.Token.ExpiryDate);
            Assert.AreEqual("rtk", credentials.Token.RefreshToken);
            Assert.AreEqual("usr", credentials.Username);
            Assert.AreEqual("pwd", credentials.UnprotectedPassword);
            Assert.IsTrue(credentials.Secure);
        }

        [Test]
        public void DecryptAfterDesrializationRespectsNullProperties()
        {
            var credentials = new SerializeableCloudStorageCredentials();
            credentials.DecryptAfterDeserialization(PseudoDecrypt);

            // The Serialization* are set and are not plaintext
            Assert.IsNull(credentials.Token);
            Assert.IsNull(credentials.Username);
            Assert.IsNull(credentials.Password);
            Assert.IsNull(credentials.Url);
            Assert.IsFalse(credentials.Secure);
        }

        [Test]
        public void SerializedXmlDoesNotContainPlaintextData()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string xml = SerializeWithXmlSerializer(credentials);
            Assert.IsFalse(xml.Contains("atk"));
            Assert.IsFalse(xml.Contains("rtk"));
            Assert.IsFalse(xml.Contains("usr"));
            Assert.IsFalse(xml.Contains("pwd"));
        }

        [Test]
        public void SerializedXmlDoesNotContainNullProperties()
        {
            var credentials = new SerializeableCloudStorageCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string xml = SerializeWithXmlSerializer(credentials);
            Assert.IsFalse(xml.Contains("<access_token>"));
            Assert.IsFalse(xml.Contains("<refresh_token>"));
            Assert.IsFalse(xml.Contains("<username>"));
            Assert.IsFalse(xml.Contains("<password>"));
            Assert.IsFalse(xml.Contains("<url>"));
            Assert.IsFalse(xml.Contains("<secure>"));
        }

        [Test]
        public void SerializedXmlCanBeReadBack()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);
            string xml = SerializeWithXmlSerializer(credentials);

            var credentials2 = DeserializeWithXmlSerializer<SerializeableCloudStorageCredentials>(xml);
            credentials2.DecryptAfterDeserialization(PseudoDecrypt);

            Assert.IsTrue(credentials.AreEqualOrNull(credentials2));
        }

        [Test]
        public void SerializedJsonDoesNotContainPlaintextData()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string json = JsonConvert.SerializeObject(credentials);
            Assert.IsFalse(json.Contains("atk"));
            Assert.IsFalse(json.Contains("rtk"));
            Assert.IsFalse(json.Contains("usr"));
            Assert.IsFalse(json.Contains("pwd"));
        }

        [Test]
        public void SerializedJsonDoesNotContainNullProperties()
        {
            var credentials = new SerializeableCloudStorageCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string json = JsonConvert.SerializeObject(credentials);
            Assert.IsFalse(json.Contains("access_token"));
            Assert.IsFalse(json.Contains("refresh_token"));
            Assert.IsFalse(json.Contains("username"));
            Assert.IsFalse(json.Contains("password"));
            Assert.IsFalse(json.Contains("url"));
            Assert.IsFalse(json.Contains("secure"));
        }

        [Test]
        public void SerializedJsonCanBeReadBack()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);
            string xml = JsonConvert.SerializeObject(credentials);

            var credentials2 = JsonConvert.DeserializeObject<SerializeableCloudStorageCredentials>(xml);
            credentials2.DecryptAfterDeserialization(PseudoDecrypt);

            Assert.IsTrue(credentials.AreEqualOrNull(credentials2));
        }

        [Test]
        public void SerializedDatacontractDoesNotContainPlaintextData()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string xml = SerializeWithDatacontract(credentials);
            Assert.IsFalse(xml.Contains("atk"));
            Assert.IsFalse(xml.Contains("rtk"));
            Assert.IsFalse(xml.Contains("usr"));
            Assert.IsFalse(xml.Contains("pwd"));
        }

        [Test]
        public void SerializedDatacontractDoesNotContainNullProperties()
        {
            var credentials = new SerializeableCloudStorageCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);

            string xml = SerializeWithDatacontract(credentials);
            Assert.IsFalse(xml.Contains("<access_token>"));
            Assert.IsFalse(xml.Contains("<refresh_token>"));
            Assert.IsFalse(xml.Contains("<username>"));
            Assert.IsFalse(xml.Contains("<password>"));
            Assert.IsFalse(xml.Contains("<url>"));
            Assert.IsFalse(xml.Contains("<secure>"));
        }

        [Test]
        public void SerializedDatacontractCanBeReadBack()
        {
            SerializeableCloudStorageCredentials credentials = CreateExampleCredentials();
            credentials.EncryptBeforeSerialization(PseudoEncrypt);
            string xml = SerializeWithDatacontract(credentials);

            var credentials2 = DeserializeWithDatacontract<SerializeableCloudStorageCredentials>(xml);
            credentials2.DecryptAfterDeserialization(PseudoDecrypt);

            Assert.IsTrue(credentials.AreEqualOrNull(credentials2));
        }

        private static SerializeableCloudStorageCredentials CreateExampleCredentials()
        {
            return new SerializeableCloudStorageCredentials
            {
                CloudStorageId = "Dropbox",
                Token = new CloudStorageToken
                {
                    AccessToken = "atk",
                    ExpiryDate = new DateTime(1999, 12, 24, 0, 0, 0, DateTimeKind.Utc),
                    RefreshToken = "rtk",
                },
                Username = "usr",
                UnprotectedPassword = "pwd",
                Url = "http://url",
                Secure = true,
            };
        }

        private static string PseudoEncrypt(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        private static string PseudoDecrypt(string cipher)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(cipher));
        }

        private static string SerializeWithXmlSerializer(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        private static T DeserializeWithXmlSerializer<T>(string xml)
        {
            using (TextReader textReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(textReader);
            }
        }

        private static string SerializeWithDatacontract(object obj)
        {
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            StringBuilder sb = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(sb))
            {
                serializer.WriteObject(xmlWriter, obj);
            }
            return sb.ToString();
        }

        private static T DeserializeWithDatacontract<T>(string xml)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (TextReader textReader = new StringReader(xml))
            using (XmlReader xmlReader = XmlReader.Create(textReader))
            {
                return (T)serializer.ReadObject(xmlReader);
            }
        }
    }
}
