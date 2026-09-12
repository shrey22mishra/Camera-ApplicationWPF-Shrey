using WpfCameraApp.Commands;

namespace WpfCameraApp.Tests
{
    public class RelayCommandTests
    {
        public void RelayCommand_Execute_Action_Is_Called()
        {
            var called = false;
            var cmd = new RelayCommand(() => called = true);

            SimpleAssert.IsTrue(cmd.CanExecute(null));
            cmd.Execute(null);
            SimpleAssert.IsTrue(called);
        }

        public void RelayCommand_CanExecute_Uses_Predicate()
        {
            bool flag = false;
            var cmd = new RelayCommand(() => { }, () => flag);

            SimpleAssert.IsFalse(cmd.CanExecute(null));
            flag = true;
            SimpleAssert.IsTrue(cmd.CanExecute(null));
        }
    }
}
