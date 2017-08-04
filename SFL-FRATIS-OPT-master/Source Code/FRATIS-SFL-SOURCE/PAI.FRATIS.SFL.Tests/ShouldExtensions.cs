//    Copyright 2014 Productivity Apex Inc.
//        http://www.productivityapex.com/
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace PAI.FRATIS.SFL.Tests
{
    public static class ShouldExtensions
    {
        public static T ShouldNotBeNull<T>(this T obj)
        {
            Assert.IsNotNull(obj);
            return obj;
        }

        public static T ShouldNotBeNull<T>(this T obj, string message)
        {
            Assert.IsNotNull(obj, message);
            return obj;
        }

        public static T ShouldEqual<T>(this T actual, object expected)
        {
            Assert.AreEqual(expected, actual);
            return actual;
        }

        public static double ShouldEqual(this double actual, double expected, double tolerance)
        {
            Assert.Less(Math.Abs(expected - actual), tolerance);
            return actual;
        }

        public static double ShouldEqual(this double actual, double expected, double tolerance, string message)
        {
            Assert.Less(Math.Abs(expected - actual), tolerance, message);
            return actual;
        }

        public static decimal ShouldEqual(this decimal actual, decimal expected, decimal tolerance)
        {
            Assert.Less(Math.Abs(expected - actual), tolerance);
            return actual;
        }

        public static decimal ShouldEqual(this decimal actual, decimal expected, decimal tolerance, string message)
        {
            Assert.Less(Math.Abs(expected - actual), tolerance, message);
            return actual;
        }

        public static void ShouldEqual(this object actual, object expected, string message)
        {
            Assert.AreEqual(expected, actual);
        }

        public static T ShouldNotEqual<T>(this T actual, object expected)
        {
            Assert.AreNotEqual(expected, actual);
            return actual;
        }

        public static void ShouldNotEqual(this object actual, object expected, string message)
        {
            Assert.AreNotEqual(expected, actual);
        }

        public static void PropertyValuesAreEquals<T>(T actual, T expected)
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var expectedValue = property.GetValue(expected, null);
                var actualValue = property.GetValue(actual, null);

                if (actualValue is IList)
                {
                    CollectionAssert.AreEqual((IList)actualValue, (IList)expectedValue);
                }
                else if (!Equals(expectedValue, actualValue))
                {
                    Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}",
                                property.DeclaringType.Name,
                                property.Name, expectedValue, actualValue);
                }
            }
        }

        public static void ShouldEqualProperties<T>(this T actual, T expected)
        {
            PropertyValuesAreEquals(actual, expected);
        }

        public static Exception ShouldBeThrownBy(this Type exceptionType, TestDelegate testDelegate)
        {
            return Assert.Throws(exceptionType, testDelegate);
        }

        public static object ShouldBe<T>(this object actual)
        {
            Assert.IsInstanceOf<T>(actual);
            return actual;
        }

        public static void ShouldBeNull(this object actual)
        {
            Assert.IsNull(actual);
        }

        public static void ShouldBeTheSameAs(this object actual, object expected)
        {
            Assert.AreSame(expected, actual);
        }

        public static void ShouldBeNotBeTheSameAs(this object actual, object expected)
        {
            Assert.AreNotSame(expected, actual);
        }

        public static void ShouldBeTrue(this bool source)
        {
            Assert.IsTrue(source);
        }

        public static void ShouldBeFalse(this bool source)
        {
            Assert.IsFalse(source);
        }

        public static void ShouldThrow<T>(this Action action) where T : Exception
        {
            Assert.That(action, Throws.TypeOf<T>());
        }

        public static void ShouldThrow<T>(this TestDelegate code) where T : Exception
        {
            Assert.Throws<T>(code);
        }

        public static void ShouldThrowContractException(this Action action)
        {
            const string contractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

            try
            {
                action.Invoke();
                Assert.Fail("Expected contract failure");
            }
            catch (Exception ex)
            {
                if (ex.GetType().FullName != contractExceptionName)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Verifies that a collection is empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null</exception>
        public static void ShouldBeEmpty(this IEnumerable collection)
        {
            CollectionAssert.IsEmpty(collection);
        }

        /// <summary>
        /// Verifies that a collection contains a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="collection">The collection to be inspected</param>
        /// <param name="expected">The object expected to be in the collection</param>
        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected)
        {
            CollectionAssert.Contains(collection, expected);
        }

        /// <summary>
        /// Verifies that a collection is not empty.
        /// </summary>
        /// <param name="collection">The collection to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when a null collection is passed</exception>
        public static void ShouldNotBeEmpty(this IEnumerable collection)
        {
            CollectionAssert.IsNotEmpty(collection);
        }

        /// <summary>
        /// Verifies that a collection does not contain a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the collection</param>
        /// <param name="collection">The collection to be inspected</param>
        public static void ShouldNotContain<T>(this IEnumerable<T> collection, T expected)
        {
            CollectionAssert.DoesNotContain(collection, expected);
        }


    }
}