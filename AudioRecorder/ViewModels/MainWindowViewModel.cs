using AudioRecorder.Service;
using AudioRecorder.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;


namespace AudioRecorder.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        public RecorderViewModel RecorderViewModel { get; }
        public MainWindowViewModel(AudioService audioService) 
        {
            RecorderViewModel = new RecorderViewModel(this, audioService);

        }
    }
}
