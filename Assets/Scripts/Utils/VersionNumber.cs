using System;
using System.IO;
using UnityEngine;

public struct Version
{
    public ushort major { get; private set; }
    public ushort minor { get; private set; }
    public ushort patch { get; private set; }

    public static implicit operator Version(ulong verCode)
    {
        var patch = verCode % 100;
        verCode /= 100;
        var minor = verCode % 100;
        verCode /= 100;
        var major = verCode;
        return new Version((ushort)major, (ushort)minor, (ushort)patch);
    }

    public static implicit operator Version(string s)
    {
        if (!VersionNumber.CheckFormat(s, out var sz))
        {
            LoggerUtils.LogError("version format invalid:" + s);
            return new Version(0, 0, 0);
        }

        return new Version(sz[0], sz[1], sz[2]);
    }


    public Version(string s)
    {
        if (!VersionNumber.CheckFormat(s, out var sz))
            throw new FormatException("version format invalid:" + s);
        major = sz[0];
        minor = sz[1];
        patch = sz[2];
    }

    public Version(ushort major, ushort minor, ushort patch)
    {
        this.major = major;
        this.minor = minor;
        this.patch = patch;
    }

    public void Serialize(BinaryWriter w)
    {
        w.Write(major);
        w.Write(minor);
        w.Write(patch);
    }

    public void Deserialize(BinaryReader r)
    {
        major = r.ReadUInt16();
        minor = r.ReadUInt16();
        patch = r.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{major}.{minor}.{patch}";
    }
}


public enum VerifyLevel
{
    Major, 
    MajorAndMinor, 
    All 
}

public partial struct VersionNumber
{
    public static bool IsYoungerThan(ushort[] v2, ushort[] v1)
    {
        for (int i = 0; i < 3; ++i)
        {
            if (v2[i] < v1[i]) return true;
            if (v2[i] > v1[i]) return false;
        }
        return false;
    }
    
    public static ulong GetVersionCode(Version v)
    {
        return v.major * 10000u + v.minor * 100u + v.patch;
    }

    public static string GetVersionString(Version v)
    {
        return $"{v.major}.{v.minor}.{v.patch}";
    }

    public static bool CheckFormat(string ver, out ushort[] sz)
    {
        sz = new ushort[3];

        if (string.IsNullOrEmpty(ver))
        {
            
            LoggerUtils.Log($"empty version!");
            return false;
        }

        string[] splinted = ver.Split('.');
        if (splinted.Length != 3)
        {
            LoggerUtils.Log($"invalid format {ver}");
            return false;
        }

        for (int i = 0; i < splinted.Length; ++i)
        {
            try
            {
                sz[i] = ushort.Parse(splinted[i]);
            }
            catch (Exception e)
            {
                LoggerUtils.LogError(e);
                return false;
            }
        }

        return true;
    }


    public static bool Verify(Version verA, Version verB, VerifyLevel level = VerifyLevel.All)
    {
        switch (level)
        {
            case VerifyLevel.Major:
                return verA.major == verB.major;
            case VerifyLevel.MajorAndMinor:
                return verA.major == verB.major && verA.minor ==verB.minor;
            case VerifyLevel.All:
                return verA.ToString() == GetVersionString(verB);
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
}