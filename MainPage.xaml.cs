namespace MauiPlayer
{
    public partial class MainPage : ContentPage
    {
        struct NAL_UNIT
        {
            public NAL_UNIT(int start, int end)
            {
                this.start = start;
                this.end = end;
            }

            public int start;
            public int end;
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            // Read the file into contents
            byte[] contents;
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("gate.h264");
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                contents = memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to read file contents: " + ex.Message, "OK");
                return;
            }
            // Iterate sequence searching for NAL blocks
            List<NAL_UNIT> units = new List<NAL_UNIT>();
            byte[]? sps = null;
            byte[]? pps = null;
            int? prev = null;
            for (int i = 0; i <= (contents.Length - 4); i++)
            {
                if (contents[i] == 0 && contents[i + 1] == 0 && contents[i + 2] == 0 && contents[i + 3] == 1)
                {
                    if (prev == null)
                    {
                        prev = i;
                    }
                    else
                    {
                        int nal_type = contents[prev.Value + 4] & 0x1f;
                        if (nal_type == 6) // SEI
                        {
                            // Ignore
                        }
                        else if (nal_type == 7) // SPS
                        {
                            if (sps == null)
                            {
                                int start = prev.Value + 4;
                                sps = contents[start..i];
                            }
                        }
                        else if (nal_type == 8) // PPS
                        {
                            if (pps == null)
                            {
                                int start = prev.Value + 4;
                                pps = contents[start..i];
                            }
                        }
                        else if (nal_type == 9) // AUD
                        {
                            // Ignore
                        }
                        else
                        {
                            units.Add(new NAL_UNIT(prev.Value + 4, i));
                        }
                        prev = i;
                    }
                }
            }
            // Make sure we have SPS and PPS
            if (sps == null)
            {
                await DisplayAlert("Error", "Failed to find SPS", "OK");
                return;
            }
            if (pps == null)
            {
                await DisplayAlert("Error", "Failed to find PPS", "OK");
                return;
            }
            // Create VideoPlayer
            layout.Remove(play_button);
            VideoPlayer video_player = new VideoPlayer(sps, pps);
            video_player.HorizontalOptions = LayoutOptions.Fill;
            video_player.VerticalOptions = LayoutOptions.Fill;
            layout.Add(video_player);
            // Wait for 10seconds while the virtual video player becomes ready, only really required for Android to setup texture. This is a really nasty but easy way to do it
            for (int i = 0; i < 100; ++i)
            {
                if (video_player.IsReady())
                {
                    break; 
                }
                await Task.Delay(100);
            }
            // Start sending packets
            foreach (NAL_UNIT unit in units)
            {
                byte[] data = contents[unit.start..unit.end];
                video_player.SendPacket(data);
                await Task.Delay(250);
            }
        }
    }
}
