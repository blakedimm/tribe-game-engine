using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;

namespace TribeGameUI.Core;

public static class AudioManager
{
    private static WaveOutEvent? _bgmDevice;
    private static WaveStream? _bgmStream;
    private static string _currentBgm = "";

    private static WaveOutEvent? _ambientDevice;
    private static WaveStream? _ambientStream;
    private static string _currentAmbient = "";

    private static WaveOutEvent? _musicDevice;
    private static WaveStream? _musicStream;
    private static string _currentMusic = "";

    private static Stream? GetResourceStream(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath) || relativePath == "clear") return null;
        string resourceName = $"TribeGameUI.{relativePath.Replace('/', '.').Replace('\\', '.')}";
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    }

    public static void PlayBGM(string filePath)
    {
        if (_currentBgm == filePath) return;
        StopBGM();

        using var stream = GetResourceStream(filePath);
        if (stream == null) return;

        try
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            _bgmStream = new Mp3FileReader(ms);
            _bgmDevice = new WaveOutEvent();
            _bgmDevice.Init(_bgmStream);
            _bgmDevice.PlaybackStopped += (s, e) =>
            {
                if (_bgmStream != null && _bgmDevice != null)
                {
                    _bgmStream.Position = 0;
                    _bgmDevice.Play();
                }
            };
            _currentBgm = filePath;
            _bgmDevice.Play();
        }
        catch { }
    }

    public static void StopBGM()
    {
        _currentBgm = "";
        _bgmDevice?.Stop();
        _bgmDevice?.Dispose();
        _bgmDevice = null;
        _bgmStream?.Dispose();
        _bgmStream = null;
    }

    public static void PlayAmbient(string filePath)
    {
        if (_currentAmbient == filePath) return;
        StopAmbient();

        using var stream = GetResourceStream(filePath);
        if (stream == null) return;

        try
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            _ambientStream = new Mp3FileReader(ms);
            _ambientDevice = new WaveOutEvent();
            _ambientDevice.Init(_ambientStream);
            _ambientDevice.PlaybackStopped += (s, e) =>
            {
                if (_ambientStream != null && _ambientDevice != null)
                {
                    _ambientStream.Position = 0;
                    _ambientDevice.Play();
                }
            };
            _currentAmbient = filePath;
            _ambientDevice.Play();
        }
        catch { }
    }

    public static void StopAmbient()
    {
        _currentAmbient = "";
        _ambientDevice?.Stop();
        _ambientDevice?.Dispose();
        _ambientDevice = null;
        _ambientStream?.Dispose();
        _ambientStream = null;
    }

    public static void PlayMusic(string filePath)
    {
        if (_currentMusic == filePath) return;
        StopMusic();

        using var stream = GetResourceStream(filePath);
        if (stream == null) return;

        try
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            _musicStream = new Mp3FileReader(ms);
            _musicDevice = new WaveOutEvent();
            _musicDevice.Init(_musicStream);
            _musicDevice.PlaybackStopped += (s, e) =>
            {
                if (_musicStream != null && _musicDevice != null)
                {
                    _musicStream.Position = 0;
                    _musicDevice.Play();
                }
            };
            _currentMusic = filePath;
            _musicDevice.Play();
        }
        catch { }
    }

    public static void StopMusic()
    {
        _currentMusic = "";
        _musicDevice?.Stop();
        _musicDevice?.Dispose();
        _musicDevice = null;
        _musicStream?.Dispose();
        _musicStream = null;
    }

    public static void PlaySFX(string filePath)
    {
        using var stream = GetResourceStream(filePath);
        if (stream == null) return;

        try
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;

            var sfxStream = new Mp3FileReader(ms);
            var sfxDevice = new WaveOutEvent();
            sfxDevice.Init(sfxStream);
            sfxDevice.PlaybackStopped += (s, e) =>
            {
                sfxDevice.Dispose();
                sfxStream.Dispose();
            };
            sfxDevice.Play();
        }
        catch { }
    }
}