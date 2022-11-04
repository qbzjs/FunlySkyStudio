using BudEngine.NetEngine.src.Util;


namespace BudEngine.NetEngine
{
    public class Player
    {
        //public static string Id => GamePlayerInfo.GetInfo().Id;
        public static string Id => GameInfo.OpenId;

        public static string OpenId => GameInfo.OpenId;

        public static string Name => GamePlayerInfo.GetInfo().Name;

        public static string TeamId => GamePlayerInfo.GetInfo().TeamId;
        public static ulong CustomPlayerStatus => GamePlayerInfo.GetInfo().CustomPlayerStatus;

        public static string CustomProfile => GamePlayerInfo.GetInfo().CustomProfile;

        public static NetworkState CommonNetworkState => GamePlayerInfo.GetInfo().CommonNetworkState;

        public static NetworkState RelayNetworkState => GamePlayerInfo.GetInfo().RelayNetworkState;

        public static long Timestamp => GamePlayerInfo.GetInfo().Timestamp;
    }
}