using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour {

    private List<string> musicList;
    private AudioSource m_audioSource;
    private string currentSong;

    public Text SongNameLabel;

	// Use this for initialization
	void Start () {
        m_audioSource = GetComponent<AudioSource>();

        // TODO: make a build-time creator for text asset
        musicList = new List<string>();
        var aFile = Resources.Load<TextAsset>("MusicPlaylist");
        if (aFile)
        {
            string[] lines = aFile.text.Split(new Char[] { '\n' });
            foreach (string m in lines)
            {
                musicList.Add(m.TrimEnd(null));
            }
        }
        else
        {
            Debug.LogError("There is no playlist!");
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!m_audioSource.isPlaying)
        {
            string rndSong = GetRandomSong();
            string[] songComponents = rndSong.Split(new char[] { '|' });
            string author = "Unknown";
            if (songComponents.Length > 1)
            {
                currentSong = songComponents[0];
                author = songComponents[1];
            } else
            {
                currentSong = rndSong;
            }
            var songName = string.Format("Music/{0}", currentSong);
            var aClip = Resources.Load<AudioClip>(songName);
            if (aClip)
            {
                m_audioSource.clip = aClip;
                m_audioSource.Play();

                if (SongNameLabel != null)
                {
                    SongNameLabel.text = string.Format("{0} by {1}", currentSong, author);
                }
            }
        }

	}

    private string GetRandomSong()
    {
        if (musicList.Count > 0)
        {
            string aSong = null;
            do
            {
                int rnd = (int)UnityEngine.Random.Range(0, musicList.Count);
                aSong = musicList[rnd];
            } while (aSong == currentSong);
            return aSong;
        } else return "";
    }

    public void NextSong()
    {
        m_audioSource.Stop();
        m_audioSource.clip = null;
    }
}
