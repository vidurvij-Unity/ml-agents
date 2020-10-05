using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataRecorder : MonoBehaviour
{
    public int epoch_no = 1;
    public int success = 0;
    public int faliure = 0;
    public int totalEpisodes = 0;
    public float currentTimestep;

    public void EpisodeEnd(bool goal)
    {
        totalEpisodes++;
        if (goal)
            success++;
        else 
            faliure++;
    }

    public void Reset()
    {
        success = 0;
        faliure = 0;
    }

}
