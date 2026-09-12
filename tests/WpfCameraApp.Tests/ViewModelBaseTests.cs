using System.ComponentModel;
using WpfCameraApp.ViewModels;

namespace WpfCameraApp.Tests
{
    public class ViewModelBaseTests
    {
        private class TestViewModel : ViewModelBase
        {
            private int _value;
            public int Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }

        public void SetProperty_Raises_PropertyChanged()
        {
            var vm = new TestViewModel();
            string? raisedName = null;
            vm.PropertyChanged += (s, e) => raisedName = e.PropertyName;

            vm.Value = 5;

            SimpleAssert.AreEqual("Value", raisedName);
        }

        public void SetProperty_DoesNot_Raise_When_Same()
        {
            var vm = new TestViewModel();
            int raised = 0;
            vm.PropertyChanged += (s, e) => raised++;

            vm.Value = 0; // same as default

            SimpleAssert.AreEqual(0, raised);
        }
    }
}
