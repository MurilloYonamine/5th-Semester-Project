using NUnit.Framework;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Tests
{
    public class ServiceLocatorTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
        }

        [Test]
        public void RegisterAndGetService_ReturnsRegisteredInstance()
        {
            var sample = new SampleService();
            ServiceLocator.Register<ISampleService>(sample);

            ISampleService retrieved = ServiceLocator.Get<ISampleService>();

            Assert.IsNotNull(retrieved);
            Assert.AreSame(sample, retrieved);
        }

        [Test]
        public void TryGet_UnregisteredService_ReturnsFalseAndNoLog()
        {
            ServiceLocator.Clear();

            bool sawError = false;
            Application.LogCallback handler = (condition, stacktrace, type) => { if (type == LogType.Error) sawError = true; };
            Application.logMessageReceived += handler;

            bool result = ServiceLocator.TryGet<INotRegisteredService>(out var service);

            Application.logMessageReceived -= handler;

            Assert.IsFalse(result);
            Assert.IsNull(service);
            Assert.IsFalse(sawError);
        }

        private interface ISampleService { }
        private class SampleService : ISampleService { }
        private interface INotRegisteredService { }
    }
}
