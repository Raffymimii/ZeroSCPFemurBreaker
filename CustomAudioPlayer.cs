using SCPSLAudioApi.AudioCore;

public class CustomAudioPlayer : AudioPlayerBase
{
    public static CustomAudioPlayer Get(ReferenceHub hub)
    {
        if (AudioPlayers.TryGetValue(hub, out AudioPlayerBase player))
        {
            if (player is CustomAudioPlayer cplayer1)
                return cplayer1;
        }

        var cplayer = hub.gameObject.AddComponent<CustomAudioPlayer>();
        cplayer.Owner = hub;

        AudioPlayers.Add(hub, cplayer);
        return cplayer;
    }
}