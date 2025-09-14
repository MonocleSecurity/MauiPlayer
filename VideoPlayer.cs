using MauiPlayer;
using System.ComponentModel;

namespace MauiPlayer;

public class VideoPlayer : Microsoft.Maui.Controls.View, INotifyPropertyChanged
{
    public byte[] sps;
    public byte[] pps;

    public VideoPlayer(byte[] sps, byte[] pps)
    {
        this.sps = sps;
        this.pps = pps;
    }

    public bool IsReady()
    {
        if (Handler == null)
        {
            return false;
        }
        else
        {
            return (Handler as VideoHandler)!.IsReady();
        }
    }

    public void SendPacket(byte[] data)
    {
        (Handler as VideoHandler)?.SendPacket(data);
    }
}
