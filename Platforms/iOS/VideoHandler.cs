using MapKit;
using Microsoft.Maui.Handlers;
using UIKit;

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
        return new VirtualVideoPlayer(VirtualView);
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
        return true;
    }

    public void SendPacket(byte[] data)
    {
        PlatformView?.SendPacket(data);
    }
}
