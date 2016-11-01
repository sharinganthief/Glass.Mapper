﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Castle.DynamicProxy;
using Glass.Mapper.Pipelines.ConfigurationResolver.Tasks.OnDemandResolver;
using Glass.Mapper.Pipelines.ObjectConstruction.Tasks.CreateInterface;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Configuration.Attributes;
using NUnit.Framework;
using Sitecore.Configuration;
using Sitecore.FakeDb;

namespace Glass.Mapper.Sc.FakeDb
{
    [TestFixture]
    public class SerializationFixture
    {
        private CreateInterfaceTask _task;

        [SetUp]
        public void Setup()
        {
            _task = new CreateInterfaceTask();
        }

        [Test]
        public void CanSerializeAndDeserializeGeneratedInterface()
        {
            //Assign

            string path = "/sitecore/content/Target";

            using (Db database = new Db
            {
                new Sitecore.FakeDb.DbItem("Target")
            })
            {
                var context = Context.Create(Utilities.CreateStandardResolver());
                context.Load(new OnDemandLoader<SitecoreTypeConfiguration>(typeof(IStubInterface)));

                var service = new SitecoreService(database.Database);

                //Act
                IStubInterface result = service.GetItem<IStubInterface>(path);
                // Console.Write(result.Path);
                byte[] serialized = Serialize(result);

                IStubInterface deserialized = Deserialize<IStubInterface>(serialized);

                //Assert
                Assert.IsNotNull(deserialized);
                Assert.AreNotEqual(result, deserialized);
                Assert.AreEqual(path, deserialized.Path);
            }
        }

        [Test]
        public void CanSerializeAndDeserializeGeneratedClass()
        {
            //Assign
            string path = "/sitecore/content/Target";

            using (Db database = new Db
            {
                new Sitecore.FakeDb.DbItem("Target")
            })
            {
                var context = Context.Create(Utilities.CreateStandardResolver());
                context.Load(new OnDemandLoader<SitecoreTypeConfiguration>(typeof(IStubInterface)));

                var service = new SitecoreService(database.Database);

                //Act
                Stub result = service.GetItem<Stub>(path);
                // Console.Write(result.Path);
                byte[] serialized = Serialize(result);

                Stub deserialized = Deserialize<Stub>(serialized);

                //Assert
                Assert.IsNotNull(deserialized);
                Assert.AreNotEqual(result, deserialized);
                Assert.AreEqual(path, deserialized.Path);
            }
        }

        [Test]
        public void CanSerializeAndDeserializeStandardProxy()
        {
            var builder = new PersistentProxyBuilder();
            var generator = new ProxyGenerator(builder);
            object proxy = generator.CreateInterfaceProxyWithoutTarget(typeof(IStubInterface), new Type[] { },
                                                new StandardInterceptor());
            byte[] serialized = Serialize(proxy);

            IStubInterface deserialized = Deserialize<IStubInterface>(serialized);
        }


        private static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        [SitecoreType(TemplateId = "{ABE81623-6250-46F3-914C-6926697B9A86}")]
        public interface IStubInterface
        {
            [SitecoreInfo(SitecoreInfoType.Path)]
            string Path { get; set; }
        }

        [SitecoreType(TemplateId = "{ABE81623-6250-46F3-914C-6926697B9A86}")]
        [Serializable]
        public class Stub
        {
            [SitecoreInfo(SitecoreInfoType.Path)]
            public string Path { get; set; }
        }
    }
}
