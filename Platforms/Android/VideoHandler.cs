using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;

namespace MauiPlayer;

public partial class VideoHandler : ViewHandler<VideoPlayer, VirtualVideoPlayer>
{
    public static IPropertyMapper<VideoPlayer, VideoHandler> PropertyMapper = new PropertyMapper<VideoPlayer, VideoHandler>(ViewHandler.ViewMapper)
    {
    };

    public static CommandMapper<VideoPlayer, VideoHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public VideoHandler() : base(PropertyMapper, CommandMapper)
    {
    }

    protected override VirtualVideoPlayer CreatePlatformView()
    {
        return new VirtualVideoPlayer(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!, VirtualView);
    }

    protected override void ConnectHandler(VirtualVideoPlayer virtual_video_player_layout)
    {
        base.ConnectHandler(virtual_video_player_layout);
    }

    protected override void DisconnectHandler(VirtualVideoPlayer virtual_video_player_layout)
    {
        virtual_video_player_layout.Dispose();
        base.DisconnectHandler(virtual_video_player_layout);
    }

    public bool IsReady()
    {
        if (PlatformView == null)
        {
            return false;
        }
        else
        {
            return PlatformView.IsReady();
        }
    }

    public void SendPacket(byte[] data)
    {
        PlatformView?.SendPacket(data);
    }
}
