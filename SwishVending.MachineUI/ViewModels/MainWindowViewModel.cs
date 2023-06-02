using ReactiveUI;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SkiaSharp;
using Avalonia.Threading;

namespace SwishVending.MachineUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private DispatcherTimer timer;
        public MainWindowViewModel()
        {
            SendChoice = ReactiveCommand.CreateFromTask<string>(RunSendChoice);
            IsQrCodeVisible = false;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);
            timer.Tick += ResetWhenTick;
        }

        public void ResetWhenTick(object? sender, EventArgs e)
        {
            IsQrCodeVisible = false;
            timer.Stop();
        }

        public bool IsQrCodeVisible { 
            get => isQrCodeVisible; 
            set 
            {
                this.RaiseAndSetIfChanged(ref isQrCodeVisible, value);
            } 
        }
        private bool isQrCodeVisible;
        public Bitmap Qr 
        {
            get => image; 
            set
            {
                this.RaiseAndSetIfChanged(ref image, value);
            } 
        }
        private Bitmap image;

        public ReactiveCommand<string, Unit> SendChoice { get; }
        async Task RunSendChoice(string parameter)
        {
            var message = new SwishTransactionRequest() { Message = $"{1},{parameter}" };
            var client = new HttpClient(); 
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var byteContent = new ByteArrayContent(buffer);
            var response = await client.PostAsync("http://localhost:7071/api/RequestSwish", byteContent);
            using (var stream = response.Content.ReadAsStream())
            {
                Qr = new Bitmap(stream);
            }
            IsQrCodeVisible = true;
            timer.Start();

        }

        public class SwishTransactionRequest
        {
            public string Message { get; set; }
            public int Price { get; set; }
        }
    }
}