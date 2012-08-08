#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion

namespace NetFx.Patterns.Adapter
{
    using System;
    using Moq;
    using Xunit;

    public class AdapterServiceSpec
    {
        [Fact]
        public void WhenAdapterImplementsMultipleAdapterInterfaces_ThenRegistersForAll()
        {
            var service = new AdapterService();

            var adapter = new Mock<IAdapter<IFrom, string>>();
            adapter.Setup(a => a.Adapt(It.IsAny<IFrom>())).Returns("from");
            adapter.As<IAdapter<IFrom2, string>>().Setup(a => a.Adapt(It.IsAny<IFrom2>())).Returns("from2");
            adapter.As<IAdapter<IFrom3, string>>().Setup(a => a.Adapt(It.IsAny<IFrom3>())).Returns("from3");

            service.Register(adapter.Object);

            Assert.Equal("from", service.To<string>(Mock.Of<IFrom>()));
            Assert.Equal("from2", service.To<string>(Mock.Of<IFrom2>()));
            Assert.Equal("from3", service.To<string>(Mock.Of<IFrom3>()));
        }

        public class GivenASingleAdapter
        {
            private AdapterService service;

            public GivenASingleAdapter()
            {
                this.service = new AdapterService();
                this.service.Register(Mock.Of<IAdapter<IFrom, string>>(a => a.Adapt(It.IsAny<IFrom>()) == "from"));
            }

            [Fact]
            public void WhenAdapterExists_ThenReturnsAdaptedObject()
            {
                var foo = Mock.Of<IFrom>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal("from", adapted);
            }

            [Fact]
            public void WhenNoAdapterExists_ThenReturnsDefaultValue()
            {
                var foo = Mock.Of<ICloneable>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal(default(string), adapted);
            }

            [Fact]
            public void WhenAdapterExistsForBaseInterface_ThenReturnsAdaptedObject()
            {
                var foo = Mock.Of<IFrom3>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal("from", adapted);
            }
        }

        public class GivenThreeAdaptersInHierarchy
        {
            private AdapterService service;

            public GivenThreeAdaptersInHierarchy()
            {
                this.service = new AdapterService();
                this.service.Register(Mock.Of<IAdapter<IFrom, string>>(a => a.Adapt(It.IsAny<IFrom>()) == "from"));
                this.service.Register(Mock.Of<IAdapter<IFrom2, string>>(a => a.Adapt(It.IsAny<IFrom2>()) == "from2"));
                this.service.Register(Mock.Of<IAdapter<IFrom3, string>>(a => a.Adapt(It.IsAny<IFrom3>()) == "from3"));
            }

            [Fact]
            public void WhenAdapterExists_ThenReturnsAdaptedObjectFromMostSpecificInterface()
            {
                var foo = Mock.Of<IFrom3>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal("from3", adapted);
            }

            [Fact]
            public void WhenAdapterExists_ThenReturnsAdaptedObjectFromSpecificInterface()
            {
                var foo = Mock.Of<IFrom2>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal("from2", adapted);
            }

            [Fact]
            public void WhenNoAdapterExists_ThenReturnsDefaultValue()
            {
                var foo = Mock.Of<ICloneable>();

                var adapted = this.service.To<string>(foo);

                Assert.Equal(default(string), adapted);
            }

            [Fact]
            public void WhenDirectlyConvertible_ThenReturnsSameObject()
            {
                var foo = new SupportsIFromITo();

                var from = this.service.To<IFrom>(foo);
                var to = this.service.To<ITo>(foo);

                Assert.Same(foo, from);
                Assert.Same(foo, to);
            }
        }

        public class GivenAConcreteTypeAdapter
        {
            private AdapterService service;

            public GivenAConcreteTypeAdapter()
            {
                this.service = new AdapterService();
                this.service.Register(new ConcreteFromAdapter());
                this.service.Register(Mock.Of<IAdapter<IFrom, string>>(a => a.Adapt(It.IsAny<IFrom>()) == "from"));
                this.service.Register(Mock.Of<IAdapter<IFrom2, string>>(a => a.Adapt(It.IsAny<IFrom2>()) == "from2"));
                this.service.Register(Mock.Of<IAdapter<IFrom3, string>>(a => a.Adapt(It.IsAny<IFrom3>()) == "from3"));
            }

            [Fact]
            public void WhenConcreteAdapterExists_ThenSelectsConcreteTypeAdapterFirst()
            {
                var from = new From();

                var adapted = this.service.To<string>(from);

                Assert.Equal("foo", adapted);
            }

            public class ConcreteFromAdapter : IAdapter<From, string>
            {
                public string Adapt(From from)
                {
                    return "foo";
                }
            }

            public class From : IFrom3
            {
            }

            public interface IFoo { }
        }

        public class StringAdapter : IAdapter<IFrom, string>
        {
            public string Adapt(IFrom from)
            {
                return from.ToString();
            }
        }

        public class SupportsIFromITo : IFrom, ITo { }

        public interface IFrom { }
        public interface IFrom2 : IFrom { }
        public interface IFrom3 : IFrom2 { }

        public interface ITo { }
        public interface ITo2 : ITo { }
        public interface ITo3 : ITo2 { }
    }
}