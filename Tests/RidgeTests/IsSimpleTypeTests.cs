using NUnit.Framework;
using Ridge;
using System;

namespace RidgeTests
{
    public class IsSimpleTypeTests
    {
        [Test]
        public void IsSimpleType()
        {
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(TestEnum)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(string)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(char)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(Guid)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(bool)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(byte)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(short)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(int)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(long)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(float)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(double)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(decimal)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(sbyte)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(ushort)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(uint)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(ulong)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(DateTime)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(DateTimeOffset)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(TimeSpan)));

            Assert.IsFalse(GeneralHelpers.IsSimpleType(typeof(TestStruct)));
            Assert.IsFalse(GeneralHelpers.IsSimpleType(typeof(TestClass1)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(TestEnum?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(char?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(Guid?)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(bool?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(byte?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(short?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(int?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(long?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(float?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(double?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(decimal?)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(sbyte?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(ushort?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(uint?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(ulong?)));

            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(DateTime?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(DateTimeOffset?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(TimeSpan?)));

#if NET6_0_OR_GREATER
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(DateOnly?)));
            Assert.IsTrue(GeneralHelpers.IsSimpleType(typeof(TimeOnly?)));
#endif


            Assert.IsFalse(GeneralHelpers.IsSimpleType(typeof(TestStruct?)));
        }

        private enum TestEnum { TheValue }
#pragma warning disable
        private struct TestStruct
        {
            public string Prop1;
            public int Prop2;
        }

        private class TestClass1
        {
            public string Prop1;
            public int Prop2;
        }
#pragma warning restore
    }
}
