using UnityEngine;
//using Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AudioManager: MonoSingleton<AudioManager>
{
	private const string SAVE_FILE = "audio_settings.txt";
	private const string MUSIC_ENABLED_KEY = "music_enabled";
	private const string SFX_ENABLED_KEY = "sfx_enabled";

	[System.Serializable]
	public class SoundEffect
	{
		[SerializeField] public string name;
		[SerializeField] public float volume;
		[SerializeField] public AudioClip sound;
	}

	[SerializeField] private SoundEffect[] soundEffectsArray;
	[SerializeField] private AudioSource m_audioSource;
	[SerializeField] private AudioSource m_musicAudioSource;

	private float m_musicDeltaVolume = 0f;	// rate it is changing per second
	private float m_musicFadeTimeLeft = 0f;
	private float m_musicDestinationVolume = 0f;
	private string m_musicToPlayAtEndOfFade = null;
	private string m_nameOfCurrentMusic = "";
	private float m_musicPausePosition;

	private bool m_musicEnabled = true;
	private bool m_sfxEnabled = true;

	public class Sound
	{
		public float volume;
		public AudioClip sound;
	}

	public Dictionary<string,Sound> soundDictionary = new Dictionary<string,Sound> ();

	protected override void Init()
	{
	}

	public void Initialise()
	{
		// copy the serialized sfx data into a convenient dictionary.
		foreach (SoundEffect s in soundEffectsArray)
		{
			Sound sound = new Sound ();
			sound.volume = s.volume;
			sound.sound = s.sound;
			soundDictionary[s.name] = sound;
		}

		if (m_audioSource == null)
		{
			m_audioSource = GetComponent<AudioSource>();
		}

		ReadAudioSettingsFromFile();
	}

	void ReadAudioSettingsFromFile()
	{
		string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

		if (File.Exists(fullpath))
		{
			string json = File.ReadAllText(fullpath);

			Hashtable settings = JSON.JsonDecode(json) as Hashtable;

			m_musicEnabled = (bool)settings[MUSIC_ENABLED_KEY];
			m_sfxEnabled = (bool)settings[SFX_ENABLED_KEY];
		}
		else
		{
			WriteAudioSettingsToFile();
		}
	}

	public void WriteAudioSettingsToFile()
	{
		string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

		Hashtable settings = new Hashtable ();

		settings.Add(MUSIC_ENABLED_KEY, m_musicEnabled);
		settings.Add(SFX_ENABLED_KEY, m_sfxEnabled);

		string json = JSON.JsonEncode(settings);

		if (File.Exists(fullpath))
		{
			File.Delete(fullpath);
		}

		StreamWriter stream = File.AppendText(fullpath);

		stream.Write(json);

		stream.Close();
	}


	private AudioClip AudioClipFromName(string name)
	{
		if (soundDictionary.ContainsKey(name))
		{
			return soundDictionary[name].sound;
		}
		else
		{
			return null;
		}
	}

	private float VolumeFromName(string name)
	{
		if (soundDictionary.ContainsKey(name))
		{
			return soundDictionary[name].volume;
		}
		else
		{
			return 0f;
		}
	}

	public void PlayAudioClip(string name)
	{
		if (!m_sfxEnabled)
		{
			return;
		}

		AudioClip audioClip = AudioClipFromName(name);
		if (audioClip)
		{
			m_audioSource.PlayOneShot(audioClip, VolumeFromName(name));
		}
	}

	public void StopSoundEffect()
	{
		m_audioSource.Stop();
	}

	public void PlayMusic(string name, bool looping = true)
	{
		if (!m_musicEnabled)
		{
			return;
		}

		if ((m_musicFadeTimeLeft > 0) && !string.IsNullOrEmpty(m_musicToPlayAtEndOfFade))	// if there is a tune lined up to be played at the end of a currently active fade
		{
			return;
		}

		AudioClip audioClip = AudioClipFromName(name);
		if (audioClip)
		{
			m_musicFadeTimeLeft = 0f;
			m_nameOfCurrentMusic = name;
			m_musicAudioSource.clip = audioClip;
			m_musicAudioSource.volume = VolumeFromName(name);
			m_musicAudioSource.time = 0f;
			m_musicAudioSource.Play();
			m_musicAudioSource.loop = looping;
		}
	}

	public void EnsureMusic(string name, bool looping = true)
	{
		// only play music if its not already playing.
		if (m_musicEnabled && !name.Equals(m_nameOfCurrentMusic))
		{
			PlayMusic(name, looping);
		}
	}

	public void StopMusic()
	{
		m_nameOfCurrentMusic = "";
		m_musicAudioSource.Stop();
	}

	public void MusicSetVolume(float volume)
	{
		m_musicAudioSource.volume = volume;
	}

	public void MusicFadeVolumeTo(float destinationVolume, float time, string nameOfNewMusicToPlayAtEndOfFade = null)
	{
		if (!string.IsNullOrEmpty(m_nameOfCurrentMusic))
		{
			m_musicDestinationVolume = destinationVolume;
			m_musicFadeTimeLeft = time;
			m_musicDeltaVolume = (m_musicDestinationVolume - m_musicAudioSource.volume) / time;
			m_musicToPlayAtEndOfFade = nameOfNewMusicToPlayAtEndOfFade;
		}
	}

	private void MusicFadeUpdate(float deltaTime)
	{
		if (m_musicFadeTimeLeft == 0f)
		{
			return;
		}
		
		m_musicFadeTimeLeft -= deltaTime;
		if (m_musicFadeTimeLeft <= 0f)
		{
			m_musicFadeTimeLeft = 0f;
			m_musicAudioSource.volume = m_musicDestinationVolume;
			if (!string.IsNullOrEmpty(m_musicToPlayAtEndOfFade))
			{
				PlayMusic(m_musicToPlayAtEndOfFade);
				m_musicToPlayAtEndOfFade = null;
			}
		}
		else
		{
			m_musicAudioSource.volume += m_musicDeltaVolume*deltaTime;
		}
	}

	void Update()
	{
		MusicFadeUpdate(Time.deltaTime);
	}

	public float GetDurationOfSound(string name)
	{
		AudioClip audioClip = AudioClipFromName(name);
		if (audioClip)
		{
			return audioClip.length;
		}
		else
		{
			return 0f;
		}
	}

	public float PlayPositionOfMusic
	{
		get	{ return m_musicAudioSource.time; }
		set { m_musicAudioSource.time = value; }
	}

	public bool MusicEnabled
	{
		get { return m_musicEnabled; }
		set 
		{
			bool justBeenSwitchedOn = value && !m_musicEnabled;

			m_musicEnabled = value;

			if (justBeenSwitchedOn)
			{
//				PlayMusic(ChallengesTableView.AUDIO_FRONT_END_MUSIC);
			}

			if (!value)
			{
				StopMusic();
			}
		}
	}

	public void FadeAndPauseMusic(float fadeTime = 0f)
	{
		m_musicPausePosition = AudioManager.Instance.PlayPositionOfMusic;
		MusicFadeVolumeTo(0f, fadeTime);
	}

	public void UnpauseAndFadeMusic(float fadeTime = 0f)
	{
		float startPos = Mathf.Max(0f, m_musicPausePosition - fadeTime);
		PlayPositionOfMusic = startPos;
		MusicFadeVolumeTo(1f, fadeTime);
	}

	public bool SfxEnabled
	{
		get { return m_sfxEnabled; }
		set { m_sfxEnabled = value; }
	}

}
