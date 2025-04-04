using AudioRecorder;
using AudioRecorder.Model.Enum;
using AudioRecorder.Service;
using AudioRecorder.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;


namespace AudioRecorder.ViewModel
{
    public partial class RecorderViewModel : ObservableObject
    {
        private readonly AudioService _audioService;
        private readonly MainWindowViewModel _host;
        private readonly PlayerRecorder _playerRecorder;

        [ObservableProperty]
        private AudioState _state = AudioState.Stop;

        [ObservableProperty]
        private RecorderMode _recorderMode = RecorderMode.Recorder;
         
        public int Volume
        {
            get
            {
                if (_playerRecorder is not null)
                {
                    _playerRecorder.Volume = Settings.Default.Volume;
                }
                return (int)(Settings.Default.Volume * 100);
            }

            set
            {
                Settings.Default.Volume = ((float)value / 100);
            }
        }

        [NotNull]
        [ObservableProperty]
        private ObservableCollection<Point>? _graphPoints = new ObservableCollection<Point>();

        [ObservableProperty]
        private double _startPosition = 0;
        [ObservableProperty]
        private double _currentPlaybackPositionDouble = 0;
        [ObservableProperty]
        private TimeSpan _currentPlaybackPositionTimeSpan = TimeSpan.FromSeconds(0);


        [ObservableProperty]
        private ObservableCollection<Tuple<string, string>> _devices = new();

        [ObservableProperty]
        private Tuple<string, string> _selectedDeviceTuple;

        private string _audio;
        

        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------CONSTRUCTOR-------//

        #region CONSTRUCTOR
        public RecorderViewModel(MainWindowViewModel mainWindowViewModel, AudioService audioService)
        {
            _audioService = audioService;
            _host = mainWindowViewModel;
            _playerRecorder = new PlayerRecorder();

            _playerRecorder.StateChanged += (s, state) => State = state;
            _playerRecorder.GraphChanged += (s, point) => _graphPoints.Add(point);

            this.LoadDevicesAsync();
            this.Load();
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------METHODS---------//

        #region METHODS

        public async Task Load(string path = null)
        {

            GraphPoints = new();

            State = AudioState.Loading;
    
            if (RecorderMode == RecorderMode.Player)
            {
                _audio = path;

                if (!string.IsNullOrEmpty(path))
                {
                    await BuildGraphAsync(_audio);
                    State = AudioState.Stop;
                }
                else
                {
                    State = AudioState.Default;
                }
            }
            else if (RecorderMode == RecorderMode.Recorder)
            {
                _audio = Path.Combine("Temp", $"Record {DateTime.Now.Ticks}.wav");
                Helpers.PathHelper.CreateDirectory(_audio);

                if (!string.IsNullOrEmpty(path))
                {
                    await _audioService.CopyAudioAsync(path, _audio);
                    await BuildGraphAsync(_audio);
                    State = AudioState.Stop;
                }
                else
                {
                    State = AudioState.Default;
                }
            }
        }


        public async Task BuildGraphAsync(string path = null)
        {
            State = AudioState.Loading;
            await Task.Run(() =>
            {                
                if (string.IsNullOrEmpty(path))
                {
                    GraphPoints = new();
                }
                else
                {
                    GraphPoints.Clear();
                    GraphPoints = new ObservableCollection<Point>(_audioService.GetPointsFromAudio(path));
                }
            });
            StartPosition = 0;
            //TODO переделать инициализацию
            State = AudioState.Stop;
        }

        /// <summary>
        /// Load WASAPI devices
        /// </summary>
        public async Task LoadDevicesAsync()
        {
            await Task.Run(() =>
            {
                Devices = new ObservableCollection<Tuple<string, string>>(_audioService.GetRecordDevices().Select(o => new Tuple<string, string>(o.ID, o.FriendlyName)));
                SelectedDeviceTuple = _devices.FirstOrDefault(o => o.Item1 == _audioService.GetDefaultInputDevice().ID);
            });
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------COMMANDS---------//

        #region COMMANDS
        public IRelayCommand StartRecordingCommand => new RelayCommand(() =>
        {
            _graphPoints.Clear();
            _playerRecorder.StartRecording(_audio, _selectedDeviceTuple.Item1);
        });



        public IRelayCommand StopRecordingCommand => new RelayCommand(() =>
        {
            _playerRecorder.StopRecording();

            CurrentPlaybackPositionDouble = 0;
            CurrentPlaybackPositionTimeSpan = TimeSpan.Zero;
        });



        public IRelayCommand StartPlaybackCommand => new RelayCommand(() =>
        {
            _playerRecorder.StartPlayback(_audio, _startPosition);
        });


        public IRelayCommand PausePlaybackCommand => new RelayCommand(() =>
        {
            _playerRecorder.PausePlayback();
        });


        public IRelayCommand StopPlaybackCommand => new RelayCommand(() =>
        {
            _playerRecorder.StopPlayback();
        });

        public IRelayCommand RewindCommand => new RelayCommand(() =>
        {
            StopPlaybackCommand?.Execute(null);
            StartPosition = 0;
        });

        public IAsyncRelayCommand TrimRightCommand => new AsyncRelayCommand(async () =>
        {
            await _audioService.CropAudioFileAsync(_audio, Path.Combine("Temp", $"Record {DateTime.Now.Ticks}.wav"), 0, (float)StartPosition);
            await BuildGraphAsync(_audio);
        });

        public IAsyncRelayCommand TrimLeftCommand => new AsyncRelayCommand(async () =>
        {
            await _audioService.CropAudioFileAsync(_audio, Path.Combine("Temp", $"Record {DateTime.Now.Ticks}.wav"), (float)StartPosition, 1);
            await BuildGraphAsync(_audio);
        });


        public IAsyncRelayCommand NormalizeCommand => new AsyncRelayCommand(async () =>
        {
            await _audioService.NormalizeAudioAsync(_audio, Path.Combine("Temp", $"Record {DateTime.Now.Ticks}.wav"));
            await BuildGraphAsync(_audio);
        });


        public IAsyncRelayCommand AcceptRecordingCommand => new AsyncRelayCommand(async () =>
        {
            if (State == AudioState.Play)
            {
                _playerRecorder.StopPlayback();
            }
            else if (State == AudioState.Record)
            {
                _playerRecorder.StopRecording();
            }
        });

        public IRelayCommand RejectRecordingCommand => new RelayCommand(async () =>
        {
            if (State == AudioState.Record)
            {
                StopRecordingCommand.Execute(null);
            }
            else if (State == AudioState.Default)
            {

            }
            else if (State == AudioState.Play)
            {
                StopPlaybackCommand.Execute(null);
            }
            else if (State == AudioState.Pause)
            {

            }
            else if (State == AudioState.Stop)
            {

            }

            StartPosition = 0;

            await _audioService.RemoveAudioAsync(_audio);
        });

        public IRelayCommand CloseCommand => new RelayCommand(() =>
        {
            if (State == AudioState.Record)
            {
                StopRecordingCommand.Execute(null);
            }
            else if (State == AudioState.Default)
            {

            }
            else if (State == AudioState.Play)
            {
                StopPlaybackCommand.Execute(null);
            }
            else if (State == AudioState.Pause)
            {

            }
            else if (State == AudioState.Stop)
            {

            }

            StartPosition = 0;
        });

        public IRelayCommand SaveRecordToFileCommand => new RelayCommand(async () =>
        {
            if (State == AudioState.Record)
            {
                return;
            }
            else if (State == AudioState.Default)
            {
                return;
            }
            else if (State == AudioState.Play)
            {
                return;
            }
            else if (State == AudioState.Pause)
            {

            }
            else if (State == AudioState.Stop)
            {

            }

            var picker = new SaveFileDialog();
            picker.Filter = "Audio|*.mp3";
            picker.FileName = $"Recording{DateTime.Now.ToString("ddMMyyyy_HHmmss")}";
            if (picker.ShowDialog() == true)
            {
                await _audioService.RenderToMp3Async(_audio, picker.FileName, 256000);
            }
            else
            {

            }
        });

        private IRelayCommand _openAudioFileCommand;
        public IRelayCommand OpenAudioFileCommand => _openAudioFileCommand ??= new RelayCommand(() =>
        {
            if (State == AudioState.Record)
            {
                return;
            }
            else if (State == AudioState.Default)
            {
                return;
            }
            else if (State == AudioState.Play)
            {
                return;
            }
            else if (State == AudioState.Pause)
            {

            }
            else if (State == AudioState.Stop)
            {

            }

            _playerRecorder.StopPlayback();

            var picker = new OpenFileDialog();
            picker.Filter = "Audio files|*.mp3;*.wav|*.MP3|*.mp3|*.WAV|*.wav";
            if (picker.ShowDialog() == true)
            {
                Load(picker.FileName);
            }
            else
            {
                return ;
            }
        });

        #endregion

        //------------------------------------------------------------------------------------------------------------------------------------------------------------------------OTHER----------//


    }
}
