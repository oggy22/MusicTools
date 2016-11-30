using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestHacks;
using MusicCore;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MusicComposer.Tests
{
    [TestClass]
    public class CompositionsTests : TestBase
    {
        private IEnumerable<string> TestSource
        {
            get
            {
                foreach (var mi in typeof(Compositions).GetMethods())
                {
                    if (mi.ReturnType == typeof(Melody12Tone))
                    yield return mi.Name;
                }
            }
        }

        [TestMethod]
        [DataSource("MusicComposer.Tests.CompositionsTests.TestSource")]
        public void Test()
        {
            var methodName = TestContext.GetRuntimeDataSourceObject<string>();
            MethodInfo mi = typeof(Compositions).GetMethod(methodName);
            Func<Melody12Tone> dg = (Func<Melody12Tone>)Delegate.CreateDelegate(typeof(Func<Melody12Tone>), mi);
            Melody12Tone m12tone = dg();
            foreach (var nwd in m12tone.Notes()) ;
        }
    }
}