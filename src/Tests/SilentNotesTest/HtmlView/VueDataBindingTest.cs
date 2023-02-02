using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotesTest.HtmView
{
    [TestFixture]
    public class VueDataBindingTest
    {
        [Test]
        public void VueScriptContainsMarkedCommands()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlViewService> view = new Mock<IHtmlViewService>();

            VueDataBinding binding = new VueDataBinding(viewmodel, view.Object);
            string script = binding.BuildVueScript();

            // Contains declaration
            Assert.IsTrue(script.Contains("MyCommand: function() { vueCommandExecute('MyCommand'); },"));
            Assert.IsFalse(script.Contains("MyUnboundCommand"));
        }

        [Test]
        public void VueScriptContainsMarkedProperties()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlViewService> view = new Mock<IHtmlViewService>();

            VueDataBinding binding = new VueDataBinding(viewmodel, view.Object);
            string script = binding.BuildVueScript();

            // Contains declaration and watch
            Assert.IsTrue(script.Contains("MyProperty: 'Horse',"));
            Assert.IsTrue(script.Contains("MyProperty: function(newVal) { vuePropertyChanged('MyProperty', newVal); },"));
            Assert.IsFalse(script.Contains("MyUnboundProperty"));
        }

        [Test]
        public void ViewChangeUpdatesViewmodel()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlView> viewMock = new Mock<IHtmlView>();
            Mock<IHtmlViewService> htmlViewServiceMock = new Mock<IHtmlViewService>();
            htmlViewServiceMock
                .SetupGet(p => p.HtmlView)
                .Returns(viewMock.Object);

            VueDataBinding binding = new VueDataBinding(viewmodel, htmlViewServiceMock.Object);
            binding.StartListening();

            // Simulate view change
            string js = string.Format("vuePropertyChanged?name=MyProperty&value=Dog");
            viewMock.Raise(m => m.Navigating += null, new object[] { null, js });
            js = string.Format("vuePropertyChanged?name=MyIntProperty&value=888");
            viewMock.Raise(m => m.Navigating += null, new object[] { null, js });

            Assert.AreEqual("Dog", viewmodel.MyProperty);
            Assert.AreEqual(888, viewmodel.MyIntProperty);
        }

        [Test]
        public void ViewChangeDoesNotTriggerCircularUpdateInView()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlView> viewMock = new Mock<IHtmlView>();
            Mock<IHtmlViewService> htmlViewServiceMock = new Mock<IHtmlViewService>();
            htmlViewServiceMock
                .SetupGet(p => p.HtmlView)
                .Returns(viewMock.Object);

            VueDataBinding binding = new VueDataBinding(viewmodel, htmlViewServiceMock.Object);
            binding.StartListening();

            // Simulate view change
            string js = string.Format("vuePropertyChanged?name=MyProperty&value=Dog");
            viewMock.Raise(m => m.Navigating += null, new object[] { null, js });

            viewMock.Verify(m => m.ExecuteJavaScript(It.IsAny<string>()), Times.Never);
            viewMock.Verify(m => m.ExecuteJavaScriptReturnString(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void ViewmodelChangeUpdatesView()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlView> viewMock = new Mock<IHtmlView>();
            Mock<IHtmlViewService> htmlViewServiceMock = new Mock<IHtmlViewService>();
            htmlViewServiceMock
                .SetupGet(p => p.HtmlView)
                .Returns(viewMock.Object);

            VueDataBinding binding = new VueDataBinding(viewmodel, htmlViewServiceMock.Object);
            binding.StartListening();

            // Simulate Viewmodel change
            viewmodel.MyProperty = "Cat";
            viewmodel.OnPropertyChanged(nameof(viewmodel.MyProperty));

            viewMock.Verify(m => m.ExecuteJavaScript(It.Is<string>(p => p == "var newValue = 'Cat'; if (vm.MyProperty != newValue) vm.MyProperty = newValue;")), Times.Once);
        }

        [Test]
        public void TryFormatForView_FormatsAccordingToType()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlViewService> viewMock = new Mock<IHtmlViewService>();
            VueDataBinding binding = new VueDataBinding(viewmodel, viewMock.Object);
            string formattedValue;

            // int
            Assert.IsTrue(binding.TryFormatForView(
                new VueBindingDescription("MyIntProperty", VueBindingMode.OneWayToView), 888, out formattedValue));
            Assert.AreEqual("888", formattedValue);

            // string
            Assert.IsTrue(binding.TryFormatForView(
                new VueBindingDescription("MyStringProperty", VueBindingMode.OneWayToView), "abc", out formattedValue));
            Assert.AreEqual("'abc'", formattedValue);

            // list<string>
            Assert.IsTrue(binding.TryFormatForView(
                new VueBindingDescription("MyStringListProperty", VueBindingMode.OneWayToView), new List<string> { "a", "b" }, out formattedValue));
            Assert.AreEqual("['a','b']", formattedValue);

            // bool
            Assert.IsTrue(binding.TryFormatForView(
                new VueBindingDescription("MyBoolProperty", VueBindingMode.OneWayToView), false, out formattedValue));
            Assert.AreEqual("false", formattedValue);
        }

        private class TestViewModel : INotifyPropertyChanged
        {
            public TestViewModel()
            {
                MyProperty = "Horse";
            }

            [VueDataBinding(VueBindingMode.TwoWay)]
            public string MyProperty { get; set; }

            [VueDataBinding(VueBindingMode.TwoWay)]
            public int MyIntProperty { get; set; }

            [VueDataBinding(VueBindingMode.TwoWay)]
            public string MyStringProperty { get; set; }

            [VueDataBinding(VueBindingMode.TwoWay)]
            public List<string> MyStringListProperty { get; set; }

            [VueDataBinding(VueBindingMode.TwoWay)]
            public bool MyBoolProperty { get; set; }

            public string MyUnboundProperty { get; set; }

            [VueDataBinding(VueBindingMode.Command)]
            public ICommand MyCommand { get; set; }

            public ICommand MyUnboundCommand { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
