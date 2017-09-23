using System;
using System.Timers;
using System.IO;
using System.Collections.Generic;

namespace PlayerLogger
{
	public class API:ModApiAbstract
	{
		public static API Instance{
			get{ 
				Mod thisMod = ModManager.GetMod ("PlayerLogger");
				return thisMod.ApiInstance as PlayerLogger.API;
			}
		}
        
		private string path = GameUtils.GetSaveGameDir()+Path.DirectorySeparatorChar+"playerlogger";
		private List<int> blockIds = new List<int>();

		public API ()
		{
			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}

			if (!Directory.Exists (path+Path.DirectorySeparatorChar+"blockchangelogs")) {
				Directory.CreateDirectory (path+Path.DirectorySeparatorChar+"blockchangelogs");
			}

			if (!Directory.Exists (path+Path.DirectorySeparatorChar+"chatlogs")) {
				Directory.CreateDirectory (path+Path.DirectorySeparatorChar+"chatlogs");
			}

			if (File.Exists (path + Path.DirectorySeparatorChar + "blocks.txt")) {
				string[] fileLines = File.ReadAllLines (path + Path.DirectorySeparatorChar + "blocks.txt");
				foreach (string line in fileLines) {
					int o;
					if (int.TryParse (line, out o)) {
						blockIds.Add (o);
					}
				}

				Log.Out ("[PlayerLogger] TRACKING " + blockIds.Count.ToString () + " BLOCKS FOR CHANGES");
			}
		}

		public void Save(){
			if (Directory.Exists (path)) {
				StreamWriter sw = new StreamWriter (path + Path.DirectorySeparatorChar + "blocks.txt");

				foreach (int blockId in blockIds) {
					sw.WriteLine (blockId.ToString ());
				}

				sw.Flush ();
				sw.Close ();

				Log.Out ("[PlayerLogger] TRACKING " + blockIds.Count.ToString () + " BLOCKS FOR CHANGES");
			}
		}

		public override void GameStartDone ()
		{
			GameManager.Instance.World.ChunkCache.OnBlockChangedDelegates += new ChunkCluster.OnBlockChangedDelegate (OnBlockChanged);
		}

		public override bool ChatMessage (ClientInfo ci, EnumGameMessages _type, string _msg, string _mainName, bool _localizeMain, string _secondaryName, bool _localizeSecondary)
		{
			if (_type == EnumGameMessages.Chat) {
				DateTime now = DateTime.Now;
				StreamWriter sw = File.AppendText (path + Path.DirectorySeparatorChar + "chatlogs"+Path.DirectorySeparatorChar+now.ToString("yyyy-MM-dd")+".txt");
				sw.WriteLine (now.ToString ("s")+ "\t"+ci.playerId+"\t"+ci.playerName+"\t"+_msg);
				sw.Close ();
			}

			return true;
		}

		private void OnBlockChanged (Vector3i _blockPos, BlockValue _blockValueOld, BlockValue _blockValueNew){
			ItemValue iv = _blockValueOld.ToItemValue ();
			ItemValue ivNew = _blockValueNew.ToItemValue ();
			if (iv.type != ivNew.type) {
				if(blockIds.Contains(iv.type)){
					//time to log the info
					LogPlayerPositions (_blockPos);
				}
			}
		}

		private void LogPlayerPositions(Vector3i blockPos){
			StreamWriter sw = File.AppendText (path + Path.DirectorySeparatorChar + "blockchangelogs"+Path.DirectorySeparatorChar+blockPos.ToString()+".txt");
			List<EntityPlayer> players = GameManager.Instance.World.GetPlayers ();
			DateTime now = DateTime.Now;
			foreach (EntityPlayer player in players) {
				ClientInfo ci = ConsoleHelper.ParseParamIdOrName (player.entityId.ToString());
				Vector3i playerPos = player.GetBlockPosition ();
				sw.WriteLine (now.ToString ("s")+ "\t"+ci.playerId+"\t"+ci.playerName+"\t"+(playerPos.x.ToString()+","+playerPos.y.ToString()+","+playerPos.z.ToString()));
			}
			sw.Close ();
		}

		public void addTrackedBlock(int blockId){
			if (blockIds.Contains (blockId)) {
				return;
			}

			blockIds.Add (blockId);
			Save ();
		}

		public void removeTrackedBlock(int blockId){
			if (!blockIds.Contains (blockId)) {
				return;
			}
			blockIds.Remove (blockId);
			Save ();
		}
	}
}