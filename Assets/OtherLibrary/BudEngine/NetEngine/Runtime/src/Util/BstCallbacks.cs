using System;
using System.Collections.Generic;
//using BudEngine.NetEngine.Runtime.src.Broadcast;
using BudEngine.NetEngine.src.Broadcast;

namespace BudEngine.NetEngine.src.Util
{
    public class BstCallbacks
    {
        // 房间广播
        public InnerRoomBstHandler Room = new InnerRoomBstHandler();
        
        // 清除全部广播回调函数
        public void ClearCallbacks()
        {
	        this.Room.ClearCallbacks();
        }
        
        // 本地网络变化
        public void OnNetwork(ResponseEvent eve)
        {
            this.Room.OnNetwork(eve);
        }
    }

    public class CallbackHandler<T>
    {
        private readonly HashSet<T> _broadcasts = new HashSet<T>();
        
        public void BindCallbacks(T broadcast)
        {
	        if (!this._broadcasts.Contains(broadcast))
	        {
		        this._broadcasts.Add(broadcast);
	        }
        }
        
        public void UnbindCallbacks(T broadcast)
        {
	        if (this._broadcasts.Contains(broadcast))
	        {
		        this._broadcasts.Remove(broadcast);
	        }
        }
        
        public void ClearCallbacks()
        {
            this._broadcasts.Clear();
        }
        
        protected void HandleBst(Action<T> action)
        {
            foreach (var broadcast in this._broadcasts)
            {
                action(broadcast);
            }
        }
    }

    public class InnerRoomBstHandler: CallbackHandler<RoomBroadcast>
    {
	    private static GlobalRoomBroadcast _globalBroadcast;

	    public void BindGlobalCallback(GlobalRoomBroadcast globalBroadcast)
	    {
		    _globalBroadcast = globalBroadcast;
	    }
	    
        //  玩家加入房间广播
        public void OnJoinRoom(BroadcastEvent eve) {
            this.HandleBst(broadcast => broadcast?.OnJoinRoom(eve));
        }

		// 玩家退出房间广播
		public void OnLeaveRoom(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnLeaveRoom(eve));
		}

		// 玩家解散房间广播
		public void OnDismissRoom(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnDismissRoom(eve));
		}

		// 玩家修改房间广播
		//public void OnChangeRoom(BroadcastEvent eve) {
		//	this.HandleBst(broadcast => broadcast?.OnChangeRoom(eve));
		//}

		// 玩家被踢广播
		public void OnRemovePlayer(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRemovePlayer(eve));
		}

		// 匹配超时广播
		//public void OnMatchTimeout(BroadcastEvent eve) {
		//	this.HandleBst(broadcast => broadcast?.OnMatchTimeout(eve));
		//	// 全局广播
		//	_globalBroadcast.OnMatchTimeout(eve);
		//}

		// 玩家匹配成功广播
		//public void OnMatchPlayers(BroadcastEvent eve) {
		//	this.HandleBst(broadcast => broadcast?.OnMatchPlayers(eve));
		//	// 全局广播
		//	_globalBroadcast.OnMatchPlayers(eve);
		//}

		// 取消组队匹配广播
		//public void OnCancelMatch(BroadcastEvent eve) {
		//	// 全局广播
		//	_globalBroadcast.OnCancelMatch(eve);
		//}

		// 收到消息广播
		public void OnRecvFromClient(string roomId, BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFromClient(roomId, eve));
		}

		// 自定义服务广播
		public void OnRecvFromGameSvr(string roomId, BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFromGameSvr(roomId, eve));
		}

		// 玩家网络状态变化广播
		public void OnChangePlayerNetworkState(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnChangePlayerNetworkState(eve));
		}

		// 收到帧同步消息
		public void OnRecvFrame(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFrame(eve));
		}

		// 玩家修改玩家状态广播
		//public void OnChangeCustomPlayerStatus(BroadcastEvent eve) {
		//	this.HandleBst(broadcast => broadcast?.OnChangeCustomPlayerStatus(eve));
		//}

		// 玩家修改玩家属性广播
		//public void OnChangeRoomPlayerProfile(BroadcastEvent eve) {
		//	this.HandleBst(broadcast => broadcast?.OnChangeRoomPlayerProfile(eve));
		//}

		// 开始帧同步广播
		public void OnStartFrameSync(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnStartFrameSync(eve));
		}

		// 结束帧同步广播
		public void OnStopFrameSync(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnStopFrameSync(eve));
		}

		// 本地网络状态变化
		public void OnNetwork(ResponseEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnNetwork(eve));
		}

		// 房间内游戏相关广播
		public void OnRecvBattleGameBst(BroadcastEvent eve)
		{
			this.HandleBst(broadcast => broadcast?.OnRecvBattleGameBst(eve));
		}
    }

}