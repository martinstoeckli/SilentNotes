using System.ComponentModel;
using System.Windows.Input;
using Moq;
using NUnit.Framework;
using SilentNotes.HtmlView;

namespace SilentNotesTest.HtmView
{
    [TestFixture]
    public class VueDataBindingTest
    {
        [Test]
        public void VueScriptContainsMarkedCommands()
        {
            TestViewModel viewmodel = new TestViewModel();
            Mock<IHtmlView> view = new Mock<IHtmlView>();

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
            Mock<IHtmlView> view = new Mock<IHtmlView>();

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

            VueDataBinding binding = new VueDataBinding(viewmodel, viewMock.Object);
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

            VueDataBinding binding = new VueDataBinding(viewmodel, viewMock.Object);
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

            VueDataBinding binding = new VueDataBinding(viewmodel, viewMock.Object);
            binding.StartListening();

            // Simulate Viewmodel change
            viewmodel.MyProperty = "Cat";
            viewmodel.OnPropertyChanged(nameof(viewmodel.MyProperty));

            viewMock.Verify(m => m.ExecuteJavaScript(It.Is<string>(p => p == "var newValue = 'Cat'; if (vm.MyProperty != newValue) vm.MyProperty = newValue;")), Times.Once);
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
