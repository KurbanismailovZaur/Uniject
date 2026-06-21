using System;
using NUnit.Framework;
using Uniject.Attributes;
using Uniject.Reflection;
using UnityEngine.Scripting;

namespace Uniject.Tests
{
    public class ReflectionCacheTests : ReflectionCacheTestFixture
    {
        [Test]
        public void InjectAttribute_WhenInspected_PreservesMarkedMembersAndDisablesInheritance()
        {
            var usage = (AttributeUsageAttribute)Attribute.GetCustomAttribute(
                typeof(InjectAttribute),
                typeof(AttributeUsageAttribute));

            Assert.That(typeof(PreserveAttribute).IsAssignableFrom(typeof(InjectAttribute)), Is.True);
            Assert.That(usage, Is.Not.Null);
            Assert.That(usage.ValidOn, Is.EqualTo(AttributeTargets.Constructor | AttributeTargets.Method));
            Assert.That(usage.Inherited, Is.False);
        }
    }
}
