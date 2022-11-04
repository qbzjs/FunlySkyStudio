using System.Collections.Generic;

public class WeatherSoundController
{
    private Dictionary<EWeatherType, int> envMusicIds;

    public WeatherSoundController()
    {
        envMusicIds = new Dictionary<EWeatherType, int>();
        //对应GameConst.ambineMusicIds和GameConst.ambientEventDict
        envMusicIds[EWeatherType.Rain] = 2007;
        envMusicIds[EWeatherType.Snow] = 2008;
    }
    public void SyncSound(EWeatherType cur,EWeatherType last)
    {
        if (cur == EWeatherType.None)
        {
            //如果现在的白噪音还是之前天气对应的白噪音，那天气取消的时候把白噪音也关了
            int nowEnvMusicId = SceneBuilder.Inst.BGMusicEntity.Get<BGEnrMusicComponent>().enrMusicId;
            if (last != EWeatherType.None && envMusicIds.ContainsKey(last) && nowEnvMusicId == envMusicIds[last])
            {
                SceneBuilder.Inst.BGMusicEntity.Get<BGEnrMusicComponent>().enrMusicId = 0;
            }
            return;
        }
        if (!envMusicIds.ContainsKey(cur))
        {
            return;
        }

        int envMusicId = envMusicIds[cur];
        SceneBuilder.Inst.BGMusicEntity.Get<BGEnrMusicComponent>().enrMusicId = envMusicId;
    }
}