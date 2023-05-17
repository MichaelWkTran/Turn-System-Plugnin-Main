using UnityEngine.Playables;

public static class TimelineUtils
{
    public static bool TryGetPlayableBindingByName(this PlayableDirector _director, string _streamName, out PlayableBinding _binding)
    {
        //Loop through all outputs in the director
        foreach (var playableAssetOutput in _director.playableAsset.outputs)
            //Check whether the playable binding names match
            if (playableAssetOutput.streamName == _streamName)
            {
                _binding = playableAssetOutput;
                return true;
            }
        
        return false;
    }   
}
