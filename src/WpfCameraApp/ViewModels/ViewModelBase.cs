using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfCameraApp.ViewModels
{
    // Minimal ViewModel base providing INotifyPropertyChanged support.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(backingField, value))
                return false;

            backingField = value!;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
